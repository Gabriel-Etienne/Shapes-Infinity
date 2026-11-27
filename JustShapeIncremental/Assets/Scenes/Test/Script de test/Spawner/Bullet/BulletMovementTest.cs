using System;
using UnityEngine;

public class BulletMovementTest : MonoBehaviour , IPoolable
{
    private Vector2 direction;
    private Vector2 center;

    private float despawnRadius;
    private float moveSpeed;

    private BulletPoolTest bulletPool;

    private bool isInitialized;
    private bool hasEnteredCircle;

    public void Initialize(
        Vector2 movementDirection,
        Vector2 circleCenter,
        float circleRadius,
        BulletPoolTest pool,
        float speed)
    {
        direction = movementDirection.normalized;
        center = circleCenter;

        despawnRadius = circleRadius;
        bulletPool = pool;
        moveSpeed = speed;

        hasEnteredCircle = false;
        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized)
            return;

        // Déplacement
        transform.position +=
            (Vector3)(moveSpeed * Time.deltaTime * direction);

        // Distance par rapport au centre
        float distance = Vector2.Distance(
            transform.position,
            center
        );

        // La bullet entre dans le cercle
        if (distance < despawnRadius)
        {
            hasEnteredCircle = true;
        }

        // La bullet ressort du cercle
        if (hasEnteredCircle && distance > despawnRadius)
        {
            ReturnToPool();
        }
    }

    public void ReturnToPool()
    {
        isInitialized = false;
        hasEnteredCircle = false;

        bulletPool.ReturnBullet(gameObject);
    }
}