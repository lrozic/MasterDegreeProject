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
        if(!destroyed
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
