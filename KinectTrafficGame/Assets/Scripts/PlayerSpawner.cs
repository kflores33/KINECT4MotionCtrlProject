using System.Collections;
using UnityEngine;

public class PlayerRespawner : MonoBehaviour
{
    [Header("Player Respawn Settings")]
    [Tooltip("玩家的预制体（包含 PlayerTrafficController、碰撞体、刚体等）")]
    public GameObject playerPrefab;

    [Tooltip("玩家出生的位置和朝向")]
    public Transform spawnPoint;

    [Tooltip("玩家被摧毁后多少秒重生")]
    public float respawnDelay = 2f;

    [Header("Lane Setup")]
    [Tooltip("场景里的四个车道 waypoint（L1, L2, R2, R1），在这里填")]
    public Transform[] lanePositions;

    [Tooltip("场景里对应的四个小人 SpriteRenderer，在这里填")]
    public SpriteRenderer[] officerRenderers;

    [Header("Audio")]
    [Tooltip("播放音效的 AudioSource（挂在这个管理物体上）")]
    public AudioSource audioSource;

    [Tooltip("玩家被销毁时播放的音效")]
    public AudioClip playerDestroyedClip;

    // 当前场景中活着的玩家引用
    private GameObject currentPlayer;
    private bool isRespawning = false;

    private void Start()
    {
        SpawnPlayer();
    }

    private void Update()
    {
        // 如果当前玩家已经被 Destroy 且还没开始重生计时，就启动重生协程
        if (!isRespawning && currentPlayer == null)
        {
            StartCoroutine(RespawnRoutine());
        }
    }

    private IEnumerator RespawnRoutine()
    {
        isRespawning = true;

        // ✅ 这里代表「刚检测到玩家已经被销毁」
        // 播放玩家死亡音效（从管理器自己的 AudioSource 播）
        if (audioSource != null && playerDestroyedClip != null)
        {
            audioSource.PlayOneShot(playerDestroyedClip);
        }

        // 等待设定的时间（比如 2 秒）
        yield return new WaitForSeconds(respawnDelay);

        SpawnPlayer();

        isRespawning = false;
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null || spawnPoint == null)
        {
            Debug.LogError("PlayerRespawner: 请在 Inspector 里指定 playerPrefab 和 spawnPoint！");
            return;
        }

        // 在出生点生成玩家，并在 Y 轴额外旋转 90 度
        Quaternion spawnRot = spawnPoint.rotation * Quaternion.Euler(0f, 90f, 0f);

        currentPlayer = Instantiate(
            playerPrefab,
            spawnPoint.position,
            spawnRot
        );

        // 玩家也参与 AICar 撞车系统的话，在这里统一设 tag
        currentPlayer.tag = "AICar";

        // 把场景里的 waypoint / officerRenderer 填给这个新玩家
        PlayerTrafficController controller = currentPlayer.GetComponent<PlayerTrafficController>();
        if (controller != null)
        {
            controller.lanePositions = lanePositions;
            controller.officerRenderers = officerRenderers;
        }
        else
        {
            Debug.LogWarning("PlayerRespawner: 生成的玩家上没有 PlayerTrafficController 组件。");
        }
    }
}
