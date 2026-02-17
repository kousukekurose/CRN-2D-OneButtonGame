using UnityEngine;
using TMPro;
using System.Collections.Generic;

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

    public void Goal()
    {
        //二重ゴール防止
        if (!isRunning) return;
        isRunning = false;

        PlayerPrefs.Save();
        rankingManager.CheckRanking(currentTime);
    }



    public void GameOver()
    {
        if (!isRunning) return;
        isRunning = false;
    }
}
