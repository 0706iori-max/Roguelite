using UnityEngine;

namespace InGame.Data
{
    // 右クリックメニューからこのデータを作成できるようにするための記述
    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "ScriptableObjects/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        // [field: SerializeField] をつけると、private set なのにUnityの画面（Inspector）からは編集できるようになります。

        /// <summary>
        /// 敵の名前
        /// </summary>
        [field: SerializeField] public string EnemyName { get; private set; }

        /// <summary>
        /// 最大HP
        /// </summary>
        [field: SerializeField] public int MaxHp { get; private set; }

        /// <summary>
        /// 移動速度
        /// </summary>
        [field: SerializeField] public float MoveSpeed { get; private set; }
    }
}