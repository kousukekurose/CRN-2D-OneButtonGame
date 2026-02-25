using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using R3;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.EventSystems.EventTrigger;

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
    public static bool startcheck = false;

    private bool isGameStart = false;
    //プレイヤー生成位置
    public Transform spawnPoint;
    public Transform cameraTransfrom;
    private Transform target; // プレイヤーをドラッグ＆ドロップ
    public Vector3 offset;   // プレイヤーとの距離


    // プレイヤーが力尽きたことを知らせる「ストリーム」
    private readonly AsyncReactiveProperty<GameObject> playerDiedSource = new(null);

    // 外部からはこれを通じて「通知」を待機（Subscribe）できる
    public IUniTaskAsyncEnumerable<GameObject> OnPlayerDied => playerDiedSource;

    // 通知を送る
    public void NotifyPlayerDied(GameObject player) => playerDiedSource.Value = player;

    // 現在シーンに存在する全エネミーを登録
    private readonly List<GameObject> activeEnemies = new();

    private readonly CompositeDisposable disposables = new();

    // 敵数が変化したときに通知するイベント
    public static readonly ReactiveProperty<int> EnemyCount = new(0);

    // シーン開始時にリストをクリア
    private void Awake()
    {
        Time.timeScale = 1f;
        ////カウントリセット
        //EnemyCount.Value = 0;

        activeEnemies.Clear();
        // ゲーム開始と同時に「誰かが死ぬのを待つ」タスクを起動
        playerDiedSource
            .Where(player => player != null)
            .Subscribe(player =>
            {
                Debug.Log("プレイヤーの死を検知");
                EndGame(false, player);
            });

        //エネミーが生成された時の通知を登録
        Enemy.OnSupawned
            .Subscribe(enemy =>
            {
                if (!activeEnemies.Contains(enemy))
                {
                    activeEnemies.Add(enemy);
                    Debug.Log("エネミーを追加");
                    EnemyCount.Value = activeEnemies.Count;
                    Debug.Log("現在のエネミーの数" + activeEnemies.Count);
                    if (!startcheck)
                    {
                        SetAllEnemiesActive(false);
                    }
                }
            }).AddTo(disposables);

        //エネミーが破棄された時の処理
        Enemy.OnDestroyed
            .Subscribe(enemy =>
            {
                activeEnemies.Remove(enemy);
                Debug.Log("エネミーの登録破棄");
                EnemyCount.Value = activeEnemies.Count;
            }).AddTo(disposables);
    }

    private void Start()
    {
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


    private void Update()
    {
        if (!isGameStart && Mouse.current.leftButton.wasPressedThisFrame)
        {
            isGameStart = true;
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
        startcheck = true;
        EnemyCount.OnNext(EnemyCount.Value);
        Debug.Log(EnemyCount.Value + "ここで0に再設定");
        if (input != null) input.enabled = true;
        if (player != null) player.enabled = true;

        timer.StratTimer();
    }


    //クリアとゲームオーバー
    public void EndGame(bool isClear, GameObject playerObj)
    {
        //プレイヤーが存在していなかったら中断
        if (playerObj == null) return;
        startcheck = false;
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
        Time.timeScale = 0f;

        // 全エネミーを止める
        SetAllEnemiesActive(false);
        //全エネミーの削除
        ClearAllEnemies(); 
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

    //タイトル画面でエネミーが出現しないように実体の削除
    public void ClearAllEnemies()
    {
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null) Destroy(enemy); 
        }
    }

    private void OnDestroy()
    {
        disposables.Dispose();
    }

    //現在の敵の数を教える
    public int GetCurrentEnemyCount()
    {
        return activeEnemies.Count;
    }

}
