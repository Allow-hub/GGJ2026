using UnityEngine;

namespace GGJ2026.InGame
{
    public class ItemFactoryTester : MonoBehaviour
    {
        [Header("テストしたいアイテム設定"), SerializeField]
        private ItemConfig testConfig;

        private void Start()
        {
            // ItemFactoryが存在するかチェック
            if (!ItemFactory.IsValid())
            {
                Debug.LogError("シーンに ItemFactory がありません！作成してください。");
                return;
            }

            if (testConfig == null)
            {
                Debug.LogWarning("テスト用の ItemConfig がセットされていません。");
                return;
            }

            Debug.Log("--- アイテム生成テスト開始 ---");

            // ファクトリーを使ってアイテムデータを生成
            ItemInstance item = ItemFactory.I.CreateItem(testConfig);

            // 結果をログ出力
            Debug.Log($"生成されたアイテム: {item.Config.itemName}");

            if (item.PassiveSkill != null)
            {
                Debug.Log($"[成功] パッシブスキルが付与されました: {item.PassiveSkill.GetDescription()}");
            }
            else
            {
                Debug.Log("パッシブスキルは付与されませんでした。");
            }

            Debug.Log("--- テスト終了 ---");
        }
    }
}