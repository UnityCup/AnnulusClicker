using R3;
using UnityEngine;
using UnityEngine.UI;

namespace AnnulusClicker
{
    public class AnnulusButtonView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        public Observable<Unit> OnClick => _button.OnClickAsObservable();
    }
}
