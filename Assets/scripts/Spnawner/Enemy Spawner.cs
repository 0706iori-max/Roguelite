using UnityEngine;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;

namespace TPSRoguelite.InGame.Spawner
{
    public class EnemySpawner : MonoBehaviour
    {
        private const float SPOWN_INTERVAL = 3.0f;
        
        private const float MAX_SPAWN_DISTANCE = 2.0f;
        
        [SerializeField]private GameObject enemyPrefab ;
        
        [SerializeField] private Transform[] spawnpoints;

        private void Start()
        {
            SpawnLoopAsync().Forget();
        }
        private async UniTaskVoid SpawnLoopAsync()
        {
            var token = this.GetCancellationTokenOnDestroy();
            while (true)
            {
               
                await UniTask.Delay(System.TimeSpan.FromSeconds(SPOWN_INTERVAL), cancellationToken: token);
                SpawnEnemy();
            }
        }

        private void SpawnEnemy()
        {
            if (enemyPrefab == null)
            {
                return;
            }
            int randomIndex =UnityEngine.Random.Range(0, spawnpoints.Length);
            Transform spawnpoint=spawnpoints[randomIndex];
            
            
            Vector3 safePosition = spawnpoint.position;
            
            if (NavMesh.SamplePosition(spawnpoint.position, out NavMeshHit hit, MAX_SPAWN_DISTANCE, NavMesh.AllAreas))
            {
                safePosition = hit.position;
            }
            else
            {
                Debug.LogWarning("近くにスポーン位置が見つかりませんでした。");
                return;
            }
            
            
            GameObject enmy = Instantiate(enemyPrefab, safePosition, spawnpoint.rotation);
            Debug.Log("敵を召喚しました！！");
        }


       
    }
}