using UnityEngine;

public class Dummy : MonoBehaviour
{
    bool invincible = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    /// <summary>
    /// Add damage to dummy
    /// </summary>
    /// <param name="collision">Collider from another gameobject</param>
    private void OnTriggerStay2D(Collider2D collision)
    {
        //Debug.Log("Does it detect it?");
        if (!invincible 
            && collision.gameObject.layer == 16)
        {
            Debug.Log("It enters at enemy");
            invincible = true;
            transform.parent.gameObject.GetComponentInParent<SpriteRenderer>().color = new Color(0.23f, 0.82f, 0.55f);
            Invoke(nameof(RemoveInvincibility), 0.3f);
        }
    }

    /// <summary>
    /// Change back invincibility to false and return the original Color
    /// </summary>
    private void RemoveInvincibility(){
        invincible = false;
        transform.parent.gameObject.GetComponentInParent<SpriteRenderer>().color = new Color(1f, 1f, 1f);
    }
}
/*
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
        while (AudioListener.volume < 1)
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
using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class BodyPart : MonoBehaviour
{
    [SerializeField] GameObject fireDestroyedBodyPart;
    [SerializeField] GameObject positionBodyPart;
    [SerializeField] GameObject maggots;
    [SerializeField] GameObject heartBlood;
    [SerializeField] protected int bodyPartHealth;
    [SerializeField] bool invincible;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rigidBody2d;
    private ChainedUndead chainedUndead;

    bool destroyed;

    // Start is called before the first frame update
    private void Start()
    {
        rigidBody2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidBody2d.gravityScale = 0;
        destroyed = false;

        if (GameObject.Find("PlayerKnight2") != null)
        {
            bodyPartHealth += (int)Math.Round(bodyPartHealth * (1d / 3d), 0);
        }

        chainedUndead = GameObject.Find("ThirdBossChainedUndead").GetComponent<ChainedUndead>();
        chainedUndead.AddBodyPartToDictionary(name);
    }

    // Update is called once per frame
    private void Update()
    {
        if (destroyed)
        {
            rigidBody2d.gravityScale = 4;
        }
    }

    /// <summary>
    /// Change the invincibility status to false.
    /// Used since it doesn't make sense to destroy upper part of leg while lower is still attached to it
    /// </summary>
    public void ChangeInvincibilityStatus()
    {
        invincible = false;
    }

    /// <summary>
    /// Call method for decreasing bosses' health after getting in contact with player's sword or projectile
    /// </summary>
    /// <param name="collision">Collider from another gameobject</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!invincible)
        {
            if (collision.gameObject.layer == 16)
            {
                bodyPartHealth -= 2;
                SingletonSFX.Instance.PlaySFX("SFX15_blood_splash");

                if (name.Equals("Heart"))
                {
                    heartBlood.GetComponent<ParticleSystem>().Play();
                }

                spriteRenderer.color = new Color(1f, 0.3915094f, 0.3915094f);

                Invoke(nameof(ReturnNormalColor), 0.2f);
            }

            if (bodyPartHealth <= 0
                && !destroyed)
            {
                if (transform.childCount == 2)
                {
                    GameObject chain = transform.GetChild(1).gameObject;
                    SingletonSFX.Instance.PlaySFX("SFX44_steel_chain_rattle");

                    float chainVelocityX = chain.transform.rotation.eulerAngles.z / 10;
                    if (chain.transform.rotation.eulerAngles.z > 90)
                    {
                        chainVelocityX *= -1;
                    }

                    chain.GetComponent<Rigidbody2D>().velocity = new Vector2(chainVelocityX, 35f);
                }

                chainedUndead.ChangeBodyPartStatusInDictionary(name);
                ButcheredDestroyed();
            }
        }
    }

    /// <summary>
    /// Return to original color after certain time when taking damage
    /// </summary>
    private void ReturnNormalColor()
    {
        spriteRenderer.color = new Color(1f, 1f, 1f);
    }

    /// <summary>
    /// Method which sets gravity scale in order for body part to fall.
    /// Also calls method for spawning fire around destroyed body
    /// </summary>
    private void ButcheredDestroyed()
    {
        destroyed = false;
        transform.GetComponent<PolygonCollider2D>().enabled = false;

        spriteRenderer.sortingOrder = 2;
        rigidBody2d.gravityScale = 4;

        if (maggots != null)
        {
            InstantiateMaggots();
        }

        GameObject.Find("RespawnController").GetComponent<ConcreteSubject>().AddBodyPartsDestroyed();

        StartCoroutine(PlayDeathFireAnimation());
        Destroy(this, 3f);
    }
    /// <summary>
    /// Method for spawning maggots
    /// </summary>
    private void InstantiateMaggots()
    {
        int randomMaggetNumber = Random.Range(3, 6);
        for (int i = 0; i < randomMaggetNumber; i++)
        {
            int randomPlacementX = Random.Range(-2, 3);
            int randomPlacementY = Random.Range(-3, 4);

            GameObject maggot = Instantiate(
               maggots,
               new Vector2(
                   positionBodyPart.transform.position.x + randomPlacementX,
                   positionBodyPart.transform.position.y + randomPlacementY),
               Quaternion.identity);

            maggot.transform.rotation = Quaternion.Euler(
                0f,
                0f,
                -90f);
        }
    }

    /// <summary>
    /// Instantiates fire around destroyed body part and plays certain SFX
    /// </summary>
    /// <returns></returns>
    private IEnumerator PlayDeathFireAnimation()
    {
        if (!name.Equals("Heart"))
        {
            int numberOfFireDestroyed = Random.Range(8, 13);

            for (int i = 0; i < numberOfFireDestroyed; i++)
            {
                int randomPlacementX = Random.Range(-2, 3);
                int randomPlacementY = Random.Range(-3, 4);

                var clonedFireDestroyedBodyPart = Instantiate(
                    fireDestroyedBodyPart, new Vector3(
                        positionBodyPart.transform.position.x + randomPlacementX,
                        positionBodyPart.transform.position.y + randomPlacementY,
                        positionBodyPart.transform.position.z),
                    Quaternion.identity);

                Destroy(clonedFireDestroyedBodyPart, 0.4f);

                SingletonSFX.Instance.PlaySFX("SFX37_body_part_destroyed");
                yield return new WaitForSecondsRealtime(0.2f);
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChainedUndead : AbstractBoss
{
    [SerializeField] List<GameObject> remainingChainsList;
    [SerializeField] GameObject floatingIslands;
    [SerializeField] GameObject skeletonLeft;
    [SerializeField] GameObject skeletonRight;
    [SerializeField] GameObject fireDestroyedBodyPart;
    [SerializeField] GameObject deathFirePosition;
    [SerializeField] GameObject sceneManager;
    [SerializeField] GameObject head;
    [SerializeField] GameObject torso;
    [SerializeField] Transform darkMatterSpawnPosition;

    private readonly Dictionary<string, bool> bodyPartsDictionary = new();

    // Start is called before the first frame update
    private void Start()
    {
        PlayerPrefs.SetString("sceneName", SceneManager.GetActiveScene().name);
        StartCoroutine(IncreaseMusicVolume());

        rigidBody2d = GetComponent<Rigidbody2D>();
        heroes = new List<GameObject>();
    }

    /// <summary>
    /// Does nothing
    /// </summary>
    public override void MinusHealth(int layer)
    {
    }

    /// <summary>
    /// Adding name of the body part to the list.
    /// False means that the body part is not destroyed.
    /// </summary>
    /// <param name="bodyPart">Name of body part</param>
    public void AddBodyPartToDictionary(string bodyPart)
    {
        bodyPartsDictionary.Add(bodyPart, false);
    }

    /// <summary>
    /// Changes status of body part to true in dictionary, which means it is destroyed
    /// </summary>
    /// <param name="bodyPart">Name of the body part</param>
    public void ChangeBodyPartStatusInDictionary(string bodyPart)
    {
        bodyPartsDictionary[bodyPart] = true;
        CheckIfInvincibilityStatusNeedsChange(bodyPart);
    }

    /// <summary>
    /// Checks which body part needs to change from invincible true to false.
    /// Depends on given param.
    /// </summary>
    /// <param name="bodyPart">Name of the body part</param>
    private void CheckIfInvincibilityStatusNeedsChange(string bodyPart)
    {
        switch (bodyPart)
        {
            case "LowerRightLeg":
                ChangeBodyPartInvincibility("UpperRightLeg");
                break;
            case "LowerLeftLeg":
                ChangeBodyPartInvincibility("UpperLeftLeg");
                break;
            case "LowerLeftArm":
                ChangeBodyPartInvincibility("UpperLeftArm");
                break;
            case "LowerRightArm":
                ChangeBodyPartInvincibility("UpperRightArm");
                break;
            case "Stomach":
                ChangeBodyPartInvincibility("Heart");
                break;
            case "Heart":
                Death(true);
                break;
        }

        // If every other body part is destroyed except heart, change the invincibility
        // of the stomach to true
        if (bodyPartsDictionary.Where(x => x.Value == true).Count() == 8)
        {
            ChangeBodyPartInvincibility("Stomach");
        }

        // Spawn floating islands after the legs are destroyed
        if (bodyPartsDictionary.Where(x => x.Value == true).Count() == 2)
        {
            SingletonSFX.Instance.PlaySFX("SFX26_teleport");
            floatingIslands.SetActive(true);
        }
    }

    /// <summary>
    /// Change the body part invincibility from true to false
    /// </summary>
    /// <param name="bodyPartToChange">Body part that no longer needs to be invincible</param>
    private void ChangeBodyPartInvincibility(string bodyPartToChange)
    {
        GameObject.Find(bodyPartToChange).GetComponent<BodyPart>().ChangeInvincibilityStatus();
    }

    /// <summary>
    /// Called when player comes near Chained Undead. 
    /// Summon skeletons.
    /// Overrides method from AbstractBoss class.
    /// </summary>
    public override void StartAttacking()
    {
        timerCountDown.GetComponent<TimeCountDown>().countTime = true;
        SingletonSFX.Instance.PlaySFX("SFX45_chained_undead_start_roar");

        if (GameObject.Find("PlayerKnight1") != null)
        {
            heroes.Add(GameObject.Find("PlayerKnight1"));
        }

        if (GameObject.Find("PlayerKnight2") != null)
        {
            heroes.Add(GameObject.Find("PlayerKnight2"));
            GameObject.Find("RespawnController").GetComponent<ConcreteSubject>().DecreaseRespawnTime();
        }

        skeletonLeft.SetActive(true);
        skeletonRight.SetActive(true);
    }

    /// <summary>
    /// Calls a virtual method from the abstract class and then uses specialized code for death.
    /// </summary>
    /// <param name="saveToDb">True if time and player name should be saved in database, false if not</param>
    protected override void Death(bool saveToDb)
    {
        base.Death(saveToDb);

        head.GetComponent<Animator>().SetTrigger("Defeated");
        head.GetComponent<PolygonCollider2D>().enabled = false;
        torso.GetComponent<PolygonCollider2D>().enabled = false;
        GameObject.Find("RespawnController").GetComponent<ConcreteSubject>().IncreaseRespawnTime();

        foreach (var enemy in FindObjectsOfType<AbstractEnemy>())
        {
            try
            {
                enemy.destroyed = true;
                enemy.Death();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        Destroy(skeletonLeft, 2f);
        Destroy(skeletonRight, 2f);

        SingletonSFX.Instance.PlaySFX("SFX46_chained_undead_death_roar");

        SingletonSFX.Instance.PlaySFX("SFX63_boss_big_damage");
        Instantiate(darkMatterEffects, darkMatterSpawnPosition);

        StartCoroutine(PlayDeathFireAnimation());
        StartCoroutine(RemoveRemainingChains());

        sceneManager.GetComponent<SceneLoader>().LoadSceneCoroutine(12f, "HubWorld");

        Destroy(gameObject, 10f);
    }

    /// <summary>
    /// Instantiates fire around destroyed Chained Undead's head and torso.
    /// It also plays certain SFX.
    /// </summary>
    /// <returns></returns>
    private IEnumerator PlayDeathFireAnimation()
    {
        int numberOfFireDestroyed = 50;
        yield return new WaitForSecondsRealtime(4f);

        for (int i = 0; i < numberOfFireDestroyed; i++)
        {
            int randomPlacementX = (int)UnityEngine.Random.Range(-10, 12);
            int randomPlacementY = (int)UnityEngine.Random.Range(-10, 20);

            var clonedFireDestroyedBodyPart = Instantiate(fireDestroyedBodyPart,
                new Vector3(
                    deathFirePosition.transform.position.x + randomPlacementX,
                    deathFirePosition.transform.position.y + randomPlacementY,
                    deathFirePosition.transform.position.z),
                    Quaternion.identity);

            Destroy(clonedFireDestroyedBodyPart, 0.4f);

            SingletonSFX.Instance.PlaySFX("SFX37_body_part_destroyed");
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    /// <summary>
    /// Removes chains that are left attached to head and torso
    /// </summary>
    /// <returns></returns>
    private IEnumerator RemoveRemainingChains()
    {
        yield return new WaitForSecondsRealtime(6.5f);
        SingletonSFX.Instance.PlaySFX("SFX44_steel_chain_rattle");

        foreach (var chain in remainingChainsList)
        {
            float chainVelocityX = chain.transform.rotation.eulerAngles.z / 10;
            if (chain.transform.rotation.eulerAngles.z > 90)
            {
                chainVelocityX *= -1;
            }
            chain.GetComponent<Rigidbody2D>().velocity = new Vector2(chainVelocityX, 25f);
        }
        GetComponent<Rigidbody2D>().gravityScale = 3f;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargingFly : AbstractEnemy
{
    public static FlyType flyType = FlyType.Charging;

    [SerializeField] float chargeDistance;

    Vector3 lastPlayerPosition;

    private float speed = 0;
    private bool isAttacking;
    private bool isCharging;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rigidBody2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        heroes = new List<GameObject>();
        lookingLeft = true;
        isAttacking = true;
        isCharging = false;
        lastPlayerPosition = new Vector3(0f, 0f, 0f);

        initialColor = spriteRenderer.color;

        Invoke(nameof(StartAttacking), 1.1f);

        transform.parent = GameObject.Find("ChargingFlies").transform;

        FindHeroes();
        ChooseHeroToAttack();
    }

    /// <summary>
    /// Overriden method to start attacking after the flight from background
    /// </summary>
    protected override void StartAttacking()
    {
        isAttacking = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!destroyed
            && heroes.ToArray().Length != 0)
        {

            if (!isAttacking
                && !isCharging
                && Vector2.Distance(
                    transform.position, heroes.ToArray()[attackRandomPlayer].transform.position) < chargeDistance)
            {
                StartCoroutine(ChargeAttack());
            }

            if (!isAttacking)
            {
                ChangeVelocityX();
                ChangeVelocityY();
                RotateEnemy();
            }

            if (isCharging
                && isAttacking)
            {
                speed += 0.85f * Time.deltaTime;
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    lastPlayerPosition,
                    speed);
            }
        }
    }

    /// <summary>
    /// Method called when charging fly is near player. 
    /// Calls methods for changing velocity but multiplied by higher numbers.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ChargeAttack()
    {
        isAttacking = true;
        RotateEnemy();

        rigidBody2d.velocity = Vector2.zero;
        lastPlayerPosition = heroes.ToArray()[0].transform.position;
        lastPlayerPosition.y -= adjustPositionY;
        yield return new WaitForSecondsRealtime(0.75f);

        SingletonSFX.Instance.PlaySFX("SFX41_fly_charged");
        isCharging = true;
        yield return new WaitForSecondsRealtime(1.2f);

        isAttacking = false;
        speed = 0;
        yield return new WaitForSecondsRealtime(3f);

        isCharging = false;
    }

    /// <summary>
    /// Overriden method for returning a type of fly
    /// </summary>
    /// <returns>Enum value of type of fly</returns>
    public override int GetFlyType()
    {
        return (int)flyType;
    }

    /// <summary>
    /// Overriden method for fly
    /// </summary>
    public override void Death()
    {
        animator.SetBool("dead", true);
        SingletonSFX.Instance.PlaySFX("SFX39_fly_die");

        GameObject maggots = Instantiate(
            maggotEffects,
            transform.position,
            Quaternion.identity);

        transform.GetComponent<PolygonCollider2D>().enabled = false;

        rigidBody2d.velocity = Vector2.zero;
        rigidBody2d.gravityScale = 5;

        Destroy(maggots, 3f);
        Destroy(gameObject, 3f);
    }
}
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
        for (int i = 0; i < flyRespawnLocationsList.Count; i++)
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
        timeForRespawn -= 3.5f;
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
public interface ISubject
{
    /// <summary>
    /// Add observer to the subject's list
    /// </summary>
    /// <param name="observerRespawner">Observer</param>
    public void AddObserver(ObserverRespawners observerRespawner);
    /// <summary>
    /// Remove observer from subject's list
    /// </summary>
    /// <param name="observerRespawner">Observer</param>
    public void RemoveObserver(ObserverRespawners observerRespawner);
    /// <summary>
    /// Notify observers that a body part was destroyed
    /// </summary>
    public void Notify();
    /// <summary>
    /// Get current number of destroyed body parts
    /// </summary>
    /// <returns>Current number of destroyed body parts</returns>
    public int GetState();
    /// <summary>
    /// Notify observers to spawn new flies
    /// </summary>
    public void Spawn();
}
using System.Collections;
using UnityEngine;

public class Maggot : MonoBehaviour
{
    private Rigidbody2D rigidbody2d;
    private Animator animator;

    private bool isMoving;
    private float time;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        time = 0f;
        isMoving = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isMoving)
        {
            time += Time.deltaTime;
            rigidbody2d.velocity = new Vector2(0f, time * -15f);
        }
    }

    /// <summary>
    /// When maggot touches ground, it starts to move
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 6
            && !isMoving)
        {
            isMoving = true;
            StartCoroutine(StartMovingThenBurrow());
        }
    }

    /// <summary>
    /// Maggot starts moving after random time and then it burrows itself after some time
    /// </summary>
    private IEnumerator StartMovingThenBurrow()
    {
        rigidbody2d.velocity = new Vector2(0f, 0f);

        int randomDirectionMove = Random.Range(0, 2);
        transform.rotation = Quaternion.Euler(
            0f,
            randomDirectionMove == 0 ? 180f : 0f,
            0f);

        int randomWaitTime = Random.Range(1, 4);
        yield return new WaitForSecondsRealtime(randomWaitTime);

        animator.SetTrigger("Move");
        rigidbody2d.velocity = new Vector2(
            randomDirectionMove == 0 ? -1f : 1f,
            0f);

        int randomTimeMoving = Random.Range(12, 15);
        Destroy(gameObject, randomTimeMoving);
    }
}
public abstract class ObserverRespawners
{
    /// <summary>
    /// Abstract method for notifying every observer interested in the message to update itself
    /// </summary>
    /// <param name="concreteSubject">Subject which carries information about destroyed body parts</param>
    public abstract void Update(ISubject subject);

    /// <summary>
    /// Method for spawning flies which needs to be overriden
    /// </summary>
    public abstract void SpawnFlies();
}
using System.Collections.Generic;
using UnityEngine;

public class OrdinaryFly : AbstractEnemy
{
    public static FlyType flyType = FlyType.Ordinary;
    private bool canAttack;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rigidBody2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        heroes = new List<GameObject>();
        lookingLeft = true;

        initialColor = spriteRenderer.color;

        transform.parent = GameObject.Find("OrdinaryFlies").transform;
        canAttack = false;

        Invoke(nameof(StartAttacking), 1f);
        FindHeroes();
        ChooseHeroToAttack();
    }

    /// <summary>
    /// Overriden method to start attacking after the flight from background
    /// </summary>
    protected override void StartAttacking()
    {
        canAttack = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!destroyed
            && heroes.ToArray().Length != 0
            && canAttack)
        {
            ChangeVelocityX();
            ChangeVelocityY();
            RotateEnemy();
        }
    }

    /// <summary>
    /// Overriden method for returning a type of fly
    /// </summary>
    /// <returns>Enum value of type of fly</returns>
    public override int GetFlyType()
    {
        return (int)flyType;
    }

    /// <summary>
    /// Overriden method for ordinary fly
    /// </summary>
    public override void Death()
    {
        animator.SetBool("dead", true);
        SingletonSFX.Instance.PlaySFX("SFX39_fly_die");

        GameObject maggots = Instantiate(
            maggotEffects,
            transform.position,
            Quaternion.identity);

        rigidBody2d.velocity = Vector2.zero;
        rigidBody2d.gravityScale = 5;

        Destroy(maggots, 3f);
        Destroy(gameObject, 3f);
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton : AbstractEnemy
{
    public static FlyType flyType = FlyType.NotFly;

    private float positionAfterTurnX;
    private int maxHealth;
    private bool isTurning;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rigidBody2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        positionAfterTurnX = transform.position.x;

        lookingLeft = true;
        isTurning = false;

        maxHealth = health;
        initialColor = spriteRenderer.color;
    }

    // Method is called when the object becomes enabled and active
    private void OnEnable()
    {
        heroes = new List<GameObject>();
        FindHeroes();
        ChooseHeroToAttack();
    }

    /// <summary>
    /// Overriden method to start attack after the flight from background.
    /// Skeletons do not fly from the background, so this does nothing
    /// </summary>
    protected override void StartAttacking()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!destroyed
            && isTurning)
        {
            if (Vector2.Distance(transform.position, new Vector2(positionAfterTurnX, transform.position.y)) < 0.5f)
            {
                isTurning = false;
            }
        }

        if (!destroyed
            && !isTurning
            && heroes.ToArray().Length != 0)
        {
            ChangeVelocityX();
            RotateEnemy();
        }
    }

    /// <summary>
    /// Overriden method for returning a type of fly
    /// </summary>
    /// <returns>Enum value of type of fly</returns>
    public override int GetFlyType()
    {
        return (int)flyType;
    }

    /// <summary>
    /// Skeleton turns around after it is near the edge of the arena
    /// </summary>
    private void TurnAround()
    {

    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Turn")
            && !isTurning)
        {
            isTurning = true;
            if (collision.gameObject.name.StartsWith("Left"))
            {
                lookingLeft = false;
                rigidBody2d.velocity = new Vector2(Math.Abs(rigidBody2d.velocity.x), 0f);
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                positionAfterTurnX = transform.position.x + 2f;
            }
            else
            {
                lookingLeft = true;
                rigidBody2d.velocity = new Vector2(Math.Abs(rigidBody2d.velocity.x) * (-1), 0f);
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                positionAfterTurnX = transform.position.x - 2f;
            }
        }
    }

    /// <summary>
    /// Overriden death method for skeleton
    /// </summary>
    public override void Death()
    {
        animator.SetBool("destroyedIdle", true);
        animator.SetTrigger("fall");
        SingletonSFX.Instance.PlaySFX("SFX40_skeleton_destroyed");

        rigidBody2d.velocity = Vector2.zero;

        Invoke(nameof(RiseAgain), 4f);
    }

    /// <summary>
    /// Called after certain amount of time for skeleton to rise
    /// again and gain health in order to attack the player
    /// </summary>
    private void RiseAgain()
    {
        destroyed = false;
        isTurning = false;

        animator.SetBool("destroyedIdle", false);
        health = maxHealth;
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToxicFly : AbstractEnemy
{
    public static FlyType flyType = FlyType.Toxic;

    [SerializeField] GameObject poisonousBubble;
    [SerializeField] float stopDistanceX;
    [SerializeField] float stopDistanceYFromUp;
    [SerializeField] float stopDistanceYFromDown;
    [SerializeField] float adjustBubblePositionX;
    [SerializeField] float adjustBubblePositionY;

    private bool isAttacking;
    private bool canAttackAgain;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rigidBody2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        heroes = new List<GameObject>();
        lookingLeft = true;
        isAttacking = false;
        canAttackAgain = false;

        initialColor = spriteRenderer.color;

        transform.parent = GameObject.Find("ToxicFlies").transform;

        FindHeroes();
        ChooseHeroToAttack();

        Invoke(nameof(StartAttacking), 1.5f);
    }

    /// <summary>
    /// Overriden method to start attack after the flight from background
    /// </summary>
    protected override void StartAttacking()
    {
        canAttackAgain = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!destroyed && heroes.ToArray().Length != 0)
        {
            if (!isAttacking
                && canAttackAgain
                && Vector2.Distance(transform.position, heroes.ToArray()[attackRandomPlayer].transform.position) < stopDistanceX + 1
                && (transform.position.y - heroes.ToArray()[attackRandomPlayer].transform.position.y) <= stopDistanceYFromUp
                && (transform.position.y - heroes.ToArray()[attackRandomPlayer].transform.position.y) >= stopDistanceYFromDown)
            {
                spriteRenderer.color = new Color(0.1756167f, 0.7264151f, 0f);

                StartCoroutine(PoisonBubbleAttack());
            }

            if (!isAttacking)
            {
                if ((transform.position.x - heroes.ToArray()[attackRandomPlayer].transform.position.x) < stopDistanceX
                    && (transform.position.x - heroes.ToArray()[attackRandomPlayer].transform.position.x) > -stopDistanceX)
                {
                    // Added 0.01 for multiplying velocity X since method for rotating
                    // characters is based on positive/negative number of velocity X
                    ChangeVelocityX(0.01f);
                }
                else
                {
                    ChangeVelocityX();
                }

                ChangeVelocityY();
                RotateEnemy();
            }
        }
    }

    /// <summary>
    /// Method called when toxic fly is near player. 
    /// Calls methods for spitting poisonous bubble.
    /// </summary>
    /// <returns></returns>
    private IEnumerator PoisonBubbleAttack()
    {
        isAttacking = true;
        canAttackAgain = false;
        rigidBody2d.velocity = Vector2.zero;
        yield return new WaitForSecondsRealtime(0.4f);

        RotateEnemy();
        InstantiatePoisonousBubble();
        spriteRenderer.color = new Color(0.4599614f, 1f, 0.2877358f);
        yield return new WaitForSecondsRealtime(1.2f);

        isAttacking = false;
        yield return new WaitForSecondsRealtime(1.8f);

        canAttackAgain = true;
    }

    /// <summary>
    /// Create poisonous bubble
    /// </summary>
    private void InstantiatePoisonousBubble()
    {
        if (health > 0)
        {
            SingletonSFX.Instance.PlaySFX("SFX42_bubble_spitted");

            GameObject clonedPoisonousBubble = Instantiate(
                 poisonousBubble,
                 new Vector3(
                     transform.position.x + (transform.rotation.eulerAngles.y == 0
                         ? adjustBubblePositionX : adjustBubblePositionX * (-1)),
                     transform.position.y + adjustBubblePositionY,
                     transform.position.z),
                     Quaternion.identity);

            clonedPoisonousBubble.transform.rotation = Quaternion.Euler(
                0f,
                transform.rotation.eulerAngles.y == 180f ? 180f : 0f,
                0);

            clonedPoisonousBubble.GetComponent<Rigidbody2D>().velocity = new Vector2(
                transform.rotation.eulerAngles.y == 180 ? 20f : -20f,
                0f);

            clonedPoisonousBubble.transform.GetChild(1).gameObject.GetComponent<Projectile>().MultipliedVelocityX = -2.5f;
            clonedPoisonousBubble.transform.GetChild(1).gameObject.GetComponent<Projectile>().MultipliedVelocityY = 1f;

            try
            {
                Destroy(clonedPoisonousBubble, 4f);
            }
            catch (Exception ex)
            {
                Debug.Log("Bubble is already destroyed: " + ex);
            }
        }
    }

    /// <summary>
    /// Overriden method for returning a type of fly
    /// </summary>
    /// <returns>Enum value of type of fly</returns>
    public override int GetFlyType()
    {
        return (int)flyType;
    }

    /// <summary>
    /// Overriden method for toxic fly
    /// </summary>
    public override void Death()
    {
        animator.SetBool("dead", true);
        SingletonSFX.Instance.PlaySFX("SFX39_fly_die");

        GameObject maggots = Instantiate(
            maggotEffects,
            transform.position,
            Quaternion.identity);

        transform.GetComponent<PolygonCollider2D>().enabled = false;

        rigidBody2d.velocity = Vector2.zero;
        rigidBody2d.gravityScale = 5;

        Destroy(maggots, 3f);
        Destroy(gameObject, 3f);
    }
}
using System;

[Serializable]
public class Player
{
    public string playerName;
    public float finishTime;

    /// <summary>
    /// Empty constructor for the Player object
    /// </summary>
    public Player()
    {
    }

    /// <summary>
    /// Constructor for the Player object
    /// </summary>
    /// <param name="playerName">Name of the palyer</param>
    /// <param name="finishTime">Time needed to beat the boss</param>
    public Player(string playerName, float finishTime)
    {
        this.playerName = playerName;
        this.finishTime = finishTime;
    }
}

using Proyecto26;
using System.Collections.Generic;
using FullSerializer;
using System.Linq;

public static class PlayerData
{
    public static fsSerializer serializer = new();

    /// <summary>
    /// Prepare data for database by checking if the data doesn't exist or if player has a 
    /// better time stored in the database than the current one
    /// </summary>
    /// <param name="boss">Name of the boss</param>
    /// <param name="playerName">Name of the player</param>
    /// <param name="fastestTime">Time needed to beat the boss</param>
    public static void PrepareForDatabase(string boss, string playerName, float fastestTime)
    {
        GetDataFromDatabase(boss, null, playerName, fastestTime, false);
    }

    /// <summary>
    /// Get the data stored in database, calls the method to see if there is a need to store the new data
    /// </summary>
    /// <param name="boss">Name of the boss</param>
    /// <param name="playerName">Name of the player</param>
    /// <param name="fastestTime">Time needed to beat the boss</param>
    /// <param name="justFetch">True if data is needed just to show the highscore, 
    /// false if it needs to compare new data with the old</param>
    public static void GetDataFromDatabase(string boss, TitleScreen titleScreen,
        string playerName = "", float fastestTime = 0, bool justFetch = true)
    {
        RestClient.Get("https://ruined-essence-of-the-divine-default-rtdb.europe-west1.firebasedatabase.app/" + boss + ".json")
        .Then(x =>
        {
            fsData playersData = fsJsonParser.Parse(x.Text);
            Dictionary<string, Player> playerDataDictionary = new Dictionary<string, Player>();
            serializer.TryDeserialize(playersData, ref playerDataDictionary);
            if (justFetch)
            {
                SortList(playerDataDictionary, titleScreen);
            }
            else
            {
                if (playersData.IsNull)
                {
                    PostToDatabase(boss, playerName, fastestTime);
                }
                else
                {
                    bool postToDatabase = CheckIfBetterTimeExists(boss, playerName, fastestTime, playerDataDictionary);
                    if (postToDatabase)
                    {
                        PostToDatabase(boss, playerName, fastestTime);
                    }
                }
            }
        });
    }

    /// <summary>
    /// Sort list by fastest time descending
    /// </summary>
    /// <param name="playerDataDictionary"></param>
    private static void SortList(Dictionary<string, Player> playerDataDictionary, TitleScreen titleScreen)
    {
        var sortedPlayerDataDictionary = playerDataDictionary.Values.OrderBy(v => v.finishTime);
        titleScreen.FillHighScoreBoard(sortedPlayerDataDictionary);
    }

    /// <summary>
    /// Checks if the data doesn't exist or if player's current finish time is better than the one stored in database
    /// </summary>
    /// <param name="boss">Name of the boss</param>
    /// <param name="playerName">Name of the player</param>
    /// <param name="finishTime">Time needed to beat the boss</param>
    /// <param name="playerData">Data about all players and their time needed to defeat the boss</param>
    /// <returns>True if player data doesn't exist or the current time is the fastest, false if the mentioned isn't the case</returns>
    private static bool CheckIfBetterTimeExists(string boss, string playerName, float finishTime,
        Dictionary<string, Player> playerDataDictionary)
    {
        foreach (var data in playerDataDictionary.Values)
        {
            if (data.playerName.Equals(playerName))
            {
                if (data.finishTime > finishTime
                    || data.finishTime == 0)
                {
                    PostToDatabase(boss, playerName, finishTime);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        PostToDatabase(boss, playerName, finishTime);
        return true;
    }

    /// <summary>
    /// Save the data to Firebase
    /// </summary>
    /// <param name="boss">Name of the boss</param>
    /// <param name="playerName">Name of the player</param>
    /// <param name="fastestTime">Fastest time of the player to beat the boss</param>
    public static void PostToDatabase(string boss, string playerName, float fastestTime)
    {
        Player player = new(playerName, fastestTime);

        RestClient.Put("https://ruined-essence-of-the-divine-default-rtdb.europe-west1.firebasedatabase.app/"
            + boss + "/" + playerName + ".json", player);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class FernBehemoth : AbstractBoss
{
    [SerializeField] AudioClip walkSFX;
    [SerializeField] GameObject greenFire;
    [SerializeField] GameObject greenFireball;
    [SerializeField] GameObject greenFireTrees;
    [SerializeField] GameObject mouthPosition;
    [SerializeField] GameObject sceneManager;

    private AudioSource audioSourceStepSFX;
    private SpriteRenderer spriteRenderer;

    private bool facingLeft;
    private float eulerTurn;

    private bool invincible;
    private bool isAttacking;
    private float velocityAttacking;

    private int minAttack;
    private int maxAttack;

    private int alreadyUsedAttackMove;

    private float shrinkNumber;
    private float shrinkDifferenceNumber;

    // Start is called before the first frame update
    void Start()
    {
        PlayerPrefs.SetString("sceneName", SceneManager.GetActiveScene().name);
        StartCoroutine(IncreaseMusicVolume());

        heroes = new List<GameObject>();

        rigidBody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSourceStepSFX = GetComponent<AudioSource>();

        rigidBody2d.velocity = Vector2.zero;

        transform.rotation = Quaternion.Euler(0f, 180, 0f);
        eulerTurn = 180;

        facingLeft = true;
        isAttacking = false;
        alreadyUsedAttackMove = 0;

        minAttack = 1;
        maxAttack = 4;

        audioSourceStepSFX.enabled = false;
        halfway = false;

        shrinkNumber = 70;
        shrinkDifferenceNumber = 10;

        Invoke(nameof(Roar), 4f);
    }

    /// <summary>
    /// Called when player comes near Behemoth. 
    /// Behemoth gets more health if there is a second player.
    /// Behemoth starts moving.
    /// Overrides method from AbstractBoss class.
    /// </summary>
    public override void StartAttacking()
    {
        timerCountDown.GetComponent<TimeCountDown>().countTime = true;


        if (GameObject.Find("PlayerKnight1") != null)
        {
            heroes.Add(GameObject.Find("PlayerKnight1"));
        }

        if (GameObject.Find("PlayerKnight2") != null)
        {
            heroes.Add(GameObject.Find("PlayerKnight2"));
        }

        rigidBody2d.velocity = new Vector2(-velocity, 0f);
        audioSourceStepSFX.clip = walkSFX;
        audioSourceStepSFX.enabled = true;
    }

    /// <summary>
    /// Used to activate Behemoth's cry
    /// </summary>
    private void Roar()
    {
        GameObject.Find("RoarEchoEffect").transform.gameObject.GetComponent<AudioSource>().enabled = false;
        GameObject.Find("RoarEchoEffect").transform.gameObject.GetComponent<AudioSource>().enabled = true;
    }

    /// <summary>
    /// Called when Behemoth enters one of the box colliders at the left and right end of arena.
    /// Makes him turn to the opposite way of facing and adds an opposite X velocity.
    /// Checks if Behemoth collided with left/right box collider for attacking.
    /// Checks if Behemoth entered the middle box collider for burning trees.
    /// Checks if Behemoth enters area which triggers him to scream after having lower HP.
    /// </summary>
    /// <param name="collision">Collider with which an object made a collision</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Turn")
            && collision.transform.name.Equals("TurnColliderLeft"))
        {
            collision.transform.gameObject.GetComponent<BoxCollider2D>().enabled = false;
            facingLeft = false;
            eulerTurn = 0;
            StartCoroutine(Turn(velocity, collision));
        }
        else if (collision.transform.CompareTag("Turn")
            && collision.transform.name.Equals("TurnColliderRight"))
        {
            collision.transform.gameObject.GetComponent<BoxCollider2D>().enabled = false;
            facingLeft = true;
            eulerTurn = 180;
            StartCoroutine(Turn(-velocity, collision));
        }
        else if (collision.transform.CompareTag("Scream")
            && !isAttacking)
        {
            Roar();
            collision.transform.gameObject.SetActive(false);
        }
        else if (collision.transform.CompareTag("Attack")
            && collision.transform.name.Equals("AttackColliderMiddle")
            && minAttack.Equals(4) && minAttack.Equals(4))
        {
            PickAttackAction();
        }
        else if (collision.transform.CompareTag("Attack")
            && minAttack < 4)
        {
            PickAttackAction();
        }
    }

    /// <summary>
    /// Called to turn Behemoth and it's child objects to the opposite side. Activates 
    /// other polygon collider for animation.
    /// </summary>
    /// <param name="velocity">Behemoth's velocity used after turning to the opposite side.</param>
    /// <param name="collision">Collider being turned off for short amount of time for Behemoth
    /// to walk out of the collider. Makes him stuck otherwise</param>
    /// <returns></returns>
    private IEnumerator Turn(float velocity, Collider2D collision)
    {
        animator.SetFloat("Speed", velocity);
        animator.SetTrigger("Turn");

        rigidBody2d.velocity = new Vector2(0f, 0f);

        isAttacking = false;
        audioSourceStepSFX.enabled = false;
        audioSourceStepSFX.pitch = 0.9f;

        transform.GetChild(0).gameObject.transform.GetComponent<PolygonCollider2D>().enabled = false;
        transform.GetChild(1).gameObject.transform.GetComponent<PolygonCollider2D>().enabled = true;
        yield return new WaitForSeconds(0.4f);

        transform.GetChild(0).gameObject.transform.GetComponent<PolygonCollider2D>().enabled = true;
        transform.GetChild(1).gameObject.transform.GetComponent<PolygonCollider2D>().enabled = false;

        transform.rotation = Quaternion.Euler(0f, eulerTurn, 0f);
        rigidBody2d.velocity = new Vector2(velocity, 0f);
        audioSourceStepSFX.enabled = true;
        yield return new WaitForSeconds(4f);

        collision.transform.gameObject.GetComponent<BoxCollider2D>().enabled = true;
    }

    /// <summary>
    /// Decreases Fern Behemoth's health
    /// </summary>
    public override void MinusHealth(int layer)
    {
        if (!invincible)
        {
            if (layer == 16)
            {
                health -= 2;
                SingletonSFX.Instance.PlaySFX("SFX15_blood_splash");
            }
            else if (layer == 19)
            {
                health -= 3;
                SingletonSFX.Instance.PlaySFX("SFX23_fire_burn");
            }

            if (health <= 0)
            {
                minAttack = 0;
                maxAttack = 0;

                Death(true);
                return;
            }

            spriteRenderer.color = new Color(1f, 1f, 1f);

            invincible = true;
            Invoke(nameof(RemoveInvincibility), 0.3f);

            if (health <= shrinkNumber
                && !halfway)
            {
                shrinkNumber -= shrinkDifferenceNumber;
                ShrinkBehemoth(3f, 1.8f);
            }

            if (health <= 60
                && !halfway)
            {
                halfway = true;
                ShrinkBehemoth(3f, 1.8f);

                minAttack = 4;
                maxAttack = 4;
            }
        }
    }

    /// <summary>
    /// Removes invincibility after certain amount of time
    /// </summary>
    private void RemoveInvincibility()
    {
        spriteRenderer.color = new Color(0.6886792f, 0.6886792f, 0.6886792f);
        invincible = false;
    }

    /// <summary>
    /// Behemoth shrinks after getting certain amount of damage
    /// </summary>
    private void ShrinkBehemoth(float scaleNumberToShrink, float positionNumberChange)
    {
        //8 times he needs to shrink in order for sprite to still look good
        Vector3 scaleChange = new(scaleNumberToShrink, scaleNumberToShrink, 0f);
        Vector3 positionChange = new(0f, positionNumberChange, 0f);

        transform.localScale -= scaleChange;
        transform.position -= positionChange;

        SingletonSFX.Instance.PlaySFX("SFX21_neo-ridley_second_scream");
    }

    /// <summary>
    /// Calls a virtual method from the abstract class and then uses specialized code for death.
    /// </summary>
    /// <param name="saveToDb">True if time and player name should be saved in database, false if not</param>
    protected override void Death(bool saveToDb)
    {
        base.Death(saveToDb);

        audioSourceStepSFX.enabled = false;

        spriteRenderer.color = new Color(0.2924528f, 0.2924528f, 0.2924528f);

        GameObject.Find("HitboxWalk").SetActive(false);
        GameObject.Find("HitboxTurn").SetActive(false);

        sceneManager.GetComponent<SceneLoader>().LoadSceneCoroutine(8f, "HubWorld");

        Destroy(gameObject, 5f);
        StartCoroutine(DeathScreams());
    }

    /// <summary>
    /// Method for playing Behemoth SFX death screams after being defeated
    /// </summary>
    /// <returns></returns>
    private IEnumerator DeathScreams()
    {
        SingletonSFX.Instance.PlaySFX("SFX21_neo-ridley_second_scream");
        yield return new WaitForSecondsRealtime(1f);

        rigidBody2d.velocity = new Vector2(0f, -5f);
        SingletonSFX.Instance.PlaySFX("SFX17_neo-ridley_scream");
        yield return new WaitForSecondsRealtime(1f);

        SingletonSFX.Instance.PlaySFX("SFX17_neo-ridley_scream");
        yield return new WaitForSecondsRealtime(1f);

        SingletonSFX.Instance.PlaySFX("SFX21_neo-ridley_second_scream");
        yield return new WaitForSecondsRealtime(1f);

        SingletonSFX.Instance.PlaySFX("SFX21_neo-ridley_second_scream");
        yield return new WaitForSecondsRealtime(0.5f);

        Roar();
    }

    /// <summary>
    /// Randomizing numbers to decide which attack should be used next.
    /// If the current chosen attack is the same as the previous one, 
    /// method will call itself (Recursive method).
    /// </summary>
    private void PickAttackAction()
    {
        velocityAttacking = facingLeft ? velocity * (-1) : velocity;
        int randomMove = (int)Random.Range(minAttack, maxAttack);

        if (!isAttacking)
        {
            if (alreadyUsedAttackMove == randomMove)
            {
                PickAttackAction();
                return;
            }

            isAttacking = true;

            switch (randomMove)
            {
                case 1:
                    StartCoroutine(LeaveGreenFireOnGround());
                    break;
                case 2:
                    StartCoroutine(AttackRun());
                    break;
                case 3:
                    StartCoroutine(BreatheFire());
                    break;
                case 4:
                    StartCoroutine(StartFireOnTrees());
                    break;
                default:
                    StartCoroutine(BreatheFire());
                    break;
            }

            alreadyUsedAttackMove = randomMove;
        }
    }

    /// <summary>
    /// Attack move where Behemoth leaves fire objects, pauses a little and leaves again fire objects,
    /// which in the end leaves a room for player to roll and be between fire objects
    /// </summary>
    /// <param name="pause">Used for not leaving fire objects for a little while after 14 fire objects for
    /// player to avoid</param>
    /// <returns></returns>
    private IEnumerator LeaveGreenFireOnGround(bool pause = false)
    {
        if (pause)
        {
            yield return new WaitForSecondsRealtime(0.7f);
        }

        for (int i = 0; i < 12; i++)
        {
            if (health <= 0)
            {
                yield break;
            }

            SingletonSFX.Instance.PlaySFX("SFX20_floor_fire");
            GameObject clonedGreenFire = Instantiate(
                greenFire,
                new Vector3(
                    transform.position.x,
                    -0.96f,
                    transform.position.z),
                Quaternion.identity);

            Destroy(clonedGreenFire, 3.5f);
            yield return new WaitForSecondsRealtime(0.3f);
        }

        if (!pause)
        {
            StartCoroutine(LeaveGreenFireOnGround(true));
        }
    }

    /// <summary>
    /// Attack move makes Behemoth run fast. Stops at the turn collider.
    /// </summary>
    /// <returns></returns>
    private IEnumerator AttackRun()
    {
        audioSourceStepSFX.enabled = false;

        animator.SetFloat("Speed", System.Math.Abs(velocityAttacking));
        animator.SetTrigger("ShortIdle");

        rigidBody2d.velocity = Vector2.zero;
        yield return new WaitForSeconds(0.5f);

        audioSourceStepSFX.pitch = 1.3f;
        audioSourceStepSFX.enabled = true;

        rigidBody2d.velocity = new Vector2(velocityAttacking * 3, 0f);
    }

    /// <summary>
    /// Breathes green fire at the player
    /// </summary>
    /// <returns></returns>
    private IEnumerator BreatheFire()
    {
        rigidBody2d.velocity = Vector2.zero;

        audioSourceStepSFX.enabled = false;
        animator.SetBool("BeIdle", true);

        int lowerNoFireballs = halfway == true ? 4 : 0;
        int noFireballs = Random.Range(7 - lowerNoFireballs, 10 - lowerNoFireballs);

        for (int i = 0; i < noFireballs; i++)
        {
            if (health <= 0)
            {
                yield break;
            }

            GameObject clonedGreenFire = Instantiate(
                   greenFireball,
                   new Vector3(
                       mouthPosition.transform.position.x,
                       mouthPosition.transform.position.y,
                       transform.position.z),
                       Quaternion.identity);

            PlaceClonedGreenFire(clonedGreenFire, i);
            SingletonSFX.Instance.PlaySFX("SFX20_floor_fire");

            Destroy(clonedGreenFire, 4f);
            yield return new WaitForSecondsRealtime(0.4f);
        }

        animator.SetBool("BeIdle", false);
        rigidBody2d.velocity = new Vector2(velocityAttacking + (facingLeft ? -8 : 8), 0f);
        audioSourceStepSFX.enabled = true;
    }

    /// <summary>
    /// Method used for placing green fire that Behemoth breathes in right position
    /// </summary>
    /// <param name="clonedGreenFire">Cloned green fire</param>
    /// <param name="noFire">Number of cloned green fire in order</param>
    private void PlaceClonedGreenFire(GameObject clonedGreenFire, int noFire)
    {
        if (!halfway)
        {
            clonedGreenFire.transform.GetComponent<Rigidbody2D>().velocity = new Vector2(
            velocityAttacking + (facingLeft ? noFire * (-1) : noFire) * 10,
            -4f);

            clonedGreenFire.GetComponent<Rigidbody2D>().gravityScale = 1;
            clonedGreenFire.transform.GetChild(1).gameObject.GetComponent<Projectile>().MultipliedVelocityX = -2.4f;
            clonedGreenFire.transform.GetChild(1).gameObject.GetComponent<Projectile>().MultipliedVelocityY = 25f;
            clonedGreenFire.transform.rotation = Quaternion.Euler(
            0f,
            eulerTurn == 180 ? 0 : 180,
            -35f - noFire * 5.5f);
        }
        else
        {
            clonedGreenFire.transform.GetComponent<Rigidbody2D>().velocity = new Vector2(
            velocityAttacking + (facingLeft ? noFire * (-1) : noFire) * 11,
            -4.5f);

            clonedGreenFire.GetComponent<Rigidbody2D>().gravityScale = 0;
            clonedGreenFire.transform.GetChild(1).gameObject.GetComponent<Projectile>().MultipliedVelocityX = -3f;
            clonedGreenFire.transform.GetChild(1).gameObject.GetComponent<Projectile>().MultipliedVelocityY = 5f;
            clonedGreenFire.transform.rotation = Quaternion.Euler(
            0f,
            eulerTurn == 180 ? 0 : 180,
            -90);
        }
    }

    /// <summary>
    /// Attack that summons green fire left and right from Behemoth and burns trees.
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartFireOnTrees()
    {
        audioSourceStepSFX.enabled = false;
        animator.SetBool("BeIdle", true);

        rigidBody2d.velocity = new Vector2(0f, 0f);

        for (int i = 0; i < 24; i++)
        {
            SingletonSFX.Instance.PlaySFX("SFX20_floor_fire");
            GameObject clonedGreenFireRight = Instantiate(
                greenFire,
                new Vector3(transform.position.x + (i * 3 + 1),
                    -0.96f,
                    transform.position.z),
                Quaternion.identity);

            SingletonSFX.Instance.PlaySFX("SFX20_floor_fire");
            GameObject clonedGreenFireLeft = Instantiate(
                greenFire,
                new Vector3(
                    transform.position.x + (-i * 3 - 1),
                    -0.96f,
                    transform.position.z),
                Quaternion.identity);

            Destroy(clonedGreenFireRight, 2.0f);
            Destroy(clonedGreenFireLeft, 2.0f);

            yield return new WaitForSecondsRealtime(0.05f);
        }

        yield return new WaitForSecondsRealtime(1.5f);

        greenFireTrees.SetActive(true);
        audioSourceStepSFX.enabled = true;
        animator.SetBool("BeIdle", false);

        rigidBody2d.velocity = new Vector2(velocityAttacking, 0f);
        minAttack = 1;
    }
}
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    [SerializeField] Button retry;
    [SerializeField] Button quitToHub;

    private string previousScene;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(IncreaseMusicVolume());
        previousScene = PlayerPrefs.GetString("sceneName");
    }

    /// <summary>
    /// Retry the boss fight again
    /// </summary>
    public void Retry()
    {
        try
        {
            PrepareTheScene(previousScene);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    /// <summary>
    /// Quit the boss fight and return to the hubworld
    /// </summary>
    public void QuitToHub()
    {
        PrepareTheScene("HubWorld");
    }

    /// <summary>
    /// Prepare the scene by making black screen appear, lower the general audio volume and call 
    /// the method to load the scene
    /// </summary>
    private void PrepareTheScene(string scene)
    {
        GameObject.Find("BlackScreenSprite").GetComponent<Animator>().SetTrigger("Appear");

        SingletonSFX.Instance.PlaySFX("SFX6_big_thing_fly_sky");

        StartCoroutine(LowerMusicVolume());
        StartCoroutine(LoadScene(scene));
    }

    /// <summary>
    /// Load scene
    /// </summary>
    /// <returns></returns>
    private IEnumerator LoadScene(string scene)
    {
        yield return new WaitForSecondsRealtime(2f);

        SceneManager.LoadScene(scene);
    }

    /// <summary>
    /// Increase the general sound of everything in the scene
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerator IncreaseMusicVolume()
    {
        while (AudioListener.volume < 1)
        {
            AudioListener.volume += 0.1f;
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    /// <summary>
    /// Lower the general sound of everything in the scene
    /// </summary>
    /// <returns></returns>
    private IEnumerator LowerMusicVolume()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        while (AudioListener.volume > 0)
        {
            AudioListener.volume -= 0.1f;
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlacialOverlord : AbstractBoss
{
    [SerializeField] List<Transform> teleportPositions;
    [SerializeField] List<Transform> flyingCrystalsPositionsList;

    [SerializeField] GameObject flyingCrystalBlue;
    [SerializeField] GameObject flyingCrystalTurquoise;
    [SerializeField] GameObject icicle;
    [SerializeField] GameObject icicleStaying;
    [SerializeField] GameObject icicleGround;
    [SerializeField] GameObject teleportIceEffect;
    [SerializeField] GameObject healingCircles;
    [SerializeField] GameObject icyWind;
    [SerializeField] GameObject sceneManager;
    [SerializeField] GameObject snowflake;
    [SerializeField] GameObject deadOverlord;

    [SerializeField] float speedToDestroyIcicle;
    [SerializeField] float iciclePositionMinX;
    [SerializeField] float iciclePositionMaxX;
    [SerializeField] float iciclePositionY;

    [SerializeField] int damageToStopHealing;

    private SpriteRenderer spriteRenderer;
    private GameObject healingCirclesReference;
    private GameObject sword;

    private bool[] attackStillInProgress;
    private bool isAttacking;
    private bool invincible;
    private bool isHealing;
    private bool shouldMoveTowardsIcicle;
    private bool moveTowardsIcicle;
    private bool moveTowardsPlayer;
    private bool lookingLeft;

    private double maxHealth;
    private double nextPhaseHealth;

    public float forcedMovement;
    private float transformToMoveTowardsX;
    private float transformToMoveTowardsY;
    private float currentTimeOfHealing;
    private float timeNeededToHeal;
    private float positionSpeed;
    private float timer;
    private float waitTime;

    private int noPlayers;
    private int playerToFence;
    private int firstPlayerPriority;
    private int secondPlayerPriority;
    private int alreadyUsedAttackMove;
    private int damageWhileHealing;
    private int overlordSwordLayerFencing;
    private int overlordSwordLayerHurting;
    private int minAttack;
    private int maxAttack;
    private int battlePhaseRemaining;
    private int swingSword;
    private int currentSwingSword;

    // Start is called before the first frame update
    void Start()
    {
        PlayerPrefs.SetString("sceneName", SceneManager.GetActiveScene().name);
        StartCoroutine(IncreaseMusicVolume());

        heroes = new List<GameObject>();

        rigidBody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rigidBody2d.velocity = new Vector2(0f, 0f);

        sword = transform.GetChild(1).gameObject;

        isAttacking = true;
        invincible = false;
        isHealing = false;
        shouldMoveTowardsIcicle = false;
        moveTowardsIcicle = false;
        moveTowardsPlayer = false;
        lookingLeft = true;

        overlordSwordLayerFencing = LayerMask.NameToLayer("OverlordSword");
        overlordSwordLayerHurting = LayerMask.NameToLayer("Enemy");

        attackStillInProgress = new bool[7];

        SetNumericValuesStart();
    }

    /// <summary>
    /// Set numeric values to variables during the start
    /// </summary>
    private void SetNumericValuesStart()
    {
        transformToMoveTowardsX = 0;
        transformToMoveTowardsY = 0;

        alreadyUsedAttackMove = 1;

        forcedMovement = 0f;
        noPlayers = 1;
        playerToFence = 0;
        firstPlayerPriority = 0;
        secondPlayerPriority = 0;
        swingSword = 1;
        currentSwingSword = 0;
        maxHealth = health;

        damageWhileHealing = 0;
        currentTimeOfHealing = 0;
        timeNeededToHeal = 5f;
        positionSpeed = 0f;

        minAttack = 1;
        maxAttack = 6;
        battlePhaseRemaining = 2;

        timer = 1.5f;
        waitTime = 2f;
    }

    /// <summary>
    /// Called when player comes near Glacial Overlord. 
    /// Glacial Overlord gets more health if there is a second player.
    /// Glacial Overlord starts attacking.
    /// Overrides method from AbstractBoss class.
    /// </summary>
    public override void StartAttacking()
    {
        nextPhaseHealth = Math.Round(maxHealth * (2d / 3d), 0);

        timerCountDown.GetComponent<TimeCountDown>().countTime = true;

        if (GameObject.Find("PlayerKnight1") != null)
        {
            heroes.Add(GameObject.Find("PlayerKnight1"));
        }

        if (GameObject.Find("PlayerKnight2") != null)
        {
            heroes.Add(GameObject.Find("PlayerKnight2"));
            damageToStopHealing += 4;
            noPlayers++;
        }

        if (heroes.ToArray().Length == 2)
        {
            health += 100;
        }

        isAttacking = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isHealing)
        {
            currentTimeOfHealing += Time.deltaTime;
            if (currentTimeOfHealing > timeNeededToHeal)
            {
                StopHealing(true);
            }
        }

        if (heroes.Count > 0)
        {
            MoveTowardsPlayer();
        }

        if (moveTowardsIcicle)
        {
            MoveTowardsIcicle();
        }

        if (timer > waitTime
            && !isAttacking)
        {
            isAttacking = true;
            PickAttackAction();
        }
        else if (!isAttacking)
        {
            timer += Time.deltaTime;
        }
    }

    /// <summary>
    /// Method for making Glacial Overlord move towards player
    /// </summary>
    private void MoveTowardsPlayer()
    {
        if (moveTowardsPlayer)
        {
            if (Mathf.Abs(transform.position.x - heroes.ToArray()[playerToFence].transform.position.x) > 7f)
            {
                ChangeVelocityX(playerToFence);
            }
            else if (Mathf.Abs(transform.position.x - heroes.ToArray()[playerToFence].transform.position.x) <= 7f
                && heroes.ToArray()[playerToFence].GetComponent<Paladin>().m_grounded)
            {
                moveTowardsPlayer = false;
                ChangeVelocityX(playerToFence, 0);
                StartCoroutine(SwingSword());
            }
        }
    }

    /// <summary>
    /// Method for making Glacial Overlord move towards icicle to destroy it
    /// </summary>
    private void MoveTowardsIcicle()
    {
        positionSpeed += speedToDestroyIcicle * Time.deltaTime;
        transform.position = Vector2.MoveTowards(
            transform.position,
            new Vector2(
                transformToMoveTowardsX,
                transformToMoveTowardsY),
                positionSpeed);

        if (Vector2.Distance(transform.position, new Vector2(transformToMoveTowardsX, transformToMoveTowardsY)) < 1f)
        {
            moveTowardsIcicle = false;
            positionSpeed = 0;

            // Need to move Overlord to the right position on Y axis, otherwise he is too high on the axis
            transform.position = new Vector2(transform.position.x, teleportPositions.ToArray()[0].transform.position.y);
            RotateEnemyConditions(transformToMoveTowardsX);

            animator.SetBool("Falling", false);
            animator.SetTrigger("AttackAfterFalling");
            sword.GetComponent<PolygonCollider2D>().enabled = true;
            Invoke(nameof(DisableOverlordSword), 0.5f);

            // 2 represents icicle attack
            attackStillInProgress[2] = false;
            isAttacking = false;
            timer = 0.5f;
        }
    }

    /// <summary>
    /// Disable polygon collider 2D of the sword
    /// </summary>
    private void DisableOverlordSword()
    {
        sword.GetComponent<PolygonCollider2D>().enabled = false;
    }

    /// <summary>
    /// Decreases Glacial Overlord's health
    /// Overrides method from AbstractBoss class.
    /// </summary>
    public override void MinusHealth(int layer)
    {
        if (isHealing)
        {
            if (layer.Equals(16))
            {
                if (!invincible)
                {
                    damageWhileHealing += 2;
                    invincible = true;
                    SingletonSFX.Instance.PlaySFX("SFX15_blood_splash");
                    spriteRenderer.color = new Color(0f, 0f, 1f);
                    Invoke(nameof(RemoveInvincibility), 0.3f);
                }

                if (damageWhileHealing > damageToStopHealing)
                {
                    StopHealing(false);
                }
            }
        }
        else
        {
            if (!invincible)
            {
                if (layer.Equals(16)
                    || layer.Equals(19))
                {
                    health -= 2;
                    SingletonSFX.Instance.PlaySFX("SFX15_blood_splash");
                }

                invincible = true;

                if (health <= 0)
                {
                    Death(true);
                }
                else if (health <= nextPhaseHealth)
                {
                    NextPhaseStats();
                }
                else
                {
                    spriteRenderer.color = new Color(0.7830189f, 0.05001981f, 0.01846742f);
                    Invoke(nameof(RemoveInvincibility), 0.3f);
                }
            }
        }
    }

    /// <summary>
    /// Changing stats for the current and next phase (3 phases in total)
    /// </summary>
    private void NextPhaseStats()
    {
        spriteRenderer.color = new Color(0.7830189f, 0.05001981f, 0.01846742f);
        Invoke(nameof(RemoveInvincibility), 1f);

        SingletonSFX.Instance.PlaySFX("SFX63_boss_big_damage");

        Instantiate(
            darkMatterEffects,
            new Vector2(
                transform.position.x,
                transform.position.y),
            Quaternion.identity);

        battlePhaseRemaining -= 1;
        nextPhaseHealth = Math.Round(maxHealth * (battlePhaseRemaining / 3d));
        swingSword++;
        minAttack = 7;
        maxAttack = 8;
    }

    /// <summary>
    /// Calls a virtual method from the abstract class and then uses specialized code for death.
    /// </summary>
    /// <param name="saveToDb">True if time and player name should be saved in database, false if not</param>
    protected override void Death(bool saveToDb)
    {
        base.Death(saveToDb);

        minAttack = 0;
        maxAttack = 0;
        isAttacking = true;

        StartCoroutine(Teleport(1));
        GameObject.Find("Hitbox").SetActive(false);
        transform.GetChild(1).gameObject.SetActive(false);

        StartCoroutine(DeathProcess());

        sceneManager.GetComponent<SceneLoader>().LoadSceneCoroutine(15f, "HubWorld");
    }

    /// <summary>
    /// Death animation and effects process
    /// </summary>
    /// <returns></returns>
    private IEnumerator DeathProcess()
    {
        StartCoroutine(Teleport(1));
        RotateEnemy(false);
        yield return new WaitForSecondsRealtime(0.9f);

        spriteRenderer.enabled = false;

        Instantiate(
            deadOverlord,
            new Vector2(
                teleportPositions.ToArray()[1].position.x,
                teleportPositions.ToArray()[1].position.y),
            Quaternion.identity);

        Destroy(gameObject);
    }


    /// <summary>
    /// Removes invincibility after certain amount of time. 
    /// Called with Invoke() command.
    /// </summary>
    private void RemoveInvincibility()
    {
        spriteRenderer.color = new Color(1f, 1f, 1f);
        invincible = false;
    }

    /// <summary>
    /// Partial randomizing attacks.
    /// If the current chosen attack is the same as the previous one, 
    /// method will call itself (Recursive method).
    /// </summary>
    private void PickAttackAction()
    {
        animator.ResetTrigger("ReturnToIdle");
        animator.ResetTrigger("AttackAfterFalling");

        int randomMove = UnityEngine.Random.Range(minAttack, maxAttack);

        if (health <= 0)
        {
            return;
        }

        if (alreadyUsedAttackMove == randomMove
            || attackStillInProgress[randomMove - 1] == true)
        {
            PickAttackAction();
            return;
        }

        // Attack for freezing player should not happen too often
        if (randomMove == 5
            || randomMove == 6)
        {
            int chanceToUseAtack = UnityEngine.Random.Range(0, 3);
            if (!(chanceToUseAtack == 0))
            {
                PickAttackAction();
                return;
            }
        }

        attackStillInProgress[randomMove - 1] = true;
        int player = heroes.Count == 1 ? 0 : DecideWhichPlayerToAttack();

        ExecuteAttack(randomMove, player);
        alreadyUsedAttackMove = randomMove == 1 ? -1 : randomMove;
    }

    /// <summary>
    /// Call a method for executing an attack move
    /// </summary>
    /// <param name="randomMove">Attack which will be called and executed</param>
    /// <param name="player">Player which will be primarily attacked</param>
    private void ExecuteAttack(int randomMove, int player)
    {
        switch (randomMove)
        {
            case 1:
                RunToPlayer(player);
                break;
            case 2:
                StartCoroutine(ShootCrystalsAtPlayer(randomMove, player));
                break;
            case 3:
                StartCoroutine(SpawnIciclesAttack(randomMove));
                break;
            case 4:
                StartCoroutine(SpawnIceCrystals(randomMove));
                break;
            case 5:
                StartCoroutine(FreezePlayer(randomMove, player));
                break;
            case 6:
                StartCoroutine(IcyWind(randomMove));
                break;
            case 7:
                StartCoroutine(Heal());
                break;
        }
    }

    /// <summary>
    /// Run to player
    /// </summary>
    /// <param name="player">Player who will face Overlord</param>
    /// <returns></returns>
    private void RunToPlayer(int player)
    {
        animator.SetTrigger("Run");
        playerToFence = player;
        RotateEnemyConditions(heroes.ToArray()[playerToFence].transform.position.x);
        moveTowardsPlayer = true;
    }

    /// <summary>
    /// Attack player with the sword
    /// </summary>
    /// <returns></returns>
    private IEnumerator SwingSword()
    {
        RotateEnemyConditions(heroes.ToArray()[playerToFence].transform.position.x);

        animator.SetTrigger("Attack1");
        yield return new WaitForSecondsRealtime(0.2f);

        sword.tag = "OverlordSword";
        sword.layer = overlordSwordLayerFencing;
        sword.GetComponent<PolygonCollider2D>().enabled = true;
        yield return new WaitForSecondsRealtime(0.1f);

        sword.tag = "Enemy";
        sword.layer = overlordSwordLayerHurting;
        yield return new WaitForSecondsRealtime(0.1f);

        sword.GetComponent<PolygonCollider2D>().enabled = false;
        yield return new WaitForSecondsRealtime(0.5f);

        currentSwingSword += 1;

        if (currentSwingSword != swingSword)
        {
            int player = heroes.Count == 1 ? 0 : DecideWhichPlayerToAttack();
            RunToPlayer(player);
            yield break;
        }

        sword.tag = "OverlordSword";
        sword.layer = overlordSwordLayerFencing;

        // 0 is RunToPlayer attack
        attackStillInProgress[0] = false;
        isAttacking = false;
        currentSwingSword = 0;
        timer = 1.75f;

        int randomTeleportPositionOnGround = UnityEngine.Random.Range(0, 3);
        StartCoroutine(Teleport(randomTeleportPositionOnGround));
    }


    /// <summary>
    /// Spawn ice crystals which fly for a short time but fall on the floor.
    /// They remain on the floor for a certain amount of time.
    /// </summary>
    /// <param name="randomMove">Numbered attack</param>
    /// <returns></returns>
    private IEnumerator SpawnIceCrystals(int randomMove)
    {
        int randomCrystalNumber = UnityEngine.Random.Range(2, 4);
        animator.SetTrigger("SummonMagic");

        for (int i = 0; i < randomCrystalNumber; i++)
        {
            int randomCrystalPrefab = UnityEngine.Random.Range(0, 1);
            int randomPosition = UnityEngine.Random.Range(0, flyingCrystalsPositionsList.Count);
            float randomSpawnTime = UnityEngine.Random.Range(1f, 1.5f);
            float randomFlyTime = UnityEngine.Random.Range(0.5f, 3f);

            GameObject clonedFlyingCrystal = Instantiate(
                  flyingCrystalBlue,
                  new Vector2(
                      flyingCrystalsPositionsList.ToArray()[randomPosition].transform.position.x,
                      flyingCrystalsPositionsList.ToArray()[randomPosition].transform.position.y),
                  Quaternion.identity);

            clonedFlyingCrystal.transform.rotation = Quaternion.Euler(
                0f,
                0f,
                randomPosition % 2 == 0
                    ? randomCrystalPrefab == 0 ? -78f : -25.7f
                    : randomCrystalPrefab == 0 ? -256f : -202f);

            clonedFlyingCrystal.GetComponent<Rigidbody2D>().velocity = new Vector2(
                randomPosition % 2 == 0 ? 10f : -10f,
                0f);

            StartCoroutine(clonedFlyingCrystal.transform.GetChild(1).gameObject
                .GetComponent<Projectile>().TimeBeforeFalling(randomFlyTime, randomPosition % 2 == 0));

            if (i == 1)
            {
                isAttacking = false;
                timer = 1.8f;
            }

            yield return new WaitForSecondsRealtime(randomSpawnTime);
        }

        yield return new WaitForSecondsRealtime(12f);

        attackStillInProgress[randomMove - 1] = false;
    }

    /// <summary>
    /// Spawn icicles from ceiling by calling method InstantiateIcicles()
    /// Number of icicles depends on the phase
    /// </summary>
    /// <param name="randomMove">Numbered attack</param>
    /// <returns></returns>
    private IEnumerator SpawnIciclesAttack(int randomMove)
    {
        int randomTeleportPositionInAir = UnityEngine.Random.Range(3, 5);
        StartCoroutine(Teleport(randomTeleportPositionInAir));
        yield return new WaitForSecondsRealtime(0.3f);

        animator.SetBool("Falling", true);
        animator.SetTrigger("FallingTrigger");

        int minIcicleNumber = battlePhaseRemaining == 0 ? 75 : 35;
        int maxIcicleNumber = battlePhaseRemaining == 0 ? 90 : 45;
        float spawnIcicleTime = battlePhaseRemaining == 0 ? 0.08f : 0.15f;

        int randomIcicleNumber = UnityEngine.Random.Range(minIcicleNumber, maxIcicleNumber);

        for (int i = 0; i < randomIcicleNumber; i++)
        {
            InstantiateIcicles(randomIcicleNumber, i);
            yield return new WaitForSecondsRealtime(spawnIcicleTime);
        }

        StartCoroutine(SpawnGroundIcicles(randomMove));
    }

    /// <summary>
    /// Methoid for instantiating falling icicles
    /// </summary>
    /// <param name="randomIcicleNumber">Max number of icicles needed to be instantiated during attack</param>
    /// <param name="orderInQueue">Numbered icicle in queue</param>
    private void InstantiateIcicles(int randomIcicleNumber, int orderInQueue)
    {
        float randomIciclePositionX;

        if (orderInQueue == randomIcicleNumber - 1)
        {
            randomIciclePositionX = UnityEngine.Random.Range(iciclePositionMinX + 20, iciclePositionMaxX - 20);
        }
        else
        {
            randomIciclePositionX = UnityEngine.Random.Range(iciclePositionMinX, iciclePositionMaxX);
        }

        GameObject clonedIcicle = Instantiate(
            orderInQueue == randomIcicleNumber - 1 ? icicleStaying : icicle,
            new Vector2(
                randomIciclePositionX,
                iciclePositionY),
            Quaternion.identity);

        if (orderInQueue == randomIcicleNumber - 1)
        {
            transformToMoveTowardsX =
                transform.position.x > clonedIcicle.transform.position.x
                    ? clonedIcicle.transform.position.x + 3
                    : clonedIcicle.transform.position.x - 3;
            transformToMoveTowardsY = 12f;

            try
            {
                Destroy(clonedIcicle, 10f);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.ToString());
            }
        }
    }

    /// <summary>
    /// Spawn icicles from ground.
    /// Attack one icicles that didn't destroy on contact with the floor in later phases.
    /// </summary>
    /// <param name="randomMove">Numbered attack</param>
    /// <returns></returns>
    private IEnumerator SpawnGroundIcicles(int randomMove)
    {
        yield return new WaitForSecondsRealtime(2.5f);

        GameObject clonedIcicleGround = Instantiate(
            icicleGround,
            new Vector2(
                transform.position.x,
                transform.position.y),
            Quaternion.identity);
        yield return new WaitForSecondsRealtime(3f);

        moveTowardsIcicle = shouldMoveTowardsIcicle;
        yield return new WaitForSecondsRealtime(1f);

        Destroy(clonedIcicleGround, 3f);

        if (!shouldMoveTowardsIcicle)
        {
            shouldMoveTowardsIcicle = true;

            // Needs to wait for a little while, otherwise he starts to attack player evens when 
            // ground icicles are still up
            yield return new WaitForSecondsRealtime(2.5f);

            int randomTeleport = UnityEngine.Random.Range(0, 3);
            StartCoroutine(Teleport(randomTeleport));

            animator.SetBool("Falling", false);
            animator.SetTrigger("ReturnToIdle");

            attackStillInProgress[randomMove - 1] = false;
            isAttacking = false;
            timer = 0f;
        }
    }

    /// <summary>
    /// Shoot many ice crystals at player
    /// </summary>
    /// <param name="randomMove"></param>
    /// <param name="player">Player which will be shot</param>
    private IEnumerator ShootCrystalsAtPlayer(int randomMove, int player)
    {
        int randomTeleport = UnityEngine.Random.Range(0, 3);
        StartCoroutine(Teleport(randomTeleport));
        yield return new WaitForSecondsRealtime(1.5f);

        RotateEnemyConditions(heroes.ToArray()[player].transform.position.x);
        bool shootLeft = transform.position.x > heroes.ToArray()[player].transform.position.x;

        animator.SetTrigger("ShootIceCrystalsTrigger");
        yield return new WaitForSecondsRealtime(0.3f);

        for (int i = 0; i < 10; i++)
        {
            SingletonSFX.Instance.PlaySFX("SFX58_shooting_ice");

            GameObject clonedFlyingCrystalProjectile = Instantiate(
                flyingCrystalTurquoise,
                new Vector2(
                    transform.position.x,
                    transform.position.y - 2.25f),
                Quaternion.identity);

            clonedFlyingCrystalProjectile.transform.rotation = Quaternion.Euler(
                0f,
                shootLeft ? 0f : 180f,
                shootLeft ? -22f : -30f);

            clonedFlyingCrystalProjectile.GetComponent<Rigidbody2D>().velocity = new Vector2(
                shootLeft ? -44f : 44f,
                0f);

            clonedFlyingCrystalProjectile.transform.GetChild(1).gameObject.GetComponent<Projectile>().MultipliedVelocityX = -1f;
            yield return new WaitForSecondsRealtime(0.15f);
        }

        yield return new WaitForSecondsRealtime(0.5f);

        animator.SetTrigger("ReturnToIdle");
        attackStillInProgress[randomMove - 1] = false;
        isAttacking = false;
        timer = 0.5f;
    }

    /// <summary>
    /// Freezes one player who has to tap attack button very fast in order to unfreeze himself
    /// </summary>
    /// <param name="randomMove">Numbered attack</param>
    /// <param name="player">Player which will be shot</param>
    /// <returns></returns>
    private IEnumerator FreezePlayer(int randomMove, int player)
    {
        yield return new WaitForSecondsRealtime(0.75f);

        SingletonSFX.Instance.PlaySFX("SFX62_ice_storm");

        animator.SetTrigger("SummonMagic");
        GetComponent<SpriteRenderer>().color = new Color(0.2216981f, 0.4079851f, 1f);

        for (int i = 0; i < 5; i++)
        {
            int randomSnowflakePositionX = UnityEngine.Random.Range(-2, 2);
            int randomSnowflakePositionY = UnityEngine.Random.Range(-1, 4);

            GameObject clonedSnowflake = Instantiate(
                snowflake,
                new Vector2(
                    transform.position.x + randomSnowflakePositionX,
                    transform.position.y + randomSnowflakePositionY),
                Quaternion.identity);

            Destroy(clonedSnowflake, 1f);
        }
        yield return new WaitForSecondsRealtime(1f);

        heroes.ToArray()[player].GetComponent<Paladin>().OnDisable();
        heroes.ToArray()[player].GetComponent<Paladin>().DisableWalkJump();
        heroes.ToArray()[player].GetComponent<Paladin>().Freeze(true);
        heroes.ToArray()[player].GetComponent<Paladin>().OnEnable();
        yield return new WaitForSecondsRealtime(0.5f);

        animator.SetTrigger("ReturnToIdle");
        GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f);
        yield return new WaitForSecondsRealtime(1f);

        attackStillInProgress[randomMove - 1] = false;
        isAttacking = false;
        timer = 0.5f;
    }

    /// <summary>
    /// Attack with winds which players must jump over or roll
    /// </summary>
    /// <param name="randomMove">Numbered attack</param>
    /// <returns></returns>
    private IEnumerator IcyWind(int randomMove)
    {
        int randomTeleportPositionInAir = UnityEngine.Random.Range(3, 5);
        int negativeNumber = randomTeleportPositionInAir == 3 ? -1 : 1;
        StartCoroutine(Teleport(randomTeleportPositionInAir));
        yield return new WaitForSecondsRealtime(1f);

        animator.SetBool("Falling", true);
        animator.SetTrigger("FallingTrigger");
        spriteRenderer.color = new Color(0f, 0f, 1f);
        yield return new WaitForSecondsRealtime(1f);

        spriteRenderer.color = new Color(1f, 1f, 1f);

        int randomFullWall = UnityEngine.Random.Range(0, 7);

        for (int i = 0; i < 7; i++)
        {
            int randomSafeSpot = -2;
            if (!(i == randomFullWall
                || i == randomFullWall + 1))
            {
                randomSafeSpot = UnityEngine.Random.Range(0, 6);
                randomSafeSpot = randomSafeSpot % 2 == 0 ? randomSafeSpot : randomSafeSpot - 1;
            }

            SingletonSFX.Instance.PlaySFX("SFX59_icy_wind");
            InstantiateIceWinds(negativeNumber, randomSafeSpot);

            yield return new WaitForSecondsRealtime(1.5f);
        }

        yield return new WaitForSecondsRealtime(0.5f);

        animator.SetBool("Falling", false);
        animator.SetTrigger("ReturnToIdle");

        int randomTeleport = UnityEngine.Random.Range(0, 3);
        StartCoroutine(Teleport(randomTeleport));

        attackStillInProgress[randomMove - 1] = false;
        isAttacking = false;
        timer = 0f;
    }

    /// <summary>
    /// Method for instantiating a column of ice winds
    /// </summary>
    /// <param name="negativeNumber">Used to define where to spawn and where to move</param>
    /// <param name="randomSafeSpot">Icy wind which will not be spawn, as well as the one after it</param>
    private void InstantiateIceWinds(int negativeNumber, int randomSafeSpot)
    {
        for (int j = 0; j < 6; j++)
        {
            if (j != randomSafeSpot
                && j != randomSafeSpot + 1)
            {
                GameObject clonedIcyWind = Instantiate(
                    icyWind,
                    new Vector2(
                        transform.position.x + 8 * negativeNumber,
                        8.5329f + 2.85f * j),
                    Quaternion.identity);

                clonedIcyWind.transform.rotation = Quaternion.Euler(
                    0f,
                    negativeNumber == -1 ? 180f : 0f,
                    0f);

                clonedIcyWind.GetComponent<Rigidbody2D>().velocity = new Vector2(
                    negativeNumber == -1 ? 33f : -33f,
                    0f);
            }
        }
    }

    /// <summary>
    /// Overlord starts healing after being teleported
    /// </summary>
    /// <returns></returns>
    private IEnumerator Heal()
    {
        StartCoroutine(Teleport(1));
        yield return new WaitForSecondsRealtime(0.90f);

        // Animation TakesHit2 is used as an animation for healing
        animator.SetTrigger("TakesHit2(Healing)");

        GameObject clonedHealingCircles = Instantiate(
            healingCircles,
            new Vector2(
                transform.position.x,
                transform.position.y - 2),
            Quaternion.identity);

        healingCirclesReference = clonedHealingCircles;
        isHealing = true;
    }

    /// <summary>
    /// Stop healing after being enough time passes or if players deal enough damage to Overlord
    /// </summary>
    /// <param name="successful">True if Overlord healed himself, false if players dealt enough damage to him</param>
    private void StopHealing(bool successful)
    {
        isHealing = false;
        currentTimeOfHealing = 0f;

        Destroy(healingCirclesReference);

        if (successful)
        {
            health += 20;
            SingletonSFX.Instance.PlaySFX("SFX64_boss_healed");
            spriteRenderer.color = new Color(1f, 1f, 0f);
            invincible = true;
            Invoke(nameof(RemoveInvincibility), 5f);
        }
        else
        {
            health -= 20;
            animator.SetTrigger("TakesHit1");
        }

        minAttack = 1;
        maxAttack = 7;

        damageWhileHealing = 0;

        // Number 6 represents healing as one of the 7 moves Glacial Overlord has
        attackStillInProgress[6] = false;
        isAttacking = false;
        timer = 0.5f;
    }

    /// <summary>
    /// Calls methods for adding priorities to the players.
    /// If the priorities are the same, random decide which one will be attacked.
    /// Before returning player number which indicates which player will be attacked by Glacial Overlord,
    /// set priorities back to zero.
    /// </summary>
    /// <returns>Player which will be attack</returns>
    private int DecideWhichPlayerToAttack()
    {
        PriorityNearestPlayer();
        PriorityLowestHealthPlayer();
        PriorityMostAggressivePlayer();

        int player = firstPlayerPriority > secondPlayerPriority ? 0 : 1;
        firstPlayerPriority = 0;
        secondPlayerPriority = 0;
        return player;
    }

    /// <summary>
    /// Add priority to player who is the nearest to Glacial Overlord
    /// </summary>
    private void PriorityNearestPlayer()
    {
        float playerOneDistance = Mathf.Abs(transform.position.x - heroes.ToArray()[0].transform.position.x);
        float playerTwoDistance = Mathf.Abs(transform.position.x - heroes.ToArray()[1].transform.position.x);

        if (playerOneDistance < playerTwoDistance)
        {
            firstPlayerPriority++;
        }
        else
        {
            secondPlayerPriority++;
        }
    }

    /// <summary>
    /// Add priority to player who has only one heart.
    /// If they both have, add priority to the first player.
    /// </summary>
    private void PriorityLowestHealthPlayer()
    {
        int playerOneHealth = heroes.ToArray()[0].GetComponent<Paladin>().m_currentNoLives;
        int playerTwoHealth = heroes.ToArray()[1].GetComponent<Paladin>().m_currentNoLives;

        if (playerOneHealth == 1)
        {
            firstPlayerPriority += 2;
        }
        else if (playerTwoHealth == 1)
        {
            secondPlayerPriority += 2;
        }
    }

    /// <summary>
    /// Add priority to player who attacked Glacial Overlord the most.
    /// If they both have, add priority to the second player
    /// </summary>
    private void PriorityMostAggressivePlayer()
    {
        var attackNumbers = transform.GetChild(0).GetComponent<BossHitBoxDamaged>().GetTotalAttackNumbers();

        if (attackNumbers.Item1 > attackNumbers.Item2)
        {
            firstPlayerPriority++;
        }
        else
        {
            secondPlayerPriority++;
        }
    }

    // Frame-rate independent messages for physics calculations
    private void FixedUpdate()
    {
        if (forcedMovement != 0)
        {
            rigidBody2d.velocity = new Vector2(forcedMovement, 0f);
        }
    }

    /// <summary>
    /// Changes velocity on X axis of Overlord based on player's location on X axis
    /// If player's X position is greater than enemy's, then enemy will have a positive speed on X axis and vice versa.
    /// </summary>
    /// <param name="player">Player that Overlord will face</param>
    /// <param name="multiplyNumber">Number to multiply velocity on X axis</param>
    private void ChangeVelocityX(int player, float multiplyNumber = 1)
    {
        if (transform.position.x < heroes.ToArray()[player].transform.position.x)
        {
            rigidBody2d.velocity = new Vector2(velocity * multiplyNumber, rigidBody2d.velocity.y);
        }
        else
        {
            rigidBody2d.velocity = new Vector2(-velocity * multiplyNumber, rigidBody2d.velocity.y);
        }

        RotateEnemyConditions(heroes.ToArray()[player].transform.position.x);
    }

    /// <summary>
    /// Rotates Overlord based on given position of the certain object
    /// </summary>
    /// <param name="objectPositionX">Objects position on X axis</param>
    private void RotateEnemyConditions(float objectPositionX)
    {
        if (transform.position.x >= objectPositionX
            && !lookingLeft)
        {
            lookingLeft = true;
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else if (transform.position.x < objectPositionX
            && lookingLeft)
        {
            lookingLeft = false;
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        }
    }

    /// <summary>
    /// Rotates Overlord
    /// </summary>
    /// <param name="lookLeft">True if overlord needs to look left, false if needs to look right</param>
    private void RotateEnemy(bool lookLeft)
    {
        lookingLeft = lookLeft;
        transform.rotation = Quaternion.Euler(0f, lookingLeft ? 0f : 180f, 0f);
    }

    /// <summary>
    /// Teleport Glacial Overlord to specific position
    /// </summary>
    private IEnumerator Teleport(int teleportPositionNumber)
    {
        InstantiateIce(transform);
        InstantiateIce(teleportPositions[teleportPositionNumber].transform);
        yield return new WaitForSecondsRealtime(0.2f);

        if (teleportPositionNumber == 2
            || teleportPositionNumber == 4)
        {
            RotateEnemy(true);
        }
        else
        {
            RotateEnemy(false);
        }

        transform.position = teleportPositions[teleportPositionNumber].transform.position;
        SingletonSFX.Instance.PlaySFX("SFX60_ice_teleport");
    }

    /// <summary>
    /// Instantiate teleport ice effect
    /// </summary>
    private void InstantiateIce(Transform iceTransform)
    {
        GameObject clonedTeleportIceEffect = Instantiate(
            teleportIceEffect,
            new Vector2(
                iceTransform.position.x,
                iceTransform.position.y - 2),
            Quaternion.identity);

        Destroy(clonedTeleportIceEffect, 0.85f);
    }
}
using UnityEngine;

public class GlacialOverlordSword : MonoBehaviour
{
    /// <summary>
    /// Set the name of arena if player is in front of the door of the arena
    /// </summary>
    /// <param name="collision">Colider from another gameobject</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Sword"))
        {
            SingletonSFX.Instance.PlaySFX("SFX61_sword_clash");

            if (transform.parent.transform.position.x > collision.transform.position.x)
            {
                GetComponent<PolygonCollider2D>().enabled = false;
                GetComponentInParent<GlacialOverlord>().forcedMovement = 35f;
            }
            else
            {
                GetComponent<PolygonCollider2D>().enabled = false;
                GetComponentInParent<GlacialOverlord>().forcedMovement = -35f;
            }

            Invoke(nameof(RemoveForcedMovement), 0.075f);
        }
    }

    /// <summary>
    /// Stop Overlord from moving backwards after paladin successfully defended 
    /// </summary>
    private void RemoveForcedMovement()
    {
        GetComponentInParent<GlacialOverlord>().forcedMovement = 0f;
        GetComponentInParent<Rigidbody2D>().velocity = Vector2.zero;
    }
}
using System.Collections;
using UnityEngine;

public class Icicles : MonoBehaviour
{
    [SerializeField] GameObject iceGenerator;
    [SerializeField] float velocityX;
    [SerializeField] float velocityY;

    private Rigidbody2D rigidbody2d;

    private bool fall;
    private bool wiggled;
    private bool firstPlayerStanding;
    private bool secondPlayerStanding;

    float time;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        rigidbody2d.velocity = new Vector2(0, velocityY);

        wiggled = false;

        firstPlayerStanding = false;
        secondPlayerStanding = false;

        time = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (fall)
        {
            time += Time.deltaTime;
            rigidbody2d.velocity = new Vector2(0f, -time * 20);
        }
    }

    /// <summary>
    /// Start an event after colliding with event collider, overlord's sword or ground
    /// </summary>
    /// <param name="collision">Event collider or ground</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name.Equals("PlayerKnight1"))
        {
            firstPlayerStanding = true;
        }

        if (collision.gameObject.name.Equals("PlayerKnight2"))
        {
            secondPlayerStanding = true;
        }

        if (collision.gameObject.CompareTag("Event")
            && !wiggled)
        {
            wiggled = true;
            StartCoroutine(Wiggle());
        }

        if (transform.CompareTag("Event")
            && collision.gameObject.layer == 6)
        {
            fall = false;
            rigidbody2d.velocity = new Vector2(0f, 0f);
        }

        if (collision.CompareTag("OverlordSword"))
        {
            SingletonSFX.Instance.PlaySFX("SFX55_icicle_destroyed");

            Instantiate(
                iceGenerator,
                new Vector2(
                    transform.position.x,
                    transform.position.y),
                Quaternion.identity);

            Instantiate(
                iceGenerator,
                new Vector2(
                    transform.position.x,
                    transform.position.y),
                Quaternion.identity);

            Destroy(gameObject, 0.05f);
        }

        CollidingWithGround(collision);
    }

    /// <summary>
    /// Do actions for icicles when colliding with ground.
    /// Staying icicles stay for short amount of time on the ground and then get destroyed
    /// or Overlord destroys them.
    /// Other icicles get destroyed with particle effects.
    /// </summary>
    /// <param name="collision"></param>
    private void CollidingWithGround(Collider2D collision)
    {
        if (collision.CompareTag("Ground")
            && transform.name.StartsWith("IcicleStaying"))
        {
            SingletonSFX.Instance.PlaySFX("SFX56_ice_crystal_grounded");
            fall = false;
            transform.tag = "Ground";

            int layerIgnoreRaycast = LayerMask.NameToLayer("Ground");
            gameObject.layer = layerIgnoreRaycast;
        }
        else if (collision.CompareTag("Ground"))
        {
            SingletonSFX.Instance.PlaySFX("SFX55_icicle_destroyed");
            Instantiate(
                iceGenerator,
                new Vector2(
                    transform.position.x,
                    transform.position.y),
                Quaternion.identity);
            Destroy(gameObject, 0.05f);
        }
    }

    /// <summary>
    /// After exiting collision with players, set false to player's standing on staying icicle
    /// </summary>
    /// <param name="collision">Collision with player</param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.name.Equals("PlayerKnight1"))
        {
            firstPlayerStanding = false;
        }

        if (collision.gameObject.name.Equals("PlayerKnight2"))
        {
            secondPlayerStanding = false;
        }
    }

    /// <summary>
    /// Makes icicle wiggle before falling
    /// </summary>
    /// <returns></returns>
    private IEnumerator Wiggle()
    {
        rigidbody2d.gravityScale = 0;
        rigidbody2d.velocity = new Vector2(0f, 0f);
        yield return new WaitForSeconds(0.3f);
        rigidbody2d.velocity = new Vector2(velocityX, 0f);
        yield return new WaitForSeconds(0.2f);
        rigidbody2d.velocity = new Vector2(-velocityX, 0f);
        yield return new WaitForSeconds(0.2f);
        rigidbody2d.velocity = new Vector2(velocityX, 0f);
        yield return new WaitForSeconds(0.2f);
        rigidbody2d.velocity = new Vector2(-velocityX, 0f);
        yield return new WaitForSeconds(0.2f);

        SingletonSFX.Instance.PlaySFX("SFX57_icicle_falling");
        fall = true;
    }

    /// <summary>
    /// Check if players are standing on icicle.
    /// If true, then decrease m_ColCount in SensorHeroKnight.cs.
    /// This is important because of bug that lets players jump indefinitely.
    /// </summary>
    private void OnDestroy()
    {
        if (transform.name.StartsWith("IcicleStaying"))
        {
            if (firstPlayerStanding)
            {
                GameObject.Find("PlayerKnight1").transform.GetChild(5).GetComponent<SensorHeroKnight>().NotStayingOnIcicle(false);
            }

            if (secondPlayerStanding)
            {
                GameObject.Find("PlayerKnight2").transform.GetChild(5).GetComponent<SensorHeroKnight>().NotStayingOnIcicle(false);
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubWorld : MonoBehaviour
{
    [SerializeField] List<GameObject> listDoors;
    [SerializeField] List<GameObject> listClouds;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(IncreaseMusicVolume());

        var numberOfDefeatedBosses = PlayerPrefs.GetInt("numberOfDefeatedBosses");
        var numberOfOpenedDoors = numberOfDefeatedBosses;

        if (numberOfDefeatedBosses >= 5)
        {
            numberOfDefeatedBosses = 4;
        }

        for (int i = 0; i <= numberOfOpenedDoors; i++)
        {
            listDoors.ToArray()[i].GetComponent<SpriteRenderer>().color = Color.white;
            listDoors.ToArray()[i].transform.tag = "Door";
        }

        if (numberOfDefeatedBosses >= 4)
        {
            foreach (var cloud in listClouds)
            {
                cloud.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Increase the general sound of everything in the scene
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerator IncreaseMusicVolume()
    {
        while (AudioListener.volume < 1)
        {
            AudioListener.volume += 0.1f;
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }
}
using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    public new GameObject camera;
    public float parallaxEffect;
    private float startPosition;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position.x;
    }

    // Update is called once per frame
    void Update()
    {
        float distanceInWorld = camera.transform.position.x * parallaxEffect;
        transform.position = new UnityEngine.Vector3(startPosition + distanceInWorld, transform.position.y, -20);
    }
}
using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] GameObject hitBoxGameobject;
    [SerializeField] Transform rotationTo;
    [SerializeField] GameObject particleEffect;

    private Rigidbody2D rigidBody2DParent;
    private Transform transformParent;
    private GameObject playerGameObject;

    private bool parried;
    private bool remainAtShield;
    private float addPositionNumber;

    // For flying ice crystals
    private float timeCount = 0f;
    private bool onTheGround;
    private bool leftSpan = false;

    /// <summary>
    /// Velocity of projectile after being parried on X axis
    /// </summary>
    public float MultipliedVelocityX { get; set; }

    /// <summary>
    /// Velocity of projectile after being parried on Y axis
    /// </summary>
    public float MultipliedVelocityY { get; set; }

    /// <summary>
    /// Used for Glacial Overlord. Set that the crystal should begin to fall and rotate if true
    /// </summary>
    public bool ShouldRotate { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        rigidBody2DParent = GetComponentInParent<Rigidbody2D>();
        transformParent = GetComponentInParent<Transform>();

        parried = false;
        remainAtShield = false;
        onTheGround = false;

        ShouldRotate = false;

        addPositionNumber = 0;
    }

    // Update is called once per frame
    void Update()
    {
        transformParent = GetComponentInParent<Transform>();

        if (transform.parent.name.Contains("Yellow"))
        {
            StarTurnWithPaladin();
            return;
        }

        if (transform.parent.name.Contains("FlyingIce"))
        {
            IceFallRotate();
        }

    }

    /// <summary>
    /// When paladin turns around, so should the star that is parried and in front of him
    /// </summary>
    private void StarTurnWithPaladin()
    {
        if (parried
            && playerGameObject != null
            && remainAtShield)
        {
            if (playerGameObject.transform.parent.GetComponent<SpriteRenderer>().flipX == true)
            {
                transform.parent.position = new Vector3(
                        playerGameObject.transform.parent.position.x - addPositionNumber,
                        playerGameObject.transform.parent.position.y + addPositionNumber - 1,
                        playerGameObject.transform.parent.position.z);
            }
            else if (playerGameObject.transform.parent.GetComponent<SpriteRenderer>().flipX == false)
            {
                transform.parent.position = new Vector3(
                        playerGameObject.transform.parent.position.x + addPositionNumber,
                        playerGameObject.transform.parent.position.y + addPositionNumber - 1,
                        playerGameObject.transform.parent.position.z);
            }
        }
    }

    /// <summary>
    /// Ice falls and rotates after flying for a short period of time
    /// </summary>
    private void IceFallRotate()
    {
        if (!parried && ShouldRotate)
        {
            rotationTo.rotation = Quaternion.Euler(
            0,
            0,
            leftSpan ? 220 : 180);

            transform.parent.rotation = Quaternion.Slerp(
               transform.parent.rotation,
               rotationTo.rotation,
               timeCount / 80);

            rigidBody2DParent.velocity = new Vector2(rigidBody2DParent.velocity.x, -timeCount * 6);
            timeCount += Time.deltaTime;
        }
    }

    /// <summary>
    /// If projectile hits shield, it will not hurt paladin. In that case, it deactivates hitbox,
    /// where hitbox is actually a collider from projectile's main gameobject and adds velocity 
    /// to projectile, resulting in going to opposite direction from initial direction
    /// </summary>
    /// <param name="collision">Collider from another gameobject, usually shield or sword</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Need to refactor immediately
        if (transform.parent.name.Contains("FlyingIce"))
        {
            CheckIceCrystalParry(collision);
            IceTouchGround(collision);
        }
        else
        {
            CheckNormalProjectileParry(collision);
        }

        LaunchStar(collision);
    }

    /// <summary>
    /// Swing sword at parried object to launch it (star from Sidus Istar)
    /// </summary>
    /// <param name="collision">Collider from another gameobject, needs to be a sword</param>
    private void LaunchStar(Collider2D collision)
    {
        if (collision.gameObject.layer == 16
            && parried
            && remainAtShield
            && transform.parent.name.Contains("Yellow"))
        {
            remainAtShield = false;
            SingletonSFX.Instance.PlaySFX("SFX49_launch_star");
            transform.gameObject.layer = 19;

            if (playerGameObject.transform.parent.position.x <= transform.parent.position.x)
            {
                rigidBody2DParent.velocity = new Vector2(100f, 0f);
            }
            else
            {
                rigidBody2DParent.velocity = new Vector2(-100f, 0f);
            }
        }
    }

    /// <summary>
    /// Check if flying crystal ice and shield's rotation are alligned well for parry.
    /// IMPORTANT: This method is similar to the previous one, you must refactor it sooner than later.
    /// </summary>
    /// <param name="collision">Collider from another gameobject - usually shield</param>
    private void CheckIceCrystalParry(Collider2D collision)
    {
        if (((collision.transform.rotation.eulerAngles.y == 180
                    && leftSpan)
                || (collision.transform.rotation.eulerAngles.y == 0
                    && !leftSpan))
            && collision.gameObject.layer == 17
            && parried == false)
        {
            parried = true;
            transform.gameObject.tag = "ParriedObject";
            transform.gameObject.layer = 19;
            hitBoxGameobject.transform.GetComponent<PolygonCollider2D>().enabled = false;

            Instantiate(
                particleEffect,
                new Vector2(
                    transformParent.position.x,
                    transformParent.position.y),
                Quaternion.identity);

            Instantiate(particleEffect, transformParent);
            SingletonSFX.Instance.PlaySFX("SFX30_shield_parry_tink");
            Destroy(transform.parent.gameObject, 0.05f);
        }
    }


    /// If projectile hits shield, it will not hurt paladin. In that case, it deactivates hitbox,
    /// where hitbox is actually a collider from projectile's main gameobject and adds velocity 
    /// to projectile, resulting in going to opposite direction from initial direction
    /// </summary>
    /// <param name="collision">Collider from another gameobject, usually shield or sword</param>
    private void CheckNormalProjectileParry(Collider2D collision)
    {
        if (((collision.transform.rotation.eulerAngles.y == 180
                && transformParent.rotation.eulerAngles.y == 180)
                || (collision.transform.rotation.eulerAngles.y == 0
                && transformParent.rotation.eulerAngles.y == 0))
            && collision.gameObject.layer == 17
            && parried == false)
        {
            parried = true;
            transform.gameObject.tag = "ParriedObject";
            transform.gameObject.layer = 19;
            hitBoxGameobject.transform.GetComponent<PolygonCollider2D>().enabled = false;

            if (transform.parent.name.Contains("Yellow"))
            {
                ChangeStarSettings(collision);
            }
            else if (transform.parent.name.Contains("IceCrystalTurquoise"))
            {
                ChangeIceCrystalSettings();
            }
            else
            {
                ParryOrdinaryFlyingProjectile();
            }
        }
    }

    /// <summary>
    /// Parrying yellow flying star.
    /// Changing it's settings and values.
    /// </summary>
    /// <param name="collision">Collision from shield</param>
    private void ChangeStarSettings(Collider2D collision)
    {
        SingletonSFX.Instance.PlaySFX("SFX48_parried_star");

        transform.gameObject.tag = "ParriedObject";
        transform.gameObject.layer = 0;

        rigidBody2DParent.velocity = new Vector2(0f, 0f);
        playerGameObject = collision.gameObject;
        addPositionNumber = 4;
        remainAtShield = true;
    }

    /// <summary>
    /// Parrying ice crystal projectile.
    /// Changing it's settings and values.
    /// </summary>
    private void ChangeIceCrystalSettings()
    {
        SingletonSFX.Instance.PlaySFX("SFX30_shield_parry_tink");

        rigidBody2DParent.velocity = new Vector2(
            rigidBody2DParent.velocity.x * MultipliedVelocityX,
            MultipliedVelocityY);

        transform.parent.transform.rotation = Quaternion.Euler(
            transform.parent.transform.rotation.x,
            transform.parent.transform.rotation.y,
            transform.parent.transform.rotation.y == 0 ? -30 : -22);
    }

    /// <summary>
    /// Parrying ordinary flying projectile.
    /// Changing it's settings and values.
    /// </summary>
    private void ParryOrdinaryFlyingProjectile()
    {
        SingletonSFX.Instance.PlaySFX("SFX30_shield_parry_tink");

        rigidBody2DParent.velocity = new Vector2(
            rigidBody2DParent.velocity.x * MultipliedVelocityX,
            MultipliedVelocityY);

        transform.parent.transform.rotation = Quaternion.Euler(
            transform.parent.transform.rotation.x,
            transform.parent.transform.rotation.y == 0 ? 180 : 0,
            -105f);
    }

    /// <summary>
    /// When flying ice crystals thrusts the ground, it will remain there for a certain amount of time
    /// </summary>
    /// <param name="collision">Ground</param>
    private void IceTouchGround(Collider2D collision)
    {
        if (collision.gameObject.layer == 6 && !onTheGround)
        {
            // See if onTheGround is necessary
            onTheGround = true;
            ShouldRotate = false;
            GetComponent<PolygonCollider2D>().enabled = false;

            rigidBody2DParent.velocity = Vector2.zero;
            SingletonSFX.Instance.PlaySFX("SFX56_ice_crystal_grounded");

            int randomDestroyTime = Random.Range(2, 4);
            Destroy(transform.parent.gameObject, randomDestroyTime);
        }
    }

    /// <summary>
    /// Time that needs to pass in order for crystal to start falling and rotating
    /// </summary>
    /// <param name="time">Time to pass</param>
    /// <returns></returns>
    public IEnumerator TimeBeforeFalling(float time, bool leftSpawn)
    {
        leftSpan = leftSpawn;
        yield return new WaitForSecondsRealtime(time);

        ShouldRotate = true;
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    /// <summary>
    /// Starts coroutine for loading a new scene
    /// </summary>
    /// <param name="seconds">Seconds to pass before loading new scene</param>
    public void LoadSceneCoroutine(float seconds, string sceneName)
    {
        StartCoroutine(LoadScene(seconds, sceneName));
    }

    /// <summary>
    /// Starts coroutine for loading a new scene
    /// </summary>
    /// <param name="seconds">Seconds to pass before loading new scene</param>
    /// <returns></returns>
    private IEnumerator LoadScene(float seconds, string sceneName)
    {
        yield return new WaitForSecondsRealtime(seconds);
        SceneManager.LoadScene(sceneName);
    }
}
using UnityEngine;

public class SingletonSFX : MonoBehaviour
{
    public static SingletonSFX Instance { get; private set; }

    private AudioClip resourceSFX;
    private AudioSource sourceOfSFX;

    // Called before Start() method
    // Destroy a duplicate instance if another one already exists
    private void Awake()
    {
        if (Instance != null
            && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    // Start is called before the first frame update
    // Gets audio source component
    private void Start()
    {
        sourceOfSFX = GetComponent<AudioSource>();
        sourceOfSFX.loop = false;
    }

    // Plays one time SFX
    public void PlaySFX(string name)
    {
        sourceOfSFX.pitch = 1f;
        sourceOfSFX.volume = 1f;

        resourceSFX = Resources.Load<AudioClip>(name);
        sourceOfSFX.PlayOneShot(resourceSFX);
    }
}
using UnityEngine;

public class TimeCountDown : MonoBehaviour
{
    public bool countTime = false;
    private float timeValue = 0;

    // Update is called once per frame
    void Update()
    {
        if (countTime)
        {
            timeValue += Time.deltaTime;
        }
    }

    /// <summary>
    /// Get measured time from the boss battle which starts when the boss starts attacking
    /// and ends when player beats the boss
    /// </summary>
    /// <returns>Measured time in seconds</returns>
    public float GetTime()
    {
        return timeValue;
    }

    /// <summary>
    /// Get time formated as strings in minutes and seconds
    /// </summary>
    /// <returns>Time in minutes and seconds</returns>
    public static string GetTimeFormated(float timeForFormating)
    {
        float minutes = Mathf.FloorToInt(timeForFormating / 60);
        float seconds = Mathf.FloorToInt(timeForFormating % 60);
        float fraction = Mathf.FloorToInt(timeForFormating * 1000);
        fraction %= 1000;

        string formatedTime = string.Format("{0:00}:{1:00}.{2:000}", minutes, seconds, fraction);
        return formatedTime;
    }
}
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
using UnityEngine;

public class KnightColliderHitbox : MonoBehaviour
{
    /// <summary>
    /// If enemy touches player, player loses one heart.
    /// </summary>
    /// <param name="collision">Collider from enemy gameobject</param>
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Enemy")
            && !transform.parent.gameObject.GetComponent<Paladin>().immortal
            && !transform.parent.gameObject.GetComponent<Paladin>().m_rolling)
        {
            transform.parent.gameObject.GetComponent<Paladin>().DecreaseHealth();
        }
    }
}
using System;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Cinemachine;

public class Paladin : MonoBehaviour
{

    [SerializeField] public Transform m_positionAfterAbyss;
    [SerializeField] public int m_currentNoLives;

    [SerializeField] GameObject m_playerTwo;
    [SerializeField] GameObject m_slideDust;
    [SerializeField] GameObject m_snowflake;
    [SerializeField] GameObject m_playerSpawnEffect;
    [SerializeField] GameObject m_invincibilityStars;
    [SerializeField] float m_speed;
    [SerializeField] float m_jumpForce;
    [SerializeField] float m_rollForce;
    [SerializeField] float m_wallSlidingSpeed;
    [SerializeField] float m_immortalTime;
    [SerializeField] bool m_noBlood = false;

    public Rigidbody2D m_body2d;
    private Animator m_animator;
    private SensorHeroKnight m_groundSensor;
    private SensorHeroKnight m_wallSensorR1;
    private SensorHeroKnight m_wallSensorR2;
    private SensorHeroKnight m_wallSensorL1;
    private SensorHeroKnight m_wallSensorL2;

    public bool m_rolling = false;
    public bool m_grounded = false;
    private bool m_isWallSliding = false;
    private bool m_isBlocking = false;
    private int m_facingDirection = 1;
    private int m_currentAttack = 0;
    private int m_noJumps = 1;
    private float m_timeSinceAttack = 0.0f;
    private float m_delayToIdle = 0.0f;
    private float m_rollDuration = 8.0f / 14.0f;
    private float m_rollCurrentTime;

    public Image[] arrayHP;

    public StaminaUI staminaUI;
    public int maxStamina = 10;
    public int currentStamina;

    public float staminaTime;
    private float currentStaminaTime = 0f;
    public bool immortal;

    public PhysicsMaterial2D material2D;
    public PhysicsMaterial2D material2DNoFriction;

    public float wallForceX;
    public float wallForceY;
    public float buddyForceX;
    public float buddyForceY;
    public float wallTime;

    Vector2 moveInput;

    private float movementDirection = 1;
    private float jumpForceHelp;
    private float rollForceHelp;

    private int noShieldUsage = 0;
    private int noPlayers;

    private bool attacking = false;
    private bool isWallJumping = false;
    private bool staminaMethodCalled = false;
    private bool alwaysJump = false;
    private bool dead = false;
    private bool frozen = false;
    private bool fallenInAbyss = false;

    private string playerNumber = "1";
    private string sceneToEnter = "";

    private readonly int unfreezeNumber = 14;
    private int currentUnfreezeNumber = 0;

    // Method is called when the script is first loaded. Called before Start()
    private void Awake()
    {
        noPlayers = PlayerPrefs.GetInt("noPlayers");

        if (GameObject.Find("PlayerKnight1") == null)
        {
            gameObject.name = "PlayerKnight1";

            if (noPlayers == 2)
            {
                m_playerTwo.SetActive(true);
            }

            if (!SceneManager.GetActiveScene().name.Equals("SidusIstar")
                && !SceneManager.GetActiveScene().name.Equals("GlacialOverlord"))
            {
                GameObject.Find("VirtualCamera").GetComponent<CinemachineVirtualCamera>().Follow = transform;
            }
        }
        else
        {
            gameObject.name = "PlayerKnight2";
            playerNumber = "2";

            CameraScreenSize();
        }

        //Not the most efficient code, it is done with dragging UI Gameobject and with SerializeField 
        Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g =>
        g.name == $"StaminaBarP{playerNumber}").SetActive(true);

        Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g =>
        g.name == $"Hearts_P{playerNumber}").SetActive(true);

        staminaUI = GameObject.Find($"StaminaBarP{playerNumber}").GetComponent<StaminaUI>();
        staminaUI.SetPlayerGameObject(gameObject);

        arrayHP = new Image[] {
            GameObject.Find("Heart_P" + playerNumber + "_1").GetComponent<Image>(),
            GameObject.Find("Heart_P" + playerNumber + "_2").GetComponent<Image>(),
            GameObject.Find("Heart_P" + playerNumber + "_3").GetComponent<Image>(),
            GameObject.Find("Heart_P" + playerNumber + "_4").GetComponent<Image>()
        };

        jumpForceHelp = m_jumpForce;
        rollForceHelp = m_rollForce;

        // Delete this after testing everything
        immortal = true;
        Invoke(nameof(InvincibilityTime), m_immortalTime);
    }

    /// <summary>
    /// Check based on scene if there is a need to make camera screen bigger if multiplayer
    /// </summary>
    private void CameraScreenSize()
    {
        var activeScene = SceneManager.GetActiveScene();
        var sceneName = activeScene.name;

        switch (sceneName)
        {
            case "FernBehemothArena":
                GameObject.Find("VirtualCamera").GetComponent<CinemachineVirtualCamera>().m_Lens.OrthographicSize += 8;
                break;
            case "ChainedUndead":
                GameObject.Find("VirtualCamera").GetComponent<CinemachineVirtualCamera>().m_Lens.OrthographicSize += 10;
                break;
        }
    }

    // Method for getting input from various devices and Player Input component - Jump
    public void JumpInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            try
            {
                JumpAction();
            }
            catch (NullReferenceException Ex)
            {
                Debug.Log(Ex.ToString());
            }
        }
    }

    // Method for getting input from various devices and Player Input component - Shield
    public void ShieldInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            BlockingAction();
        }
        else if (ctx.canceled)
        {
            UnblockingAction();
        }
    }

    // Method for getting input from various devices and Player Input component - Roll
    public void RollInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            RollingAction();
        }
    }

    // Method for getting input from various devices and Player Input component - Attack
    public void AttackInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            AttackAction();
        }
    }

    // Method for getting input from various devices and Player Input component - Movement
    public void MovementInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            moveInput = ctx.ReadValue<Vector2>() * movementDirection;
        }
        else if (ctx.canceled)
        {
            moveInput = Vector2.zero;
        }
    }

    // Method for getting input from various devices and Player Input component - Do actions (enter arena)
    public void ActionInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (!sceneToEnter.Equals("")
                && gameObject.name.Equals("PlayerKnight1"))
            {
                SingletonSFX.Instance.PlaySFX("SFX66_open_door");
                GameObject.Find("BlackScreen").GetComponent<Animator>().SetTrigger("Appear");

                StartCoroutine(LowerMusicVolume());
                StartCoroutine(LoadScene(sceneToEnter));
            }
        }
    }

    // Method for getting input from various devices and Player Input component - Quit scene
    public void QuitInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            GameObject.Find("BlackScreen").GetComponent<Animator>().SetTrigger("Appear");

            string sceneToLoad = "HubWorld";
            var sceneManager = SceneManager.GetActiveScene();

            if (sceneManager.name.Equals("HubWorld"))
            {
                sceneToLoad = "TitleScreen";
            }

            StartCoroutine(LowerMusicVolume());
            StartCoroutine(LoadScene(sceneToLoad));
        }
    }

    // Method for getting input from various devices and Player Input component - Retry
    public void RetryInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            var sceneManager = SceneManager.GetActiveScene();

            if (!sceneManager.name.Equals("HubWorld"))
            {
                GameObject.Find("BlackScreen").GetComponent<Animator>().SetTrigger("Appear");

                StartCoroutine(LowerMusicVolume());
                StartCoroutine(LoadScene(sceneManager.name));
            }
        }
    }

    // Method for getting inputs from jumping, rolling and attacking in order to unfreeze
    public void UnfreezeInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed
            && frozen)
        {
            float moveToUnfreeze = (currentUnfreezeNumber % 2 == 0) ? -0.3f : 0.3f;
            transform.position = new Vector2(
                transform.position.x + moveToUnfreeze,
                transform.position.y);

            currentUnfreezeNumber++;
            if (currentUnfreezeNumber >= unfreezeNumber)
            {
                currentUnfreezeNumber = 0;
                EnableWalkJumpActions();
                Freeze(false);
            }
        }
    }

    /// <summary>
    /// Method used to invert player's movement direction. Used in boss battle Psychic Psycho
    /// </summary>
    public void InvertMovementDirection()
    {
        movementDirection *= -1;
    }

    /// <summary>
    /// Enabling actions if knight is not dead
    /// </summary>
    public void OnEnable()
    {
        if (!dead)
        {
            GetComponent<PlayerInput>().ActivateInput();
        }
        if (!frozen)
        {
            GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        }
    }

    /// <summary>
    /// Disabling actions
    /// <summary>
    public void OnDisable()
    {
        GetComponent<PlayerInput>().DeactivateInput();
    }

    /// <summary>
    /// Enable walking and jumping. Used in Psychic Psycho battle
    /// </summary>
    public void EnableWalkJumpActions()
    {
        movementDirection = 1;
        m_jumpForce = jumpForceHelp;
        m_rollForce = rollForceHelp;
    }

    /// <summary>
    /// Disable walking, jumping and rolling
    /// </summary>
    /// <param name="movementDirection">Movement direction speed</param>
    public void DisableWalkJump(float movementDirection = 0)
    {
        this.movementDirection = movementDirection;
        m_jumpForce = 0;
        m_rollForce = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        //SingletonSFX.Instance.playSFX("SFX17_neo-ridley_scream");

        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<SensorHeroKnight>();
        m_wallSensorR1 = transform.Find("WallSensorR1").GetComponent<SensorHeroKnight>();
        m_wallSensorR2 = transform.Find("WallSensorR2").GetComponent<SensorHeroKnight>();
        m_wallSensorL1 = transform.Find("WallSensorL1").GetComponent<SensorHeroKnight>();
        m_wallSensorL2 = transform.Find("WallSensorL2").GetComponent<SensorHeroKnight>();

        currentStamina = maxStamina;
    }

    // Update is called once per frame
    void Update()
    {
        CheckStamina();

        m_timeSinceAttack += Time.deltaTime;
        RollingBlockingTime();
        SwapSpriteDirection(moveInput.x);
        CheckIfWallSliding(moveInput.x);
    }

    /// <summary>
    /// Method for checking ground for animation and for adding 1 for total number of jumps.
    /// Also used when player is always jumping because of the effect from Psychic Psycho.
    /// </summary>
    private void CheckGround()
    {
        if (m_body2d.velocity.y > 0.5f)
        {
            m_animator.SetBool("Jumping", true);
        }
        else
        {
            m_animator.SetBool("Jumping", false);
        }

        if (m_grounded)
        {
            m_noJumps = 1;

            if (alwaysJump)
            {
                JumpAction();
            }
        }
    }

    /// <summary>
    /// Method for counting time for stamina to be regenerated.
    /// Starts coroutine for stamina regeneration. 
    /// </summary>
    private void CheckStamina()
    {
        if (!m_isBlocking
            && currentStaminaTime <= staminaTime)
        {
            currentStaminaTime += Time.deltaTime;
        }

        if (!staminaMethodCalled
            && currentStamina < 10)
        {
            staminaMethodCalled = true;
            StartCoroutine(StaminaRegeneration());
        }
    }

    /// <summary>
    /// Method for stamina regeneration. Starts after staminaTime while stamina is below 10 units
    /// </summary>
    private IEnumerator StaminaRegeneration()
    {
        while (currentStamina < 10)
        {
            yield return new WaitForSeconds(0.5f);

            if (currentStaminaTime > staminaTime)
            {
                currentStamina += 1;
            }
        }

        staminaMethodCalled = false;
    }

    /// <summary>
    /// Called when hopping on another player
    /// </summary>
    public void BuddyHopping()
    {
        if (!dead
            && !isWallJumping
            && !m_grounded)
        {
            MovementForceOnAction(buddyForceX, buddyForceY);
        }
    }

    /// <summary>
    /// Transparent player colors when invincible
    /// </summary>
    private IEnumerator TransparentColourInvincibility()
    {
        yield return new WaitForSeconds(0.4f);

        for (int i = 0; i < 20; i++)
        {
            if ((i % 2) == 0)
            {
                GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.3f);
            }
            else
            {
                GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
            }
            yield return new WaitForSeconds(0.05f);
        }
    }

    /// <summary>
    /// Freeze or unfreeze player by disabling inputs and changing color to blue
    /// </summary>
    /// <param name="freeze">True if player is frozen, false if not</param>
    public void Freeze(bool freeze)
    {
        frozen = freeze;
        GetComponent<SpriteRenderer>().color = frozen ? new Color(0.2216981f, 0.4079851f, 1f) : new Color(1f, 1f, 1f);
        m_animator.enabled = !frozen;

        for (int i = 0; i < 5; i++)
        {
            int randomSnowflakePositionX = UnityEngine.Random.Range(-2, 2);
            int randomSnowflakePositionY = UnityEngine.Random.Range(-1, 4);

            GameObject clonedSnowflake = Instantiate(
                m_snowflake,
                new Vector2(
                    transform.position.x + randomSnowflakePositionX,
                    transform.position.y + randomSnowflakePositionY),
                Quaternion.identity);

            Destroy(clonedSnowflake, 1f);
        }

        SingletonSFX.Instance.PlaySFX("SFX58_shooting_ice");
    }

    /// <summary>
    /// Method based on jump movement.
    /// Checks if paladin is allowed to jump.
    /// </summary>
    public void JumpAction()
    {
        // Ordinary jump
        if (!m_isBlocking
            && m_noJumps > 0
            && !m_rolling
            && !m_isWallSliding
            && Time.timeScale != 0f)
        {
            SoundAnimationJump();

            m_grounded = false;
            m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
            m_noJumps--;
        }
        // Jump when grounded and near wall
        else if (m_grounded && m_isWallSliding)
        {
            SoundAnimationJump();

            m_grounded = false;
            m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
        }

        // Jump when wall sliding
        if (m_isWallSliding
            && !isWallJumping
            && !m_grounded)
        {
            MovementForceOnAction();
        }
    }

    /// <summary>
    /// Additional method for setting animation for jumping and playing jump SFX
    /// </summary>
    private void SoundAnimationJump()
    {
        if (m_jumpForce != 0)
        {
            SingletonSFX.Instance.PlaySFX("SFX16_jump_player");
        }

        m_animator.SetBool("Grounded", m_grounded);
        m_animator.SetTrigger("Jump");
    }

    /// <summary>
    /// Method that applies force to the player after specific action (jumping from wall, jumping on another player)
    /// </summary>
    /// <param name="forceMethodX">Strength of force X</param>
    /// <param name="forceMethodY">Strength of force Y</param>
    private void MovementForceOnAction(float wallForceMethodX = 0, float wallForceMethodY = 0)
    {
        m_animator.SetBool("WallSlide", false);
        m_animator.SetTrigger("Jump");

        SingletonSFX.Instance.PlaySFX("SFX16_jump_player");

        m_grounded = false;
        m_animator.SetBool("Grounded", m_grounded);

        isWallJumping = true;
        m_animator.SetFloat("AirSpeedY", m_body2d.velocity.y);
        GetComponent<SpriteRenderer>().flipX = !GetComponent<SpriteRenderer>().flipX;

        if (GetComponent<SpriteRenderer>().flipX)
        {
            RotateChildren(180);
        }
        else
        {
            RotateChildren(0);
        }

        // If given wallForceMethodX is empty, it means that paladin must normally jump from the wall,
        // otherwise it means that he is jumping on the shoulder from another player
        if (wallForceMethodX == 0)
        {
            wallForceMethodX = wallForceX;
            wallForceMethodY = wallForceY;
        }

        Invoke(nameof(SetWallJumpingToFalse), wallTime);
        m_body2d.velocity = new Vector2(wallForceMethodX * -m_facingDirection, wallForceMethodY);
    }

    /// <summary>
    /// Method for for counting time for rolling invincibility
    /// </summary>
    private void RollingBlockingTime()
    {
        if (!attacking)
        {
            if (m_rolling)
            {
                m_rollCurrentTime += Time.deltaTime;
            }

            if (m_rollCurrentTime > m_rollDuration)
            {
                m_rolling = false;
                m_rollCurrentTime = 0;
            }
        }
    }

    /// <summary>
    /// Method which starts a blocking (shielding) action
    /// </summary>
    private void BlockingAction()
    {
        // Block - shield
        if (!m_rolling
            && !m_isBlocking
            && currentStamina >= 3
            && !frozen)
        {
            SingletonSFX.Instance.PlaySFX("SFX12_shield_up");
            currentStaminaTime = 0f;
            currentStamina -= 3;
            m_isBlocking = true;

            transform.GetChild(10).GetComponent<BoxCollider2D>().enabled = true;

            m_animator.SetTrigger("Block");
            m_animator.SetBool("IdleBlock", true);
            noShieldUsage++;

            StartCoroutine(ShieldingEnumerator(noShieldUsage));
        }
    }

    /// <summary>
    /// Called when player doesn't hold a button anymore
    /// </summary>
    private void UnblockingAction()
    {
        m_isBlocking = false;
        m_animator.SetBool("IdleBlock", false);
        transform.GetChild(10).GetComponent<BoxCollider2D>().enabled = false;
    }

    /// <summary>
    /// Called when player is rolling
    /// </summary>
    private void RollingAction()
    {
        if (!m_rolling
            && !m_isWallSliding
            && m_grounded
            && currentStamina >= 4
            && !frozen)
        {
            currentStamina -= 4;
            currentStaminaTime = 0f;
            m_rolling = true;

            SingletonSFX.Instance.PlaySFX("SFX10_heavy_roll");
            m_animator.SetTrigger("Roll");
            m_body2d.velocity = new Vector2(m_facingDirection * m_rollForce, m_body2d.velocity.y);
        }
    }

    /// <summary>
    /// Method which disables shielding if characters shields himself for too long
    /// </summary>
    /// <param name="noShieldInMethod">Used for checking if current click on shielding is from the same method</param>
    /// <returns></returns>
    private IEnumerator ShieldingEnumerator(int noShieldInMethod)
    {
        yield return new WaitForSeconds(staminaTime);

        if (noShieldUsage == noShieldInMethod
            && m_isBlocking)
        {
            m_isBlocking = false;
            m_animator.SetBool("IdleBlock", false);
            transform.GetChild(10).GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    /// <summary>
    /// Method for activating attack collider and animation.
    /// There are three attack combos.
    /// Loops back to the first combo after the third combo attack 
    /// or if the measured interval of time since the last attack is too large.
    /// </summary>
    private void AttackAction()
    {
        if (!m_isBlocking
            && m_timeSinceAttack > 0.25f
            && !m_rolling
            && !m_isWallSliding
            && !frozen)
        {
            m_currentAttack++;

            if (m_currentAttack > 3
                || m_timeSinceAttack > 1.0f)
            {
                m_currentAttack = 1;
            }

            m_animator.SetTrigger("Attack" + m_currentAttack);
            SingletonSFX.Instance.PlaySFX("SFX" + (6 + m_currentAttack).ToString() + "_sword_combo_" + m_currentAttack);

            StartCoroutine(CanAttackAgain(m_currentAttack));

            m_timeSinceAttack = 0.0f;
        }
    }

    /// <summary>
    /// Method which sets certain 2D polygon collider active for a short period of time based on attack combo
    /// </summary>
    /// <param name="attackNo">Current number from 3 different attacks in a row</param>
    /// <returns></returns>
    private IEnumerator CanAttackAgain(int attackNo)
    {
        attacking = true;
        float timeForDisable = 0.15f;

        if (attackNo == 3)
        {
            timeForDisable = 0.25f;
        }

        GameObject child = transform.GetChild(5 + attackNo).gameObject;
        yield return new WaitForSeconds(0.1f);

        // Can't attack if hurt animation is playing
        if (!m_animator.GetCurrentAnimatorStateInfo(0).IsName("Hurt"))
        {
            child.SetActive(true);
        }
        yield return new WaitForSeconds(timeForDisable);

        child.SetActive(false);
        attacking = false;
    }

    /// <summary>
    /// Method based on hurt and death animation.
    /// It also does other necessary actions after players death, 
    /// like switching the camera to the other player if he is still alive.
    /// </summary>
    private void HurtDeathAnimation()
    {
        // Death
        if (m_currentNoLives <= 0)
        {
            if (!dead)
            {
                SingletonSFX.Instance.PlaySFX("SFX15_blood_splash");
                SingletonSFX.Instance.PlaySFX("SFX14_player_dead_scream");
                dead = true;

                gameObject.transform.GetChild(11).GetComponent<BoxCollider2D>().transform.gameObject.SetActive(false);
                gameObject.GetComponent<BoxCollider2D>().enabled = false;

                OnDisable();
                ChangeCameraOrScene();
                EnemiesFocusOnAnotherPlayer();

                GameObject.Find($"StaminaBarP{playerNumber}").transform.gameObject.SetActive(false);

                m_animator.SetBool("noBlood", m_noBlood);
                m_animator.SetTrigger("Death");
                m_body2d.velocity = new Vector2(0, 0);
            }
        }
        // Hurt
        else
        {
            SingletonSFX.Instance.PlaySFX("SFX13_player_hurt");
            m_animator.SetTrigger("Hurt");

            if (playerNumber.Equals("2"))
            {
                GameObject clonedInvincibilityStars = Instantiate(
                    m_invincibilityStars,
                    new Vector2(
                        transform.position.x,
                        transform.position.y),
                    Quaternion.identity);

                clonedInvincibilityStars.transform.parent = transform;
                Destroy(clonedInvincibilityStars, m_immortalTime);
            }

            StartCoroutine(TransparentColourInvincibility());
        }
    }

    /// <summary>
    /// Method for changing the camrea to other player if there are more than two players.
    /// Method also checks if both players don't have any more lives, for which the black screen
    /// appears and the game over scene loads
    /// </summary>
    private void ChangeCameraOrScene()
    {
        if (noPlayers == 2)
        {
            var otherPlayerHealth = m_playerTwo.GetComponent<Paladin>().m_currentNoLives;

            if (otherPlayerHealth > 0)
            {
                EnemiesFocusOnAnotherPlayer();

                if (name.Equals("PlayerKnight1")
                    && !SceneManager.GetActiveScene().name.Equals("SidusIstar")
                    && !SceneManager.GetActiveScene().name.Equals("GlacialOverlord"))
                {
                    GameObject.Find("VirtualCamera").GetComponent<CinemachineVirtualCamera>().Follow = m_playerTwo.transform;
                }
            }
            else
            {
                GameObject.Find("BlackScreen").GetComponent<Animator>().SetTrigger("Appear");

                StartCoroutine(LowerMusicVolume());
                StartCoroutine(LoadScene("GameOver"));
            }
        }
        else
        {
            GameObject.Find("BlackScreen").GetComponent<Animator>().SetTrigger("Appear");

            StartCoroutine(LowerMusicVolume());
            StartCoroutine(LoadScene("GameOver"));
        }
    }

    /// <summary>
    /// When this player dies, then tell enemies to focus their attacks on 
    /// another player if he is still alive
    /// </summary>
    private void EnemiesFocusOnAnotherPlayer()
    {
        var otherPlayerHealth = m_playerTwo.GetComponent<Paladin>().m_currentNoLives;

        if (otherPlayerHealth > 0)
        {
            int playerStillAlive;

            if (name.Equals("PlayerKnight1"))
            {
                playerStillAlive = 2;
            }
            else
            {
                playerStillAlive = 1;
            }

            foreach (var boss in FindObjectsOfType<AbstractBoss>())
            {
                try
                {
                    boss.GetComponent<AbstractBoss>().ChangeFocusToAnotherPlayer(playerStillAlive);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            if (SceneManager.GetActiveScene().name.Equals("ChainedUndead"))
            {
                foreach (var enemy in FindObjectsOfType<AbstractEnemy>())
                {
                    try
                    {
                        enemy.GetComponent<AbstractEnemy>().ChangeFocusToAnotherPlayer(playerStillAlive);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }
    }

    /// <summary>
    /// FixedUpdate - useful when dealing with physics
    /// </summary>
    private void FixedUpdate()
    {
        float inputX = moveInput.x;

        m_animator.SetFloat("AirSpeedY", m_body2d.velocity.y);

        CheckGround();

        PutMaterial();
        LandingFallingCheck();

        PaladinMovement(inputX);
        RunIdleAnimation(inputX);
    }

    /// <summary>
    /// Movement based on rigidbody velocity. Also checks if paladin is rolling or blocking
    /// </summary>
    /// <param name="inputX">Input direction from player</param>
    private void PaladinMovement(float inputX)
    {
        // Move
        if (!m_rolling
            && !m_isBlocking
            && !isWallJumping)
        {
            m_body2d.velocity = new Vector2(inputX * m_speed, m_body2d.velocity.y);
        }

        // If blocking, then paladin can't move
        if (m_isBlocking)
        {
            m_body2d.velocity = new Vector2(0, m_body2d.velocity.y);
        }
    }

    /// <summary>
    /// Set m_isWallSliding to true if both wall sensors on one side detect wall set bool for animation.
    /// Change velocity of paladin if player is holding a button towards wall.
    /// </summary>
    /// <param name="inputX">Input direction from player</param>
    private void CheckIfWallSliding(float inputX)
    {
        if ((m_wallSensorR1.State()
                && m_wallSensorR2.State()
                && !gameObject.GetComponent<SpriteRenderer>().flipX)
            || (m_wallSensorL1.State()
                && m_wallSensorL2.State()
                && gameObject.GetComponent<SpriteRenderer>().flipX))
        {
            m_isWallSliding = true;
            m_animator.SetBool("WallSlide", m_isWallSliding);
            AE_SlideDust();
        }
        else
        {
            m_isWallSliding = false;
            m_animator.SetBool("WallSlide", m_isWallSliding);
        }

        if ((m_wallSensorR1.State()
                && m_wallSensorR2.State()
                && inputX > 0)
            || m_wallSensorL1.State()
            && m_wallSensorL2.State()
            && inputX < 0
            && !m_grounded)
        {
            m_noJumps = 0;
            AE_SlideDust();
            m_body2d.velocity = new Vector2(
                m_body2d.velocity.x,
                Mathf.Clamp(
                    m_body2d.velocity.y,
                    -m_wallSlidingSpeed,
                    float.MaxValue));
        }
    }

    /// <summary>
    /// Set wall jumping to false after certain amount of time
    /// </summary>
    private void SetWallJumpingToFalse()
    {
        isWallJumping = false;
    }

    /// <summary>
    /// Run and idle animation conditions and settings
    /// </summary>
    /// <param name="inputX">Input direction from player</param>
    private void RunIdleAnimation(float inputX)
    {
        if (Mathf.Abs(inputX) > Mathf.Epsilon)
        {
            m_delayToIdle = 0.05f;
            m_animator.SetInteger("AnimState", 1);
        }
        else if (!isWallJumping)
        {
            m_delayToIdle -= Time.deltaTime;
            if (m_delayToIdle < 0)
            {
                m_animator.SetInteger("AnimState", 0);
            }
        }
    }

    /// <summary>
    /// Swap sprite direction depending on walk direction - input.GetAxis() (i.e. inputX)
    /// </summary>
    /// <param name="inputX">Input direction from player</param>
    private void SwapSpriteDirection(float inputX)
    {
        if (inputX > 0
            && !m_rolling
            && !isWallJumping)
        {
            GetComponent<SpriteRenderer>().flipX = false;
            m_facingDirection = 1;
            RotateChildren(0);
        }
        else if (inputX < 0
            && !m_rolling
            && !isWallJumping)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            m_facingDirection = -1;
            RotateChildren(180);
        }
    }

    /// <summary>
    /// Checking if knight is falling on the ground based on sensor detection of the ground
    /// </summary>
    private void LandingFallingCheck()
    {
        if (!m_grounded
            && m_groundSensor.State())
        {
            m_grounded = true;
            m_animator.SetBool("Grounded", m_grounded);
        }

        if (m_grounded
            && !m_groundSensor.State())
        {
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
        }
    }

    /// <summary>
    /// Putting proper material on Rigidbody 2D:
    /// <para>-> material2D used for flour</para>
    /// <para>-> material2DNoFriction used for walls</para>
    /// </summary>
    private void PutMaterial()
    {
        if ((m_wallSensorR1.State()
            && m_wallSensorR2.State())
            || (m_wallSensorL1.State()
                && m_wallSensorL2.State())
            || !m_grounded)
        {
            m_body2d.sharedMaterial = material2DNoFriction;
        }
        else
        {
            m_body2d.sharedMaterial = material2D;
        }
    }

    /// <summary>
    /// Takes a certain amount of health from the player
    /// </summary>
    public void DecreaseHealth()
    {
        try
        {
            if (frozen)
            {
                currentUnfreezeNumber = 0;
                EnableWalkJumpActions();
                Freeze(false);
            }

            immortal = true;
            arrayHP[m_currentNoLives - 1].enabled = false;
            m_currentNoLives--;
            HurtDeathAnimation();

            Invoke(nameof(InvincibilityTime), m_immortalTime);
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }

    /// <summary>
    /// Method being invoked for a certain duration, that is, duration of immortality after being hit
    /// </summary>
    private void InvincibilityTime()
    {
        immortal = false;
    }

    /// <summary>
    /// Rotate the child gameobjects in order to face the direction in the same direction as player
    /// </summary>
    /// <param name="number">0 for rotating to the right, 180 for rotating to the left</param>
    private void RotateChildren(int number)
    {
        transform.GetChild(6).gameObject.transform.rotation = Quaternion.Euler(0, number, 0);
        transform.GetChild(7).gameObject.transform.rotation = Quaternion.Euler(0, number, 0);
        transform.GetChild(8).gameObject.transform.rotation = Quaternion.Euler(0, number, 0);
        transform.GetChild(10).gameObject.transform.rotation = Quaternion.Euler(0, number, 0);
    }

    /// <summary>
    /// Force knight to always jump. Used in Psychic Psycho battle
    /// </summary>
    public void AlwaysJumpMode()
    {
        alwaysJump = !alwaysJump;

        if (alwaysJump)
        {
            m_jumpForce *= 1.5f;
            return;
        }

        m_jumpForce = jumpForceHelp;
    }

    /// <summary>
    /// Set the name of arena if player is in front of the door of the arena.
    /// If player is falling into the abyss, then call method for needed actions for it.
    /// </summary>
    /// <param name="collision">Door of the arena</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Door"))
        {
            sceneToEnter = collision.gameObject.name;
        }

        if (collision.transform.CompareTag("Abyss")
            && !fallenInAbyss)
        {
            fallenInAbyss = true;
            StartCoroutine(FallingInAbyss());
        }
    }

    /// <summary>
    /// Delete the name of the arena
    /// </summary>
    /// <param name="collision">Door of the arena</param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Door"))
        {
            sceneToEnter = "";
        }
    }

    /// <summary>
    /// Disable player's input, set velocity to 0, decrease health if not in hubworld
    /// and spawn back at a specific position
    /// </summary>
    /// <returns></returns>
    private IEnumerator FallingInAbyss()
    {
        OnDisable();
        m_body2d.velocity = Vector2.zero;
        yield return new WaitForSecondsRealtime(1f);

        var sceneManager = SceneManager.GetActiveScene();
        if (!sceneManager.name.Equals("HubWorld"))
        {
            immortal = false;
            DecreaseHealth();
        }

        yield return new WaitForSecondsRealtime(1f);

        if (m_positionAfterAbyss != null
            && m_currentNoLives > 0)
        {
            transform.position = new Vector2(m_positionAfterAbyss.position.x, m_positionAfterAbyss.position.y);
            MovementForceOnAction(0.1f, 8f);

            SingletonSFX.Instance.PlaySFX("SFX67_spawn.stars");

            GameObject clonedSpawnEffect = Instantiate(
                m_playerSpawnEffect,
                new Vector2(
                    transform.position.x,
                    transform.position.y),
                Quaternion.identity);

            Destroy(clonedSpawnEffect, 2f);

            OnEnable();
        }

        yield return new WaitForSecondsRealtime(0.5f);
        fallenInAbyss = false;
    }

    /// <summary>
    /// Loads scene after some time passes
    /// </summary>
    /// <param name="sceneToLoad">Scene which will be loaded</param>
    /// <returns></returns>
    private IEnumerator LoadScene(string sceneToLoad)
    {
        yield return new WaitForSecondsRealtime(1.25f);

        SceneManager.LoadScene(sceneToLoad);
    }

    /// <summary>
    /// Lower the general sound of everything in the scene
    /// </summary>
    /// <returns></returns>
    private IEnumerator LowerMusicVolume()
    {
        while (AudioListener.volume > 0)
        {
            AudioListener.volume -= 0.1f;
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    /// <summary>
    /// Animation Events - Called in slide animation
    /// </summary>
    void AE_SlideDust()
    {
        Vector3 spawnPosition;

        if (m_facingDirection == 1)
        {
            spawnPosition = m_wallSensorR2.transform.position;
        }
        else
        {
            spawnPosition = m_wallSensorL2.transform.position;
        }

        if (m_slideDust != null)
        {
            GameObject dust = Instantiate(
                m_slideDust,
                spawnPosition,
                gameObject.transform.localRotation)
            as GameObject;
            dust.transform.localScale = new Vector3(m_facingDirection, 1, 1);
        }
    }
}
using UnityEngine;

public class SensorHeroKnight : MonoBehaviour
{

    private float m_DisableTimer;
    private int m_ColCount = 0;

    /// <summary>
    /// When enabled, call this method
    /// </summary>
    private void OnEnable()
    {
        m_ColCount = 0;
    }

    /// <summary>
    /// Get state from the sensor.
    /// NOTE: Currently, m_DisableTimer is not used anywhere in the project
    /// </summary>
    /// <returns>True if sensor is detecting ground</returns>
    public bool State()
    {
        if (m_DisableTimer > 0)
        {
            return false;
        }
        return m_ColCount > 0;
    }

    /// <summary>
    /// Add number by one if sensor detects collider with tag Ground
    /// </summary>
    /// <param name="other">Collider from another gameobject</param>
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
        {
            m_ColCount++;
        }
    }

    /// <summary>
    /// Decrease number by one if sensor detects collider with tag Ground
    /// </summary>
    /// <param name="other">Collider from another gameobject</param>
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
        {
            m_ColCount--;
        }
    }

    // Update is called once per frame
    void Update()
    {
        m_DisableTimer -= Time.deltaTime;
    }

    /// <summary>
    /// When disabled, call this method
    /// </summary>
    /// <param name="duration">Duration of being disabled</param>
    public void Disable(float duration)
    {
        m_DisableTimer = duration;
    }

    /// <summary>
    /// If paladin was standing on icicle while icicle was destroyed, decrease m_ColCount
    /// </summary>
    /// <param name="shouldCallAgain">True if method should call itself for double check for m_colCount, false if not</param>
    public void NotStayingOnIcicle(bool shouldCallAgain = false)
    {
        m_ColCount = 0;

        if (shouldCallAgain)
        {
            Invoke(nameof(NotStayingOnIcicle), 0.4f);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour
{
    public Slider slider;
    public GameObject heroKnight;
    public Gradient gradient;
    public Image fill;

    // Start of class
    public void Start()
    {
        SetMaxStamina(10);
    }

    /// <summary>
    /// Set this scripts knight gameobject to real knight gameobject
    /// </summary>
    /// <param name="gameObject">Knight gameobject</param>
    public void SetPlayerGameObject(GameObject gameObject)
    {
        heroKnight = gameObject;
    }

    // Checking the stamina of player
    public void Update()
    {
        if (heroKnight.transform.GetComponent<Paladin>().currentStamina <= 10)
        {
            SetStamina(heroKnight.transform.GetComponent<Paladin>().currentStamina);
        }
    }

    // Setting stamina at start
    public void SetMaxStamina(int stamina)
    {
        slider.maxValue = stamina;
        slider.value = stamina;
        fill.color = gradient.Evaluate(10);
    }

    // Set stamina
    public void SetStamina(int stamina)
    {
        slider.value = stamina;
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }
}
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PsychicPsycho : AbstractBoss
{
    [SerializeField] List<GameObject> teleportPositions;
    [SerializeField] GameObject feather;
    [SerializeField] GameObject magicCircle;
    [SerializeField] GameObject twisterConfusion;
    [SerializeField] GameObject springBounciness;
    [SerializeField] GameObject smokeDisappear;
    [SerializeField] GameObject smokeDisappearFeather;
    [SerializeField] GameObject forestBackground;
    [SerializeField] GameObject rocksGenerator;
    [SerializeField] GameObject rainGenerator;
    [SerializeField] GameObject angelFlash;
    [SerializeField] GameObject deathGameobject;
    [SerializeField] GameObject virtualCamera;
    [SerializeField] GameObject sceneManager;
    [SerializeField] float waitTime = 2.0f;

    private SpriteRenderer spriteRenderer;

    private bool[] attackStillInProgress;
    private bool isAttacking;
    private bool invincible;
    private float timer = 0.0f;

    private int noPlayers = 1;
    private int maxHealth;
    private int alreadyUsedAttackMove;
    private int alreadyTeleportedPlaceMain = 1;
    private int minAttack = 1;
    private int maxAttack = 6;
    private bool revertingCameraBack = false;

    // Start is called before the first frame update
    void Start()
    {
        PlayerPrefs.SetString("sceneName", SceneManager.GetActiveScene().name);
        StartCoroutine(IncreaseMusicVolume());

        heroes = new List<GameObject>();

        rigidBody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rigidBody2d.velocity = new Vector2(0f, 0f);

        isAttacking = true;
        invincible = false;

        alreadyUsedAttackMove = 0;

        maxHealth = health;

        // Default bool is false
        attackStillInProgress = new bool[6];
    }

    /// <summary>
    /// Called when player comes near Psychic Psycho. 
    /// Psychic Psycho gets more health if there is a second player.
    /// Psychic Psycho starts summoning magic psychic attacks.
    /// Overrides method from AbstractBoss class.
    /// </summary>
    public override void StartAttacking()
    {
        timerCountDown.GetComponent<TimeCountDown>().countTime = true;

        if (GameObject.Find("PlayerKnight1") != null)
        {
            heroes.Add(GameObject.Find("PlayerKnight1"));
        }

        if (GameObject.Find("PlayerKnight2") != null)
        {
            heroes.Add(GameObject.Find("PlayerKnight2"));
            noPlayers++;
        }

        if (heroes.ToArray().Length == 2)
        {
            health += 50;
        }

        isAttacking = false;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (timer > waitTime
            && !isAttacking)
        {
            BeginAttackingMove();
            PickAttackAction();
        }
    }

    /// <summary>
    /// Decreases Psychic Psycho's health.
    /// Overrides method from AbstractBoss class.
    /// </summary>
    public override void MinusHealth(int layer)
    {
        if (!invincible)
        {
            if (layer.Equals(16))
            {
                health -= 2;
                SingletonSFX.Instance.PlaySFX("SFX15_blood_splash");
            }

            invincible = true;

            if (health <= System.Math.Round((double)maxHealth / 2, 0)
                && !halfway)
            {
                spriteRenderer.color = new Color(1f, 1f, 0f);
                halfway = true;
                minAttack = 6;
            }
            else if (health <= 0)
            {
                Death(true);
            }
            else
            {
                spriteRenderer.color = new Color(0.7830189f, 0.05001981f, 0.01846742f);
                Invoke(nameof(RemoveInvincibility), 0.3f);
            }
        }
    }

    /// <summary>
    /// Calls a virtual method from the abstract class and then uses specialized code for death.
    /// </summary>
    /// <param name="saveToDb">True if time and player name should be saved in database, false if not</param>
    protected override void Death(bool saveToDb)
    {
        base.Death(saveToDb);

        GameObject.Find("Hitbox").SetActive(false);
        SingletonSFX.Instance.PlaySFX("SFX34_death_psychic_psycho");

        spriteRenderer.enabled = false;
        minAttack = 0;
        maxAttack = 0;
        deathGameobject.transform.position = transform.position;
        deathGameobject.SetActive(true);

        sceneManager.GetComponent<SceneLoader>().LoadSceneCoroutine(3f, "HubWorld");

        Destroy(deathGameobject, 3f);
        Destroy(gameObject, 0.05f);
    }

    /// <summary>
    /// Removes invincibility after certain amount of time. 
    /// Called with Invoke() command.
    /// </summary>
    private void RemoveInvincibility()
    {
        spriteRenderer.color = new Color(1f, 1f, 1f);
        invincible = false;
    }

    /// <summary>
    /// Change of the scenery after getting halfway with the max health
    /// </summary>
    /// <returns></returns>
    private IEnumerator ChangeScenery()
    {
        StartCoroutine(RotateCameraBack());

        isAttacking = true;
        alreadyTeleportedPlaceMain = 1;

        SingletonSFX.Instance.PlaySFX("SFX26_teleport");
        InstantiateSmoke();
        transform.position = teleportPositions[1].transform.position;
        InstantiateSmoke();

        animator.SetBool("Fly at", false);
        animator.SetBool("Attack", true);
        yield return new WaitForSecondsRealtime(1f);

        rocksGenerator.SetActive(true);
        yield return new WaitForSecondsRealtime(0.75f);

        angelFlash.SetActive(true);
        yield return new WaitForSecondsRealtime(1f);

        SingletonSFX.Instance.PlaySFX("SFX31_short_boom");
        rainGenerator.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(0.5f);

        SingletonSFX.Instance.PlaySFX("SFX31_short_boom");
        yield return new WaitForSecondsRealtime(0.8f);

        SingletonSFX.Instance.PlaySFX("SFX32_long_boom");
        yield return new WaitForSecondsRealtime(2.5f);

        transform.GetComponent<AudioSource>().enabled = true;
        GameObject.Find("Church").SetActive(false);
        forestBackground.SetActive(true);
        yield return new WaitForSecondsRealtime(2f);

        angelFlash.SetActive(false);
        rocksGenerator.GetComponent<ParticleSystem>().Stop(true);
        NewStats();
    }

    /// <summary>
    /// New boss stats/set to normal after the change of the scenery
    /// </summary>
    private void NewStats()
    {
        minAttack = 1;
        invincible = false;
        spriteRenderer.color = new Color(1f, 1f, 1f);
        StopAttackingMove();
        velocity *= 1.5f;
    }

    /// <summary>
    /// Method used to rotate camera back to normal view
    /// </summary>
    /// <returns></returns>
    private IEnumerator RotateCameraBack()
    {
        if (!revertingCameraBack
            && virtualCamera.GetComponent<CinemachineVirtualCamera>().m_Lens.Dutch != 0)
        {
            revertingCameraBack = true;
            for (int i = 180; i >= 0; i--)
            {
                virtualCamera.GetComponent<CinemachineVirtualCamera>().m_Lens.Dutch = i;
                yield return new WaitForSecondsRealtime(0.01f);
            }
            revertingCameraBack = false;
        }
    }

    /// <summary>
    /// Randomizing numbers to decide which attack should be used next.
    /// If the current chosen attack is the same as the previous one, 
    /// method will call itself (Recursive method).
    /// </summary>
    private void PickAttackAction()
    {
        int randomMove = Random.Range(minAttack, maxAttack);

        if (alreadyUsedAttackMove == randomMove
            || attackStillInProgress[randomMove - 1] == true)
        {
            PickAttackAction();
            return;
        }

        attackStillInProgress[randomMove - 1] = true;

        switch (randomMove)
        {
            case 1:
                StartCoroutine(InvertControlsAttack(randomMove));
                break;
            case 2:
                StartCoroutine(TeleportNearPlayerAttack(randomMove));
                break;
            case 3:
                StartCoroutine(halfway == false ? RotateCameraAttack(randomMove) : TeleportNearPlayerAttack(randomMove));
                break;
            case 4:
                StartCoroutine(ChangeBouncinessAttack(randomMove));
                break;
            case 5:
                StartCoroutine(FeatherAttack(randomMove));
                break;
            case 6:
                StartCoroutine(ChangeScenery());
                break;
        }
        alreadyUsedAttackMove = randomMove;
    }

    /// <summary>
    /// Attack move where Psychic Psycho reverts players' controls
    /// </summary>
    /// <param name="randomMove">Numbered attack</param>
    /// <returns></returns>
    private IEnumerator InvertControlsAttack(int randomMove)
    {
        yield return new WaitForSecondsRealtime(1.5f);

        SummonMagicAttackArea();
        yield return new WaitForSecondsRealtime(1f);

        InvertControls(true);
        yield return new WaitForSecondsRealtime(2f);

        StopAttackingMove();
        Teleport();
        yield return new WaitForSecondsRealtime(6f);

        attackStillInProgress[randomMove - 1] = false;
        InvertControls(false);
    }

    /// <summary>
    /// Invert players' controls by calling public method from Paladin script
    /// </summary>
    private void InvertControls(bool invert)
    {
        string playerName = "";

        foreach (GameObject go in heroes)
        {
            if (!playerName.Equals(go.name))
            {
                playerName = go.name;

                go.GetComponent<Paladin>().OnDisable();
                go.GetComponent<Paladin>().InvertMovementDirection();
                go.GetComponent<Paladin>().OnEnable();

                if (invert)
                {
                    var clonedTwisterConfusion = Instantiate(
                        twisterConfusion, new Vector3(
                            go.transform.position.x,
                            go.transform.position.y + 5,
                            go.transform.position.z),
                        Quaternion.identity);

                    clonedTwisterConfusion.transform.parent = go.transform;
                    SingletonSFX.Instance.PlaySFX("SFX25_twister_confused");

                    Destroy(clonedTwisterConfusion, 8f);
                }
            }
        }
    }

    /// <summary>
    /// Preparing for teleportation fly attack
    /// </summary>
    /// <param name="randomMove">Numbered attack</param>
    /// <returns></returns>
    private IEnumerator TeleportNearPlayerAttack(int randomMove)
    {
        StartCoroutine(TeleportPreperationAnimation());
        yield return new WaitForSecondsRealtime(1.2f);

        animator.SetBool("Fly at", true);
        animator.SetBool("Attack", false);
        yield return new WaitForSecondsRealtime(0.5f);

        InstantiateSmoke();
        SingletonSFX.Instance.PlaySFX("SFX26_teleport");
        StartCoroutine(TeleportAttack(1, randomMove));
    }

    /// <summary>
    /// Method used to let the player know what kind of attack will be used (teleport attack)
    /// </summary>
    /// <returns></returns>
    private IEnumerator TeleportPreperationAnimation()
    {
        for (int i = 0; i < 3; i++)
        {
            SingletonSFX.Instance.PlaySFX("SFX26_teleport");

            GameObject clonedSmokeDisappear = Instantiate(
                      smokeDisappearFeather,
                      new Vector3(
                          transform.position.x - 5 + i * 3,
                          transform.position.y - 3 + i * (i == 2 ? -2 : 2),
                          transform.position.z),
                      Quaternion.identity);

            Destroy(clonedSmokeDisappear, 0.5f);
            yield return new WaitForSecondsRealtime(0.33f);
        }
    }

    /// <summary>
    /// Teleport near random player in front/behind him. 
    /// Sometimes will attack from the above when having half health
    /// </summary>
    /// <param name="spawnPlaceX">Number added to distance number from player</param>
    /// <param name="differentAngleSpawn">Spawn in front(1) or behind(-1) player</param>
    /// <returns></returns>
    private IEnumerator TeleportAttack(int differentAngleSpawn, int randomMove)
    {
        var attackRandomPlayer = Random.Range(0, noPlayers);
        rigidBody2d.velocity = new Vector2(0f, 0f);
        SingletonSFX.Instance.PlaySFX("SFX26_teleport");

        transform.position = new Vector3(
            heroes.ToArray()[attackRandomPlayer].transform.position.x + 20 * differentAngleSpawn,
            heroes.ToArray()[attackRandomPlayer].transform.position.y + 6,
            transform.position.y);

        InstantiateSmoke();
        SingletonSFX.Instance.PlaySFX("SFX26_teleport");
        yield return new WaitForSecondsRealtime(0.3f);

        var attackDirection = heroes.ToArray()[attackRandomPlayer].transform.position.x > transform.position.x ? 1 : -1;
        rigidBody2d.velocity = new Vector2(velocity * attackDirection, 0f);
        yield return new WaitForSecondsRealtime(2f);

        if (differentAngleSpawn == -1)
        {
            rigidBody2d.velocity = new Vector2(0f, 0f);
            StartCoroutine(FlyAtPlayerFromAbove(attackRandomPlayer, randomMove));
        }
        else
        {
            InstantiateSmoke();
            SingletonSFX.Instance.PlaySFX("SFX26_teleport");
            StartCoroutine(TeleportAttack(-1, randomMove));
        }
    }

    /// <summary>
    /// Rotate camera for 180 degrees. Return to normal after specific time.
    /// Psychic Psycho can attack with different move after she turns the camera to 180 degrees.
    /// </summary>
    /// <param name="randomMove">Numbered attack</param>
    /// <returns></returns>
    private IEnumerator RotateCameraAttack(int randomMove)
    {
        isAttacking = true;
        yield return new WaitForSecondsRealtime(1.5f);

        SummonMagicAttackArea();
        yield return new WaitForSecondsRealtime(1f);

        SingletonSFX.Instance.PlaySFX("SFX27_rotate_camera");
        for (int i = 0; i <= 180; i++)
        {
            virtualCamera.GetComponent<CinemachineVirtualCamera>().m_Lens.Dutch = i;
            yield return new WaitForSecondsRealtime(0.01f);
        }

        StopAttackingMove();
        Teleport();
        yield return new WaitForSecondsRealtime(10f);

        StartCoroutine(RotateCameraBack());
        attackStillInProgress[randomMove - 1] = false;
    }

    /// <summary>
    /// Attack move where Psychic Psycho changes players bounciness -> players always jump even when still
    /// </summary>
    /// <param name="randomMove">Numbered attack</param>
    /// <returns></returns>
    private IEnumerator ChangeBouncinessAttack(int randomMove)
    {
        yield return new WaitForSecondsRealtime(1.5f);

        SummonMagicAttackArea();
        yield return new WaitForSecondsRealtime(1f);

        SingletonSFX.Instance.PlaySFX("SFX28_bounciness");
        ChangeBounciness(false);
        yield return new WaitForSecondsRealtime(2f);

        StopAttackingMove();
        Teleport();
        yield return new WaitForSecondsRealtime(4.8f);

        ChangeBounciness(true);
        attackStillInProgress[randomMove - 1] = false;
    }

    /// <summary>
    /// Changes players bounciness by calling a public method in HeroKnight script
    /// </summary>
    /// <param name="setToNormal">Revert back to normal</param>
    private void ChangeBounciness(bool setToNormal)
    {
        string playerName = "";
        foreach (GameObject go in heroes)
        {
            if (!playerName.Equals(go.name))
            {
                playerName = go.name;

                go.GetComponent<Paladin>().AlwaysJumpMode();
                if (!setToNormal)
                {
                    var clonedSpringBounciness = Instantiate(
                        springBounciness, new Vector3(
                            go.transform.position.x,
                            go.transform.position.y + 5,
                            go.transform.position.z),
                        Quaternion.identity);

                    clonedSpringBounciness.transform.parent = go.transform;
                    Destroy(clonedSpringBounciness, 6.8f);
                }
            }
        }
    }

    /// <summary>
    /// Attack which instantiates feathers that attack selected player who must
    /// parry them with shield. At halfway, Psychic Psycho will herself attack
    /// player from above.
    /// </summary>
    /// <returns></returns>
    private IEnumerator FeatherAttack(int randomMove)
    {
        isAttacking = true;
        yield return new WaitForSecondsRealtime(0.8f);

        var attackRandomPlayer = Random.Range(0, noPlayers);
        var numberOfFeathers = Random.Range(0, 2);
        yield return new WaitForSecondsRealtime(0.8f);

        SummonMagicAttackArea();
        InstantiateFeatherPreparationAnimation(numberOfFeathers);

        SingletonSFX.Instance.PlaySFX("SFX29_sharp_feather_attack");
        SingletonSFX.Instance.PlaySFX("SFX29_sharp_feather_attack");
        yield return new WaitForSecondsRealtime(2f);

        SingletonSFX.Instance.PlaySFX("SFX35_stone_heavy");
        heroes.ToArray()[attackRandomPlayer].GetComponent<Paladin>().OnDisable();
        heroes.ToArray()[attackRandomPlayer].GetComponent<Paladin>().DisableWalkJump(0.01f);
        heroes.ToArray()[attackRandomPlayer].GetComponent<Paladin>().OnEnable();
        yield return new WaitForSecondsRealtime(0.5f);

        for (int i = 0; i < 4 + numberOfFeathers; i++)
        {
            InstantiateFeather(attackRandomPlayer);
            yield return new WaitForSecondsRealtime(0.8f);
        }

        SingletonSFX.Instance.PlaySFX("SFX36_not_stone_heavy_relief");
        heroes.ToArray()[attackRandomPlayer].GetComponent<Paladin>().EnableWalkJumpActions();
        yield return new WaitForSecondsRealtime(1f);

        StartCoroutine(FlyAtPlayerFromAbove(attackRandomPlayer, randomMove));
    }

    /// <summary>
    /// Method used to let the player know what kind of attack will be used (feather attack)
    /// </summary>
    /// <param name="numberOfFeathers">Number used for instantiating certain number
    /// of feathers at certain distance</param>
    private void InstantiateFeatherPreparationAnimation(int numberOfFeathers)
    {
        for (int i = 0; i < 4 + numberOfFeathers; i++)
        {
            GameObject clonedFeather = Instantiate(
                  feather,
                  new Vector3(
                      transform.position.x - 8 + 5 * i,
                      transform.position.y + 3,
                      transform.position.z),
                  Quaternion.identity);

            clonedFeather.transform.rotation = Quaternion.Euler(0f, 0f, 135f);

            GameObject clonedSmokeDisappear = Instantiate(
              smokeDisappearFeather,
              new Vector3(
                  clonedFeather.transform.position.x,
                  clonedFeather.transform.position.y,
                  clonedFeather.transform.position.z),
              Quaternion.identity);

            Destroy(clonedSmokeDisappear, 0.5f);

            clonedFeather.GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 40f);
            Destroy(clonedFeather, 1f);
        }
    }

    /// <summary>
    /// Method for instantiating feather and proper visual and sound effects for it
    /// </summary>
    /// <param name="attackRandomPlayer">Player which feather will attack</param>
    private void InstantiateFeather(int attackRandomPlayer)
    {
        var randomDistance = Random.Range(15, 20);
        var addSubtractNumber = Random.Range(0, 2) == 0 ? -1 : 1;

        GameObject clonedFeather = Instantiate(
                  feather,
                  new Vector3(
                      heroes[attackRandomPlayer].transform.position.x + randomDistance * addSubtractNumber,
                      heroes[attackRandomPlayer].transform.position.y + 3,
                      transform.position.z),
                  Quaternion.identity);

        GameObject clonedSmokeDisappear = Instantiate(
              smokeDisappearFeather,
              new Vector3(
                  clonedFeather.transform.position.x,
                  clonedFeather.transform.position.y,
                  clonedFeather.transform.position.z),
              Quaternion.identity);

        Destroy(clonedSmokeDisappear, 0.5f);

        clonedFeather.transform.rotation = Quaternion.Euler(
        0f,
        addSubtractNumber == 1 ? 0f : 180f,
        225f);

        clonedFeather.transform.GetChild(1).gameObject.GetComponent<Projectile>().MultipliedVelocityX = -0.1f;
        clonedFeather.transform.GetChild(1).gameObject.GetComponent<Projectile>().MultipliedVelocityY = -4.5f;
        clonedFeather.GetComponent<Rigidbody2D>().velocity = new Vector2(
            (halfway == false ? 30f : 43f) * addSubtractNumber * (-1),
            0f);

        SingletonSFX.Instance.PlaySFX("SFX29_sharp_feather_attack");
        Destroy(clonedFeather, 20f);
    }

    /// <summary>
    /// Fly attack from above the player. 
    /// Used at halfway.
    /// </summary>
    /// <param name="attackRandomPlayer">Attack certain player</param>
    /// <returns></returns>
    private IEnumerator FlyAtPlayerFromAbove(int attackRandomPlayer, int randomMove)
    {
        if (halfway)
        {
            SingletonSFX.Instance.PlaySFX("SFX26_teleport");
            InstantiateSmoke();

            transform.position = new Vector3(
            heroes.ToArray()[attackRandomPlayer].transform.position.x,
            heroes.ToArray()[attackRandomPlayer].transform.position.y + 35,
            transform.position.y);

            InstantiateSmoke();

            rigidBody2d.velocity = new Vector2(0f, -140f);
        }

        yield return new WaitForSecondsRealtime(1f);

        attackStillInProgress[randomMove - 1] = false;
        StopAttackingMove();
        Teleport();
    }

    /// <summary>
    /// Spawn magic area before at Psychic Psycho before attack. 
    /// Hurts player if he touches it.
    /// </summary>
    private void SummonMagicAttackArea()
    {
        SingletonSFX.Instance.PlaySFX("SFX24_magic_attack");

        GameObject clonedCircle = Instantiate(
                    magicCircle,
                    new Vector3(
                        transform.position.x,
                        transform.position.y,
                        transform.position.z),
                    Quaternion.identity);

        Destroy(clonedCircle, 1f);
    }

    /// <summary>
    /// Begin with attack animation and set gameobject to attack value -> true
    /// </summary>
    private void BeginAttackingMove()
    {
        isAttacking = true;
        animator.SetBool("Attack", true);
    }

    /// <summary>
    /// Stop with the attack animation, 
    /// set timer to 0 in order to start counting on when to attack again
    /// and set gameobject to attack value -> false
    /// </summary>
    private void StopAttackingMove()
    {
        rigidBody2d.velocity = new Vector2(0f, 0f);
        isAttacking = false;
        timer = 0.0f;
        animator.SetBool("Attack", false);
        animator.SetBool("Fly at", false);
    }

    /// <summary>
    /// Teleport Psychic Psycho to random position
    /// </summary>
    private void Teleport()
    {
        var teleportPositionNumber = Random.Range(0, 3);

        if (alreadyTeleportedPlaceMain == teleportPositionNumber)
        {
            Teleport();
            return;
        }

        InstantiateSmoke();
        SingletonSFX.Instance.PlaySFX("SFX26_teleport");
        alreadyTeleportedPlaceMain = teleportPositionNumber;
        transform.position = teleportPositions[teleportPositionNumber].transform.position;
        InstantiateSmoke();
    }

    /// <summary>
    /// Instantiate disappear smoke
    /// </summary>
    private void InstantiateSmoke()
    {
        GameObject clonedSmokeDisappear = Instantiate(
                      smokeDisappear,
                      new Vector3(
                          transform.position.x,
                          transform.position.y),
                      Quaternion.identity);

        Destroy(clonedSmokeDisappear, 0.5f);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallEvents : AbstractBoss
{
    [SerializeField] List<GameObject> ballPositions;
    [SerializeField] GameObject cameraGameObject;
    [SerializeField] GameObject leavesLeft;
    [SerializeField] GameObject leavesRight;
    [SerializeField] GameObject rockBoulder;
    [SerializeField] GameObject rockBoulderSpawnPosition;
    [SerializeField] GameObject rocksGenerator;
    [SerializeField] GameObject blueStar;
    [SerializeField] GameObject darkMatter;
    [SerializeField] float speed;

    private CameraControl cameraControl;
    private Rigidbody2D rigidbody2d;

    public int numberedEvent = 0;
    private int decrementNumberEvent = 0;

    // Start is called before the first frame update
    void Start()
    {
        cameraControl = cameraGameObject.GetComponent<CameraControl>();
        rigidbody2d = GetComponent<Rigidbody2D>();
        heroes = new List<GameObject>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (numberedEvent == 3)
        {
            MoveBallToDifferentPosition(1.5f, 3, 3);
        }

        if (numberedEvent == 6)
        {
            MoveBallToDifferentPosition(1.2f, 4, 4);
        }

        if (numberedEvent == 9)
        {
            MoveBallToDifferentPosition(1f, 4, 5);
        }
    }

    /// <summary>
    /// Moves ball to different position. 
    /// Can be seen in the game.
    /// </summary>
    /// <param name="longDelay">Time value, used for shooting objects</param>
    /// <param name="objectForDelay">Shooted star or dark matter after which the long delay will occur</param>
    /// <param name="ballPosition">To which ball position will the ball move towards to</param>
    private void MoveBallToDifferentPosition(float longDelay, int objectForDelay, int ballPosition)
    {
        // Move balls position a step closer to the target.
        var move = speed * Time.deltaTime; // calculate distance to move
        transform.position = Vector3.MoveTowards(transform.position, ballPositions[ballPosition].transform.position, move);

        if (Vector3.Distance(transform.position, ballPositions[ballPosition].transform.position) < 0.001f)
        {
            AddNumberedEvent();
            rigidbody2d.velocity = Vector2.zero;
            ShootStarsDarkMatter(0.3f, longDelay, objectForDelay, true);
        }
    }

    /// <summary>
    /// Increase numberedEvent by one, which leads to another ball event
    /// </summary>
    public void AddNumberedEvent()
    {
        numberedEvent++;
    }

    /// <summary>
    /// Move the ball up
    /// </summary>
    public void MoveBallEventUp(float velocityY)
    {
        rigidbody2d.velocity = new Vector2(0f, velocityY);
    }

    /// <summary>
    /// Start new event after camera's collider touches ball event or when player touches event colliders
    /// <para>Events:</para>
    /// <para>Case 0: Spawn leaves for players to further progess in the level arena</para>
    /// <para>Case 1: Spawn rock boulders from the right side to roll down at the players</para>
    /// <para>Case 2: Increment numberEvent by one, which in Update() will move the ball to position for shooting stars 
    ///               at the wall</para>
    /// <para>Case 4: Increment numberEvent by one, which will stop the ball from shooting and move to another position</para>
    /// <para>Case 11: Move the ball high in the sky after the last position of shooting at the mountain wall.
    ///                After certain amount of seconds, it will teleport the ball in position of shooting the dark matter, 
    ///                where decreaseNumberEvent is used for correcting the spawn position of array, otherwise it would be 
    ///                index out of array</para>
    /// <para>Case 13: Shoot dark matter balls at the player</para>
    /// <para>Case 14: Ball will go up to the space and will get destroyed</para>
    /// </summary>
    public void StartEvent()
    {
        switch (numberedEvent)
        {
            case 0:
                StartAttacking();
                Invoke(nameof(MovePlatformLeaves), 2f);
                Invoke(nameof(MoveCameraUp), 7f);
                Invoke(nameof(TeleportBallToDifferentPosition), 9f);
                break;
            case 1:
                StartCoroutine(SpawnRockBoulders());
                Invoke(nameof(TeleportBallToDifferentPosition), 6f);
                break;
            case 2:
                Invoke(nameof(AddNumberedEvent), 12f);
                break;
            case 4:
                AddNumberedEvent();
                break;
            case 11:
                MoveBallEventUp(50f);
                decrementNumberEvent = 6;
                Invoke(nameof(TeleportBallToDifferentPosition), 5f);
                break;
            case 13:
                ShootStarsDarkMatter(0.1f, 2f, 30, false);
                break;
            case 14:
                MoveBallEventUp(20f);
                Destroy(gameObject, 1.5f);
                break;
        }
    }

    /// <summary>
    /// Move leaves to the scene in order for player to climb
    /// </summary>
    private void MovePlatformLeaves()
    {
        SingletonSFX.Instance.PlaySFX("SFX54_vines");
        leavesLeft.GetComponent<Animator>().SetTrigger("VinesAnimation");
        leavesRight.GetComponent<Animator>().SetTrigger("VinesAnimation");
    }

    /// <summary>
    /// Slowly move camera up
    /// </summary>
    private void MoveCameraUp()
    {
        cameraControl.MoveCamera(5);
        rigidbody2d.velocity = new Vector2(0f, 30f);
        AddNumberedEvent();
    }

    /// <summary>
    /// Makes the ball destroy part of mountain, causing rock boulders to spawn
    /// </summary>
    /// <returns></returns>
    private IEnumerator SpawnRockBoulders()
    {
        yield return new WaitForSecondsRealtime(3f);

        rigidbody2d.velocity = new Vector2(75f, 12f);
        yield return new WaitForSecondsRealtime(1.5f);

        rocksGenerator.SetActive(true);
        SingletonSFX.Instance.PlaySFX("SFX50_boulder_explosion");
        yield return new WaitForSecondsRealtime(0.5f);

        SingletonSFX.Instance.PlaySFX("SFX51_boulders_rolling");
        AddNumberedEvent();
        for (int i = 0; i < 7; i++)
        {
            var clonedRockBoulder = Instantiate(
                rockBoulder,
                new Vector3(
                    rockBoulderSpawnPosition.transform.position.x,
                    rockBoulderSpawnPosition.transform.position.y,
                    rockBoulderSpawnPosition.transform.position.z),
                Quaternion.identity);

            clonedRockBoulder.GetComponent<Rigidbody2D>().velocity = new Vector2(-20f, 0f);
            yield return new WaitForSecondsRealtime(2f);
        }
    }

    /// <summary>
    /// Shoot blue stars when current event is equal to: 4, 7, 10.
    /// Shoot dark matter balls when current event is equal to: 14.
    /// </summary>
    /// <param name="delay">Delay time before shooting another star/dark matter</param>
    /// <param name="longDelay">Longer delay time before another shooting, players should use that time to further progress</param>
    /// <param name="objectForDelay">After which star or dark matter ball should there be small delay of shooting</param>
    /// <param name="shootBlueStars">Instantiate in coroutine blue stars if true, dark matter balls if false</param>
    private void ShootStarsDarkMatter(float delay, float longDelay, int objectForDelay, bool shootBlueStars)
    {
        StartCoroutine(SpawnBlueStarsDarkMatter(delay, longDelay, objectForDelay, numberedEvent, shootBlueStars));
    }

    /// <summary>
    /// Spawn blue stars when on position to attack player who wall jumps or hops in the clouds (depends on event).
    /// Alternatively, spawn dark matter balls and shoot at the players alternately when they're near event ball's position
    /// </summary>
    /// <param name="delay">Delay time before shooting another star/dark matter</param>
    /// <param name="longDelay">Longer delay time before another shooting, players should use that time to further progress</param>
    /// <param name="objectForDelay">Shooted star or dark matter ball after which the long delay will occur</param>
    /// <param name="currentNumberedEvent">Event for which the stars/dark matter will be shot</param>
    /// <param name="shootBlueStars">Instantiate blue stars if true, dark matter balls if false</param>
    /// <returns></returns>
    private IEnumerator SpawnBlueStarsDarkMatter(float delay, float longDelay, int objectForDelay, int currentNumberedEvent, bool shootBlueStars)
    {
        GameObject objectToShoot = shootBlueStars ? blueStar : darkMatter;
        int attackPlayerNumber = Random.Range(0, heroes.Count);

        for (int i = 0; i < objectForDelay; i++)
        {
            if (currentNumberedEvent == numberedEvent)
            {
                SingletonSFX.Instance.PlaySFX(shootBlueStars ? "SFX52_blue_star" : "SFX55_dark_matter");

                var clonedObjectToShoot = Instantiate(
                    objectToShoot,
                        new Vector3(
                            transform.position.x,
                            transform.position.y,
                            transform.position.z),
                        Quaternion.identity);

                SetVelocityOfShootedObjects(clonedObjectToShoot, shootBlueStars, attackPlayerNumber);
                Destroy(clonedObjectToShoot, 4f);

                if (i == objectForDelay - 1)
                {
                    yield return new WaitForSecondsRealtime(longDelay);
                    i = -1;
                }
                else
                {
                    yield return new WaitForSecondsRealtime(delay);
                }
            }
            else
            {
                Invoke(nameof(AddNumberedEvent), 0.2f);
                break;
            }
        }
    }

    /// <summary>
    /// Set velocity of cloned objects to move at te wall/players
    /// </summary>
    /// <param name="clonedObjectToShoot"></param>
    /// <param name="shootBlueStars"></param>
    private void SetVelocityOfShootedObjects(GameObject clonedObjectToShoot, bool shootBlueStars, int playerNumber)
    {
        if (shootBlueStars)
        {
            clonedObjectToShoot.GetComponent<Rigidbody2D>().velocity = new Vector2(speed, 0f);
        }
        else
        {
            clonedObjectToShoot.GetComponent<DarkMatter>().CalculateExtendedVectorEndingPoint(
                heroes[playerNumber].transform.position,
                40);

            clonedObjectToShoot.GetComponent<DarkMatter>().shouldMove = true;
        }
    }

    /// <summary>
    /// Moves ball event to different position for different event
    /// </summary>
    private void TeleportBallToDifferentPosition()
    {
        rigidbody2d.velocity = new Vector2(0f, 0f);
        transform.position = ballPositions[numberedEvent - decrementNumberEvent].transform.position;
    }

    /// <summary>
    /// Called when players come near
    /// </summary>
    public override void StartAttacking()
    {
        if (GameObject.Find("PlayerKnight1") != null)
        {
            heroes.Add(GameObject.Find("PlayerKnight1"));
        }

        if (GameObject.Find("PlayerKnight2") != null)
        {
            heroes.Add(GameObject.Find("PlayerKnight2"));
        }
    }

    /// <summary>
    /// Does nothing
    /// </summary>
    /// <param name="layer">Layer</param>
    public override void MinusHealth(int layer)
    {
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallForm : AbstractBoss
{
    [SerializeField] List<GameObject> teleportPositions;
    [SerializeField] GameObject star;
    [SerializeField] GameObject ballEvent;
    [SerializeField] GameObject purpleCloudElevator;
    [SerializeField] GameObject trueForm;
    [SerializeField] float addedStarDistanceX;
    [SerializeField] float addedStarDistanceY;
    [SerializeField] float waitTime;

    private SpriteRenderer spriteRenderer;
    private int alreadyUsedAttackMove;
    private int noPlayers = 1;
    private int minAttack = 1;
    private int maxAttack = 4;

    private bool isAttacking;
    private bool invincible;
    private bool moveUpNextPhase;

    private float timer;

    // Start is called before the first frame update
    void Start()
    {
        heroes = new List<GameObject>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        animator = gameObject.GetComponent<Animator>();
        rigidBody2d = GetComponent<Rigidbody2D>();

        alreadyUsedAttackMove = 0;
        timer = 0.0f;

        isAttacking = true;
        invincible = false;
        moveUpNextPhase = false;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (timer > waitTime
            && !isAttacking)
        {
            isAttacking = true;
            PickAttackAction();
        }

        if (moveUpNextPhase)
        {
            transform.GetChild(0).gameObject.GetComponent<Collider2D>().enabled = true;
            rigidBody2d.velocity = new Vector2(0f, 3f + timer * 10);
        }
    }

    /// <summary>
    /// Called when player comes near Sidus Istar's ball form. 
    /// Sidus Istar gets more health if there is a second player.
    /// Overrides method from AbstractBoss class.
    /// </summary>
    public override void StartAttacking()
    {
        trueForm.GetComponent<SidusIstar>().BeginCountingTime();

        if (GameObject.Find("PlayerKnight1") != null)
        {
            heroes.Add(GameObject.Find("PlayerKnight1"));
        }

        if (GameObject.Find("PlayerKnight2") != null)
        {
            heroes.Add(GameObject.Find("PlayerKnight2"));
            noPlayers++;
        }

        if (heroes.ToArray().Length == 2)
        {
            health += 8;
        }

        isAttacking = false;
    }

    /// <summary>
    /// Decreases Sidus Istar's ball form health
    /// Overrides method from AbstractBoss class.
    /// </summary>
    public override void MinusHealth(int layer = 19)
    {
        if (layer == 19
            && !invincible)
        {
            SingletonSFX.Instance.PlaySFX("SFX47_hurt_thump");
            health -= 1;
            invincible = true;

            spriteRenderer.color = new Color(1f, 0.1294118f, 0f);
            Invoke(nameof(RemoveInvincibility), 0.3f);
        }

        if (health <= 0)
        {
            Death(false);
        }
    }

    /// <summary>
    /// Calls a virtual method from the abstract class and then uses specialized code for death.
    /// </summary>
    /// <param name="saveToDb">True if time and player name should be saved in database, false if not</param>
    protected override void Death(bool saveToDb)
    {
        base.Death(saveToDb);

        minAttack = 4;
        maxAttack = 5;
        ballEvent.transform.position = GameObject.Find("BallEventPosition").transform.GetChild(0).transform.position;

        Destroy(gameObject, 10f);
    }

    /// <summary>
    /// Removes invincibility after certain amount of time. Called with Invoke() command
    /// </summary>
    private void RemoveInvincibility()
    {
        spriteRenderer.color = new Color(1f, 1f, 1f);
        invincible = false;
    }

    /// <summary>
    /// Randomizing numbers to decide which attack should be used next.
    /// If the current chosen attack is the same as the previous one, 
    /// method will call itself (Recursive method).
    /// </summary>
    private void PickAttackAction()
    {
        rigidBody2d.velocity = new Vector2(0f, 0f);
        int randomMove = Random.Range(minAttack, maxAttack);

        if (randomMove == 2
            || randomMove == 3)
        {
            int randomMoveDecreasePosibility = Random.Range(0, 2);
            if (randomMoveDecreasePosibility == 1)
            {
                PickAttackAction();
                return;
            }
        }

        if (alreadyUsedAttackMove == randomMove)
        {
            PickAttackAction();
            return;
        }

        switch (randomMove)
        {
            case 1:
                StartCoroutine(ShootStar(randomMove));
                alreadyUsedAttackMove = 0;
                break;
            case 2:
                StartCoroutine(MoveHorizontally(randomMove));
                alreadyUsedAttackMove = randomMove;
                break;
            case 3:
                StartCoroutine(TeleportAround());
                alreadyUsedAttackMove = randomMove;
                break;
            case 4:
                StartCoroutine(Teleport(randomMove));
                break;
        }
    }

    /// <summary>
    /// Teleports around arena.
    /// Now is the time for players to shoot stars at Sidus Istar.
    /// </summary>
    /// <param name="randomMove">Numbered attack</param>
    /// <returns></returns>
    private IEnumerator TeleportAround()
    {
        float timeToWaitAfterTeleport = Random.Range(0.5f, 0.85f);

        StartCoroutine(Teleport());
        yield return new WaitForSecondsRealtime(2f);

        StartCoroutine(WaitAfterTeleport(timeToWaitAfterTeleport));
        yield return new WaitForSecondsRealtime(timeToWaitAfterTeleport);

        timer = 0.0f;
        isAttacking = false;
    }

    /// <summary>
    /// Instantiating star and shooting at player
    /// </summary>
    /// <param name="randomMove">Numbered attack</param>
    /// <returns></returns>
    private IEnumerator ShootStar(int randomMove)
    {
        StartCoroutine(Teleport(randomMove));
        var attackRandomPlayer = (int)Random.Range(0, noPlayers);
        yield return new WaitForSecondsRealtime(1.5f);

        SingletonSFX.Instance.PlaySFX("SFX53_yellow_star");
        GameObject clonedStar = Instantiate(
                  star,
                  new Vector3(
                      transform.position.x + addedStarDistanceX,
                      transform.position.y + addedStarDistanceY,
                      transform.position.z),
                  Quaternion.identity);

        clonedStar.transform.GetChild(1).gameObject.GetComponent<Projectile>().MultipliedVelocityX = -2f;
        clonedStar.transform.GetChild(1).gameObject.GetComponent<Projectile>().MultipliedVelocityY = -1f;

        if (transform.position.x < heroes.ToArray()[attackRandomPlayer].transform.position.x)
        {
            clonedStar.GetComponent<Rigidbody2D>().velocity = new Vector2(60f, 0f);
            clonedStar.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
        else
        {
            clonedStar.GetComponent<Rigidbody2D>().velocity = new Vector2(-60f, 0f);
            clonedStar.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }

        timer = 0.0f;
        isAttacking = false;
    }

    /// <summary>
    /// Attack horizontally
    /// </summary>
    /// <param name="randomMove">Numbered attack</param>
    /// <returns></returns>
    private IEnumerator MoveHorizontally(int randomMove)
    {
        StartCoroutine(Teleport(randomMove));
        var attackRandomPlayer = Random.Range(0, noPlayers);
        yield return new WaitForSecondsRealtime(1f);

        if (transform.position.x < heroes.ToArray()[attackRandomPlayer].transform.position.x)
        {
            rigidBody2d.velocity = new Vector2(110f, 0f);
        }
        else
        {
            rigidBody2d.velocity = new Vector2(-110f, 0f);
        }

        yield return new WaitForSecondsRealtime(1f);

        timer = 0.0f;
        isAttacking = false;
    }

    /// <summary>
    /// Call method for decreasing bosses' health after getting in contact with player's sword or projectile
    /// </summary>
    /// <param name="collision">Collider from another gameobject</param>
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 19)
        {
            Destroy(collision.gameObject);
            MinusHealth();
        }
    }

    /// <summary>
    /// Teleports Sidus Istar to random position. 
    /// Some positions are filtered based on chosen attack move.
    /// </summary>
    private IEnumerator Teleport(int randomMove = 0)
    {
        int minRange = 2;
        int maxRange = 8;

        if (randomMove == 1)
        {
            minRange = 2;
            maxRange = 5;
        }
        else if (randomMove == 2)
        {
            minRange = 0;
            maxRange = 2;
        }
        else if (randomMove == 4)
        {
            minRange = 2;
            maxRange = 3;
        }

        var teleportPositionNumber = Random.Range(minRange, maxRange);
        transform.GetChild(0).gameObject.GetComponent<Collider2D>().enabled = false;

        for (int i = 0; i <= 50; i++)
        {
            spriteRenderer.color = new Color(1f, 1f, 1f, 1f - i * 2 / 100f);
            invincible = true;
            yield return new WaitForSecondsRealtime(0.01f);
        }

        transform.position = teleportPositions[teleportPositionNumber].transform.position;

        for (int i = 0; i <= 50; i++)
        {
            spriteRenderer.color = new Color(1f, 1f, 1f, 0f + i * 2 / 100f);
            yield return new WaitForSecondsRealtime(0.01f);
        }

        if (randomMove == 4)
        {
            moveUpNextPhase = true;
            yield break;
        }

        transform.GetChild(0).gameObject.GetComponent<Collider2D>().enabled = true;
        invincible = false;
    }

    /// <summary>
    /// How long to wait after being teleported
    /// </summary>
    /// <param name="timeToWaitAfterTeleport">Time to wait after teleport</param>
    /// <returns></returns>
    private IEnumerator WaitAfterTeleport(float timeToWaitAfterTeleport)
    {
        yield return new WaitForSecondsRealtime(timeToWaitAfterTeleport);
    }
}
using UnityEngine;

public class BlueDVDStar : MonoBehaviour
{
    private Rigidbody2D rigidbody2d;

    // Called before Start
    private void Awake()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        rigidbody2d.velocity = new Vector2(1f, 1f);
    }

    /// <summary>
    /// Collision of the DVD star with ground, walls and ceiling
    /// </summary>
    /// <param name="collision">Collision from ground, wall or ceiling</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name.StartsWith("ChangeX"))
        {
            ChangeVelocity(directionX: -1f);
        }
        else if (collision.name.StartsWith("ChangeY"))
        {
            ChangeVelocity(directionY: -1f);
        }
    }

    /// <summary>
    /// Change the velocity of the DVD star
    /// </summary>
    /// <param name="directionX">Velocity X</param>
    /// <param name="directionY">Velocity Y</param>
    public void ChangeVelocity(float directionX = 1f, float directionY = 1f)
    {
        SingletonSFX.Instance.PlaySFX("SFX48_parried_star");

        rigidbody2d.velocity = new Vector2(
            rigidbody2d.velocity.x * directionX,
            rigidbody2d.velocity.y * directionY);
    }
}
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField] GameObject ballEventsGameObject;

    private BallEvents eventsScript;
    private Rigidbody2D rigidbody2d;

    // Delete this after testing
    public bool cameraShouldMoveUp;

    // Start is called before the first frame update
    void Start()
    {
        eventsScript = ballEventsGameObject.GetComponent<BallEvents>();
        rigidbody2d = GetComponent<Rigidbody2D>();
        rigidbody2d.velocity = Vector2.zero;
    }

    // Update is called once per frame
    private void Update()
    {
        // Delete all of this after testing
        if (cameraShouldMoveUp)
        {
            MoveCamera(5);
            cameraShouldMoveUp = false;
        }
    }

    /// <summary>
    /// Start  an event after detecting a ball event or event collider
    /// </summary>
    /// <param name="collision">Collider from ball event of event collider</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name.Equals("BallEvent"))
        {
            if (eventsScript.numberedEvent != 13)
            {
                eventsScript.StartEvent();
            }
        }
        else if (collision.gameObject.name.Equals("FifthCollider"))
        {
            // Will not add numbered event since the same attack would perform at the center of screen,
            // therefore there is no need for it
            Destroy(collision.gameObject);
            eventsScript.MoveBallEventUp(5f);
        }
        else if (collision.gameObject.name.Equals("SixthCollider"))
        {
            Destroy(collision.gameObject);
            eventsScript.AddNumberedEvent();
            eventsScript.StartEvent();
        }
        else if (collision.gameObject.name.Equals("PurpleClouds"))
        {
            MoveCamera(0f);
        }
    }

    /// <summary>
    /// Move camera up
    /// </summary>
    /// <param name="velocityY">Velocity on Y axis</param>
    public void MoveCamera(float velocityY)
    {
        rigidbody2d.velocity = new Vector2(0f, velocityY);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarkMatter : MonoBehaviour
{
    [SerializeField] float speed;

    Vector2 targetPosition;

    public bool shouldMove = false;
    private float positionSpeed = 0;

    // Update is called once per frame
    void Update()
    {
        if (shouldMove)
        {
            positionSpeed += speed * Time.deltaTime;
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, positionSpeed);
        }
    }

    /// <summary>
    /// Calculates the ending point for extending vector which has starting point at ball event's position
    /// and ending point at player's position
    /// <para>Formula for point x (same as for y):</para>
    /// <para>Cx = Ax + kBx = kBx + (1 - k)Ax</para>
    /// </summary>
    /// <param name="playerPosition">Ending point of original Vector</param>
    public void CalculateExtendedVectorEndingPoint(Vector2 playerPosition, int k)
    {
        float Cx = k * playerPosition.x + (1 - k) * transform.position.x;
        float Cy = k * playerPosition.y + (1 - k) * transform.position.y;

        targetPosition = new Vector2(Cx, Cy);
        shouldMove = true;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event : MonoBehaviour
{
    [SerializeField] GameObject ballEventsGameObject;

    private BallEvents eventsScript;

    // Start is called before the first frame update
    void Start()
    {
        eventsScript = ballEventsGameObject.GetComponent<BallEvents>();
    }

    /// <summary>
    /// When detecting player or certain collider, start an event 
    /// </summary>
    /// <param name="collision">Collider from player or event collider</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")
            && !gameObject.name.Equals("FourthCollider"))
        {
            StartBallEvent();
        }
        else if (gameObject.name.Equals("FourthCollider")
            && collision.CompareTag("MainCamera"))
        {
            StartBallEvent();
        }
    }

    /// <summary>
    /// Adds number to eventNumber at ball event gameobject and starts that event
    /// </summary>
    private void StartBallEvent()
    {
        eventsScript.AddNumberedEvent();
        eventsScript.StartEvent();
        Destroy(gameObject);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    /// <summary>
    /// Speed of the cloud on X axis.
    /// If negative, then it will move to the left.
    /// </summary>
    public float SpeedX { get; set; }

    private Rigidbody2D rigidbody2d;
    public float friction = 0;

    // Called before Start
    private void Awake()
    {
        SpeedX = 0;
        rigidbody2d = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Move cloud nad set it's friction.
    /// Refactor method in order to stay with SOLID principles
    /// </summary>
    /// <param name="friction">Stickiness of the ground</param>
    public void MoveCloud(float friction)
    {
        PhysicsMaterial2D material = new();
        material.friction = friction;

        rigidbody2d.sharedMaterial = material;
        rigidbody2d.velocity = new Vector2(SpeedX, 0f);

        this.friction = rigidbody2d.sharedMaterial.friction;
    }
}
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
        for (int i = 0; i < timers.Count; i++)
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
using UnityEngine;

public class PurpleCloud : MonoBehaviour
{
    [SerializeField] GameObject cameraUp;
    [SerializeField] float velocityY;
    [SerializeField] float lifeTime;

    public bool secondPlayerStanding;
    private bool firstPlayerStanding;
    private bool movingUp;

    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        movingUp = false;
        firstPlayerStanding = false;
        secondPlayerStanding = false;

        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// When player one (and two if playing multiplayer) stands on cloud, it moves up.
    /// If collision is from space, then lower the gravity scale to players and make cloud disappear.
    /// Note: cloud will udpdate the speed of the camera, which will be the same as the cloud's
    /// </summary>
    /// <param name="collision">Collision with another gameobject</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name.Equals("PlayerKnight1"))
        {
            firstPlayerStanding = true;
        }

        if (collision.gameObject.name.Equals("PlayerKnight2"))
        {
            secondPlayerStanding = true;
        }

        if (!movingUp
            && (firstPlayerStanding
                || secondPlayerStanding))
        {
            movingUp = true;
            cameraUp.GetComponent<CameraControl>().MoveCamera(velocityY);
            GetComponent<Rigidbody2D>().velocity = Vector2.up * velocityY;
            Invoke(nameof(InSpace), lifeTime);
        }
    }

    /// <summary>
    /// Both players must be standing on the cloud if playing multiplayer
    /// </summary>
    /// <param name="collision">Collision with another gameobject</param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.name.Equals("PlayerKnight1"))
        {
            firstPlayerStanding = false;
        }

        if (collision.gameObject.name.Equals("PlayerKnight2"))
        {
            secondPlayerStanding = false;
        }
    }

    /// <summary>
    /// Purple clouds arrival to space.
    /// Method changes the player's gravity
    /// </summary>
    private void InSpace()
    {
        GetComponent<EdgeCollider2D>().enabled = false;
        animator.SetTrigger("Disappear");

        if (GameObject.Find("PlayerKnight1") != null)
        {
            GameObject.Find("PlayerKnight1").GetComponent<Paladin>().m_body2d.gravityScale = 1;
        }

        if (GameObject.Find("PlayerKnight2") != null)
        {
            GameObject.Find("PlayerKnight2").GetComponent<Paladin>().m_body2d.gravityScale = 1;
        }

        Destroy(gameObject, 1f);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SidusIstar : AbstractBoss
{
    [SerializeField] List<Transform> boltPositions;
    [SerializeField] List<Transform> boltLightPositions;
    [SerializeField] Transform spaceArenaMiddlePosition;
    [SerializeField] Transform upperLeftPosition;
    [SerializeField] Transform lowerRightPosition;
    [SerializeField] GameObject wizardTransformation;
    [SerializeField] GameObject sceneManager;
    [SerializeField] GameObject blueDVDStar;
    [SerializeField] GameObject arm;
    [SerializeField] GameObject bolt;
    [SerializeField] GameObject longBolt;
    [SerializeField] GameObject longBoltLight;
    [SerializeField] float velocityDVDStar;
    [SerializeField] float velocityBoltDown;
    [SerializeField] float velocityLongBoltHorizontal;

    private SpriteRenderer spriteRenderer;

    private bool isMoving;
    private bool isAttacking;
    private bool invincible;
    private bool lookingLeft;
    private bool dead;
    private bool shootLongBolt;

    private float newPositionX;
    private float newPositionY;
    private float timer;
    private float waitTime;

    private int attackNo;

    // Start is called before the first frame update
    void Start()
    {
        PlayerPrefs.SetString("sceneName", SceneManager.GetActiveScene().name);
        StartCoroutine(IncreaseMusicVolume());

        heroes = new List<GameObject>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidBody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        dead = false;
        isMoving = false;
        isAttacking = true;
        invincible = false;
        lookingLeft = true;
        shootLongBolt = false;

        newPositionX = 0f;
        newPositionY = 0f;

        timer = 0f;
        waitTime = 2f;

        attackNo = 1;
    }

    /// <summary>
    /// Begin counting time.
    /// Used here since the stop counting time method will be also called in SidusIstar.cs.
    /// </summary>
    public void BeginCountingTime()
    {
        timerCountDown.GetComponent<TimeCountDown>().countTime = true;
    }

    /// <summary>
    /// Decreases Sidus Istar's health.
    /// Overrides method from AbstractBoss class.
    /// </summary>
    public override void MinusHealth(int layer)
    {
        if (layer.Equals(16))
        {
            if (!invincible)
            {
                health -= 2;
                invincible = true;
                SingletonSFX.Instance.PlaySFX("SFX15_blood_splash");
                spriteRenderer.color = new Color(0.7830189f, 0.05001981f, 0.01846742f);
                arm.GetComponent<SpriteRenderer>().color = new Color(0.7830189f, 0.05001981f, 0.01846742f);
                Invoke(nameof(RemoveInvincibility), 0.3f);
            }

            if (health <= 0)
            {
                dead = true;
                Death(true);
            }
        }
    }

    /// <summary>
    /// Removes invincibility after certain amount of time. 
    /// Called with Invoke() command.
    /// </summary>
    private void RemoveInvincibility()
    {
        spriteRenderer.color = new Color(1f, 1f, 1f);
        arm.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f);
        invincible = false;
    }

    /// <summary>
    /// Calls a virtual method from the abstract class and then uses specialized code for death.
    /// </summary>
    /// <param name="saveToDb">True if time and player name should be saved in database, false if not</param>
    protected override void Death(bool saveToDb)
    {
        base.Death(saveToDb);

        animator.SetTrigger("Death");

        sceneManager.GetComponent<SceneLoader>().LoadSceneCoroutine(5f, "HubWorld");
        Destroy(gameObject, 3f);
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > waitTime
            && !isAttacking)
        {
            isAttacking = true;
            if (!dead)
            {
                MoveToPosition();
            }
            else
            {
                rigidBody2d.velocity = Vector2.zero;
            }
        }
        else if (!isAttacking)
        {
            timer += Time.deltaTime;
        }
    }

    // Frame-rate independent method for physics calculations
    public void FixedUpdate()
    {
        if (isMoving)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                new Vector2(
                    newPositionX,
                    newPositionY),
                    velocity);

            if (Vector2.Distance(transform.position, new Vector2(newPositionX, newPositionY)) < 0.5f)
            {
                isMoving = false;
                animator.SetBool("GoingLeft", false);
                animator.SetBool("FlyingRight", false);

                int randomIdleAnimation = Random.Range(0, 2);

                if (randomIdleAnimation == 0)
                {
                    animator.SetBool("Cloaked", true);
                    animator.SetTrigger("CloakedTrigger");
                }
                else
                {
                    animator.SetBool("Sideways", true);
                    animator.SetTrigger("SidewaysTrigger");
                }

                StartCoroutine(PickAttackAction());
            }
        }
    }

    /// <summary>
    /// Move Sidus Istar to another position.
    /// The right animation must be played
    /// </summary>
    /// <returns></returns>
    private void MoveToPosition()
    {
        newPositionX = Random.Range(upperLeftPosition.position.x, lowerRightPosition.position.x);
        newPositionY = Random.Range(lowerRightPosition.position.y, upperLeftPosition.position.y);
        isMoving = true;

        if (transform.position.x > newPositionX)
        {
            animator.SetBool("GoingLeft", true);
            animator.SetTrigger("GoingLeftTrigger");
            RotateEnemyLeft(true);
        }
        else
        {
            animator.SetBool("FlyingRight", true);
            animator.SetTrigger("FlyingRightTrigger");
            RotateEnemyLeft(true);
        }
    }

    /// <summary>
    /// Called when player comes near Sidus Istar. 
    /// Overrides method from AbstractBoss class.
    /// </summary>
    public override void StartAttacking()
    {
        if (GameObject.Find("PlayerKnight1") != null)
        {
            heroes.Add(GameObject.Find("PlayerKnight1"));
        }

        if (GameObject.Find("PlayerKnight2") != null)
        {
            heroes.Add(GameObject.Find("PlayerKnight2"));
        }

        if (heroes.ToArray().Length == 2)
        {
            health += 124;
        }

        wizardTransformation.SetActive(true);

        Invoke(nameof(AppearOnTheBattlefield), 10f);
    }

    /// <summary>
    /// Make Sidus Istar appear on the battlefield
    /// </summary>
    private void AppearOnTheBattlefield()
    {
        Destroy(wizardTransformation);
        animator.SetTrigger("Appear");
        transform.position = spaceArenaMiddlePosition.position;

        SingletonSFX.Instance.PlaySFX("SFX72_sidus_istar_appear");
        isAttacking = false;
    }

    /// <summary>
    /// Going from one attack to another
    /// Developer note: currently there are only two types of attack moves, there will be more in the future
    /// </summary>
    private IEnumerator PickAttackAction()
    {
        if (transform.position.x > spaceArenaMiddlePosition.position.x)
        {
            RotateEnemyLeft(true);
        }
        else
        {
            RotateEnemyLeft(false);
        }

        yield return new WaitForSecondsRealtime(1.5f);

        if (!dead)
        {
            switch (attackNo)
            {
                case 1:
                    StartCoroutine(StarDVDLogoAttack());
                    break;
                case 2:
                    StartCoroutine(BoltAttacks());
                    break;
            }
        }
    }

    /// <summary>
    /// Shots big blue star which changes it's flying direction everytime it hits a wall, ground or ceiling,
    /// like a DVD logo on old DVD players.
    /// Stars will be always shoot towards ground
    /// </summary>
    private IEnumerator StarDVDLogoAttack()
    {
        animator.SetBool("AttackingLeftNoArm", true);
        animator.SetTrigger("AttackLeftNoArmTrigger");
        arm.SetActive(true);
        yield return new WaitForSecondsRealtime(0.5f);

        SingletonSFX.Instance.PlaySFX("SFX68_launching_DVD_stars");

        for (int i = 0; i < 2; i++)
        {
            GameObject clonedBlueDVDStar = Instantiate(
                blueDVDStar,
                new Vector2(
                    arm.transform.position.x,
                    arm.transform.position.y),
                Quaternion.identity);

            clonedBlueDVDStar.GetComponent<BlueDVDStar>().ChangeVelocity(
                lookingLeft ? -velocityDVDStar - i * 5 : velocityDVDStar + i * 5,
                i == 0 ? velocityDVDStar - i * 5 : -velocityDVDStar + i * 5);

            Destroy(clonedBlueDVDStar, 20f);
        }

        timer = 0f;
        isAttacking = false;

        animator.SetBool("AttackingLeftNoArm", false);
        arm.SetActive(false);

        attackNo = 2;
    }

    /// <summary>
    /// Method for attacking with bolts.
    /// </summary>
    private IEnumerator BoltAttacks()
    {
        if (shootLongBolt)
        {
            // Shoot vertical meteor bolts
            for (int i = 0; i < 4; i++)
            {
                yield return new WaitForSecondsRealtime(1f);
                int addPositionX = Random.Range(-3, 3);
                InstantiateBolts(bolt, 2, 5, addPositionX, 0f, 0f, velocityBoltDown);
            }
        }
        else
        {
            // Shoot horizontal long bolts
            int addPositionY = Random.Range(-3, 3);
            InstantiateBolts(longBolt, 0, 2, 0f, addPositionY, velocityLongBoltHorizontal, 0f);
        }

        shootLongBolt = !shootLongBolt;

        timer = -2f;
        isAttacking = false;

        attackNo = 1;
    }

    /// <summary>
    /// Method for instantiating bolts
    /// </summary>
    /// <param name="boltToClone">Gameobject bolt to clone</param>
    /// <param name="minRandom">Minimal random number for range</param>
    /// <param name="maxRandom">Maximal random number for range</param>
    /// <param name="addPositionX">Add position of gameobject on X axis</param>
    /// <param name="addPositionY">Add position of gameobject on Y axis</param>
    /// <param name="velocityX">Velocity X of bolt</param>
    /// <param name="velocityY">Velocity Y of bolt</param>
    private void InstantiateBolts(
        GameObject boltToClone,
        int minRandom,
        int maxRandom,
        float addPositionX,
        float addPositionY,
        float velocityX,
        float velocityY)
    {
        int randomBoltPosition = Random.Range(minRandom, maxRandom);

        Transform boltPosition = boltPositions.ToArray()[randomBoltPosition];
        GameObject clonedBolt = Instantiate(
            boltToClone,
            new Vector2(
                boltPosition.position.x + addPositionX,
                boltPosition.position.y + addPositionY),
            Quaternion.identity);

        if (randomBoltPosition == 0)
        {
            SingletonSFX.Instance.PlaySFX("SFX69_long_meteor_bolt");
            InstantiateBallOfLight(randomBoltPosition, clonedBolt.transform.position.y);

        }
        else if (randomBoltPosition == 1)
        {
            clonedBolt.transform.rotation = Quaternion.Euler(
                0f,
                180f,
                0f);

            velocityX *= -1f;
            SingletonSFX.Instance.PlaySFX("SFX69_long_meteor_bolt");
            InstantiateBallOfLight(randomBoltPosition, clonedBolt.transform.position.y);
        }
        else
        {
            clonedBolt.transform.rotation = Quaternion.Euler(
               0f,
               0f,
               90f);

            SingletonSFX.Instance.PlaySFX("SFX70_meteor_bolt");
        }

        clonedBolt.GetComponent<Rigidbody2D>().velocity = new Vector2(velocityX, velocityY);
        Destroy(clonedBolt, 8f);
    }

    /// <summary>
    /// Instantiate ball of light effect so that players will know the spawn position of the
    /// long bolt
    /// </summary>
    /// <param name="longBoltListPosition">Order in list of spawn positions</param>
    /// <param name="longBoltPositionY">Spawn position on Y axis</param>
    private void InstantiateBallOfLight(int longBoltListPosition, float longBoltPositionY)
    {
        GameObject clonedBolt = Instantiate(
        longBoltLight,
            new Vector2(
                boltLightPositions[longBoltListPosition].position.x,
                longBoltPositionY),
            Quaternion.identity);

        Destroy(clonedBolt, 1f);
    }

    /// <summary>
    /// Rotates Sidus Istar
    /// </summary>
    /// <param name="lookLeft">True if Istar needs to look left, false if needs to look right</param>
    private void RotateEnemyLeft(bool lookLeft)
    {
        lookingLeft = lookLeft;
        transform.rotation = Quaternion.Euler(0f, lookingLeft ? 0f : 180f, 0f);
    }
}

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
            foreach (var hero in heroes)
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour
{
    [SerializeField] TMP_InputField playerNameInp;
    [SerializeField] Button submitPlayerNameBtn;

    [SerializeField] Button startTheGameSngBtn;
    [SerializeField] Button startTheGameMltBtn;
    [SerializeField] Button viewHighScoresBtn;
    [SerializeField] Button deleteUserBtn;
    [SerializeField] Button quitGameBtn;

    [SerializeField] Button leftArrowBtn;
    [SerializeField] Button rightArrowBtn;
    [SerializeField] Button closeHighScores;

    [SerializeField] GameObject playerNameBox;
    [SerializeField] GameObject mainMenuButtons;
    [SerializeField] GameObject highScoreBox;

    [SerializeField] Text bossNameTxt;
    [SerializeField] Text playerNamesTxt;
    [SerializeField] Text fastestTimesTxt;

    private readonly List<string> bossNamesList = new();
    private int highScoreBoss;
    private bool startingTheGame;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(IncreaseMusicVolume());
        startingTheGame = false;

        bossNamesList.InsertRange(bossNamesList.Count, new string[]
        {
            "FernBehemoth",
            "PsychicPsycho",
            "ChainedUndead",
            "SidusIstar",
            "GlacialOverlord"
        });

        highScoreBoss = 0;
        playerNameInp.text = PlayerPrefs.GetString("playerName");

        if (playerNameInp.text.Length != 0)
        {
            ClosePlayerNameBox();
        }
    }

    /// <summary>
    /// Saves player's name in PlayerPrefs
    /// </summary>
    public void SubmitPlayerName()
    {
        if (playerNameInp.text.Length == 0)
        {
            playerNameInp.text = "You must enter your name";
        }
        else
        {
            PlayerPrefs.SetInt("numberOfDefeatedBosses", 0);
            ClosePlayerNameBox();
        }
    }

    /// <summary>
    /// Closes box for filling player's name
    /// </summary>
    private void ClosePlayerNameBox()
    {
        bossNameTxt.text = "Fastest players who have beaten the boss: Fern Behemoth";
        playerNameBox.SetActive(false);
        mainMenuButtons.SetActive(true);
    }

    /// <summary>
    /// Start the game with only one player
    /// </summary>
    public void StartTheGameSingleplayer()
    {
        if (!startingTheGame)
        {
            startingTheGame = true;
            PlayerPrefs.SetInt("noPlayers", 1);

            PrepareTheGame();
        }
    }

    /// <summary>
    /// Start the game with two players
    /// </summary>
    public void StartTheGameMultiplayer()
    {
        if (!startingTheGame)
        {
            startingTheGame = true;
            PlayerPrefs.SetInt("noPlayers", 2);

            PrepareTheGame();
        }
    }

    /// <summary>
    /// Start the game by making black screen appear, lower the general audio volume and call 
    /// the method to load the hub world scene
    /// </summary>
    private void PrepareTheGame()
    {
        GameObject.Find("BlackScreenSprite").GetComponent<Animator>().SetTrigger("Appear");

        SingletonSFX.Instance.PlaySFX("SFX6_big_thing_fly_sky");

        StartCoroutine(LowerMusicVolume());
        StartCoroutine(LoadHubWorld());
    }

    /// <summary>
    /// Load hub world scene
    /// </summary>
    /// <returns></returns>
    private IEnumerator LoadHubWorld()
    {
        yield return new WaitForSecondsRealtime(2f);

        SceneManager.LoadScene("HubWorld");
    }

    /// <summary>
    /// Shows high score board
    /// </summary>
    public void ShowHighScoreBoard()
    {
        highScoreBoss = -1;
        mainMenuButtons.SetActive(false);
        highScoreBox.SetActive(true);
        ShowRightHighScores();
    }

    /// <summary>
    /// Shows high scores of certain boss after clicking left arrow
    /// </summary>
    public void ShowLeftHighScores()
    {
        playerNamesTxt.text = "";
        fastestTimesTxt.text = "";

        EmptyFillHighScoreBoard(0);

        ChangeBossOrder(false);

        // Get only alphabet characters
        string bossName = Regex.Replace(bossNamesList[highScoreBoss], "([a-z])([A-Z])", "$1 $2");
        bossNameTxt.text = $"Fastest players who have beaten the boss: {bossName}";
        PlayerData.GetDataFromDatabase(bossNamesList[highScoreBoss], this);
    }

    /// <summary>
    /// Shows high scores of certain boss after clicking right arrow
    /// </summary>
    public void ShowRightHighScores()
    {
        playerNamesTxt.text = "";
        fastestTimesTxt.text = "";

        EmptyFillHighScoreBoard(0);

        ChangeBossOrder(true);
        string bossName = Regex.Replace(bossNamesList[highScoreBoss], "([a-z])([A-Z])", "$1 $2");
        bossNameTxt.text = $"Fastest players who have beaten the boss: {bossName}";
        PlayerData.GetDataFromDatabase(bossNamesList[highScoreBoss], this);
    }

    /// <summary>
    /// Changes boss order in high score board
    /// </summary>
    /// <param name="right">True if right arrow was clicked</param>
    private void ChangeBossOrder(bool right)
    {
        if (right)
        {
            highScoreBoss = highScoreBoss == 4 ? 0 : highScoreBoss + 1;
        }
        else
        {
            highScoreBoss = highScoreBoss == 0 ? 4 : highScoreBoss - 1;
        }
    }

    /// <summary>
    /// Fills high score boards with player names and fastest times
    /// </summary>
    /// <param name="playerList">List of players and fastest times</param>
    public void FillHighScoreBoard(IOrderedEnumerable<Player> playerList)
    {
        int noPlayersInTop = playerList.Count() >= 10 ? 10 : playerList.Count();

        playerNamesTxt.text = "";
        fastestTimesTxt.text = "";

        for (int i = 0; i < noPlayersInTop; i++)
        {
            playerNamesTxt.text += $"{i + 1}. " + playerList.ToArray()[i].playerName + "\n";
            fastestTimesTxt.text += TimeCountDown.GetTimeFormated(playerList.ToArray()[i].finishTime) + "\n";
        }

        EmptyFillHighScoreBoard(noPlayersInTop);
    }

    /// <summary>
    /// Fills boards with zeros and hyphens if there are less than 10 players saved in DB
    /// </summary>
    /// <param name="noPlayersInTop"></param>
    private void EmptyFillHighScoreBoard(int noPlayersInTop)
    {
        for (int i = noPlayersInTop; i < 10; i++)
        {
            playerNamesTxt.text += $"{i + 1}. ---\n";
            fastestTimesTxt.text += "00:00.000\n";
        }
    }

    /// <summary>
    /// Close high score board
    /// </summary>
    public void CloseHighScores()
    {
        mainMenuButtons.SetActive(true);
        highScoreBox.SetActive(false);
    }

    /// <summary>
    /// Deletes user data on local computer
    /// </summary>
    public void DeleteUser()
    {
        playerNameInp.text = "";
        DeletePlayerPrefsData();
        mainMenuButtons.SetActive(false);
        playerNameBox.SetActive(true);
    }

    /// <summary>
    /// Deletes PlayerPrefs data
    /// </summary>
    private void DeletePlayerPrefsData()
    {
        PlayerPrefs.DeleteAll();
    }

    /// <summary>
    /// Quits game
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }

    /// <summary>
    /// Saves player's name as string in PlayerPrefs
    /// </summary>
    private void OnDisable()
    {
        PlayerPrefs.SetString("playerName", playerNameInp.text);
    }

    /// <summary>
    /// Increase the general sound of everything in the scene
    /// </summary>
    /// <returns></returns>
    private IEnumerator IncreaseMusicVolume()
    {
        while (AudioListener.volume < 1)
        {
            AudioListener.volume += 0.1f;
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    /// <summary>
    /// Lower the general sound of everything in the scene
    /// </summary>
    /// <returns></returns>
    private IEnumerator LowerMusicVolume()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        while (AudioListener.volume > 0)
        {
            AudioListener.volume -= 0.1f;
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }
}*/
