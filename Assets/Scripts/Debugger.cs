using UnityEngine;
using VContainer;
using VContainer.Unity;
using ZeroMessenger;

namespace AnnulusClicker
{
    public class Debugger : IInitializable
    {
        [Inject] private IMessageSubscriber<ClickEvent> _clickEventSubscriber;
        [Inject] private Score _score;

        public void Initialize()
        {
            _clickEventSubscriber.Subscribe(_ => { Debug.Log($"Score: {_score.Value}"); });
        }
    }
}
