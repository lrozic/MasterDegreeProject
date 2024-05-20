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
        }
    }

    // Fixed update
    private void FixedUpdate()
    {
        if (isCharging
            && isAttacking
            && !destroyed)
        {
            speed = 1.2f;
            transform.position = Vector3.MoveTowards(
                transform.position,
                lastPlayerPosition,
                speed);
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
        lastPlayerPosition = heroes.ToArray()[attackRandomPlayer].transform.position;
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
