using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BulletProjectile : MonoBehaviour
{
    [Header("총알 설정")]
    [SerializeField] private float lifeTime;
    public float damage;
    public float Damage => damage;

    private Rigidbody bulletRigidbody;

    private Vector3 previousPosition;

    private void Awake()
    {
        bulletRigidbody = GetComponent<Rigidbody>();
    }


    public void Fire(Vector3 direction, float speed)
    {
        bulletRigidbody.linearVelocity = direction.normalized * speed;

        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
    }
}