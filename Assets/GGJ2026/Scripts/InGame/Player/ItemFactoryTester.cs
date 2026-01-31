using UnityEngine;

namespace GGJ2026.InGame
{
    public class ItemFactoryTester : MonoBehaviour
    {
        private void Start()
        {
            // ItemFactoryが存在するかチェック
            if (!ItemFactory.IsValid())
            {
                Debug.LogError("シーンに ItemFactory がありません！作成してください。");
                return;
            }

            Debug.Log("--- アイテム生成テスト開始 ---");

            // 1. ファクトリーを使ってランダムにアイテムデータを生成 (データ作成)
            ItemInstance item = ItemFactory.I.ChooseItem();

            if (item != null)
            {
                // 結果をログ出力
                Debug.Log($"生成されたアイテムデータ: {item.Config.itemName}");

                if (item.PassiveSkill != null)
                {
                    Debug.Log($"[成功] パッシブスキルが付与されました: {item.PassiveSkill.GetDescription()}");
                }
                else
                {
                    Debug.Log("パッシブスキルは付与されませんでした。");
                }

                // 2. 生成したデータを使って、実際に画面上にスポーンさせる (表示作成)
                ItemFactory.I.SpawnItem(item);
            }
            else
            {
                Debug.LogWarning("ChooseItem でアイテムが生成されませんでした。ItemFactoryのリスト設定を確認してください。");
            }

            Debug.Log("--- テスト終了 ---");
        }
    }
}