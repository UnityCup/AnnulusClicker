using LitMotion;
using LitMotion.Extensions;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace AnnulusClicker
{
    public class AnnulusButtonView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Vector2 _pressedSize;
        [SerializeField] private float _pressDuration;
        [SerializeField] private Ease _pressEase;
        [SerializeField] private float _releaseDuration;
        [SerializeField] private Ease _releaseEase;
        private Vector2 _initialSize;
        private MotionHandle _motionHandle;
        public Observable<Unit> OnClick => _button.OnClickAsObservable();

        private void Awake()
        {
            _motionHandle.AddTo(this);
            _initialSize = transform.localScale;

            _button.OnPointerDownAsObservable()
                .Subscribe(_ =>
                {
                    if (_motionHandle.IsActive()) _motionHandle.Cancel();
                    _motionHandle = LMotion
                        .Create((Vector2)transform.localScale, _pressedSize, _pressDuration)
                        .WithEase(_pressEase)
                        .BindToLocalScaleXY(transform);
                })
                .AddTo(this);

            _button.OnPointerUpAsObservable()
                .Subscribe(_ =>
                {
                    if (_motionHandle.IsActive()) _motionHandle.Cancel();
                    _motionHandle = LMotion
                        .Create((Vector2)transform.localScale, _initialSize, _pressDuration)
                        .WithEase(_releaseEase)
                        .BindToLocalScaleXY(transform);
                })
                .AddTo(this);
        }
    }
}
