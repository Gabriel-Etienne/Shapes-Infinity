using UnityEngine;

public class ZoneSpawnerTest : MonoBehaviour
{
    [Header("Pool")]
    [SerializeField] private ZonePoolTest zonePool;

    [Header("Camera")]
    [SerializeField] private CameraShakeTest cameraShake;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnFrequency = 2f;

    [Header("Spawn Area")]
    [SerializeField] private Vector2 spawnAreaSize = new Vector2(10f, 5f);

    [Header("Gizmos")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color gizmoColor = Color.yellow;

    private float spawnTimer;

    private void Update()
    {
        if (zonePool == null || spawnFrequency <= 0f)
            return;

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnFrequency)
        {
            spawnTimer -= spawnFrequency;
            SpawnZone();
        }
    }

    private void SpawnZone()
    {
        Vector2 randomOffset = new Vector2(
            Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f),
            Random.Range(-spawnAreaSize.y / 2f, spawnAreaSize.y / 2f)
        );

        Vector3 spawnPosition = transform.position +
                                new Vector3(randomOffset.x, randomOffset.y, 0f);

        ZoneVisualTest zone = zonePool.GetZone(spawnPosition);

        if (zone != null)
        {
            zone.Initialize(zonePool, cameraShake);
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos)
            return;

        Gizmos.color = gizmoColor;

        Gizmos.DrawWireCube(
            transform.position,
            new Vector3(spawnAreaSize.x, spawnAreaSize.y, 0f)
        );
    }
}