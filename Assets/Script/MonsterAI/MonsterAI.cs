using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MonsterAI : MonoBehaviour
{
    [Header("감지 거리")]
    [SerializeField] private float detectRange;
    [SerializeField] private float attackRange;

    [Header("공격")]
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("회전")]
    [SerializeField] private float rotationSpeed = 10f;

    [Header("체력")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    private bool isDead;

    [Header("플레이어 접촉 데미지")]
    [SerializeField] private float touchDamage;
    [SerializeField] private float touchDamageInterval = 1f;
    private float nextTouchDamageTime;

    [Header("경로 갱신 주기")]
    [SerializeField] private float destinationUpdateInterval = 0.2f;
    private float nextDestinationUpdateTime;

    private Animator animator;
    private NavMeshAgent agent;
    private Transform player;

    private float nextAttackTime;

    private Collider hitCollider;

    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private static readonly int AttackHash = Animator.StringToHash("IsAttack");
    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int DieHash = Animator.StringToHash("IsDead");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        hitCollider = GetComponent<Collider>();

        agent.stoppingDistance = attackRange * 0.8f;
        agent.updateRotation = false;

        currentHealth = maxHealth;
    }

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        player = playerObject.transform;
    }

    private void Update()
    {
        if (isDead || player == null || animator == null || !agent.isOnNavMesh)
        {
            return;
        }

        Vector3 playerOffset = player.position - transform.position;

        float distanceSquared = playerOffset.sqrMagnitude;
        float attackRangeSquared = attackRange * attackRange;
        float detectRangeSquared = detectRange * detectRange;

        if (distanceSquared <= attackRangeSquared)
        {
            AttackPlayer(playerOffset);
        }
        else if (distanceSquared <= detectRangeSquared)
        {
            ChasePlayer();
        }
        else
        {
            Idle();
        }
    }

    private void Idle()
    { 
        agent.isStopped = true;

        if (agent.hasPath)
        {
            agent.ResetPath();
        }

        animator.SetBool(IsWalkingHash, false);
    }

    private void ChasePlayer()
    {
        agent.isStopped = false;

        if (Time.time >= nextDestinationUpdateTime)
        {
            agent.SetDestination(player.position);
            nextDestinationUpdateTime = Time.time + destinationUpdateInterval;
        }

        animator.SetBool(IsWalkingHash, true);

        Vector3 moveDirection = agent.steeringTarget - transform.position;
        RotateToward(moveDirection);
    }

    private void AttackPlayer(Vector3 playerOffset)
    {
        agent.isStopped = true;

        if (agent.hasPath)
        {
            agent.ResetPath();
        }

        animator.SetBool(IsWalkingHash, false);

        RotateToward(playerOffset);
        TryAttack();
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime)
        {
            return;
        }

        nextAttackTime = Time.time + attackCooldown;

        animator.SetTrigger(AttackHash);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead || !other.CompareTag("Bullet"))
        {
            return;
        }

        BulletProjectile bullet = other.GetComponentInParent<BulletProjectile>();

        currentHealth -= bullet.Damage;

        if (currentHealth <= 0f)
        {
            isDead = true;

            agent.isStopped = true;
            if (agent.hasPath)
            {
                agent.ResetPath();
            }

            hitCollider.enabled = false;

            animator.SetBool(IsWalkingHash, false);
            animator.SetTrigger(DieHash);

            Destroy(gameObject, 2);
        }
        else
        {
            animator.SetTrigger(HitHash);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isDead || !other.CompareTag("Player"))
        {
            return;
        }

        if (Time.time < nextTouchDamageTime)
        {
            return;
        }

        nextTouchDamageTime = Time.time + touchDamageInterval;

        if (other.TryGetComponent(out PlayerHealth playerHealth))
        {
            playerHealth.TakeDamage(touchDamage, transform.position);
        }
    }

    private void RotateToward(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,rotationSpeed * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position,detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}