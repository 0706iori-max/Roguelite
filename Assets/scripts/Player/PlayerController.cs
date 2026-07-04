using Core.Interface;
using Cysharp.Threading.Tasks;
using InGame.Data;
using InGame.Data;
using System;
using System.Threading;
using TPSRoguelite.InGame.Enum;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TPSRoguelite.InGame.Player
{

    public class PlayerController : MonoBehaviour
    {
        //移動速度
        private const float moveSpeed = 5.0f;

        //回転速度
        private const float ROTATE_SPEED = 10f;

        //レーザーポインターの描画距離
        private const float LASER_MAX_DISTANCE = 50f;

        //攻撃距離(射撃範囲)
        private const float ATACK_RANGE = 50;

        //物理演算コンポーネント
        [SerializeField] private Rigidbody rigidbody;

        //銃口のトランスフォーム
        [SerializeField] private Transform weponOrigin;

        //レーザープリンターの描画コンポーネント
        [SerializeField] private LineRenderer laserLineRenderer;

        //武器のデータ
        [SerializeField] private WeaponData CurrentWeapon;

        //自動生成されたインプット
        private PlayerInputActions inputActions;

        private Vector2 moveInput;

        private Transform mainCameraTransform;

        //リロードしているか
        private bool isReloading;

        //射撃可能か
        private bool canShot = true;

        private CancellationTokenSource fireCts;

        //現在の弾数
        public int CurrentAmmo { get; private set; }

        //外部(アニメーションとかUIとか)に現在の速度を伝えるために保存する
        public Vector3 CurrentVelocity { get; private set; }

        private void Awake()
        {
            if (CurrentWeapon != null)
            {
                CurrentAmmo = CurrentWeapon.MaxAmmo;
            }
            else
            {
                Debug.LogError("WeaponDataがありません");
            }

            inputActions = new PlayerInputActions();
            inputActions.Player.fire.performed += OnFire;
            inputActions.Player.fire.canceled += OnFire;
            inputActions.Player.Reload.performed += OnReload;

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


        void Update()
        {
            moveInput = inputActions.Player.Move.ReadValue<Vector2>();
            DrawLaserPointer();
        }
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
            if (context.performed)
            {

                if (!canShot || isReloading || CurrentWeapon == null) 
                {
                    return;
                 }
                // 押された瞬間に、新しいキャンセルスイッチを作成
                fireCts = new CancellationTokenSource();
                
                               // プレイヤーが消滅した時と、ボタンを離した時のトークンを合体させる
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(fireCts.Token, this.GetCancellationTokenOnDestroy());
                switch (CurrentWeapon.WeaponFireType)
                {
                    case Enum.FireType.semiAuto:
                        ShootSemAutoAsync(this.GetCancellationTokenOnDestroy()).Forget();
                        break;

                    case Enum.FireType.Burst:
                        ShootBurstAsync(this.GetCancellationTokenOnDestroy()).Forget();
                        break;

                    case Enum.FireType.FullAuto:
          
                                              // フルオートは指を離した時に止めるため、合体させたトークンを渡す
                       ShootFullAutoAsync(linkedCts.Token).Forget();
                                              break;
                                          default:

                        Debug.LogWarning($"割り当てられてない射撃タイプがあります。{CurrentWeapon.WeaponFireType}");
                        break;
                }






            }

            // ボタンが離れたときに、フルオートのループを解除するために、キャンセルトークンのキャンセル処理を行う
                       if (context.canceled)
                       {
            fireCts?.Cancel();
            fireCts?.Dispose();
            fireCts = null;
                       }
        }
        private async UniTaskVoid ShootSemAutoAsync(CancellationToken token)
        {

            if (CurrentAmmo == 0)
            {
                ReloadAsync().Forget();
                return;
            }
            canShot = false;

            CurrentAmmo--;
            Debug.Log($"セミオートで撃った!弾数残り{CurrentAmmo}");
            Shoot();

            await UniTask.Delay(System.TimeSpan.FromSeconds(CurrentWeapon.FireRate), cancellationToken: token);

            canShot = true;
        }
              
       private async UniTaskVoid ShootBurstAsync(CancellationToken token)
       {
           canShot= false;
           for (int i = 0; i< 3; i++)
           {
               if (CurrentAmmo <= 0)
               {
                   canShot = true;
                   return;
               }

               CurrentAmmo--;
               Shoot();
               Debug.Log($"バースト！ 残弾: {CurrentAmmo}");

               await UniTask.Delay(TimeSpan.FromSeconds(CurrentWeapon.FireInterval), cancellationToken: token);
           }

await UniTask.Delay(TimeSpan.FromSeconds(CurrentWeapon.FireRate), cancellationToken: token);
canShot = true;
        }

       private async UniTaskVoid ShootFullAutoAsync(CancellationToken token)
       {
           canShot = false;

           while (!token.IsCancellationRequested)
           {
               if (CurrentAmmo <= 0)
               {
                   ReloadAsync().Forget();
                   break;
               }

               CurrentAmmo--;
               Debug.Log($"フルオート発射！ 残弾: {CurrentAmmo}");
               Shoot();

               bool isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(CurrentWeapon.FireInterval), cancellationToken: token).SuppressCancellationThrow();

               if (isCanceled)
               {
                   break;
               }
           }

           await UniTask.Delay(TimeSpan.FromSeconds(CurrentWeapon.FireRate), cancellationToken: this.GetCancellationTokenOnDestroy());

           canShot= true;
       }

        //共通の攻撃処理
        private void Shoot()
        {
            Ray ray = new Ray(mainCameraTransform.position, mainCameraTransform.forward);

            //光線に何かが当たったか判定
            if (Physics.Raycast(ray, out RaycastHit hitInfo, ATACK_RANGE))
            {
                Debug.Log($"{hitInfo.collider.name}に命中!");

                //当たった相手がIDamageableを持っているか
                IDamageble target = hitInfo.collider.GetComponent<IDamageble>();

                
                if (target != null)
                {
                    target.TakeDamage(CurrentWeapon.AttackPower);
                }
            }

        }

        private void OnReload(InputAction.CallbackContext context)
        {
            if (isReloading || CurrentAmmo == CurrentWeapon.MaxAmmo)
            {
                return;
            }
            ReloadAsync().Forget();
        }

        private async UniTask ReloadAsync()
        {
            isReloading = true;
            Debug.Log("リロード中");

            await UniTask.Delay(TimeSpan.FromSeconds(CurrentWeapon.ReloadTime), cancellationToken: this.GetCancellationTokenOnDestroy());

            CurrentAmmo = CurrentWeapon.MaxAmmo;
            isReloading = false;
            Debug.Log("リロード完了");
        }


        //レーザーポインターの描画
        private void DrawLaserPointer()
        {
            if (laserLineRenderer == null || weponOrigin == null || mainCameraTransform == null)
            {
                return;
            }

            laserLineRenderer.SetPosition(0, weponOrigin.position);

            Ray ray = new Ray(mainCameraTransform.position, mainCameraTransform.forward);
            if (Physics.Raycast(ray, out RaycastHit hitinfo, LASER_MAX_DISTANCE))
            {
                laserLineRenderer.SetPosition(1, hitinfo.point);
            }
            else
            {
                laserLineRenderer.SetPosition(1, ray.GetPoint(LASER_MAX_DISTANCE));
            }
        }
    }
}