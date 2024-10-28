using System;
using System.Threading;
using System.Threading.Tasks;
using Lua;
using Lua.Standard;
using VContainer;
using ZeroMessenger;

namespace AnnulusClicker
{
    public class ItemOnClickFilter : MessageFilter<ClickEvent>
    {
        private LuaState _itemOnClickLuaState;
        private Shop _shop;

        [Inject]
        public void Constructor(Shop shop)
        {
            _shop = shop;
            _itemOnClickLuaState = LuaState.Create();

            _itemOnClickLuaState.OpenStandardLibraries();
        }

        public override async ValueTask InvokeAsync(ClickEvent message, CancellationToken cancellationToken,
            Func<ClickEvent, CancellationToken, ValueTask> next)
        {
            var currentPoint = message.Score;

            foreach (var item in _shop.Items)
            {
                var code = item.ClickEventCode;
                if (code == "") continue;

                _itemOnClickLuaState.Environment["score"] = currentPoint;
                _itemOnClickLuaState.Environment["amount"] = item.Amount;
                var results = await _itemOnClickLuaState.DoStringAsync(code, cancellationToken: cancellationToken);
                var result = (int)results[0].Read<float>();

                currentPoint = result;
            }

            message.Score = currentPoint;

            await next(message, cancellationToken);
        }
    }
}
