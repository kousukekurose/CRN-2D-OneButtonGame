using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject score;
    public UnityEngine.UI.Button[] pauseButton;

    //ゲーム終了
    public void Exit()
    {
        // ログを出して動いているか確認（エディタ用）
        Debug.Log("ゲームを終了します");

        // アプリケーションを終了させる
        Application.Quit();
    }


    // ステージシーンに遷移
    public void GoToGameScene()
    {
        SceneManager.LoadScene("StageScene");
    }

    // タイトルシーンに遷移
    public void TitleScene()
    {
        SceneManager.LoadScene("TitleScene");
    }

    public void Restart()
    {
        // 現在アクティブなシーンの名前を自動で取得して読み直す
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    public void scoreTextOff()
    {
        for (int i = 0; i < pauseButton.Length; i++)
        {
            pauseButton[i].interactable = true;
        }
        score.SetActive(false);
    }
    public void scoreTextOn()
    {
        for(int i = 0; i < pauseButton.Length; i++)
        {
            pauseButton[i].interactable = false;
        }
        score.SetActive(true);
    }
}
