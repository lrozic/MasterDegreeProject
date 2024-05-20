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
    [SerializeField] GameObject rockSolid;
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
    private void NewStats() {
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

        var clonedRockSolid = Instantiate(
                        rockSolid, new Vector3(
                            heroes.ToArray()[attackRandomPlayer].transform.position.x,
                            heroes.ToArray()[attackRandomPlayer].transform.position.y + 5,
                            heroes.ToArray()[attackRandomPlayer].transform.position.z),
                        Quaternion.identity);

        clonedRockSolid.transform.parent = heroes.ToArray()[attackRandomPlayer].transform;

        yield return new WaitForSecondsRealtime(0.5f);

        for (int i = 0; i < 4 + numberOfFeathers; i++)
        {
            InstantiateFeather(attackRandomPlayer);
            yield return new WaitForSecondsRealtime(0.8f);
        }

        Destroy(clonedRockSolid);
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
