using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubWorld : MonoBehaviour
{
    [SerializeField] List<GameObject> listDoors;
    [SerializeField] List<GameObject> listClouds;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(IncreaseMusicVolume());

        var numberOfDefeatedBosses = PlayerPrefs.GetInt("numberOfDefeatedBosses");

        // Delete this after FOI event is done
        numberOfDefeatedBosses = 4;

        var numberOfOpenedDoors = numberOfDefeatedBosses;

        if (numberOfDefeatedBosses >= 5)
        {
            numberOfDefeatedBosses = 4;
        }

        for (int i = 0; i <= numberOfOpenedDoors; i++)
        {
            listDoors.ToArray()[i].GetComponent<SpriteRenderer>().color = Color.white;
            listDoors.ToArray()[i].transform.tag = "Door";
        }

        if (numberOfDefeatedBosses >= 4)
        {
            foreach(var cloud in listClouds)
            {
                cloud.SetActive(true);
            }
        }
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
}
