using UnityEngine;

public class CameraTargetFollower : MonoBehaviour
{
    [Header("추적 대상")]
    [SerializeField] private Transform player;

    [Header("타깃 위치 보정")]
    [SerializeField]
    private Vector3 targetOffset = Vector3.zero;

    private void LateUpdate()
    {
        if (player == null)
        {
            return;
        }

        // 회전은 따라가지 않고 위치만 추적
        transform.position = player.position + targetOffset;

        // 카메라 타깃 자체의 회전을 항상 고정합니다.
        transform.rotation = Quaternion.identity;
    }
}