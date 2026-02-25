using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemySpawner : MonoBehaviour
{
    private Tilemap tilemap;
    [SerializeField]
    private int maxEnemyCount = 2;
    private bool isSpawning = false;

    [SerializeField]
    private GameObject enemyObject;

    private void Start()
    {
        var ct = this.GetCancellationTokenOnDestroy();

        StageScene.EnemyCount
            .Subscribe(async count => 
            {
                Debug.Log("通知の確認" + StageScene.EnemyCount.Value);
                if (isSpawning || ct.IsCancellationRequested) return;
                int num = maxEnemyCount - StageScene.EnemyCount.Value; 
                for(int i = 0 ; i < num ; i++)
                {
                    isSpawning = true;
                    SpawnEnemy();
                    bool canceled = await UniTask.Yield(ct).SuppressCancellationThrow();
                    if (canceled) return;
                    isSpawning = false;
                }
            }).AddTo(this);
    }


    private void SpawnEnemy()
    {
        // 仮に敵を生成
        if (this == null || gameObject == null) return;
        GameObject enemy = Instantiate(enemyObject, transform.position, enemyObject.transform.rotation);
    }
}
