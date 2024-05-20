using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatformsGenerator : MonoBehaviour
{
    [SerializeField] List<Transform> spawnPositions;
    [SerializeField] List<GameObject> clouds;
    [SerializeField] List<float> cloudsSpeedX;
    [SerializeField] List<float> timers;
    [SerializeField] List<float> cloudTimerToSpawn;
    [SerializeField] List<float> friction;

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < timers.Count; i++)
        {
            timers[i] += Time.deltaTime;

            if (timers[i] > cloudTimerToSpawn[i])
            {
                SpawnCloud(i, cloudsSpeedX[i], friction[i]);
                timers[i] = 0f;
            }
        }
    }

    /// <summary>
    /// Method for instantiating moving cloud platforms
    /// </summary>
    /// <param name="spawnPositionNumber">In which spawn position will cloud spawn</param>
    /// <param name="cloudSpeedX">Speed of the spawned cloud platform</param>   
    /// <param name="friction">Speed of the spawned cloud platform</param> 
    private void SpawnCloud(int spawnPositionNumber, float cloudSpeedX, float friction)
    {
        int randomCloud = Random.Range(0, clouds.Count);

        var clonedCloud = Instantiate(clouds[randomCloud],
                new Vector3(
                    spawnPositions[spawnPositionNumber].position.x,
                    spawnPositions[spawnPositionNumber].position.y,
                    spawnPositions[spawnPositionNumber].position.z),
                Quaternion.identity);

        clonedCloud.GetComponent<MovingPlatform>().SpeedX = cloudSpeedX;
        clonedCloud.GetComponent<MovingPlatform>().MoveCloud(friction);
        Destroy(clonedCloud, 20f);
    }
}
