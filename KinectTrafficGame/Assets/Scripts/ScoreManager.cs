using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    // 单例给 NpcCarController 等脚本使用
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    public float timer = 30.0f;
    public int loseScore = 0;  // <=-10 失败

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public GameObject EndGameObject;
    public TextMeshProUGUI endGameText;

    public float absoluteScore;
    private float playTime = 0f;
    private bool gameEnded = false;
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
        EndGameObject.SetActive(false);
    }

    /// <summary>
    /// 车被正确指挥一次（+1 分）
    /// </summary>
    public void OnCarGuidedSuccessfully(NpcCarController car)
    {
        timer += 20;
        CheckEndGame();
    }

    /// <summary>
    /// 车发生碰撞（-1 分）
    /// </summary>
    public void OnCarCrash()
    {
        timer -= 10;
        CheckEndGame();
    }

    private void Update()
    {
        if (!gameEnded)
        {
            playTime += Time.deltaTime;
            absoluteScore = playTime;
            timer -= Time.deltaTime;
            UpdateTimer();
            CheckEndGame();
        }
    }

    private void CheckEndGame()
    {
        if (timer <= loseScore && !gameEnded)
        {
            gameEnded = true;
            absoluteScore = playTime;
            Time.timeScale = 0;

            if (endGameText != null)
            {
                endGameText.text = "Game End!";
                EndGameObject.SetActive(true);
            }

        }
        else return;
    }

    void UpdateTimer()
    {
        float minutes = Mathf.FloorToInt(timer/60);
        float seconds = Mathf.FloorToInt(timer % 60);

        scoreText.text = string.Format("{0:00} : {1:00}", minutes, seconds);
    }


}
