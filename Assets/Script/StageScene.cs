using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;

//プレイ終了時の挙動
public class StageScene : MonoBehaviour
{
    public Timer timer;
    public RankingManager rankingManager;
    public TileMapGimmick mapGimmick;
    public GameObject gameClearUI;
    public GameObject gameOverUI;
    public GameObject gameOverArea;
    public GameObject gameClearArea;
    public GameObject playerPrefab;
    public GameObject timeTextUI;
    public TextMeshProUGUI countdown;
    public TextMeshProUGUI resultText;

    private bool isGameStrat = false;
    //プレイヤー生成位置
    public Transform spawnPoint;
    public Transform cameraTransfrom;
    private Transform target; // プレイヤーをドラッグ＆ドロップ
    public Vector3 offset;   // プレイヤーとの距離（例: 0, 5, -10）


    // プレイヤーが力尽きたことを知らせる「ストリーム」
    private readonly AsyncReactiveProperty<GameObject> playerDiedSource = new(null);

    // 外部からはこれを通じて「通知」を待機（Subscribe）できる
    public IUniTaskAsyncEnumerable<GameObject> OnPlayerDied => playerDiedSource;

    // 通知を送る
    public void NotifyPlayerDied(GameObject player) => playerDiedSource.Value = player;

    // 現在シーンに存在する全エネミーを登録
    private static readonly List<GameObject> activeEnemies = new List<GameObject>();

    // エネミーが自分を登録する
    public static void RegisterEnemy(GameObject enemy) => activeEnemies.Add(enemy);

    // エネミーが消える（倒される）時に自分を外す
    public static void UnregisterEnemy(GameObject enemy) => activeEnemies.Remove(enemy);

    // シーン開始時にリストをクリア
    private void Awake()
    {
        activeEnemies.Clear();
    }

    private void Start()
    {
        // ゲーム開始と同時に「誰かが死ぬのを待つ」タスクを起動
        WatchDeath().Forget();
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log($"クリック検知！ 現在の状態: isGameStrat = {isGameStrat}");

        }
        if (!isGameStrat)
        {
            SetAllEnemiesActive(false);
        }
        if (!isGameStrat && Mouse.current.leftButton.wasPressedThisFrame)
        {
            isGameStrat = true;
            StartCoroutine(StartGameSequence());
        }
    }

    //プレイヤーを生成しカウントダウン開始
    private System.Collections.IEnumerator StartGameSequence()
    {
        SetAllEnemiesActive(false);
        //指定した位置にプレイヤーを生成
        GameObject newPlayer = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
        //カメラを生成したプレイヤーを指定
        target = newPlayer.transform;
        //カウントダウン中はプレイヤーは動けないように設定
        if (newPlayer.TryGetComponent<PlayerInput>(out var input)) input.enabled = false;
        if (newPlayer.TryGetComponent<Player>(out var player)) player.enabled = false;

        //カウントダウン
        for (int i = 3; i > 0; i--)
        {
            if (countdown != null) countdown.text = i.ToString();
            yield return new WaitForSeconds(1.0f);
        }
        if (countdown != null) countdown.text = "GO!";
        yield return new WaitForSeconds(1.0f);
        countdown.gameObject.SetActive(false);

        SetAllEnemiesActive(true);

        if (input != null) input.enabled = true;
        if (player != null) player.enabled = true;

        timer.StratTimer();
    }

    private async UniTaskVoid WatchDeath()
    {
        // playerDiedSource に値（GameObject）がセットされるのをずっと監視する
        await foreach (var playerObj in OnPlayerDied)
        {
            // 初期値（null）の時は無視
            if (playerObj == null) continue;

            Debug.Log("StageScene: プレイヤーの死を検知");

            // 実際にゲーム終了処理を実行
            EndGame(false, playerObj);
        }
    }

    //クリアとゲームオーバー
    public void EndGame(bool isClear, GameObject playerObj)
    {
        //プレイヤーが存在していなかったら中断
        if (playerObj == null) return;

        //Clearかgameover時の判定
        if (isClear)
        {
            gameOverArea.SetActive(false);
            // タイマー停止＆タイム取得
            float clearTime = timer.Goal();
            bool isNewRecord = rankingManager.CheckRanking(clearTime);
            if (isNewRecord)
            {
                resultText.text = $"New Record!! {clearTime:F1}s";
            }
            else
            {
                resultText.text = $"Time: {clearTime:F1}s";
            }
            gameClearUI.SetActive(true);
            gameOverUI.SetActive(false);
            timeTextUI.SetActive(false);
        }
        else
        {
            if (gameClearArea != null) gameClearArea.SetActive(false);
            timer.GameOver();
            gameOverUI.SetActive(true);
            gameClearUI.SetActive(false);
        }

        //プレイヤーの停止(ボタンをクリックするとプレイヤーも動くため)
        if (playerObj != null)
        {
            //setActiveだとカメラ機能も停止してしまうためenabledをfalse
            if (playerObj.TryGetComponent<PlayerInput>(out var input)) input.enabled = false;
            if (playerObj.TryGetComponent<Player>(out var player)) player.enabled = false;

            //物理挙動の停止(滑りながら止まってしまうため)
            if (playerObj.TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Static;
            }
        }

        // 全エネミーを止める
        SetAllEnemiesActive(false);
    }

    //エネミー停止機能
    private void StopObject(GameObject obj)
    {
        if (obj == null) return;

        //スプリクト類を止める
        foreach (var comp in obj.GetComponents<MonoBehaviour>())
        {
            if (comp != this) comp.enabled = false;
        }

        //アニメーションも止める
        if (obj.TryGetComponent<Animator>(out var animator))
        {
            animator.enabled = false;
        }

        //物理挙動を止める
        if (obj.TryGetComponent<Rigidbody2D>(out var rb))
        {
            if (rb.bodyType == RigidbodyType2D.Dynamic)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Static;
            }
        }
    }

    //エネミー機能開始
    private void StartEnemy(GameObject obj)
    {
        if (obj == null) return;

        //スプリクト類を開始
        foreach (var comp in obj.GetComponents<MonoBehaviour>())
        {
            if (comp != this) comp.enabled = true;
        }

        //アニメーション開始
        if (obj.TryGetComponent<Animator>(out var animator))
        {
            animator.enabled = true;
        }

        //物理挙動を開始
        if (obj.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    // 全エネミーの状態を一括制御する（trueなら動く、falseなら止まる）
    private void SetAllEnemiesActive(bool isActive)
    {
        foreach (var enemy in activeEnemies)
        {
            if (enemy == null) continue;

            if (isActive) StartEnemy(enemy);
            else StopObject(enemy);
        }
    }

    //カメラのコントロール
    private void LateUpdate()
    {
        if (target != null && cameraTransfrom != null)
        {
            //XとZはプレイヤーに追従し、Yはカメラ自身の現在の高さをキープする
            cameraTransfrom.position = new Vector3(target.position.x + offset.x, transform.position.y, target.position.z + offset.z);
        }
        Debug.Log("現在のエネミーの数" + activeEnemies.Count);
    }

}
