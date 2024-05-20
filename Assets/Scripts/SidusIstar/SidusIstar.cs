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
        if (isMoving
            && !dead)
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

