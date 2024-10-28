using System;
using R3;
using VContainer;
using VContainer.Unity;
using ZeroMessenger;

namespace AnnulusClicker
{
    public class Presenter : IInitializable, IDisposable, ITickable
    {
        [Inject] private AnnulusButtonView _annulusButtonView;

        private DisposableBag _disposableBag;
        [Inject] private IMessagePublisher<ItemBuyEvent> _itemBuyEventPublisher;
        [Inject] private IMessagePublisher<ClickEvent> _publisher;
        [Inject] private Score _score;
        [Inject] private ScoreView _scoreView;
        [Inject] private Shop _shop;
        [Inject] private ShopView _shopView;

        public void Dispose()
        {
            _disposableBag.Dispose();
        }

        public void Initialize()
        {
            _annulusButtonView.OnClick.Subscribe(_ =>
                {
                    _publisher.Publish(new ClickEvent
                    {
                        Score = 1
                    });
                })
                .AddTo(ref _disposableBag);

            _shopView.OnBuyButtonClicked.Subscribe(index =>
            {
                _itemBuyEventPublisher.Publish(new ItemBuyEvent
                {
                    Index = index
                });
            }).AddTo(ref _disposableBag);

            foreach (var item in _shop.Items) _shopView.AddItemView(item.Sprite, item.Price);
        }

        public void Tick()
        {
            _scoreView.SetText(_score.Value);

            for (var i = 0; i < _shop.Items.Length; i++)
            {
                var item = _shop.Items[i];
                var price = item.Price;
                _shopView.UpdateItemView(i, price, item.Amount, _score.Value >= price);
            }
        }
    }
}
