using UnityEngine;

public class GameEndTrigger : MonoBehaviour
{
    public StageScene stageScene;
    //クリアかゲームオーバーをインスペクターで設定
    public bool isClearTrigger;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ぶつかった相手のレイヤーが場外かどうか判定
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            stageScene.EndGame(isClearTrigger);
        }

        if(collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Destroy(collision.gameObject);
            Debug.Log("エネミー場外");
        }
    }
}
