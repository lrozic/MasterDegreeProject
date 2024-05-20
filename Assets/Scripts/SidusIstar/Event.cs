using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event : MonoBehaviour
{
    [SerializeField] GameObject ballEventsGameObject;

    private BallEvents eventsScript;

    // Start is called before the first frame update
    void Start()
    {
        eventsScript = ballEventsGameObject.GetComponent<BallEvents>();
    }

    /// <summary>
    /// When detecting player or certain collider, start an event 
    /// </summary>
    /// <param name="collision">Collider from player or event collider</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") 
            && !gameObject.name.Equals("FourthCollider"))
        {
            StartBallEvent();
        } 
        else if (gameObject.name.Equals("FourthCollider") 
            && collision.CompareTag("MainCamera"))
        {
            StartBallEvent();
        }
    }

    /// <summary>
    /// Adds number to eventNumber at ball event gameobject and starts that event
    /// </summary>
    private void StartBallEvent()
    {
        eventsScript.AddNumberedEvent();
        eventsScript.StartEvent();
        Destroy(gameObject);
    }
}
