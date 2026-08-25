using UnityEngine;

/// <summary>
/// Makes the on-screen fire control launch the assigned projectile toward the fort.
/// </summary>
public sealed class BlastFireControl : MonoBehaviour
{
    public Transform projectile;
    public Vector3 launchTarget = new Vector3(0f, 3.2f, 0f);
    public float flightDuration = 0.55f;
    public float resetDelay = 0.35f;

    private Vector3 launchStart;
    private float elapsed;
    private bool isLaunching;

    private void Awake()
    {
        if (projectile != null)
        {
            launchStart = projectile.position;
        }
    }

    private void OnMouseDown()
    {
        Launch();
    }

    public void Launch()
    {
        if (projectile == null || isLaunching)
        {
            return;
        }

        launchStart = projectile.position;
        elapsed = 0f;
        isLaunching = true;
    }

    private void Update()
    {
        if (!isLaunching || projectile == null)
        {
            return;
        }

        elapsed += Time.deltaTime;
        float normalizedTime = Mathf.Clamp01(elapsed / flightDuration);
        Vector3 position = Vector3.Lerp(launchStart, launchTarget, normalizedTime);
        position.y += Mathf.Sin(normalizedTime * Mathf.PI) * 0.8f;
        projectile.position = position;
        projectile.Rotate(0f, 0f, -720f * Time.deltaTime);

        if (normalizedTime >= 1f)
        {
            isLaunching = false;
            Invoke(nameof(ResetProjectile), resetDelay);
        }
    }

    private void ResetProjectile()
    {
        if (projectile != null)
        {
            projectile.position = launchStart;
            projectile.rotation = Quaternion.identity;
        }
    }
}
