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
                iceTransform.position.y -2),
            Quaternion.identity);

        Destroy(clonedTeleportIceEffect, 0.85f);
    }
}
