using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PortalTeleport : MonoBehaviour
{
    [Header("반대편 스폰 지점")]
    [SerializeField] private Transform targetSpawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (targetSpawnPoint == null)
        {
            Debug.LogWarning($"{name}에 Target Spawn Point가 연결되지 않았습니다.", this);

            return;
        }

        CharacterController characterController = other.GetComponentInParent<CharacterController>();

        Transform playerTransform;

        if (characterController != null)
        {
            playerTransform = characterController.transform;

            characterController.enabled = false;
        }
        else
        {
            playerTransform = other.transform.root;
        }

        playerTransform.SetPositionAndRotation(targetSpawnPoint.position, targetSpawnPoint.rotation);

        if (characterController != null)
        {
            characterController.enabled = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (targetSpawnPoint == null)
        {
            return;
        }

        Gizmos.color = Color.cyan;

        Gizmos.DrawLine(transform.position, targetSpawnPoint.position);

        Gizmos.DrawWireSphere(targetSpawnPoint.position, 0.5f);
    }
}