using UnityEngine;
using UnityEngine.AI;

namespace InGame.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        private const string PLAYER_TAG_NAME = "Player";
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private EnemyState enemyState;
        private Transform targetPlayer;

        private void Awake()
        {
            // シーンから"Player"というタグが付いたオブジェクトを探す
            GameObject player = GameObject.FindGameObjectWithTag(PLAYER_TAG_NAME);
            if (player != null)
            {
                targetPlayer = player.transform;
            }
            else
            {
                Debug.LogError($"{PLAYER_TAG_NAME}というタグのついたオブジェクトが見つかりませんでした。");
            }

            // EnemyState が持っているカセットから、速度のデータを読み取ってナビにセットする！
            if (enemyState != null && enemyState.EnemyDataAsset != null)
            {
                navMeshAgent.speed = enemyState.EnemyDataAsset.MoveSpeed;
            }
        }
        private void Update()
        {
            //ターゲット(プレイヤー)とナビが存在しているか
            if (targetPlayer != null && navMeshAgent != null)
            {
                //プレイヤーの現在位置を毎フレーム目的地としてカーナビにセットする
                navMeshAgent.SetDestination(targetPlayer.position);
            }
        }



        //もう無理ですなあ
    }
}