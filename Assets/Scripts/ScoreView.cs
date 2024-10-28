using TMPro;
using UnityEngine;

namespace AnnulusClicker
{
    public class ScoreView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _tmpText;

        public void SetText(double score)
        {
            _tmpText.text = score.ToString("F0");
        }
    }
}
