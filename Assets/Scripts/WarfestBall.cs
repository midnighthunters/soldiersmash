using UnityEngine;

/// <summary>
/// A physics projectile fired by the cannon. It uses continuous 2D collision
/// detection and only scores when its own collider reaches a target collider.
/// </summary>
public sealed class WarfestBall : MonoBehaviour
{
    /// <summary>How a shot behaves when it reaches a target, selected by the armed booster.</summary>
    public enum ShotMode
    {
        Normal,    // breaks the first target it strikes, then is spent
        Piercing,  // skull booster: smashes through and breaks every target along its path
        Explosive, // missile booster: detonates a blast on the first target it strikes
    }

    private Vector2 launchDirection;
    private float impactImpulse;
    private bool spent;
    private ShotMode mode = ShotMode.Normal;
    private WarfestGameController controller;
    private Collider2D ownCollider;
    private WarfestTarget intendedTarget;
    private float forwardSpeed = 17f;

    private static readonly Collider2D[] s_overlapResults = new Collider2D[32];

    private void Awake()
    {
        ownCollider = GetComponent<Collider2D>();
    }

    public void Initialize(Vector2 direction, float force, float lifetime,
        ShotMode shotMode = ShotMode.Normal, WarfestGameController owner = null,
        WarfestTarget target = null)
    {
        launchDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.up;
        impactImpulse = force;
        mode = shotMode;
        controller = owner;
        intendedTarget = target;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            forwardSpeed = Mathf.Max(16f, rb.linearVelocity.magnitude);
        }
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        if (spent) return;

        if (mode == ShotMode.Piercing)
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
            {
                // Lock velocity strictly along the launch trajectory at full speed so that
                // physics collisions never deflect or bounce the heavy wrecking ball downwards.
                float currentSpeed = Mathf.Max(forwardSpeed, rb.linearVelocity.magnitude);
                rb.linearVelocity = launchDirection * currentSpeed;
            }

            // Continuously sweep for targets within the 3x ball radius to ensure every block
            // along the trajectory is pulverized as the wrecking ball plows through.
            if (ownCollider != null)
            {
                float radius = (ownCollider is CircleCollider2D circle) ? circle.radius : 0.48f;
                int hitCount = Physics2D.OverlapCircle(transform.position, radius, ContactFilter2D.noFilter, s_overlapResults);
                for (int i = 0; i < hitCount; i++)
                {
                    Collider2D col = s_overlapResults[i];
                    if (col == null || col == ownCollider) continue;
                    WarfestTarget target = col.GetComponent<WarfestTarget>();
                    if (target != null && !target.IsBroken)
                    {
                        ExecuteHit(target, col.bounds.center);
                    }
                }
            }
            return;
        }

        if (intendedTarget == null) return;
        if (intendedTarget.IsBroken)
        {
            intendedTarget = null;
            return;
        }

        Vector2 ballPos = transform.position;
        Vector2 targetCenter = intendedTarget.Collider != null
            ? (Vector2)intendedTarget.Collider.bounds.center
            : (Vector2)intendedTarget.transform.position;

        Rigidbody2D body = GetComponent<Rigidbody2D>();
        if (body != null && body.bodyType == RigidbodyType2D.Dynamic)
        {
            Vector2 toTarget = targetCenter - ballPos;
            if (toTarget.sqrMagnitude > 0.0001f)
            {
                float speed = Mathf.Max(12f, body.linearVelocity.magnitude);
                body.linearVelocity = toTarget.normalized * speed;
                launchDirection = toTarget.normalized;
            }
        }

        float dist = Vector2.Distance(ballPos, targetCenter);
        if (dist <= 0.35f)
        {
            ExecuteHit(intendedTarget, targetCenter);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (spent) return;

        WarfestTarget target = collision.collider != null ? collision.collider.GetComponent<WarfestTarget>() : null;
        if (target == null)
        {
            if (mode == ShotMode.Piercing)
            {
                if (ownCollider != null && collision.collider != null)
                {
                    Physics2D.IgnoreCollision(ownCollider, collision.collider, true);
                }
                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
                {
                    float currentSpeed = Mathf.Max(forwardSpeed, rb.linearVelocity.magnitude);
                    rb.linearVelocity = launchDirection * currentSpeed;
                }
            }
            return;
        }

        // When a specific target was intended, only hit that exact target (unless skull piercing)
        if (intendedTarget != null && mode != ShotMode.Piercing && target != intendedTarget)
        {
            if (ownCollider != null && collision.collider != null)
            {
                Physics2D.IgnoreCollision(ownCollider, collision.collider, true);
            }
            return;
        }

        Vector2 contactPoint = collision.contactCount > 0
            ? collision.GetContact(0).point
            : (Vector2)transform.position;

        ExecuteHit(target, contactPoint);
    }

    private void ExecuteHit(WarfestTarget target, Vector2 contactPoint)
    {
        if (spent || target == null || target.IsBroken) return;

        Rigidbody2D targetBody = target.Body;

        if (mode == ShotMode.Explosive)
        {
            spent = true;
            if (controller != null) controller.ExplodeAt(contactPoint);
            else target.Break();
            Destroy(gameObject);
            return;
        }

        if (mode == ShotMode.Piercing)
        {
            target.Break();
            if (targetBody != null)
            {
                targetBody.bodyType = RigidbodyType2D.Dynamic;
                targetBody.gravityScale = 1f;
                targetBody.AddForceAtPosition(launchDirection * impactImpulse, contactPoint, ForceMode2D.Impulse);
            }
            if (ownCollider != null)
            {
                Collider2D[] targetColliders = target.GetComponentsInChildren<Collider2D>();
                for (int i = 0; i < targetColliders.Length; i++)
                {
                    if (targetColliders[i] != null) Physics2D.IgnoreCollision(ownCollider, targetColliders[i], true);
                }
            }

            // Immediately re-assert velocity along launch direction to maintain forward momentum
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
            {
                float currentSpeed = Mathf.Max(forwardSpeed, rb.linearVelocity.magnitude);
                rb.linearVelocity = launchDirection * currentSpeed;
            }
            return;
        }

        spent = true;
        target.Break();

        if (targetBody != null)
        {
            targetBody.AddForceAtPosition(launchDirection * impactImpulse, contactPoint, ForceMode2D.Impulse);
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (controller != null) controller.NotifyBallDestroyed();
    }
}
