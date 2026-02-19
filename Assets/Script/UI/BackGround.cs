using UnityEngine;

public class BackGround : MonoBehaviour
{
    // 配置したいPrefab
    public GameObject spritePrefab;

    // 横に並べる数
    public int count = 5;

    // 並べる間隔
    public float spacing = 1.5f;

    void Start()
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 pos = new Vector3(i * spacing, 0f, 0f); // 横方向に配置
            Instantiate(spritePrefab, pos, Quaternion.identity, transform);
        }
    }
}
