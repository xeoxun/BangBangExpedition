using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class MachineGun : MonoBehaviour
{
    [Header("발사 설정")]
    [SerializeField] private Transform muzzle;
    [SerializeField] private Transform aimDirectionSource;
    [SerializeField] private BulletProjectile bulletPrefab;

    [Tooltip("1초 동안 발사하는 총알 수")]
    [SerializeField] private float fireRate = 10f;

    [SerializeField] private float bulletSpeed = 30f;

    [Header("탄창")]
    [SerializeField] private int magazineSize = 30;
    [SerializeField] private float reloadTime = 1.5f;

    [Header("UI")]
    [SerializeField] private AmmoUI ammoUI;

    private int currentAmmo;
    private bool triggerHeld;
    private bool isReloading;
    private float nextFireTime;

    private void Awake()
    {
        currentAmmo = magazineSize;
        triggerHeld = false;
        isReloading = false;
        nextFireTime = 0f;
    }

    private void Start()
    {
        ammoUI?.UpdateAmmo(currentAmmo, magazineSize);
    }

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.rKey.wasPressedThisFrame)
        {
            StartReload();
        }

        if (!triggerHeld || isReloading)
        {
            return;
        }

        TryFire();
    }

    public void SetTriggerHeld(bool isHeld)
    {
        triggerHeld = isHeld;

        if (triggerHeld && !isReloading)
        {
            TryFire();
        }
    }

    private void TryFire()
    {
        if (isReloading || currentAmmo <= 0)
        {
            return;
        }

        if (Time.time < nextFireTime)
        {
            return;
        }

        if (muzzle == null || bulletPrefab == null)
        {
            return;
        }

        Vector3 fireDirection = aimDirectionSource != null ? aimDirectionSource.forward : muzzle.forward;

        fireDirection.y = 0f;

        if (fireDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        fireDirection.Normalize();

        nextFireTime = Time.time + (1f / fireRate);

        BulletProjectile bullet = Instantiate(bulletPrefab, muzzle.position, Quaternion.LookRotation(fireDirection));
        bullet.Fire(fireDirection, bulletSpeed);

        currentAmmo--;

        ammoUI?.UpdateAmmo(currentAmmo, magazineSize);
    }

    private void StartReload()
    {
        if (isReloading || currentAmmo >= magazineSize)
        {
            return;
        }

        StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;

        ammoUI?.ShowReloading();

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = magazineSize;
        isReloading = false;

        ammoUI?.UpdateAmmo(currentAmmo, magazineSize);
    }
}