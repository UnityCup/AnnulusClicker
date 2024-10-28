using Lua;
using Lua.Standard;
using VContainer;
using VContainer.Unity;
using ZeroMessenger;

namespace AnnulusClicker
{
    public class AutoClicker : IFixedTickable
    {
        private IMessagePublisher<AddScoreEvent> _addScoreEventPublisher;
        private LuaState _autoClickCalcLuaState;
        private Score _score;
        private Shop _shop;

        public void FixedTick()
        {
            foreach (var item in _shop.Items)
            {
                var code = item.AutoClickerCode;
                if (code == "") continue;

                _autoClickCalcLuaState.Environment["amount"] = item.Amount;
                _autoClickCalcLuaState.Environment["score"] = _score.Value;

                var results = _autoClickCalcLuaState.DoStringAsync(code).Result;
                var result = results[0].Read<double>() / 60f;

                _addScoreEventPublisher.Publish(new AddScoreEvent
                {
                    Value = result
                });
            }
        }

        [Inject]
        public void Constructor(Shop shop, IMessagePublisher<AddScoreEvent> addScoreEventPublisher, Score score)
        {
            _shop = shop;
            _score = score;
            _addScoreEventPublisher = addScoreEventPublisher;
            _autoClickCalcLuaState = LuaState.Create();


            _autoClickCalcLuaState.OpenStandardLibraries();
        }
    }
}
