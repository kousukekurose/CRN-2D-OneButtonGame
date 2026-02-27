using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public RankingManager rankingManager;
    
    private float currentTime;
    private bool isRunning = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentTime = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (isRunning)
        {
            currentTime += Time.deltaTime;
            timerText.text = currentTime.ToString("F1");
        }
    }
    public void StratTimer()
    {
        isRunning = true;
        currentTime = 0f;
    }

    public float Goal()
    {
        //二重ゴール防止
        if (!isRunning) return currentTime;
        isRunning = false;
        rankingManager.CheckRanking(currentTime);
        return currentTime;
    }



    public void GameOver()
    {
        if (!isRunning) return;
        isRunning = false;
    }
}
