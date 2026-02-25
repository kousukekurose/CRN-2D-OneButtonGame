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
                Debug.Log("startcheck" + StageScene.startcheck);
                if (!StageScene.startcheck || isSpawning || ct.IsCancellationRequested) return;
                if(count < maxEnemyCount)
                {
                    isSpawning = true;
                    SpawnEnemy();

                    bool canceled = await UniTask.Yield(ct).SuppressCancellationThrow();
                    if(canceled) return;
                    isSpawning = false;
                }
            }).AddTo(this);
    }


    private void SpawnEnemy()
    {
        // 仮に敵を生成
        if (this == null || gameObject == null) return;
        GameObject enemy = Instantiate(enemyObject, transform.position, Quaternion.identity);
    }
}
