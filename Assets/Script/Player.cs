using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 5.0f;
    public Rigidbody2D rb;
    public float jump = 5f;
    private int jumpCount = 0;
    public int jumpMaxCount = 3;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 右方向（X軸のプラス方向）に移動
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    public void OnJump()
    {
        if (jumpCount < jumpMaxCount)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jump);
            jumpCount++;
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                //壁にぶつかってもリセットされないように
                if (contact.normal.y > 0.5f)
                {
                    //地面に接触したらカウントを0にリセット
                    jumpCount = 0;
                    return;
                }
            }
        }

        // 引数が「Collision2D」から「Collider2D」に変わる点に注意
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Debug.Log("敵と接触した（トリガー）");
            FindObjectOfType<StageScene>().NotifyPlayerDied(gameObject);
        }
    }
}
