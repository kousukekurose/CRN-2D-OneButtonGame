using UnityEngine;
using R3;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public Rigidbody2D rb;
    public float jump = 5f;
    private int jumpCount = 0;
    public int jumpMaxCount = 3;

    //無敵関数
    private bool isInvincible = false;
    [SerializeField]
    private float invincibleTime = 1.5f;
    [SerializeField]
    private float blinkInterval = 0.1f;
    private SpriteRenderer spriteRenderer;

    public static readonly ReactiveProperty<int> HP = new(3);
    // プレイヤーが力尽きたことを知らせる「ストリーム」
    private static readonly Subject<GameObject> playerDiedSource = new();

    // 外部からはこれを通じて「通知」を待機（Subscribe）できる
    public static Observable<GameObject> OnPlayerDied => playerDiedSource;

    private void Awake()
    {
        HP.Value = 3;
    }


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        // 右方向（X軸のプラス方向）に移動
        transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);
    }

    public void OnJump()
    {
        if (jumpCount < jumpMaxCount)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jump);
            jumpCount++;
        }

    }

    //ダメージと死亡
    public void Damage()
    {
        if (HP.Value <= 0) return;

        HP.Value--;
        if (HP.Value == 0)
        {
            playerDiedSource.OnNext(gameObject);
        }
        else
        {
            StartInvincible().Forget();
        }
    }

    //無敵時間
    private async UniTaskVoid StartInvincible()
    {
        isInvincible = true;

        //エネミーとの接触
        Physics2D.IgnoreLayerCollision
        (
            LayerMask.NameToLayer("Player"),
            LayerMask.NameToLayer("Enemy"),
            true
        );

        float timer = 0f;

        while (timer < invincibleTime) 
        { 
            spriteRenderer.enabled = !spriteRenderer.enabled;
            await UniTask.Delay((int)(blinkInterval * 1000));
            timer += blinkInterval;
        }

        spriteRenderer.enabled = true;

        Physics2D.IgnoreLayerCollision
        (
            LayerMask.NameToLayer("Player"),
            LayerMask.NameToLayer("Enemy"),
            false
        );

        isInvincible = false;
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
                    //地面に接触したら0にリセット
                    jumpCount = 0;
                    return;
                }
            }
        }

        // 引数が「Collision2D」から「Collider2D」に変わる点に注意
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Debug.Log("敵と接触した（トリガー）");
            Damage();
        }
    }
}
