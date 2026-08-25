using UnityEngine;

/// <summary>
/// A physics projectile fired by the cannon. It uses continuous 2D collision
/// detection and only scores when its own collider reaches a target collider.
/// </summary>
public sealed class WarfestBall : MonoBehaviour
{
    private Vector2 launchDirection;
    private float impactImpulse;
    private bool spent;

    public void Initialize(Vector2 direction, float force, float lifetime)
    {
        launchDirection = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.up;
        impactImpulse = force;
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (spent) return;

        WarfestTarget target = collision.collider.GetComponent<WarfestTarget>();
        if (target == null) return;

        spent = true;
        Rigidbody2D targetBody = target.GetComponent<Rigidbody2D>();
        if (targetBody != null && collision.contactCount > 0)
        {
            targetBody.AddForceAtPosition(launchDirection * impactImpulse, collision.GetContact(0).point, ForceMode2D.Impulse);
        }

        target.Break();
        Destroy(gameObject);
    }
}
