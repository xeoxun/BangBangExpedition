using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{
    [Header("플레이어 이동")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float rotationSpeed;

    private bool isSprinting;

    [Header("중력 설정")]
    [SerializeField] private float gravity = -20f;

    [Header("참조")]
    [SerializeField] private Camera playerTargetCamera;
    [SerializeField] private Transform characterVisual;

    [Header("애니메이션")]
    [SerializeField] private Animator animator;
    [SerializeField] private float animationDampTime = 0.1f;

    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int MoveYHash =  Animator.StringToHash("MoveY");
    private static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");
    private static readonly int IsSprintingHash = Animator.StringToHash("IsSprinting");
    private static readonly int IsAttackingHash = Animator.StringToHash("IsAttacking");

    [Header("무기")]
    [SerializeField] private MachineGun machineGun;

    private CharacterController characterController;
    private Vector2 moveInput;
    private Vector3 verticalVelocity;


    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {

        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);

        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);


        bool isMoving = moveDirection.sqrMagnitude > 0.001f;

        float currentSpeed = isSprinting && isMoving? sprintSpeed : moveSpeed;
        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);

        RotateTowardMouse();
        ApplyGravity();
        UpdateMovementAnimation();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnSprint(InputValue value)
    {
        isSprinting = value.isPressed;
    }

    public void OnAttack(InputValue value)
    {
        bool isAttacking = value.isPressed;
        animator.SetBool(IsAttackingHash, isAttacking);

        if (machineGun != null)
        {
            machineGun.SetTriggerHeld(isAttacking);
            Debug.Log("인식을 해버렷징");
        }
        else
        {
            Debug.Log("인식을 못행");
        }
    }

    private void UpdateMovementAnimation()
    {
        if (animator == null || characterVisual == null)
        {
            return;
        }

        // 이동 입력을 월드 이동 방향으로 변환
        Vector3 worldMoveDirection = new Vector3(moveInput.x, 0f, moveInput.y);

        bool isMoving = worldMoveDirection.sqrMagnitude > 0.001f;

        // 대각선 이동 속도 보정
        worldMoveDirection = Vector3.ClampMagnitude(worldMoveDirection, 1f);

        // 월드 이동 방향을 캐릭터가 바라보는 방향 기준으로 변환
        Vector3 localMoveDirection = characterVisual.InverseTransformDirection(worldMoveDirection);

        float moveSpeed = worldMoveDirection.magnitude;

        animator.SetFloat(MoveXHash, localMoveDirection.x, animationDampTime, Time.deltaTime);
        animator.SetFloat(MoveYHash, localMoveDirection.z, animationDampTime, Time.deltaTime);
        animator.SetFloat(MoveSpeedHash, moveSpeed, animationDampTime, Time.deltaTime);

        animator.SetBool(IsSprintingHash, isSprinting && isMoving);
    }
    private void RotateTowardMouse()
    {
        if (playerTargetCamera == null || Mouse.current == null)
        {
            return;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray mouseRay = playerTargetCamera.ScreenPointToRay(mousePosition);

        // 캐릭터 높이를 기준으로 수평 조준 평면 생성
        Plane aimPlane = new Plane(Vector3.up, transform.position);

        if (!aimPlane.Raycast(mouseRay, out float hitDistance))
        {
            return;
        }

        Vector3 targetPosition = mouseRay.GetPoint(hitDistance);
        Vector3 aimDirection = targetPosition - transform.position;

        aimDirection.y = 0f;

        if (aimDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation =  Quaternion.LookRotation(aimDirection);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -2f;
        }

        verticalVelocity.y += gravity * Time.deltaTime;

        characterController.Move(verticalVelocity * Time.deltaTime);
    }
}
