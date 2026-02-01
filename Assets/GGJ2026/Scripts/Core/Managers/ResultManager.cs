using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GGJ2026.Core.Managers
{
    /// <summary>
    /// リザルト管理クラス
    /// </summary>
    public class ResultManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI stageText;
        [SerializeField] private Button continueButton;


        private void Awake()
        {
            scoreText.text = $"{PointManager.I.Points}";
            timeText.text = GameManager.I.AliveTimer.ToString("F0");
            stageText.text = $"{GameManager.I.ResultFloor}";
        }

        private void Start()
        {
            continueButton.onClick.AddListener(() => GameManager.I.ChangeTitleState());
        }
    }
}
