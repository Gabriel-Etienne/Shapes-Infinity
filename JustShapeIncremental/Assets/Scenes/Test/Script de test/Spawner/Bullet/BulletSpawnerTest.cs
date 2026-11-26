
using UnityEngine;

public class BulletSpawnerTest : MonoBehaviour
{
    [Header("Pool")]
    [SerializeField] private BulletPoolTest bulletPool;

    [Header("Spawn Area")]
    [SerializeField] private float radius = 5f;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnFrequency = 1f;

    [Header("Bullet Quantity Range")]
    [SerializeField] private int minBulletQuantity = 3;
    [SerializeField] private int maxBulletQuantity = 8;

    [Header("Bullet Scale Range")]
    [SerializeField] private float minBulletScale = 0.5f;
    [SerializeField] private float maxBulletScale = 1.5f;

    [Header("Bullet Speed Range")]
    [SerializeField] private float minBulletSpeed = 3f;
    [SerializeField] private float maxBulletSpeed = 7f;

    [Header("Bullet Direction")]
    [SerializeField] private float directionVariation = 20f;

    [Header("Gizmos")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color gizmoColor = Color.green;

    private float spawnTimer;

    private void Update()
    {
        if (bulletPool == null || spawnFrequency <= 0f)
            return;

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnFrequency)
        {
            spawnTimer -= spawnFrequency;
            SpawnBullets();
        }
    }

    private void SpawnBullets()
    {
        int quantity = Random.Range(
            minBulletQuantity,
            maxBulletQuantity + 1
        );

        for (int i = 0; i < quantity; i++)
        {
            // Position sur le contour du cercle
            Vector2 outwardDirection =
                Random.insideUnitCircle.normalized;

            Vector2 spawnPosition =
                (Vector2)transform.position + outwardDirection * radius;

            // Direction vers le centre avec variation
            Vector2 inwardDirection = -outwardDirection;

            float randomAngle = Random.Range(
                -directionVariation,
                directionVariation
            );

            Vector2 finalDirection =
                Quaternion.Euler(0f, 0f, randomAngle) * inwardDirection;

            // Récupération d'une bullet dans le pool
            GameObject bullet = bulletPool.GetBullet();

            bullet.transform.position = spawnPosition;
            bullet.transform.rotation = Quaternion.identity;

            // Taille aléatoire
            float randomScale = Random.Range(
                minBulletScale,
                maxBulletScale
            );

            bullet.transform.localScale = Vector3.one * randomScale;

            // Vitesse aléatoire
            float randomSpeed = Random.Range(
                minBulletSpeed,
                maxBulletSpeed
            );

            // Initialisation du déplacement
            BulletMovementTest movement =
                bullet.GetComponent<BulletMovementTest>();

            if (movement != null)
            {
                movement.Initialize(
                    finalDirection,
                    transform.position,
                    radius,
                    bulletPool,
                    randomSpeed
                );
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos)
            return;

        Gizmos.color = gizmoColor;

        const int segments = 64;

        Vector3 previousPoint = transform.position +
            new Vector3(radius, 0f, 0f);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;

            Vector3 currentPoint = transform.position +
                new Vector3(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius,
                    0f
                );

            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }
    }
}
