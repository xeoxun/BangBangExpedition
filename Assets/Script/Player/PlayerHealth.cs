using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("체력 관리")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private float maxHealth;
    private float currentHealth;
    private bool isDead;

    [Header("무적 시간")]
    [SerializeField] private float invincibleDuration = 1f;

    [Header("넉백")]
    [SerializeField] private float knockbackForce = 3f;
    [SerializeField] private float knockbackDuration = 0.2f;

    private bool isInvincible;

    private Animator animator;
    private CharacterController characterController;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        currentHealth = maxHealth;

        healthBar.value = currentHealth;
    }

    public void TakeDamage(float amount, Vector3 sourcePosition)
    {
        if (isInvincible || isDead)
            return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        healthBar.value = currentHealth;

        if (currentHealth <= 0f)
        {
            isDead = true;
            animator.SetTrigger("IsDead");

            Destroy(gameObject, 1f);
            return;
        }
        else
        {
            animator.SetTrigger("Hit");
            Debug.Log("닿아서 아픔");

            StartCoroutine(InvincibilityCoroutine());
            StartCoroutine(KnockbackCoroutine(sourcePosition));
        }
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibleDuration);
        isInvincible = false;
    }

    private IEnumerator KnockbackCoroutine(Vector3 sourcePosition)
    {
        Vector3 direction = transform.position - sourcePosition;
        direction.y = 0f;
        direction.Normalize();

        float elapsed = 0f;

        while (elapsed < knockbackDuration)
        {
            characterController.Move(direction * knockbackForce * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}