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
    private GameObject[] enemyObject;

    private void Start()
    {
        isSpawning = false;
        var ct = this.GetCancellationTokenOnDestroy();

        StageScene.EnemyCount
            .Subscribe(async count => 
            {
                if (!StageScene.startcheck || isSpawning || ct.IsCancellationRequested) return;
                int num = maxEnemyCount - StageScene.EnemyCount.Value;
                isSpawning = true;
                for (int i = 0 ; i < num ; i++)
                {
                    if (!StageScene.startcheck || ct.IsCancellationRequested) break;
                    SpawnEnemy();
                    bool canceled = await UniTask.Yield(PlayerLoopTiming.Update, ct).SuppressCancellationThrow();
                    if (canceled || !StageScene.startcheck) return;
                    isSpawning = false;
                }
            }).AddTo(this);
    }


    private void SpawnEnemy()
    {
        // 仮に敵を生成
        if (this == null || gameObject == null) return;
        int randomId = Random.Range(0, enemyObject.Length);
        GameObject selectPrefab = enemyObject[randomId];
        //重なり防止
        float randomOffset = Random.Range(-0.5f, 0.5f);
        if (selectPrefab == null) return;
        Vector3 spawnPos = new Vector3(transform.position.x + randomOffset, transform.position.y, transform.position.z);
        Instantiate(selectPrefab, spawnPos, selectPrefab.transform.rotation);
    }
}
