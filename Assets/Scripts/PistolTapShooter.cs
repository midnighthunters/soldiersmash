using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Keeps the pistol aimed at the active mouse or touch position and sends a
/// hitscan shot through the muzzle whenever the player taps/clicks.
/// </summary>
public sealed class PistolTapShooter : MonoBehaviour
{
    [SerializeField] private float shotImpulse = 5.5f;
    [SerializeField] private float hitBlockLifetime = 0.9f;
    [SerializeField] private float maxShotDistance = 40f;
    [SerializeField] private float recoilDistance = 0.16f;
    [SerializeField] private float recoilDuration = 0.09f;
    [SerializeField] private float spriteForwardAngle = 90f;

    private Camera gameplayCamera;
    private Transform pistolVisual;
    private Transform muzzle;
    private Vector3 pistolRestLocalPosition;
    private float recoilTimeRemaining;
    private bool shotPending;
    private Vector2 pendingShotOrigin;
    private Vector2 pendingShotDirection;

    private void Awake()
    {
        gameplayCamera = Camera.main;
        pistolVisual = transform.Find("PistolVisual");
        muzzle = transform.Find("Muzzle");

        if (pistolVisual != null)
        {
            pistolRestLocalPosition = pistolVisual.localPosition;
        }
    }

private void Update()
    {
        if (gameplayCamera == null || muzzle == null)
        {
            return;
        }

        if (TryGetPointer(out Vector2 screenPosition, out bool pressed) && pressed)
        {
            AimAt(screenPosition);
            Shoot();
        }

        UpdateRecoil();
    }

private bool TryGetPointer(out Vector2 screenPosition, out bool pressed)
    {
        var touchscreen = Touchscreen.current;
        if (touchscreen != null && touchscreen.primaryTouch.press.isPressed)
        {
            screenPosition = touchscreen.primaryTouch.position.ReadValue();
            pressed = touchscreen.primaryTouch.press.wasPressedThisFrame;
            return true;
        }

        var pointer = Pointer.current;
        if (pointer == null)
        {
            screenPosition = default;
            pressed = false;
            return false;
        }

        screenPosition = pointer.position.ReadValue();
        pressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        return true;
    }

    private void AimAt(Vector2 screenPosition)
    {
        Vector3 worldPosition = gameplayCamera.ScreenToWorldPoint(
            new Vector3(screenPosition.x, screenPosition.y, -gameplayCamera.transform.position.z));
        Vector2 aimDirection = (Vector2)(worldPosition - transform.position);

        if (aimDirection.sqrMagnitude < 0.0001f)
        {
            return;
        }

        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - spriteForwardAngle);
    }

private void Shoot()
    {
        pendingShotOrigin = muzzle.position;
        pendingShotDirection = muzzle.up;
        shotPending = true;
        recoilTimeRemaining = recoilDuration;
    }

private void FixedUpdate()
    {
        if (!shotPending)
        {
            return;
        }

        shotPending = false;
        RaycastHit2D[] hits = Physics2D.RaycastAll(pendingShotOrigin, pendingShotDirection, maxShotDistance);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.rigidbody == null || hit.rigidbody.bodyType == RigidbodyType2D.Static)
            {
                continue;
            }

            if (hit.rigidbody.bodyType != RigidbodyType2D.Dynamic)
            {
                hit.rigidbody.bodyType = RigidbodyType2D.Dynamic;
            }

            hit.rigidbody.AddForceAtPosition(pendingShotDirection * shotImpulse, hit.point, ForceMode2D.Impulse);
            Destroy(hit.rigidbody.gameObject, hitBlockLifetime);
            break;
        }
    }


    private void UpdateRecoil()
    {
        if (pistolVisual == null)
        {
            return;
        }

        if (recoilTimeRemaining <= 0f)
        {
            pistolVisual.localPosition = pistolRestLocalPosition;
            return;
        }

        recoilTimeRemaining = Mathf.Max(0f, recoilTimeRemaining - Time.deltaTime);
        float normalizedTime = recoilTimeRemaining / recoilDuration;
        float recoilOffset = Mathf.Sin(normalizedTime * Mathf.PI) * recoilDistance;
        pistolVisual.localPosition = pistolRestLocalPosition - Vector3.up * recoilOffset;
    }
}
