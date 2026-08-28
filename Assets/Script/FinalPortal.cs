using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class FinalPortal : MonoBehaviour
{
    [Header("게임 클리어 UI")]
    [SerializeField] private GameObject gameClearPanel;
    [SerializeField] private Button retryButton;

    [Header("다시 시작할 씬")]
    [SerializeField] private int retrySceneIndex = 0;

    [Header("클리어 설정")]
    [SerializeField] private bool pauseOnClear = true;

    private bool isCleared;

    private void Awake()
    {
        isCleared = false;

        if (gameClearPanel != null)
        {
            gameClearPanel.SetActive(false);
        }

        if (retryButton != null)
        {
            retryButton.onClick.AddListener(
                RetryGame
            );
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCleared)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        ClearGame();
    }

    private void ClearGame()
    {
        isCleared = true;

        if (gameClearPanel != null)
        {
            gameClearPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "Game Clear Panel이 연결되지 않았습니다.",
                this
            );
        }

        if (pauseOnClear)
        {
            Time.timeScale = 0f;
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void RetryGame()
    {
        // 정지 상태 해제
        Time.timeScale = 1f;

        // Index 0 게임 씬을 처음부터 다시 로드
        SceneManager.LoadScene(
            retrySceneIndex,
            LoadSceneMode.Single
        );
    }

    private void OnDestroy()
    {
        if (retryButton != null)
        {
            retryButton.onClick.RemoveListener(
                RetryGame
            );
        }

        // 씬 전환 중 TimeScale이 0으로 남는 것 방지
        Time.timeScale = 1f;
    }
}