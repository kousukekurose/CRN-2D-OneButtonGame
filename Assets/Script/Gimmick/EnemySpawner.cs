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

    public Transform[] transforms;
    //敵の間隔
    public float enemyPosition1 = -2f;
    public float enemyPosition2 = 2f;

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
                try
                {
                    int targetTotal = maxEnemyCount * transforms.Length;
                    int numToSpawn = targetTotal - StageScene.EnemyCount.Value;
                    isSpawning = true;
                    for (int i = 0; i < numToSpawn; i++)
                    {
                        if (!StageScene.startcheck || ct.IsCancellationRequested) break;
                        int pointIndex = StageScene.EnemyCount.Value % transforms.Length;
                        SpawnEnemyAt(pointIndex);
                        bool canceled = await UniTask.Yield(PlayerLoopTiming.Update, ct).SuppressCancellationThrow();
                        if (canceled || !StageScene.startcheck) return;
                    }
                }
                finally
                {
                    isSpawning = false;
                }
            }).AddTo(this);
    }

    private void SpawnEnemyAt(int index)
    {
        if (this == null || enemyObject == null || transforms.Length <= index) return;

        int randomId = Random.Range(0, enemyObject.Length);
        GameObject selectPrefab = enemyObject[randomId];
        if (selectPrefab == null) return;

        // 指定された index の場所を使用する
        Transform targetTransform = transforms[index];

        float randomOffset = Random.Range(enemyPosition1, enemyPosition2);
        Vector3 spawnPos = new Vector3(targetTransform.position.x + randomOffset, targetTransform.position.y, targetTransform.position.z);

        Instantiate(selectPrefab, spawnPos, selectPrefab.transform.rotation);
    }
}
