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
    private bool beginFirstMove;

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
        beginFirstMove = false;

        initialColor = spriteRenderer.color;

        transform.parent = GameObject.Find("ToxicFlies").transform;

        FindHeroes();
        ChooseHeroToAttack();

        Invoke(nameof(StartAttacking), 1f);
    }

    /// <summary>
    /// Overriden method to start attack after the flight from background
    /// </summary>
    protected override void StartAttacking()
    {
        canAttackAgain = true;
        beginFirstMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!destroyed 
            && heroes.ToArray().Length != 0)
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

            if (!isAttacking
                && beginFirstMove)
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
            catch(Exception ex)
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
