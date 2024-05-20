using UnityEngine;

public class BuddyHopping : MonoBehaviour
{
    /// <summary>
    /// Collider collision for buddy hopping when on another player character.
    /// </summary>
    /// <param name="collision">Collider from enemy gameobject</param>
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("PlayerCharacterHopping"))
        {
            transform.parent.gameObject.GetComponent<Paladin>().BuddyHopping();
        }
    }
}
