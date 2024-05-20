using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarkMatter : MonoBehaviour
{
    [SerializeField] float speed;

    Vector2 targetPosition;

    public bool shouldMove = false;
    private float positionSpeed = 0;

    // Fixed update used for physics calculation
    private void FixedUpdate()
    {
        if (shouldMove)
        {
            positionSpeed += speed * Time.fixedDeltaTime;
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, positionSpeed);
        }
    }

    /// <summary>
    /// Calculates the ending point for extending vector which has starting point at ball event's position
    /// and ending point at player's position
    /// <para>Formula for point x (same as for y):</para>
    /// <para>Cx = Ax + kBx = kBx + (1 - k)Ax</para>
    /// </summary>
    /// <param name="playerPosition">Ending point of original Vector</param>
    public void CalculateExtendedVectorEndingPoint(Vector2 playerPosition, int k)
    {
        float Cx = k * playerPosition.x + (1 - k) * transform.position.x;
        float Cy = k * playerPosition.y + (1 - k) * transform.position.y;

        targetPosition = new Vector2(Cx, Cy);
        shouldMove = true;
    }
}
