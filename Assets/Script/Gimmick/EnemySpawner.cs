using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemySpawner : MonoBehaviour
{
    private Tilemap tilemap;
    [SerializeField]
    private int maxEnemyCount1 = 2;
    //[SerializeField] 
    //private float respawnCheckInterval = 2f;

    [SerializeField]
    private GameObject[] enemyObject;

    //private int currentEnemyCount = 0;

    [SerializeField]
    StageScene scene;


    private void SpawnEnemy()
    {
        // âºÇ…ìGÇê∂ê¨
        GameObject enemy = Instantiate(enemyObject[0], transform.position, Quaternion.identity);
    }

    private void CheckEnemyCount(int currentCount)
    {
        int toSpawn = maxEnemyCount1 - currentCount;
        for (int i = 0; i < toSpawn; i++)
        {
            SpawnEnemy();
        }
    }

    private void OnEnable()
    {
        StageScene.OnEnemyCountChanged += CheckEnemyCount;
        //CheckEnemyCount(StageScene.GetCurrentEnemyCount());
    }

    private void OnDisable()
    {
        StageScene.OnEnemyCountChanged -= CheckEnemyCount;
    }
}
