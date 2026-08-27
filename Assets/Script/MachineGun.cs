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
        Debug.Log($"[Awake] {gameObject.name}, InstanceID={GetInstanceID()}, bulletPrefab={bulletPrefab}");
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
        Debug.Log($"TryFire 호출됨: Time.time={Time.time}, nextFireTime={nextFireTime}");

        if (Time.time < nextFireTime)
        {
            Debug.Log("쿨다운 중이라 리턴");
            return;
        }

        if (muzzle == null || bulletPrefab == null)
        {
            Debug.LogWarning($"muzzle={muzzle}, bulletPrefab={bulletPrefab}");
            Debug.Log($"[Awake] {gameObject.name}, InstanceID={GetInstanceID()}, bulletPrefab={bulletPrefab}");
            return;
        }

        nextFireTime = Time.time + (1f / fireRate);

        Vector3 fireDirection = aimDirectionSource != null ? aimDirectionSource.forward : muzzle.forward;

        fireDirection.y = 0f;
        fireDirection.Normalize();

        Debug.Log($"Instantiate 직전: position={muzzle.position}, direction={fireDirection}");

        BulletProjectile bullet = Instantiate(bulletPrefab, muzzle.position, Quaternion.LookRotation(fireDirection));
        bullet.Fire(fireDirection, bulletSpeed);
    }
}