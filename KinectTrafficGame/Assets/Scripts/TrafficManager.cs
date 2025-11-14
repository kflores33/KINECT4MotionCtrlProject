using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrafficManager : MonoBehaviour
{
    public static TrafficManager Instance { get; private set; }

    [Header("References")]
    [Tooltip("刷 NPC 车的脚本（我们前面写的 VehicleSpawner）")]
    public VehicleSpawner vehicleSpawner;

    [Header("Scenes")]
    [Tooltip("胜利时切换的场景名（可以留空，只在本场景显示胜利 UI）")]
    public string winSceneName;

    [Tooltip("失败时切换的场景名（可以留空，只在本场景显示失败 UI）")]
    public string loseSceneName;

    private bool gameEnded = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // 如果想跨场景保持这个管理器，可以打开：
        // DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // 开局：让 VehicleSpawner 开始刷车
        if (vehicleSpawner != null)
        {
            vehicleSpawner.StartSpawning();
        }
    }

    /// <summary>
    /// 由 ScoreManager 调用：游戏结束（win = true 胜利）
    /// </summary>
    public void EndGame(bool win)
    {
        if (gameEnded) return;
        gameEnded = true;

        // 1. 停止刷车
        if (vehicleSpawner != null)
        {
            vehicleSpawner.StopSpawning();
        }

        // 2. 清理场上的所有 NpcCarController
        ClearAllCars();

        // 3. 切场景（如果你想只在本场景做结算，也可以都留空）
        string sceneToLoad = win ? winSceneName : loseSceneName;
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.Log("Game ended: " + (win ? "WIN" : "LOSE") + " (no end scene configured).");
        }
    }

    private void ClearAllCars()
    {
        // 用 NpcCarController 的静态列表
        List<NpcCarController> carsCopy = new List<NpcCarController>(NpcCarController.AllCars);
        foreach (var car in carsCopy)
        {
            if (car != null)
            {
                Destroy(car.gameObject);
            }
        }
    }
}
