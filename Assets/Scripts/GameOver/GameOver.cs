using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    [SerializeField] Button retry;
    [SerializeField] Button quitToHub;

    private string previousScene;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(IncreaseMusicVolume());
        previousScene = PlayerPrefs.GetString("sceneName");
    }

    /// <summary>
    /// Retry the boss fight again
    /// </summary>
    public void Retry()
    {
        try
        {
            PrepareTheScene(previousScene);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    /// <summary>
    /// Quit the boss fight and return to the hubworld
    /// </summary>
    public void QuitToHub()
    {
        PrepareTheScene("HubWorld");
    }

    /// <summary>
    /// Prepare the scene by making black screen appear, lower the general audio volume and call 
    /// the method to load the scene
    /// </summary>
    private void PrepareTheScene(string scene)
    {
        GameObject.Find("BlackScreenSprite").GetComponent<Animator>().SetTrigger("Appear");

        SingletonSFX.Instance.PlaySFX("SFX6_big_thing_fly_sky");

        StartCoroutine(LowerMusicVolume());
        StartCoroutine(LoadScene(scene));
    }

    /// <summary>
    /// Load scene
    /// </summary>
    /// <returns></returns>
    private IEnumerator LoadScene(string scene)
    {
        yield return new WaitForSecondsRealtime(2f);

        SceneManager.LoadScene(scene);
    }

    /// <summary>
    /// Increase the general sound of everything in the scene
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerator IncreaseMusicVolume()
    {
        while (AudioListener.volume < 1)
        {
            AudioListener.volume += 0.1f;
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    /// <summary>
    /// Lower the general sound of everything in the scene
    /// </summary>
    /// <returns></returns>
    private IEnumerator LowerMusicVolume()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        while (AudioListener.volume > 0)
        {
            AudioListener.volume -= 0.1f;
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }
}
