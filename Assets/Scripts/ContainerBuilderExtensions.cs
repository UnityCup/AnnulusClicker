using System;
using VContainer;
using ZeroMessenger.Internal;

namespace ZeroMessenger.VContainer
{
    public static class ContainerBuilderExtensions
    {
        public static void AddZeroMessenger(this IContainerBuilder builder)
        {
            AddZeroMessenger(builder, _ => { });
        }

        public static void AddZeroMessenger(this IContainerBuilder builder, Action<MessageBrokerBuilder> configuration)
        {
            var brokerBuilder = new MessageBrokerBuilder(builder);
            configuration(brokerBuilder);

            builder.Register(typeof(MessageBroker<>), Lifetime.Singleton);
            builder.Register(typeof(IMessagePublisher<>), typeof(MessageBrokerPublisher<>), Lifetime.Singleton);
            builder.Register(typeof(IMessageSubscriber<>), typeof(MessageBrokerSubscriber<>), Lifetime.Singleton);
            builder.Register(typeof(MessageFilterProvider<>), Lifetime.Singleton);
        }
    }

    public readonly struct MessageBrokerBuilder
    {
        readonly IContainerBuilder builder;

        public MessageBrokerBuilder(IContainerBuilder builder)
        {
            this.builder = builder;
        }

        public void AddFilter<TFilter>() where TFilter : MessageFilterBase
        {
            builder.Register<MessageFilterBase, TFilter>(Lifetime.Transient);
        }

        public void AddFilter<TFilter>(TFilter filter) where TFilter : MessageFilterBase
        {
            builder.RegisterInstance<MessageFilterBase>(filter);
        }

        public void AddFilter(Type type)
        {
            builder.Register(typeof(MessageFilter<>), type, Lifetime.Transient);
        }
    }
}