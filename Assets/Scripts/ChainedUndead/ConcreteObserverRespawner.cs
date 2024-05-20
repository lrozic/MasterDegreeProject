using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConcreteObserverRespawner : ObserverRespawners
{
    private readonly GameObject flyPrefab;
    private List<GameObject> flyObjectsList = new();
    private Vector3 respawnLocation;

    private int bodyPartsNumberForUpdate;
    private int maxNumberOfFlies = 0;

    /// <summary>
    /// Constructor for Respawner
    /// </summary>
    /// <param name="respawnLocation">Location where flies will spawn</param>
    /// <param name="flyPrefab">GameObject fly</param>
    /// <param name="bodyPartsNumberForUpdate">Number of body parts needed to be destroyed in order to update spawn settings</param>
    public ConcreteObserverRespawner(Vector3 respawnLocation, GameObject flyPrefab, int bodyPartsNumberForUpdate)
    {
        this.respawnLocation = respawnLocation;
        this.flyPrefab = flyPrefab;
        this.bodyPartsNumberForUpdate = bodyPartsNumberForUpdate;
    }

    /// <summary>
    /// Method which checks if Respawner as an Observer is interested in the message, 
    /// in other words if adequate number of body parts are destroyed to begin spawning
    /// flies or increase the spawn number.
    /// Object bodyPartsNumberForUpdate is updated, which means that the number of destroyed
    /// body parts needed for the next update for spawn settings is increased.
    /// </summary>
    /// <param name="concreteSubject">Subject which notified observer and contains information about destroyed body parts</param>
    public override void Update(ISubject concreteSubject)
    {
        int currentDestroyedBodyParts = concreteSubject.GetState();
        if (bodyPartsNumberForUpdate == currentDestroyedBodyParts)
        {
            UpdateSpawnSettings();
            bodyPartsNumberForUpdate += bodyPartsNumberForUpdate;
        }
    }

    /// <summary>
    /// Method which updates maximum number of certain type of flies in the arena
    /// </summary>
    private void UpdateSpawnSettings()
    {
        maxNumberOfFlies += flyPrefab.GetComponent<AbstractEnemy>().GetFlyType();
    }

    /// <summary>
    /// Spawn flies on certain location
    /// </summary>
    public override void SpawnFlies()
    {
        flyObjectsList = flyObjectsList.Where(x => x != null).ToList();

        if (flyObjectsList.Count < maxNumberOfFlies)
        {
            flyObjectsList.Add(GameObject
                .Find("FlyCreatorObject")
                    .GetComponent<FlyCreator>()
                    .CreateFlies(flyPrefab, respawnLocation));
        }
    }
}
