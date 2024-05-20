using System;
using UnityEngine;

public class BossHitBoxDamaged : MonoBehaviour
{
    // This can be used for other fights as well, not just for Glacial Overlord
    int playerOneAttacked = 0;
    int playerTwoAttacked = 0;

    /// <summary>
    /// Call method for decreasing bosses' health after getting in contact with player's sword or projectile
    /// </summary>
    /// <param name="collision">Collider from another gameobject, usually it's sword layered as 16 of projectile layered as 19</param>
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 16)
        {
            AddPlayerAggressiveness(collision.transform.parent.name);

            transform.parent.gameObject.GetComponentInParent<AbstractBoss>().MinusHealth(collision.gameObject.layer);
        }

        if (collision.gameObject.layer == 19)
        {
            collision.GetComponent<BoxCollider2D>().enabled = false;
            transform.parent.gameObject.GetComponentInParent<AbstractBoss>().MinusHealth(collision.gameObject.layer);
        }
    }

    /// <summary>
    /// Increase number of certain player's attack at this enemy
    /// </summary>
    /// <param name="playerName">Player's name</param>
    private void AddPlayerAggressiveness(string playerName)
    {
        if (playerName.Equals("PlayerKnight1"))
        {
            playerOneAttacked++;
        }
        else if (playerName.Equals("PlayerKnight2"))
        {
            playerTwoAttacked++;
        }
    }

    /// <summary>
    /// Get total number of attacks from each player against this enemy
    /// </summary>
    /// <returns>Item1 = attacks from player 1, Item2 = attacks from player two</returns>
    public Tuple<int, int> GetTotalAttackNumbers()
    {
        Tuple<int, int> playerAttacks = new(playerOneAttacked, playerTwoAttacked);
        return playerAttacks;
    }
}
