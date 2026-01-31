using GGJ2026.Core.Managers;
using UnityEngine;

namespace GGJ2026.InGame
{
    /// <summary>
    /// プレイヤーのステータス（HP, Speed, Attack）を管理するクラス
    /// </summary>
    public class PlayerManager : Singleton<PlayerManager>
    {
        [Header("Player Stats")]
        [SerializeField] private int maxHp = 100;
        [SerializeField] private float speed = 10f;
        [SerializeField] private int attackPower = 5;

        // 外部公開用のプロパティ
        public int CurrentHp { get; private set; }
        public float Speed => speed;
        public int AttackPower => attackPower;

        // シーン遷移で破棄するため false
        protected override bool UseDontDestroyOnLoad => false;

        // Singleton<T> の仕様上、UseDontDestroyOnLoadがfalseの場合は
        // 自動でInitが呼ばれないため、Awakeで明示的に呼ぶ
        private new void Awake()
        {
            InitializeSingleton();
        }

        public override void Init()
        {
            base.Init();

            // HPを全快で初期化
            CurrentHp = maxHp;
            Debug.Log($"PlayerManager Initialized. HP: {CurrentHp}");
        }

        /// <summary>
        /// ダメージ処理
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (CurrentHp <= 0) return; // 既に死んでいたら無視

            CurrentHp -= damage;
            if (CurrentHp < 0) CurrentHp = 0;

            Debug.Log($"Player Took Damage: {damage}, CurrentHP: {CurrentHp}");

            // HPが0になったらゲームオーバー処理へ
            if (CurrentHp == 0)
            {
                OnPlayerDead();
            }
        }

        /// <summary>
        /// 回復処理
        /// </summary>
        public void Heal(int amount)
        {
            if (CurrentHp <= 0) return; // 死んでいたら回復しない（蘇生が必要ならロジックを変える）

            CurrentHp += amount;
            if (CurrentHp > maxHp) CurrentHp = maxHp;

            Debug.Log($"Player Healed: {amount}, CurrentHP: {CurrentHp}");
        }

        /// <summary>
        /// プレイヤー死亡時の処理
        /// </summary>
        private void OnPlayerDead()
        {
            Debug.Log("Player is Dead.");
            
            // InGameManagerが存在していれば、ゲーム終了（リザルト）へ移行させる例
            /*
            if (InGameManager.IsValid())
            {
                // InGameManager側に PlayerDead() などのメソッドを作って呼ぶと良い
                // InGameManager.I.ChangeState(InGameState.Result); 
            }
            */
        }
        
        /// <summary>
        /// パッシブスキルの効果を適用（または解除）する
        /// </summary>
        /// <param name="skill">スキルの実体</param>
        /// <param name="isEquip">装備ならtrue, 外すならfalse</param>
        public void ApplyPassiveEffect(PassiveSkillInstance skill, bool isEquip)
        {
            if (skill == null) return;

            // 装備時は「+値」、外す時は「-値」にする
            float value = isEquip ? skill.Value : -skill.Value;

            // 何に対する補正かで分岐
            switch (skill.Config._modifierType)
            {
                case ModifierType.PlayerParam:
                    ApplyPlayerParam(skill.Config._targetPlayerParam, value);
                    break;

                case ModifierType.ItemParam:
                    // 今回はプレイヤーの実装なので省略（アイテム価格変動など）
                    break;
                
                // ... 他のタイプも必要に応じて追加
            }
        }

        private void ApplyPlayerParam(PlayerParam param, float value)
        {
            switch (param)
            {
                case PlayerParam.Health:
                    // MaxHPが増える処理（現在HPの割合維持などは一旦省略）
                    maxHp += (int)value;
                    Debug.Log($"MaxHP Changed: {maxHp} ({value})");
                    break;

                case PlayerParam.AttackPower:
                    attackPower += (int)value;
                    Debug.Log($"AttackPower Changed: {attackPower} ({value})");
                    break;

                case PlayerParam.Agility:
                    speed += value;
                    Debug.Log($"Speed Changed: {speed} ({value})");
                    break;
            }
        }

        // =========================================================
        // デバッグ用 UI (左上にステータス表示 & ダメージテストボタン)
        // 本番ビルドでは #if UNITY_EDITOR 等で囲うか削除してください
        // =========================================================
        private void OnGUI()
        {
            // 左上にウィンドウを表示
            GUILayout.BeginArea(new Rect(10, 10, 250, 200), "Debug: Player Status", GUI.skin.window);

            GUILayout.Label($"HP: {CurrentHp} / {maxHp}");
            GUILayout.Label($"Speed: {Speed}");
            GUILayout.Label($"Attack: {AttackPower}");

            GUILayout.Space(10);

            if (GUILayout.Button("Damage 10"))
            {
                TakeDamage(10);
            }

            if (GUILayout.Button("Heal 10"))
            {
                Heal(10);
            }

            GUILayout.EndArea();
        }
    }
}