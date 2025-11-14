using UnityEngine;

public class TrafficSpawner : MonoBehaviour
{
    [Header("Car Prefab")]
    public GameObject horizontalCarPrefab;

    [Header("Spawn Points")]
    public Transform spawnLeft;
    public Transform spawnRight;

    [Header("Exit Points")]
    public Transform exitLeft;
    public Transform exitRight;

    [Header("Spawn Timing")]
    public float spawnIntervalMin = 2f;
    public float spawnIntervalMax = 4f;

    private float nextSpawnTime;

    private void Start()
    {
        ScheduleNextSpawn();
    }

    private void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnHorizontalCar();
            ScheduleNextSpawn();
        }
    }

    private void ScheduleNextSpawn()
    {
        nextSpawnTime = Time.time + Random.Range(spawnIntervalMin, spawnIntervalMax);
    }

    private void SpawnHorizontalCar()
    {
        if (horizontalCarPrefab == null)
        {
            Debug.LogWarning("Horizontal car prefab not assigned!");
            return;
        }

        // Ëæ»ú×ó -> ÓÒ »ò ÓÒ -> ×ó
        bool fromLeft = Random.value > 0.5f;

        Transform spawn = fromLeft ? spawnLeft : spawnRight;
        Transform exit = fromLeft ? exitRight : exitLeft;

        if (spawn == null || exit == null)
        {
            Debug.LogWarning("Spawn or Exit point missing!");
            return;
        }

        GameObject carObj = Instantiate(horizontalCarPrefab, spawn.position, spawn.rotation);

        HorizontalTrafficCar car = carObj.GetComponent<HorizontalTrafficCar>();
        if (car != null)
        {
            car.targetPoint = exit;
        }
        else
        {
            Debug.LogError("HorizontalTrafficCar script missing on prefab!");
        }
    }
}
