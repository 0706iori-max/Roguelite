using Core.Interface;
using InGame.Data;
using UnityEngine;
using UnityEngine.Events;
using InGame.Data; // EnemyDataを使うために追加

namespace InGame.Enemy
{
    public class EnemyState : MonoBehaviour, IDamageble
    {
        // Inspectorでカセットをセットしつつ、他のプログラムからは「読み取り（Get）」だけできるように公開する
        [field: SerializeField] public EnemyData EnemyDataAsset { get; private set; }

        public int CurrentHP { get; private set; }

        public event UnityAction<EnemyState> OnReturnToPoolAction;

        private void OnEnable()
        {
            // カセットがセットされていれば、そのカセットの最大HPを読み込む
            if (EnemyDataAsset != null)
            {
                CurrentHP = EnemyDataAsset.MaxHp;
            }
            else
            {
                Debug.LogError("EnemyDataがセットされていません！");
            }
        }

        public void TakeDamage(int damageAmount)
        {
            // マイナスのダメージ（回復）を防ぐ
            if (damageAmount <= 0)
            {
                return;
            }

            CurrentHP -= damageAmount;
            Debug.Log($"{EnemyDataAsset.EnemyName}に{damageAmount}のダメージ！残りHP:{CurrentHP}");
            if (CurrentHP <= 0) {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log($"{EnemyDataAsset.EnemyName}を倒しました");
            gameObject.SetActive(false);
            OnReturnToPoolAction?.Invoke(this);
        }
    }
}