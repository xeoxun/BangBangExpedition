using UnityEngine;

public class MachineGun : MonoBehaviour
{
    [Header("발사 설정")]
    [SerializeField] private Transform muzzle;
    [SerializeField] private Transform aimDirectionSource;
    [SerializeField] private BulletProjectile bulletPrefab;

    [Tooltip("1초 동안 발사하는 총알 수")]
    [SerializeField] private float fireRate = 10f;

    [SerializeField] private float bulletSpeed = 30f;

    private bool triggerHeld;
    private float nextFireTime;

    private void Awake()
    {
        nextFireTime = 0f;
        triggerHeld = false;
    }

    private void Update()
    {
        if (!triggerHeld)
        {
            return;
        }

        TryFire();
    }

    public void SetTriggerHeld(bool isHeld)
    {
        triggerHeld = isHeld;

        if (triggerHeld)
        {
            TryFire();
        }
    }

    private void TryFire()
    {
        if (Time.time < nextFireTime)
        {
            return;
        }

        if (muzzle == null || bulletPrefab == null)
        {
            return;
        }

        nextFireTime = Time.time + (1f / fireRate);

        Vector3 fireDirection = aimDirectionSource != null ? aimDirectionSource.forward : muzzle.forward;

        fireDirection.y = 0f;
        fireDirection.Normalize();

        BulletProjectile bullet = Instantiate(bulletPrefab, muzzle.position, Quaternion.LookRotation(fireDirection));
        bullet.Fire(fireDirection, bulletSpeed);
    }
}