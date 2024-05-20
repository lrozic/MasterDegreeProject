using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractBoss : MonoBehaviour
{
    [SerializeField] protected GameObject timerCountDown;
    [SerializeField] protected GameObject darkMatterEffects;
    [SerializeField] protected float velocity;
    [SerializeField] protected int health;
    [SerializeField] protected int bossNumber;

    protected List<GameObject> heroes;
    protected Rigidbody2D rigidBody2d;
    protected Animator animator;
    protected bool halfway = false;

    /// <summary>
    /// Start attacking after player enters collider
    /// </summary>
    public abstract void StartAttacking();

    /// <summary>
    /// Decrease health from a boss
    /// </summary>
    /// <param name="layer">Layer 16 = minor damage, Layer 19 = major damage</param>
    public abstract void MinusHealth(int layer);

    /// <summary>
    /// Increase the general sound of everything in the scene
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerator IncreaseMusicVolume()
    {
        while (AudioListener.volume <1)
        {
            AudioListener.volume += 0.1f;
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

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
    /// Stop the boss from attacking.
    /// Save measured time in Firebase.
    /// </summary>
    protected virtual void Death(bool saveToDb) 
    {
        if (saveToDb)
        {
            string playerName = PlayerPrefs.GetString("playerName");
            timerCountDown.GetComponent<TimeCountDown>().countTime = false;
            float countedTime = timerCountDown.GetComponent<TimeCountDown>().GetTime();
            PlayerData.PrepareForDatabase(GetType().Name, playerName, countedTime);
        }

        rigidBody2d.velocity = Vector2.zero;

        var numberOfDefeatedBosses = PlayerPrefs.GetInt("numberOfDefeatedBosses");
        if (numberOfDefeatedBosses == bossNumber)
        {
            numberOfDefeatedBosses++;
            PlayerPrefs.SetInt("numberOfDefeatedBosses", numberOfDefeatedBosses);
        }

        if (bossNumber != 2
            && bossNumber != 4)
        {
            SingletonSFX.Instance.PlaySFX("SFX63_boss_big_damage");

            Instantiate(
            darkMatterEffects,
            new Vector2(
                transform.position.x,
                transform.position.y),
            Quaternion.identity);
        }
    }
}
