using UnityEngine;

public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// 移動速度
    /// </summary>
    private const float MOVE_SPEED = 5.0f;

    /// <summary>
    /// 物理演算コンポーネント
    /// </summary>
    [SerializeField] private Rigidbody rigidbody;

    /// <summary>
    /// 移動方向のベクトル
    /// </summary>
    private Vector3 moveDirection = Vector3.zero;

    /// <summary>
    /// 外部（アニメーションとかUIとか）に現在の速度を教えるために保持する
    /// </summary>
    public Vector3 CurrentVelocity { get;private set; }
    //STERTはｔ後のＵＮＤＡＴＥの最初の実行前に一度呼び出されます
    

  
   private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        //入力値から移動方向のベクトルを作成する
        moveDirection=new Vector3 (x,0,z).normalized;
            }
    private void FixedUpdate()
    {
        Move();
    }


    private void Move() 
    {
        if (rigidbody == null)
        {
            Debug.LogError("RIGIDBODYが設定されていません");
            return;
        }

        //入力がない場合は、ピタッと止めておく
        if (moveDirection == Vector3.zero)
        {
            rigidbody.linearVelocity = new Vector3(0f,rigidbody.linearVelocity.y, 0f);
            CurrentVelocity=Vector3.zero;
            return;
        }
        //実際の移動速度を計算
        Vector3 targetVelocity = moveDirection * MOVE_SPEED;

        rigidbody.linearVelocity = new Vector3(
            targetVelocity.x,
            rigidbody.linearVelocity.y
            , targetVelocity.z);


        CurrentVelocity=rigidbody.linearVelocity;
    
    }



}
