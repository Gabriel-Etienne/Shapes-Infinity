using System.Collections.Generic;
using UnityEngine;

public class BulletPoolTest : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int initialPoolSize = 100;

    private Queue<GameObject> availableBullets = new Queue<GameObject>();

    private void Awake()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateBullet();
        }
    }

    private GameObject CreateBullet()
    {
        GameObject bullet = Instantiate(bulletPrefab, transform);
        bullet.SetActive(false);

        availableBullets.Enqueue(bullet);

        return bullet;
    }

    public GameObject GetBullet()
    {
        // Si aucune bullet n'est disponible,
        // on en crée une nouvelle.
        if (availableBullets.Count == 0)
        {
            CreateBullet();
        }

        GameObject bullet = availableBullets.Dequeue();

        bullet.transform.SetParent(null);
        bullet.SetActive(true);

        return bullet;
    }

    public void ReturnBullet(GameObject bullet)
    {
        bullet.SetActive(false);
        bullet.transform.SetParent(transform);

        availableBullets.Enqueue(bullet);
    }
}