using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class RankingManager: MonoBehaviour
{
    public TextMeshProUGUI[] rankingTextSlots;
    private int RankingCount => rankingTextSlots.Length;

    private void Start()
    {
        ShowRanking();
    }


    //スコアリセット機能
    public void ResetRanking()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // 大事：消した直後に「表示」を更新する
        ShowRanking();
        Debug.Log("すべての記録をリセットしました");
    }

    //ランキング表示
    public void ShowRanking()
    {
        int count = rankingTextSlots.Length;
        for (int i = 0; i < count; i++)
        {
            float t = PlayerPrefs.GetFloat("Timer" + (i + 1), 999f);
            string timeStr = (t < 999f) ? t.ToString("F1") + "s" : "--,---";

            rankingTextSlots[i].text = (i + 1) + ": " + timeStr;
        }
    }

    //ランキング更新
    public void CheckRanking(float newTime)
    {
        List<float> times = new List<float>();
        for (int i = 1; i <= RankingCount; i++)
        {
            times.Add(PlayerPrefs.GetFloat("Timer" + i, 999f));
        }

        times.Add(newTime);
        //順番並び替え
        times.Sort();

        //上位10位だけ保存
        for (int i = 0; i < RankingCount; i++)
        {
            PlayerPrefs.SetFloat("Timer" + (i + 1), times[i]);
        }
        PlayerPrefs.Save();
    }
}
