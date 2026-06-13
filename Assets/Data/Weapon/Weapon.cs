
using TPSRoguelite.InGame.Enum;
using UnityEngine;

namespace InGame.Data
{
    [CreateAssetMenu(fileName = "NewWeaponData", menuName = "ScriptableObjects/WeaponData")]
    public class WeaponData : ScriptableObject
    {
        /// <summary>
        /// 武器の名前
        /// </summary>
        [field: SerializeField] public string WeaponName { get; private set; }

        /// <summary>
        /// 連射タイプ
        /// </summary>
        [field: SerializeField] public FireType WeaponFireType { get; private set; }

        /// <summary>
        /// 攻撃力
        /// </summary>
        [field: SerializeField] public int AttackPower { get; private set; }

        /// <summary>
        /// フルオートやバースト時の連射間隔
        /// </summary>
        [field: SerializeField] public float FireInterval { get; private set; }

        /// <summary>
        /// 次の球が撃てるまでの待機時間
        /// </summary>
        [field: SerializeField] public float FireRate { get; private set; }

        /// <summary>
        /// マガジンの最大弾数
        /// </summary>
        [field: SerializeField] public int MaxAmmo { get; private set; }

        /// <summary>
        /// リロードにかかる時間
        /// </summary>
        [field: SerializeField] public float ReloadTime { get; private set; }
    }
}