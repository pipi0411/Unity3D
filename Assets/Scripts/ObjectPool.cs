using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int poolSize = 10;
    private Queue<GameObject> bulletPool;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        bulletPool = new Queue<GameObject>();
        CreateInitialPool();
    }
    public GameObject GetBullet()
    {
        if (bulletPool.Count == 0)
        {
            CreateNewBullet();
        }
        GameObject bulletToGet = bulletPool.Dequeue();
        bulletToGet.SetActive(true);
        bulletToGet.transform.parent = null; // Detach from pool parent
        return bulletToGet;
    }
    public void ReturnBullet(GameObject bullet)
    {
        bullet.SetActive(false);
        bulletPool.Enqueue(bullet);
        bullet.transform.parent = transform; // Reattach to pool parent
    }
    private void CreateInitialPool()
    {
        bulletPool = new Queue<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewBullet();
        }
    }

    private void CreateNewBullet()
    {
        GameObject bullet = Instantiate(bulletPrefab, transform);
        bullet.SetActive(false);
        bulletPool.Enqueue(bullet);
    }
}
