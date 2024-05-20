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
