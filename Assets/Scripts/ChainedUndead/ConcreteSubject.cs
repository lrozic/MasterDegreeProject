using System.Collections.Generic;
using UnityEngine;

public class ConcreteSubject : MonoBehaviour, ISubject
{
    [SerializeField] List<Transform> flyRespawnLocationsList;
    [SerializeField] List<GameObject> flyTypesList;
    [SerializeField] List<int> bodyPartsNumberList;

    [SerializeField] int bodyPartsToLowerRespawnTime;
    [SerializeField] float numberForDecreasingRespawnTime;

    private int destroyedBodyParts;
    private float timer = 0f;
    private float timeForRespawn = 8f;

    readonly List<ObserverRespawners> observerRespawnersList = new();

    // Start is called before the first frame update
    void Start()
    {
        destroyedBodyParts = 0;
        PrepareObservers();
    }

    // Update is called once per frame
    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > timeForRespawn)
        {
            Spawn();
            timer = 0;
        }
    }

    /// <summary>
    /// Prepare observers as objects of type ConcreteObserverRespawner before adding them to the list
    /// </summary>
    public void PrepareObservers()
    {
        for(int i = 0; i < flyRespawnLocationsList.Count; i++)
        {
            ObserverRespawners concreteObserverRespawner = new ConcreteObserverRespawner(
                flyRespawnLocationsList.ToArray()[i].position,
                flyTypesList.ToArray()[i],
                bodyPartsNumberList.ToArray()[i]
                );

            AddObserver(concreteObserverRespawner);
        }
    }

    /// <summary>
    /// Add observer to the list which is interested in being notified about body parts being destroyed
    /// </summary>
    public void AddObserver(ObserverRespawners observerRespawner)
    {
        observerRespawnersList.Add(observerRespawner);
    }

    /// <summary>
    /// Remove observer from the list
    /// </summary>
    /// <param name="observerRespawners">Observer</param>
    public void RemoveObserver(ObserverRespawners observerRespawner)
    {
        observerRespawnersList.Remove(observerRespawner);
    }

    /// <summary>
    /// Notify observers that the total number of body parts being destroyed is increased
    /// </summary>
    public void Notify()
    {
        for (int i = 0; i < observerRespawnersList.Count; i++)
        {
            observerRespawnersList[i].Update(this);
        }

        if (destroyedBodyParts == bodyPartsToLowerRespawnTime)
        {
            timeForRespawn -= numberForDecreasingRespawnTime;
        }
    }

    /// <summary>
    /// Get current number of DestroyedBodyParts
    /// </summary>
    /// <returns></returns>
    public int GetState()
    {
        return destroyedBodyParts;
    }

    /// <summary>
    /// Add one body part to sum of destroyed body parts
    /// and call method to notify observers
    /// </summary>
    public void AddBodyPartsDestroyed()
    {
        destroyedBodyParts++;
        Notify();
    }

    /// <summary>
    /// Increase respawn time of flies
    /// </summary>
    public void IncreaseRespawnTime()
    {
        timeForRespawn += 99f;
    }

    /// <summary>
    /// Decrease respawn time of flies
    /// </summary>
    public void DecreaseRespawnTime()
    {
        timeForRespawn -= 1.75f;
    }

    /// <summary>
    /// Notify that the observers needs to spawn flies
    /// </summary>
    public void Spawn()
    {
        for (int i = 0; i < observerRespawnersList.Count; i++)
        {
            observerRespawnersList[i].SpawnFlies();
        }
    }
}
