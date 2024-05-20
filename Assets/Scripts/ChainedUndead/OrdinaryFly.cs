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
    public override void Death() {
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
