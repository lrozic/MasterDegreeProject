using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    /// <summary>
    /// Speed of the cloud on X axis.
    /// If negative, then it will move to the left.
    /// </summary>
    public float SpeedX { get; set; }

    private Rigidbody2D rigidbody2d;
    public float friction = 0;

    // Called before Start
    private void Awake()
    {
        SpeedX = 0;
        rigidbody2d = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Move cloud nad set it's friction.
    /// Refactor method in order to stay with SOLID principles
    /// </summary>
    /// <param name="friction">Stickiness of the ground</param>
    public void MoveCloud(float friction)
    {
        PhysicsMaterial2D material = new();
        material.friction = friction;

        rigidbody2d.sharedMaterial = material;
        rigidbody2d.velocity = new Vector2(SpeedX, 0f);

        this.friction = rigidbody2d.sharedMaterial.friction;
    }
}
