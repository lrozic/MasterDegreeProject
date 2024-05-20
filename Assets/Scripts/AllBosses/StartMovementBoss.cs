using UnityEngine;

public class StartMovementBoss : MonoBehaviour
{
    [SerializeField] GameObject boss;

    /// <summary>
    /// If player's collider enters this object's collider, boss will start fighting
    /// </summary>
    /// <param name="collision">Player's collider</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag.Equals("Player"))
        {
            FindObjectOfType<AbstractBoss>().StartAttacking();
            transform.gameObject.SetActive(false);
        }
    }
}
