using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    // 单例给 NpcCarController 等脚本使用
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    public int score = 0;
    public int winScore = 30;    // >=30 胜利
    public int loseScore = -10;  // <=-10 失败

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI messageText;

    [Header("备用 Scene 设置（如果没有 TrafficManager 时使用）")]
    public string winSceneName;
    public string loseSceneName;

    private void Awake()
    {
        // 标准单例
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 如果希望跨场景保留分数，可以打开：
        // DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        UpdateUI();
    }

    /// <summary>
    /// 车被正确指挥一次（+1 分）
    /// </summary>
    public void OnCarGuidedSuccessfully(NpcCarController car)
    {
        score++;
        UpdateUI();
        CheckEndGame();
    }

    /// <summary>
    /// 车发生碰撞（-1 分）
    /// </summary>
    public void OnCarCrash()
    {
        score--;
        UpdateUI();
        CheckEndGame();
    }

    private void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    private void CheckEndGame()
    {
        if (score >= winScore)
        {
            if (messageText != null)
                messageText.text = "You Win!";

            HandleEnd(true);
        }
        else if (score <= loseScore)
        {
            if (messageText != null)
                messageText.text = "You Lose!";

            HandleEnd(false);
        }
    }

    private void HandleEnd(bool win)
    {
        // 优先让 TrafficManager 统一处理（停刷车 + 清车 + 切场景）
        if (TrafficManager.Instance != null)
        {
            TrafficManager.Instance.EndGame(win);
        }
        else
        {
            // 万一你没挂 TrafficManager，就让 ScoreManager 自己切场景
            string sceneToLoad = win ? winSceneName : loseSceneName;
            if (!string.IsNullOrEmpty(sceneToLoad))
            {
                SceneManager.LoadScene(sceneToLoad);
            }
            else
            {
                Debug.Log("Game ended: " + (win ? "WIN" : "LOSE") + " (no TrafficManager / end scenes configured).");
            }
        }
    }
}
