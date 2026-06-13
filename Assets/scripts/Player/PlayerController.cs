using Core.Interface;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TPSRoguelite.InGame.Player
{

    public class PlayerController : MonoBehaviour
    {
        //移動速度
        private const float moveSpeed = 5.0f;

        private const float ROTATE_SPEED = 10f;

        private const float LASER_MAX_DISTANCE = 50.0f;

        private const int ATTACK_DAMAGE = 20;
        private const float ATTACK_RANGE = 50f;

        

        //物理演算コンポーネント
        [SerializeField] private Rigidbody rigidbody;
        private PlayerInputActions inputActions;

        private Vector2 moveInput;

        private Transform mainCameraTransform;

        [SerializeField] private LineRenderer laserLineRenderer;

        [SerializeField] private Transform weaponOrigin;

        //外部(アニメーションとかUIとか)に現在の速度を伝えるために保存する
        public Vector3 CurrentVelocity { get; private set; }

        private void Awake()
        {
            inputActions = new PlayerInputActions();
            inputActions.Player.fire.performed += OnFire;

            if (UnityEngine.Camera.main != null)
            {
                mainCameraTransform = UnityEngine.Camera.main.transform;
            }
            else
            {
                Debug.LogError("MainCameraが見つかりません");
            }
        }

        private void OnEnable()
        {
            inputActions.Enable();
        }
        private void OnDisable()
        {
            inputActions.Disable();
        }
  private void DrawLaserPointer()
     {
         if (laserLineRenderer == null || weaponOrigin == null || mainCameraTransform == null)
         {
            return;
         }
 
         laserLineRenderer.SetPosition(0, weaponOrigin.position);

         // カメラの中央から真っ直ぐ前へ光線を飛ばす
         Ray ray = new Ray(mainCameraTransform.position, mainCameraTransform.forward);
 
         // 光線が何かに当たったか判定
         if (Physics.Raycast(ray, out RaycastHit hitInfo, LASER_MAX_DISTANCE))
         {
             laserLineRenderer.SetPosition(1, hitInfo.point);
         }
         else
         {
             // 何も当たらなかったら、最大距離の場所を終点にする    
             laserLineRenderer.SetPosition(1, ray.GetPoint(LASER_MAX_DISTANCE));
         }
     }

        void Update()
        {
            moveInput = inputActions.Player.Move.ReadValue<Vector2>();
     DrawLaserPointer();    }
        private void FixedUpdate()
        {
            Move();
        }
        private void Move()//移動処理
        {
            if (rigidbody == null)
            {
                Debug.LogError("Rigidbodyが設定されていません");
                return;
            }

            //入力がない場合はピタッと止めておく
            if (moveInput == Vector2.zero)
            {
                rigidbody.linearVelocity = new Vector3(0, rigidbody.linearVelocity.y, 0);
                CurrentVelocity = Vector3.zero;
                return;
            }

            //カメラ基準の計算に変更
            Vector3 cameraFoward = mainCameraTransform.forward;
            Vector3 cameraRight = mainCameraTransform.right;

            cameraFoward.y = 0f;
            cameraRight.y = 0f;
            cameraFoward.Normalize();
            cameraRight.Normalize();

            Vector3 moveDirection = (cameraFoward * moveInput.y + cameraRight * moveInput.x).normalized;

            //キャラクターを進行方向へ滑らかに振り向かせる
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rigidbody.rotation = Quaternion.Slerp(rigidbody.rotation, targetRotation, ROTATE_SPEED * Time.deltaTime);

            Vector3 targetVelocity = moveDirection * moveSpeed;
            rigidbody.linearVelocity = new Vector3(targetVelocity.x, rigidbody.linearVelocity.y, targetVelocity.z);

            //外部(アニメーションとかUIとか)に現在の速度を伝えるために保存する
            CurrentVelocity = rigidbody.linearVelocity;
        }

        private void OnFire(InputAction.CallbackContext context)
        {
            Ray ray = new Ray(mainCameraTransform.position, mainCameraTransform.forward);
            if(Physics.Raycast(ray,out RaycastHit hitInfo,ATTACK_RANGE)){
                
                Debug.Log($"(hitinfo.collider.GetConponent<IDamageble)");
                IDamageble target = hitInfo.collider.GetComponent<IDamageble>();
                if (target != null) {
                    target.TakeDamage(ATTACK_DAMAGE);

                }
            }

        }
    }
}
