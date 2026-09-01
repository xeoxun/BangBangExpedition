using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
public class CompanionAI : MonoBehaviour
{
    [Header("동료 영입")]
    [SerializeField] private float interactionRange = 2.5f;
    private Transform playerTransform;
    [SerializeField] private bool isFollowing;

    [Header("플레이어 추적")]
    [SerializeField] private Transform followPoint;
    [SerializeField] private float followStopDistance = 1f;
    [SerializeField] private float pathUpdateInterval = 0.2f;

    [Header("이동속도")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float sprintSpeed = 5f;

    [Header("몬스터 감지")]
    [SerializeField] private float detectionRange;
    [SerializeField] private LayerMask monsterLayer;
    [SerializeField] private float scanInterval = 0.2f;

    [Header("사격")]
    [SerializeField] private Transform muzzle;
    [SerializeField] private BulletProjectile bulletPrefab;
    [SerializeField] private float bulletSpeed = 30f;
    [SerializeField] private float fireRate = 5f;
    [SerializeField] private float aimHeight = 0.8f;

    [Header("회전")]
    [SerializeField] private float rotationSpeed = 10f;
    private Vector3 previousPosition;

    [Header("애니메이션")]
    [SerializeField] private Animator animator;

    private NavMeshAgent agent;
    private PlayerControl playerControl;
    private MonsterAI currentTarget;

    private float nextPathUpdateTime;
    private float nextScanTime;
    private float nextFireTime;

    // 몬스터 검색마다 배열이 새로 생성되는 것을 방지
    private readonly Collider[] monsterResults = new Collider[32];

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.updatePosition = true;
        agent.updateRotation = false;
        agent.isStopped = true;

        agent.speed = walkSpeed;
        agent.acceleration = 12f;
        agent.stoppingDistance = followStopDistance;

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator != null)
        {
            animator.applyRootMotion = false;
        }
    }

    private void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogError(
                "Player 태그 오브젝트를 찾지 못했습니다.",
                this
            );

            return;
        }

        playerTransform = playerObject.transform;

        playerControl =
            playerObject.GetComponent<PlayerControl>();

        if (followPoint == null)
        {
            followPoint = playerTransform;
        }

        if (playerControl == null)
        {
            Debug.LogError(
                "플레이어의 PlayerControl을 찾지 못했습니다.",
                this
            );
        }

        StopCompanion();

        previousPosition = transform.position;
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            return;
        }

        // 영입 전에는 근처에서 스페이스바 입력만 검사
        if (!isFollowing)
        {
            TryStartFollowing();
            return;
        }

        if (followPoint == null ||
            !agent.isOnNavMesh)
        {
            return;
        }

        UpdateMovementSpeed();
        ScanForMonster();
        FollowPlayer();
        UpdateAimAndAttack();
        UpdateAnimation();
    }

    private void TryStartFollowing()
    {
        Vector3 playerOffset =
            playerTransform.position -
            transform.position;

        playerOffset.y = 0f;

        if (playerOffset.sqrMagnitude >
            interactionRange * interactionRange)
        {
            return;
        }

        if (Keyboard.current == null ||
            !Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            return;
        }

        isFollowing = true;

        agent.isStopped = false;

        Debug.Log(
            $"{gameObject.name} 동료 영입 완료",
            this
        );
    }

    private void StopCompanion()
    {
        agent.isStopped = true;

        if (agent.hasPath)
        {
            agent.ResetPath();
        }

        currentTarget = null;

        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsSprinting", false);
            animator.SetBool("IsAttack", false);
        }
    }

    private void UpdateMovementSpeed()
    {
        bool playerIsSprinting =
            playerControl != null &&
            playerControl.IsSprinting;

        agent.speed = playerIsSprinting
            ? sprintSpeed
            : walkSpeed;
    }

    private void FollowPlayer()
    {
        if (playerTransform == null ||
            !agent.isOnNavMesh)
        {
            return;
        }

        Vector3 playerOffset =
            playerTransform.position -
            transform.position;

        playerOffset.y = 0f;

        float distanceSquared =
            playerOffset.sqrMagnitude;

        float stopDistanceSquared =
            followStopDistance *
            followStopDistance;

        if (distanceSquared <= stopDistanceSquared)
        {
            agent.isStopped = true;

            if (agent.hasPath)
            {
                agent.ResetPath();
            }

            return;
        }

        agent.isStopped = false;

        if (Time.time >= nextPathUpdateTime)
        {
            nextPathUpdateTime =
                Time.time + pathUpdateInterval;

            agent.SetDestination(
                playerTransform.position
            );
        }
    }

    private void ScanForMonster()
    {
        if (Time.time < nextScanTime)
        {
            return;
        }

        nextScanTime = Time.time + scanInterval;

        int detectedCount =
            Physics.OverlapSphereNonAlloc(
                transform.position,
                detectionRange,
                monsterResults,
                monsterLayer,
                QueryTriggerInteraction.Collide
            );

        MonsterAI nearestMonster = null;

        float nearestDistanceSquared = float.PositiveInfinity;

        for (int i = 0; i < detectedCount; i++)
        {
            Collider detectedCollider = monsterResults[i];

            if (detectedCollider == null)
            {
                continue;
            }

            MonsterAI monster = detectedCollider.GetComponentInParent<MonsterAI>();

            if (monster == null || monster.IsDead)
            {
                continue;
            }

            Vector3 monsterOffset = monster.transform.position - transform.position;

            float distanceSquared = monsterOffset.sqrMagnitude;

            if (distanceSquared < nearestDistanceSquared)
            {
                nearestDistanceSquared = distanceSquared;

                nearestMonster = monster;
            }
        }

        currentTarget = nearestMonster;
    }

    private void UpdateAimAndAttack()
    {
        bool hasTarget =
            currentTarget != null &&
            !currentTarget.IsDead;

        if (hasTarget)
        {
            Vector3 targetOffset =
                currentTarget.transform.position -
                transform.position;

            targetOffset.y = 0f;

            if (targetOffset.sqrMagnitude >
                detectionRange * detectionRange)
            {
                currentTarget = null;
                hasTarget = false;
            }
            else
            {
                RotateToward(targetOffset);
                TryShoot();
            }
        }

        if (animator != null)
        {
            animator.SetBool("IsAttack", hasTarget);
        }

        if (!hasTarget)
        {
            RotateAlongMovement();
        }
    }

    private void TryShoot()
    {
        if (currentTarget == null ||
            currentTarget.IsDead)
        {
            currentTarget = null;
            return;
        }

        if (Time.time < nextFireTime ||
            muzzle == null ||
            bulletPrefab == null)
        {
            return;
        }

        Vector3 targetPosition =
            currentTarget.transform.position +
            Vector3.up * aimHeight;

        Vector3 shootDirection =
            targetPosition - muzzle.position;

        if (shootDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        nextFireTime =
            Time.time + (1f / fireRate);

        BulletProjectile bullet = Instantiate(
            bulletPrefab,
            muzzle.position,
            Quaternion.LookRotation(shootDirection)
        );

        bullet.Fire(
            shootDirection.normalized,
            bulletSpeed
        );
    }

    private void RotateAlongMovement()
    {
        if (agent.desiredVelocity.sqrMagnitude < 0.01f)
        {
            return;
        }

        RotateToward(agent.desiredVelocity);
    }

    private void RotateToward(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void UpdateAnimation()
    {
        if (animator == null)
        {
            return;
        }

        float normalizedSpeed = 0f;

        if (!agent.isStopped && agent.speed > 0f)
        {
            normalizedSpeed =
                agent.velocity.magnitude / agent.speed;
        }

        normalizedSpeed = Mathf.Clamp01(normalizedSpeed);

        bool isMoving = normalizedSpeed > 0.01f;

        bool isSprinting =
            isMoving &&
            playerControl != null &&
            playerControl.IsSprinting;

        animator.SetBool("IsWalking", isMoving);
        animator.SetBool("IsSprinting", isSprinting);
        animator.SetFloat("MoveSpeed", normalizedSpeed);
    }
}