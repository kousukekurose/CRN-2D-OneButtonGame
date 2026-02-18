using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType { Normal,Jump }

public class Enemy : MonoBehaviour
{
    //敵の種類を選択
    public EnemyType type;

    //敵の移動スピード
    public float speed;
    //敵のジャンプ力
    public float jump = 5;
    private float jumpTimer = 0f;
    public float jumpInterval = 2f;

    //敵の重さ
    public float gravity;
    //画面外でも行動
    public bool nonVisibleAct;

    private SpriteRenderer sr = null;
    private Rigidbody2D rb = null;
    private BoxCollider2D box = null;
    //private ObjectCollision oc = null;
    private bool isDead = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 生成されたらリストに自分を追加
        StageScene.RegisterEnemy(this.gameObject);
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if(sr.isVisible)
        {
            switch (type)
            {
                case EnemyType.Normal:
                    NormalEnemy();
                    break;
                case EnemyType.Jump:
                    JumpEnemy();
                    break;
                default:
                    break;
            }
        }
    }

    private void NormalEnemy()
    {
        if(sr.isVisible || nonVisibleAct)
        {
            //行動
            transform.Translate(Vector2.left * speed * Time.deltaTime);
        }
    }
    
    private void JumpEnemy()
    {
        if (sr.isVisible || nonVisibleAct)
        {
            jumpTimer -= Time.deltaTime;
            if (jumpTimer <= 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jump);
                jumpTimer = jumpInterval;
            }
        }
    }

    //まだ消える処理の仕様はない
    private void OnDestroy()
    {
        // 自分が破棄されたらリストから削除
        StageScene.UnregisterEnemy(this.gameObject);
    }
}
