using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPositions : MonoBehaviour
{
    [SerializeField] List<Transform> spawnPositionsList;
    [SerializeField] List<GameObject> heroes;

    private int currentPosition;

    // Start is called before the first frame update
    void Start()
    {
        currentPosition = 0;
    }

    /// <summary>
    /// If abyss boundary collider touches spawn position, then
    /// the next spawn position will be set to the player
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Respawn"))
        {
            currentPosition++;
            foreach(var hero in heroes)
            {
                try
                {
                    hero.GetComponent<Paladin>().m_positionAfterAbyss = spawnPositionsList[currentPosition];
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }
    }
}
