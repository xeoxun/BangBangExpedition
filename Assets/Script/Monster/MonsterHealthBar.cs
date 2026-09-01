using UnityEngine;
using UnityEngine.UI;

public class MonsterHealthBar : MonoBehaviour
{
    [Header("체력바")]
    [SerializeField] private Slider healthSlider;

    [Header("설정")]
    [SerializeField] private bool hideWhenFull;
    [SerializeField] private bool hideWhenDead = true;

    private Camera mainCamera;
    private Canvas healthCanvas;

    private void Awake()
    {
        mainCamera = Camera.main;
        healthCanvas = GetComponent<Canvas>();
    }

    private void LateUpdate()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;

            if (mainCamera == null)
            {
                return;
            }
        }

        // 항상 카메라 방향을 바라보게 함
        transform.rotation =
            mainCamera.transform.rotation;
    }

    public void Initialize(
        float maxHealth,
        float currentHealth)
    {
        if (healthSlider == null)
        {
            return;
        }

        healthSlider.minValue = 0f;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        UpdateVisibility(currentHealth, maxHealth);
    }

    public void SetHealth(
        float currentHealth,
        float maxHealth)
    {
        if (healthSlider == null)
        {
            return;
        }

        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        UpdateVisibility(currentHealth, maxHealth);
    }

    private void UpdateVisibility(
        float currentHealth,
        float maxHealth)
    {
        if (healthCanvas == null)
        {
            return;
        }

        if (hideWhenDead && currentHealth <= 0f)
        {
            healthCanvas.enabled = false;
            return;
        }

        if (hideWhenFull &&
            currentHealth >= maxHealth)
        {
            healthCanvas.enabled = false;
            return;
        }

        healthCanvas.enabled = true;
    }

    public void Hide()
    {
        if (healthCanvas != null)
        {
            healthCanvas.enabled = false;
        }
    }
}