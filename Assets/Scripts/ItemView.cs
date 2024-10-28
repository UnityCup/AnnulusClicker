using System.Numerics;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnnulusClicker
{
    public class ItemView : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private TMP_Text _amountText;
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _buttonText;
        [SerializeField] public int Index;
        [SerializeField] private Color _enabledColor;
        [SerializeField] private Color _disabledColor;
        public Observable<Unit> OnClick => _button.OnClickAsObservable();

        public void SetIndex(int index)
        {
            Index = index;
        }

        public void SetSprite(Sprite sprite)
        {
            _image.sprite = sprite;
        }

        public void SetPrice(BigInteger price)
        {
            _priceText.text = price.ToString();
        }

        public void SetAmount(BigInteger amount)
        {
            _amountText.text = $"x{amount.ToString()}";
        }

        public void SetEnabled()
        {
            _button.interactable = true;
            _buttonText.color = _enabledColor;
        }

        public void SetDisabled()
        {
            _button.interactable = false;
            _buttonText.color = _disabledColor;
        }
    }
}
