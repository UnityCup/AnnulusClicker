using System;
using UnityEngine;

namespace AnnulusClicker
{
    [Serializable]
    public class Shop
    {
        public Item[] Items;
    }

    [Serializable]
    public class Item
    {
        public Sprite Sprite;
        public int Amount;
        public int InitialPrice;
        public float PriceDivide;
        public float Pow;

        [Multiline] public string ClickEventCode;
        [Multiline] public string AutoClickerCode;

        public int Price =>
            Amount == 0 ? InitialPrice : Mathf.CeilToInt(Mathf.Pow((Amount + 1) / PriceDivide, Pow) * InitialPrice);
    }
}
