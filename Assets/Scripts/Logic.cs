using VContainer;
using VContainer.Unity;
using ZeroMessenger;

namespace AnnulusClicker
{
    public class Logic : IInitializable
    {
        [Inject] private MessageBroker<AddScoreEvent> _addScoreEventBroker;
        [Inject] private IMessageSubscriber<ClickEvent> _clickEventSubscriber;
        [Inject] private IMessageSubscriber<ItemBuyEvent> _itemBuyEventSubscriber;
        [Inject] private Score _score;
        [Inject] private Shop _shop;

        public void Initialize()
        {
            _clickEventSubscriber.Subscribe(click =>
            {
                _addScoreEventBroker.Publish(new AddScoreEvent
                {
                    Value = click.Score
                });
            });

            _addScoreEventBroker.Subscribe(addScoreEvent => { _score.Add(addScoreEvent.Value); });

            _itemBuyEventSubscriber.Subscribe(itemBuyEvent =>
            {
                var item = _shop.Items[itemBuyEvent.Index];

                if (_score.Value >= item.Price)
                {
                    _score.Add(-item.Price);
                    item.Amount++;
                }
            });
        }
    }
}
