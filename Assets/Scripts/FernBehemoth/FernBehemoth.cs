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
            if (health <= 0) {
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
