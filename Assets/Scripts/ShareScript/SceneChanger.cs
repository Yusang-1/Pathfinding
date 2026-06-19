using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger
{
    public static void ChangeScene(string sceneName)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(sceneName);
    }
}
