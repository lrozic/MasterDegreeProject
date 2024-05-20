using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    /// <summary>
    /// Starts coroutine for loading a new scene
    /// </summary>
    /// <param name="seconds">Seconds to pass before loading new scene</param>
    public void LoadSceneCoroutine(float seconds, string sceneName)
    {
        StartCoroutine(LoadScene(seconds, sceneName));
    }

    /// <summary>
    /// Starts coroutine for loading a new scene
    /// </summary>
    /// <param name="seconds">Seconds to pass before loading new scene</param>
    /// <returns></returns>
    private IEnumerator LoadScene(float seconds, string sceneName)
    {
        yield return new WaitForSecondsRealtime(seconds);
        SceneManager.LoadScene(sceneName);
    }
}
