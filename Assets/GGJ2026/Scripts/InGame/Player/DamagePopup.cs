using TMPro;
using UnityEngine;

namespace GGJ2026.InGame
{
    public class DamagePopup : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float moveSpeed = 2f; // 右へ移動する速度
        [SerializeField] private float fadeSpeed = 3f; // フェードインする速度
        [SerializeField] private float lifeTime = 1.0f; // 表示されている時間

        private TextMeshPro textMesh;
        private Color textColor;
        private float disappearTimer;

        private void Awake()
        {
            textMesh = GetComponent<TextMeshPro>();
        }

        public void Setup(int damageAmount)
        {
            // テキスト設定 (-10 のように表示)
            textMesh.text = "-" + damageAmount.ToString();
            
            // 赤色に設定
            textColor = Color.red;
            
            // 最初は透明(Alpha = 0)にする
            textColor.a = 0f;
            textMesh.color = textColor;

            disappearTimer = lifeTime;
        }

        private void Update()
        {
            // 1. 左から右へ移動 (X座標をプラス)
            transform.position += new Vector3(moveSpeed, 0, 0) * Time.deltaTime;

            // 2. だんだん濃く表示 (フェードイン)
            if (textColor.a < 1f)
            {
                textColor.a += fadeSpeed * Time.deltaTime;
                textMesh.color = textColor;
            }

            // 3. 一定時間経過後に削除
            disappearTimer -= Time.deltaTime;
            if (disappearTimer < 0)
            {
                Destroy(gameObject);
            }
        }
    }
}