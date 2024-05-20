using System.Collections;
using UnityEngine;

public class Maggot : MonoBehaviour
{
    private Rigidbody2D rigidbody2d;
    private Animator animator;

    private bool isMoving;
    private float time;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        time = 0f;
        isMoving = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isMoving)
        {
            time += Time.deltaTime;
            rigidbody2d.velocity = new Vector2(0f, time * -15f);
        }
    }

    /// <summary>
    /// When maggot touches ground, it starts to move
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 6
            && !isMoving)
        {
            isMoving = true;
            StartCoroutine(StartMovingThenBurrow());
        }
    }

    /// <summary>
    /// Maggot starts moving after random time and then it burrows itself after some time
    /// </summary>
    private IEnumerator StartMovingThenBurrow()
    {
        rigidbody2d.velocity = new Vector2(0f, 0f);

        int randomDirectionMove = Random.Range(0, 2);
        transform.rotation = Quaternion.Euler(
            0f,
            randomDirectionMove == 0 ? 180f : 0f,
            0f);

        int randomWaitTime = Random.Range(1, 4);
        yield return new WaitForSecondsRealtime(randomWaitTime);

        animator.SetTrigger("Move");
        rigidbody2d.velocity = new Vector2(
            randomDirectionMove == 0 ? -1f : 1f,
            0f);

        int randomTimeMoving = Random.Range(12, 15);
        Destroy(gameObject, randomTimeMoving);
    }
}
