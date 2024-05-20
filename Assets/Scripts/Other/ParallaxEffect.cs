using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    public new GameObject camera;
    public float parallaxEffect;
    private float startPosition;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position.x;
    }

    // Update is called once per frame
    void Update()
    {
        float distanceInWorld = camera.transform.position.x * parallaxEffect;
        transform.position = new UnityEngine.Vector3(startPosition + distanceInWorld, transform.position.y, -20);  
    }
}
