using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public enum GimmickType { Erase, Create, Blink }

public class TileMapGimmick : MonoBehaviour
{
    public Tilemap tilemap;
    //インスペクターでモード切り替え
    public GimmickType type;
    //消す範囲をインスペクターで設定
    public int width = 5;
    public int height = 5;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            switch (type)
            {
                case GimmickType.Erase:
                    EraseArea();
                    break;
                case GimmickType.Create:
                    break;
                case GimmickType.Blink:
                    break;
                default:
                    break;
            }
        }
    }


    //タイルマップを消す
    public void EraseArea()
    {
        Vector3Int originCelll = tilemap.WorldToCell(transform.position);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //中央から広げたり、下方向に伸ばしたり自由に計算
                Vector3Int tragetPos = new Vector3Int(originCelll.x + x, originCelll.y - y, 0);
                tilemap.SetTile(tragetPos, null);
            }
        }
        //一度発動したらトリガーを消す
        Destroy(gameObject);
    }
}
