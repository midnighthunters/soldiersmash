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

    private void Awake()
    {
        ownCollider = GetComponent<Collider2D>();
    }

    public void Initialize(Vector2 direction, float force, float lifetime,
        ShotMode shotMode = ShotMode.Normal, WarfestGameController owner = null)
    {
        launchDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.up;
        impactImpulse = force;
        mode = shotMode;
        controller = owner;
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (spent) return;

        WarfestTarget target = collision.collider.GetComponent<WarfestTarget>();
        if (target == null) return;

        Vector2 contactPoint = collision.contactCount > 0
            ? collision.GetContact(0).point
            : (Vector2)transform.position;
        Rigidbody2D targetBody = target.Body;

        // Missile booster: hand off to the controller's blast, which breaks the struck block and
        // everything caught in its radius. Falls back to a plain break if fired without an owner.
        if (mode == ShotMode.Explosive)
        {
            spent = true;
            if (controller != null) controller.ExplodeAt(contactPoint);
            else target.Break();
            Destroy(gameObject);
            return;
        }

        // Skull booster: break the block, shove it, then stop colliding with it so the heavy ball
        // keeps ploughing forward into the next target instead of stopping on the first one.
        if (mode == ShotMode.Piercing)
        {
            target.Break();
            if (targetBody != null)
            {
                targetBody.AddForceAtPosition(launchDirection * impactImpulse, contactPoint, ForceMode2D.Impulse);
            }
            if (ownCollider != null && collision.collider != null)
            {
                Physics2D.IgnoreCollision(ownCollider, collision.collider, true);
            }
            return; // not spent - the skull ball carries on
        }

        // Standard shot.
        spent = true;
        target.Break();

        // Break releases an authored 3D structure from its perfectly aligned kinematic pose.
        // Apply the impact after that release so the struck block receives the full impulse.
        if (targetBody != null && collision.contactCount > 0)
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
