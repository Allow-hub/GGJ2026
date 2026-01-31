using GGJ2026.InGame.Enemy;
using UnityEngine;
using UnityEngine.UI;

namespace GGJ2026.InGame
{
    /// <summary>
    /// Hpバーの表記
    /// </summary>
    public class HpView : MonoBehaviour
    {
        [SerializeField] private EnemyController enemyController;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private Slider slider;
        [SerializeField] private bool isEnemy = false;
        [SerializeField] private float lerpSpeed = 10f;

        private int lastHp = -1;
        private float displayHp;

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            UpdateHp();
        }

        private void Initialize()
        {
            if (slider == null)
            {
                Debug.LogError("Slider is not assigned.");
                enabled = false;
                return;
            }

            if (isEnemy && enemyController == null)
            {
                Debug.LogError("EnemyController is not assigned.");
                enabled = false;
                return;
            }

            if (!isEnemy && playerController == null)
            {
                Debug.LogError("PlayerController is not assigned.");
                enabled = false;
                return;
            }

            int maxHp = isEnemy ? enemyController.MaxHP : playerController.MaxHp; //プロパティを更新したら修正 
            
            slider.maxValue = maxHp;

            int currentHp = isEnemy
                ? enemyController.CurrentHP
                : playerController.CurrentHp;

            displayHp = currentHp;
            slider.value = displayHp;
            lastHp = currentHp;
        }

        private void UpdateHp()
        {
            int currentHp = isEnemy
                ? enemyController.CurrentHP
                : playerController.CurrentHp;

            // HPが変わったら目標値更新
            if (currentHp != lastHp)
            {
                lastHp = currentHp;
            }

            // Lerpで表示HPを追従
            displayHp = Mathf.Lerp(displayHp, currentHp, Time.deltaTime * lerpSpeed);
            slider.value = displayHp;

            // 誤差対策（ピタッと止める）
            if (Mathf.Abs(displayHp - currentHp) < 0.1f)
            {
                displayHp = currentHp;
                slider.value = displayHp;
            }
        }
    }
}