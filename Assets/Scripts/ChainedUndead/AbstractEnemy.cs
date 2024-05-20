using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractEnemy : MonoBehaviour
{
    [SerializeField] protected List<GameObject> heroes;
    [SerializeField] protected GameObject maggotEffects;
    [SerializeField] protected int health;
    [SerializeField] protected float speedX;
    [SerializeField] protected float speedY;
    [SerializeField] protected float adjustPositionY;

    protected Rigidbody2D rigidBody2d;
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;

    protected Color initialColor;

    public bool destroyed = false;
    protected bool lookingLeft;
    protected int attackRandomPlayer;

    /// <summary>
    /// Type of flies with values which are used for increasing maximum number of specific types of flies for respawning
    /// </summary>
    public enum FlyType 
    { 
        NotFly = 0,
        Toxic = 1,
        Charging = 2,
        Ordinary = 3
    }

    /// <summary>
    /// Start attacking after the flight from background
    /// </summary>
    protected abstract void StartAttacking();

    /// <summary>
    /// Find players in the arena
    /// </summary>
    protected virtual void FindHeroes()
    {
        if (GameObject.Find("PlayerKnight1") != null)
        {
            heroes.Add(GameObject.Find("PlayerKnight1"));
        }

        if (GameObject.Find("PlayerKnight2") != null)
        {
            heroes.Add(GameObject.Find("PlayerKnight2"));
        }

        if (heroes.Count == 2)
        {
            // First case is questioning if the player one doesn't have any lives left,
            // if true then enemy will focus attack on playerNumber (2).
            // Before entering 2nd time foreach, playerNumber will be decreased by one,
            // so that the focus will be on player one if true
            int playerNumber = 2;
            foreach (GameObject hero in heroes)
            {
                if (hero.GetComponent<Paladin>().m_currentNoLives <= 0)
                {
                    try
                    {
                        ChangeFocusToAnotherPlayer(playerNumber);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
                playerNumber--;
            }
        }
    }

    /// <summary>
    /// Choose one player to always attack
    /// </summary>
    protected virtual void ChooseHeroToAttack() 
    {
        attackRandomPlayer = UnityEngine.Random.Range(0, heroes.Count);
    }

    /// <summary>
    /// Changes velocity on X axis of enemy based on player's location on X axis
    /// If player's X position is greater than enemy's, then enemy will have a positive speed on X axis and vice versa.
    /// </summary>
    /// <param name="multiplyNumber">Number to multiply velocity on X axis</param>
    protected virtual void ChangeVelocityX(float multiplyNumber = 1)
    {
        if (transform.position.x < heroes.ToArray()[attackRandomPlayer].transform.position.x)
        {
            rigidBody2d.velocity = new Vector2(speedX * multiplyNumber, rigidBody2d.velocity.y);
        }
        else
        {
            rigidBody2d.velocity = new Vector2(-speedX * multiplyNumber, rigidBody2d.velocity.y);
        }
    }

    /// <summary>
    /// Changes velocity on Y axis of enemy based on player's location on Y axis.
    /// If player's Y position is greater than enemy's, then enemy will have a positive speed on Y axis and vice versa.
    /// </summary>
    /// /// <param name="multiplyNumber">Number to multiply velocity on X axis</param>
    protected virtual void ChangeVelocityY(float multiplyNumber = 1) 
    {
        if (transform.position.y + adjustPositionY < heroes.ToArray()[attackRandomPlayer].transform.position.y)
        {
            rigidBody2d.velocity = new Vector2(rigidBody2d.velocity.x, speedY * multiplyNumber);
        }
        else
        {
            rigidBody2d.velocity = new Vector2(rigidBody2d.velocity.x, -speedY * multiplyNumber);
        }
    }

    /// <summary>
    /// Rotates enemy in order to always face at the player.
    /// Does not rotate if player is very near to the enemy on the X axis.
    /// </summary>
    protected virtual void RotateEnemy()
    {
        if ((transform.position.x - heroes.ToArray()[attackRandomPlayer].transform.position.x) > 1f 
            || (transform.position.x - heroes.ToArray()[attackRandomPlayer].transform.position.x) < -1f)
        {
            if (rigidBody2d.velocity.x > 0 
                && lookingLeft)
            {
                lookingLeft = false;
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
            else if (rigidBody2d.velocity.x < 0 
                && !lookingLeft)
            {
                lookingLeft = true;
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
        }
    }

    /// <summary>
    /// Lower the enemy's health by 2 when attacked with the sword.
    /// Call method Death() when the value of health is lower or equal to 0.
    /// </summary>
    /// <param name="collision">Collider from another gameobject</param>
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 16 
            || collision.gameObject.layer == 19)
        {
            if (collision.gameObject.layer == 19) 
            {
                health -= 4;
            }
            else 
            {
                health -= 2;
                SingletonSFX.Instance.PlaySFX("SFX15_blood_splash");
            }

            spriteRenderer.color = new Color(1f, 0f, 0f);

            Invoke(nameof(ReturnNormalColor), 0.2f);
        }

        if (health <= 0 
            && !destroyed)
        {
            destroyed = true;
            Death();
        }
    }

    /// <summary>
    /// Method for returning enemy to normal color after being hit
    /// </summary>
    private void ReturnNormalColor()
    {
        spriteRenderer.color = initialColor;
    }

    /// <summary>
    /// Get type of fly
    /// </summary>
    public abstract int GetFlyType();

    /// <summary>
    /// Change focus of enemies on player that is still alive after the other one died.
    /// </summary>
    /// <param name="playerAlive">Player that is still alive (1,2)</param>
    public virtual void ChangeFocusToAnotherPlayer(int playerAlive)
    {
        if (heroes.Count > 1)
        {
            if (playerAlive == 1)
            {
                heroes[1] = heroes[0];
            }
            else
            {
                heroes[0] = heroes[1];
            }
        }
    }

    /// <summary>
    /// Method which is overriden in order to implement death SFX, animation and statuses
    /// for certain types of enemies
    /// </summary>
    public abstract void Death();
}
