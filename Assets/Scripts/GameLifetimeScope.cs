using UnityEngine;
using VContainer;
using VContainer.Unity;
using ZeroMessenger.VContainer;

namespace AnnulusClicker
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private AnnulusButtonView _annulusButtonView;
        [SerializeField] private ScoreView _scoreView;
        [SerializeField] private ShopView _shopView;
        [SerializeField] private Shop _shop;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.AddZeroMessenger(brokerBuilder => { brokerBuilder.AddFilter<ItemOnClickFilter>(); });

            builder.Register<Score>(Lifetime.Singleton);
            builder.RegisterInstance(_shop).AsSelf();
            builder.RegisterComponent(_shopView);
            builder.RegisterComponent(_annulusButtonView);
            builder.RegisterComponent(_scoreView);
            builder.RegisterEntryPoint<Presenter>();
            builder.RegisterEntryPoint<AutoClicker>();
            builder.RegisterEntryPoint<Logic>();
            builder.RegisterEntryPoint<Debugger>();
        }
    }
}
