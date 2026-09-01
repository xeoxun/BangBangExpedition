using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneControl : MonoBehaviour
{
    public void StartGame()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        SceneManager.LoadScene(nextSceneIndex);
    }
}
