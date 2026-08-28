using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MinimapExpandController : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private RectTransform mapRoot;
    [SerializeField] private RectTransform canvasRect;

    [Header("확대 설정")]
    [Range(0.3f, 0.95f)]
    [SerializeField] private float expandedScreenRatio = 0.8f;

    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private bool pauseWhenExpanded = true;

    public bool IsExpanded { get; private set; }

    private Vector3 originalPosition;
    private Vector3 originalScale;
    private int originalSiblingIndex;

    private Coroutine animationCoroutine;

    private float previousTimeScale = 1f;

    private void Start()
    {
        if (mapRoot == null || canvasRect == null)
        {
            Debug.LogError("Map Root 또는 Canvas Rect가 연결되지 않았습니다.");

            enabled = false;
            return;
        }

        Canvas.ForceUpdateCanvases();

        originalPosition = mapRoot.position;
        originalScale = mapRoot.localScale;
        originalSiblingIndex = mapRoot.GetSiblingIndex();
    }

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.mKey.wasPressedThisFrame)
        {
            ToggleMap();
        }
    }

    public void ToggleMap()
    {
        IsExpanded = !IsExpanded;

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        if (IsExpanded)
        {
            // 다른 UI보다 앞에 표시
            mapRoot.SetAsLastSibling();
            PauseGame();
        }

        animationCoroutine = StartCoroutine(AnimateMap(IsExpanded));
    }

    private IEnumerator AnimateMap(bool expand)
    {
        Vector3 startPosition = mapRoot.position;

        Vector3 startScale = mapRoot.localScale;

        Vector3 targetPosition;
        Vector3 targetScale;

        if (expand)
        {
            targetPosition = canvasRect.TransformPoint(canvasRect.rect.center);

            float canvasSize = Mathf.Min(canvasRect.rect.width, canvasRect.rect.height);

            float targetMapSize = canvasSize * expandedScreenRatio;

            float currentMapSize = Mathf.Max(mapRoot.rect.width, mapRoot.rect.height);

            float scaleMultiplier = targetMapSize / currentMapSize;

            targetScale = new Vector3(
                originalScale.x * scaleMultiplier,
                originalScale.y * scaleMultiplier,
                originalScale.z
            );
        }
        else
        {
            targetPosition = originalPosition;
            targetScale = originalScale;
        }

        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float normalizedTime =
                Mathf.Clamp01(
                    elapsedTime /
                    Mathf.Max(
                        animationDuration,
                        0.01f
                    )
                );

            // 부드러운 가속과 감속
            float smoothTime =
                normalizedTime *
                normalizedTime *
                (3f - 2f * normalizedTime);

            mapRoot.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                smoothTime
            );

            mapRoot.localScale = Vector3.Lerp(
                startScale,
                targetScale,
                smoothTime
            );

            yield return null;
        }

        mapRoot.position = targetPosition;
        mapRoot.localScale = targetScale;

        if (!expand && !IsExpanded)
        {
            mapRoot.SetSiblingIndex(
                originalSiblingIndex
            );

            ResumeGame();
        }

        animationCoroutine = null;
    }

    private void PauseGame()
    {
        if (!pauseWhenExpanded)
        {
            return;
        }

        previousTimeScale = Time.timeScale;
    }

    private void ResumeGame()
    {
        Time.timeScale = previousTimeScale;
    }

    private void OnDisable()
    {
        ResumeGame();
    }
}