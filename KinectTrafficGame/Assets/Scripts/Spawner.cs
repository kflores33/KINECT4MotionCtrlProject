using UnityEngine;

public class VehicleSpawner : MonoBehaviour
{
    [Header("Controllable NPC Car Prefab")]
    public GameObject npcCarPrefab;

    [Header("Spawn Points (4 lanes: L1, L2, R2, R1)")]
    public Transform[] laneSpawnPoints;   // 4 个出生点 Cube

    [Header("Stop Points (4 lanes)")]
    public Transform[] laneStopPoints;    // 对应每条车道的停止线

    [Header("Straight Points (4 lanes)")]
    public Transform[] laneStraightPoints; // 对应每条车道的直行目标

    [Header("Turn Points (shared)")]
    public Transform leftTurnPoint;       // 左转目标（全局一个）
    public Transform rightTurnPoint;      // 右转目标（全局一个）

    [Header("Spawn Timing")]
    public float spawnIntervalMin = 1.5f;
    public float spawnIntervalMax = 3f;

    [Tooltip("勾上则一开始就自动刷车，否则需要 TrafficManager 调 StartSpawning()")]
    public bool autoStart = true;

    private float nextSpawnTime;
    private bool spawningEnabled = false;

    private void Start()
    {
        // 根据 autoStart 决定开局是否刷车
        spawningEnabled = autoStart;
        ScheduleNextSpawn();
    }

    private void Update()
    {
        if (!spawningEnabled) return;

        if (Time.time >= nextSpawnTime)
        {
            SpawnCar();
            ScheduleNextSpawn();
        }
    }

    // =============================
    //     提供给 TrafficManager 的接口
    // =============================

    public void StartSpawning()
    {
        spawningEnabled = true;
        // 重新排一次时间，防止刚打开时卡帧
        ScheduleNextSpawn();
    }

    public void StopSpawning()
    {
        spawningEnabled = false;
    }

    // =============================
    //           内部逻辑
    // =============================

    private void ScheduleNextSpawn()
    {
        nextSpawnTime = Time.time + Random.Range(spawnIntervalMin, spawnIntervalMax);
    }

    private void SpawnCar()
    {
        if (npcCarPrefab == null || laneSpawnPoints == null || laneSpawnPoints.Length == 0)
            return;

        int laneIndex = Random.Range(0, laneSpawnPoints.Length);
        Transform spawn = laneSpawnPoints[laneIndex];

        GameObject carObj = Instantiate(npcCarPrefab, spawn.position, spawn.rotation);
        carObj.tag = "AICar";

        var car = carObj.GetComponent<NpcCarController>();
        if (car != null)
        {
            // 设置车的车道索引
            car.laneIndex = laneIndex;

            // 填本车道的 Stop / Straight
            car.stopPoint = SafeGet(laneStopPoints, laneIndex);
            car.straightPoint = SafeGet(laneStraightPoints, laneIndex);

            // 填全局的 Left / Right
            car.leftTurnPoint = leftTurnPoint;
            car.rightTurnPoint = rightTurnPoint;

            // 随机一个期望方向（给头顶 UI 看）
            car.desiredDirection = (CarDesiredDirection)Random.Range(0, 3);
        }
    }

    private Transform SafeGet(Transform[] arr, int index)
    {
        if (arr == null || arr.Length <= index) return null;
        return arr[index];
    }
}
