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
