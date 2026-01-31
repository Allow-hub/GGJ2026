using GGJ2026.Interface;

namespace GGJ2026.InGame.Events
{
    /// <summary>
    /// 攻撃イベントクラス
    /// </summary>
    public class AttackEvents
    {
        /// <summary>
        /// 攻撃を実行したクラス
        /// </summary>
        /// <value></value>
        public IAttackable Attacker { get; private set; }

        /// <summary>
        /// 攻撃を受けたクラス
        /// </summary>
        /// <value></value>
        public IDamageable Target { get; private set; }

        public AttackEvents(IAttackable attacker, IDamageable target)
        {
            Attacker = attacker;
            Target = target;
        }
    }
}
