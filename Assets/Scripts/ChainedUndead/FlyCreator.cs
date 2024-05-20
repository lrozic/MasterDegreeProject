using UnityEngine;

public class FlyCreator : MonoBehaviour
{
    /// <summary>
    /// Instantiate flies at the certain position
    /// </summary>
    /// <param name="flyPrefab">Fly gameobject that needs to be cloned</param>
    /// <param name="respawnLocation">Respawn location where specific types of flies are cloned</param>
    /// <returns>Fly Gameobject</returns>
    public GameObject CreateFlies(GameObject flyPrefab, Vector3 respawnLocation)
    {
        GameObject clonedFlyPrefab = Instantiate(
            flyPrefab, 
            respawnLocation, 
            Quaternion.identity);

        return clonedFlyPrefab;
    }
 }
