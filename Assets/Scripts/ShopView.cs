using System.Collections.Generic;
using R3;
using UnityEngine;

namespace AnnulusClicker
{
    public class ShopView : MonoBehaviour
    {
        [SerializeField] private List<ItemView> _itemViews;
        [SerializeField] private GameObject _itemViewListObject;
        [SerializeField] private ItemView _itemViewPrefab;
        private readonly Subject<int> _onBuyButtonClickedSubject = new();
        public Observable<int> OnBuyButtonClicked => _onBuyButtonClickedSubject;

        public void AddItemView(Sprite sprite, int price)
        {
            var view = Instantiate(_itemViewPrefab, _itemViewListObject.transform);
            view.SetSprite(sprite);
            view.SetPrice(price);
            view.SetAmount(0);
            view.SetIndex(_itemViews.Count);
            _itemViews.Add(view);
            view.OnClick.Subscribe(_ => { _onBuyButtonClickedSubject.OnNext(view.Index); });
        }

        public void UpdateItemView(int index, int price, int amount, bool canBuy)
        {
            var itemView = _itemViews[index];
            itemView.SetPrice(price);
            itemView.SetAmount(amount);
            if (canBuy)
                itemView.SetEnabled();
            else
                itemView.SetDisabled();
        }
    }
}
