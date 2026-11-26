using System.Collections.Generic;
using UnityEngine;

public class ZonePoolTest : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private ZoneVisualTest zonePrefab;
    [SerializeField] private int initialPoolSize = 20;

    private Queue<ZoneVisualTest> availableZones =
        new Queue<ZoneVisualTest>();

    private void Awake()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateZone();
        }
    }

    private ZoneVisualTest CreateZone()
    {
        ZoneVisualTest zone = Instantiate(zonePrefab, transform);
        zone.gameObject.SetActive(false);

        availableZones.Enqueue(zone);

        return zone;
    }

    public ZoneVisualTest GetZone(Vector3 position)
    {
        if (zonePrefab == null)
            return null;

        if (availableZones.Count == 0)
        {
            CreateZone();
        }

        ZoneVisualTest zone = availableZones.Dequeue();

        zone.transform.SetParent(null);
        zone.transform.position = position;
        zone.transform.rotation = Quaternion.identity;
        zone.gameObject.SetActive(true);

        return zone;
    }

    public void ReturnZone(ZoneVisualTest zone)
    {
        if (zone == null)
            return;

        zone.gameObject.SetActive(false);
        zone.transform.SetParent(transform);

        availableZones.Enqueue(zone);
    }
}