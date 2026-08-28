using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CapsuleCollider))]
public class MonsterAI : MonoBehaviour
{
    [Header("감지 거리")]
    [SerializeField] private float detectRange;
    [SerializeField] private float attackRange;

    [Header("공격")]
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackDamage = 5;

    [Tooltip("애니메이션 타격 순간의 실제 적중 거리")]
    [SerializeField] private float hitRange;

    [Tooltip("몬스터 정면 기준 공격 판정 각도")]
    [Range(0f, 180f)]
    [SerializeField] private float hitAngle = 90f;
    private bool hasDealtDamageThisAttack;


    [Header("회전")]
    [SerializeField] private float rotationSpeed = 10f;

    [Header("체력")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    private bool isDead;
    public bool IsDead => isDead;

    [Header("체력바")]
    [SerializeField] private MonsterHealthBar healthBar;


    [Header("플레이어 접촉 데미지")]
    [SerializeField] private float touchDamage;
    [SerializeField] private float touchDamageInterval = 1f;
    private float nextTouchDamageTime;

    [Header("경로 갱신 주기")]
    [SerializeField] private float destinationUpdateInterval = 0.2f;
    private float nextDestinationUpdateTime;

    [Header("사망 이펙트")]
    [SerializeField] private ParticleSystem deathEffectPrefab;
    [SerializeField] private Vector3 deathEffectOffset;

    private Animator animator;
    private NavMeshAgent agent;
    private Collider hitCollider;  
    private Transform player;
    private PlayerHealth playerHealth;

    private float nextAttackTime;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        hitCollider = GetComponent<Collider>();

        agent.stoppingDistance = attackRange * 0.8f;
        agent.updateRotation = false;

        currentHealth = maxHealth;

        if (healthBar == null)
        {
            healthBar =
                GetComponentInChildren<MonsterHealthBar>(true);
        }

        if (healthBar != null)
        {
            healthBar.Initialize(maxHealth, currentHealth);
        }
    }

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        playerHealth = playerObject.GetComponentInParent<PlayerHealth>();

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

        animator.SetBool("IsWalking", false);
    }

    private void ChasePlayer()
    {
        agent.isStopped = false;

        if (Time.time >= nextDestinationUpdateTime)
        {
            agent.SetDestination(player.position);
            nextDestinationUpdateTime = Time.time + destinationUpdateInterval;
        }

        animator.SetBool("IsWalking", true);

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

        animator.SetBool("IsWalking", false);

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

        hasDealtDamageThisAttack = false;

        animator.SetTrigger("IsAttack");
    }

    public void ApplyAttackDamage()
    {
        if (isDead || player == null || playerHealth == null || hasDealtDamageThisAttack)
        {
            return;
        }

        Vector3 directionToPlayer = player.position - transform.position;

        directionToPlayer.y = 0f;

        if (directionToPlayer.sqrMagnitude > hitRange * hitRange)
        {
            return;
        }

        if (directionToPlayer.sqrMagnitude < 0.001f)
        {
            return;
        }

        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer.normalized);

        if (angleToPlayer > hitAngle * 0.5f)
        {
            return;
        }

        hasDealtDamageThisAttack = true;

        Debug.Log("공격해서 아프당");
        playerHealth.TakeDamage(attackDamage, transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead || !other.CompareTag("Bullet"))
        {
            return;
        }

        BulletProjectile bullet = other.GetComponentInParent<BulletProjectile>();

        if (bullet == null)
        {
            return;
        }

        currentHealth = Mathf.Max(0f, currentHealth - bullet.Damage);

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
        }

        if (currentHealth <= 0f)
        {
            isDead = true;

            agent.isStopped = true;

            if (agent.hasPath)
            {
                agent.ResetPath();
            }

            hitCollider.enabled = false;

            if (healthBar != null)
            {
                healthBar.Hide();
            }

            animator.SetBool("IsWalking", false);
            animator.SetTrigger("IsDead");

            Invoke(nameof(Disappear), 2);
        }
        else
        {
            animator.SetTrigger("Hit");
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

    private void Disappear()
    {
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position + deathEffectOffset, Quaternion.identity);
        }

        Destroy(gameObject);
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