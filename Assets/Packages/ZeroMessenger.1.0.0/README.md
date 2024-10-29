# Zero Messenger
 Zero-allocation, extremely fast in-memory messaging library for .NET and Unity.

[![NuGet](https://img.shields.io/nuget/v/ZeroMessenger.svg)](https://www.nuget.org/packages/ZeroMessenger)
[![Releases](https://img.shields.io/github/release/AnnulusGames/ZeroMessenger.svg)](https://github.com/AnnulusGames/ZeroMessenger/releases)
[![license](https://img.shields.io/badge/LICENSE-MIT-green.svg)](LICENSE)

English | [日本語](README_JA.md)

## Overview

Zero Messenger is a high-performance messaging library for .NET and Unity. It provides `MessageBroker<T>` as an easy-to-use event system for subscribing and unsubscribing, as well as support for implementing the Pub/Sub pattern with `IMessagePublisher<T>/IMessageSubscriber<T>`.

Zero Messenger is designed with performance as a priority, achieving faster `Publish()` operations than libraries such as [MessagePipe](https://github.com/Cysharp/MessagePipe) and [VitalRouter](https://github.com/hadashiA/VitalRouter). Moreover, there are no allocations during publishing.

![img](docs/img_benchmark_publish_graph.png)

![img](docs/img_benchmark_publish.png)

Additionally, it minimizes allocations when constructing message pipelines compared to other libraries. Below is a benchmark result from executing `Subscribe/Dispose` 10,000 times.

![img](docs/img_benchmark_subscribe.png)

## Installation

### NuGet Packages

Zero Messenger requires .NET Standard 2.1 or later. The package is available on NuGet.

### .NET CLI

```ps1
dotnet add package ZeroMessenger
```

### Package Manager

```ps1
Install-Package ZeroMessenger
```

### Unity

You can use Zero Messenger in Unity by utilizing NuGetForUnity. For more details, see the [Unity](#unity-1) section.

## Quick Start

You can easily implement global Pub/Sub using `MessageBroker<T>.Default`.

```cs
using System;
using ZeroMessenger;

// Subscribe to messages
var subscription = MessageBroker<Message>.Default.Subscribe(x =>
{
    Console.WriteLine(x.Text);
});

// Publish a message
MessageBroker<Message>.Default.Publish(new Message("Hello!"));

// Unsubscribe
subscription.Dispose();

// Type used for the message
public record struct Message(string Text) { }
```

Additionally, an instance of `MessageBroker<T>` can be used similarly to `event` or Rx's `Subject<T>`.

```cs
var broker = new MessageBroker<int>();

broker.Subscribe(x =>
{
    Console.WriteLine(x);
});

broker.Publish(10);

broker.Dispose();
```

## Dependency Injection

By adding Zero Messenger to a DI container, you can easily implement Pub/Sub between services.

Zero Messenger supports Pub/Sub on `Microsoft.Extensions.DependencyInjection`, which requires the [ZeroMessenger.DependencyInjection](https://www.nuget.org/packages/ZeroMessenger.DependencyInjection/) package.

#### .NET CLI

```ps1
dotnet add package ZeroMessenger.DependencyInjection
```

#### Package Manager

```ps1
Install-Package ZeroMessenger.DependencyInjection
```

### Generic Host

Adding `services.AddZeroMessenger()` registers Zero Messenger in `IServiceCollection`. The following example demonstrates Pub/Sub implementation on a Generic Host.

```cs
using ZeroMessenger;
using ZeroMessenger.DependencyInjection;

Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        // Add Zero Messenger
        services.AddZeroMessenger();

        services.AddSingleton<ServiceA>();
        services.AddSingleton<ServiceB>();
    })
    .Build()
    .Run();

public record struct Message(string Text) { }

public class ServiceA
{
    IMessagePublisher<Message> publisher;

    public ServiceA(IMessagePublisher<Message> publisher)
    {
        this.publisher = publisher;
    }

    public Task SendAsync(CancellationToken cancellationToken = default)
    {
        publisher.Publish(new Message("Hello!"));
    }
}

public class ServiceB : IDisposable
{
    IDisposable subscription;

    public ServiceB(IMessageSubscriber<Message> subscriber)
    {
        subscription = subscriber.Subscribe(x =>
        {
            Console.WriteLine(x);
        });
    }

    public void Dispose()
    {
        subscription.Dispose();
    }
}
```

## Publisher/Subscriber

The interfaces used for Pub/Sub are `IMessagePublisher<T>` and `IMessageSubscriber<T>`. The `MessageBroker<T>` implements both of these interfaces.

```cs
public interface IMessagePublisher<T>
{
    void Publish(T message, CancellationToken cancellationToken = default);
    ValueTask PublishAsync(T message, AsyncPublishStrategy publishStrategy = AsyncPublishStrategy.Parallel, CancellationToken cancellationToken = default);
}

public interface IMessageSubscriber<T>
{
    IDisposable Subscribe(MessageHandler<T> handler);
    IDisposable SubscribeAwait(AsyncMessageHandler<T> handler, AsyncSubscribeStrategy subscribeStrategy = AsyncSubscribeStrategy.Sequential);
}
```

### IMessagePublisher

`IMessagePublisher<T>` is an interface for publishing messages. You can publish messages using `Publish()`, and with `PublishAsync()`, you can wait for all processing to complete.

```cs
IMessagePublisher<Message> publisher;

// Publish a message (Fire-and-forget)
publisher.Publish(new Message("Foo!"));

// Publish a message and wait for all subscribers to finish processing
await publisher.PublishAsync(new Message("Bar!"), AsyncPublishStrategy.Parallel, cancellationToken);
```

You can specify `AsyncPublishStrategy` to change how asynchronous message handlers are handled.

| `AsyncPublishStrategy`            | -                                                                          |
| --------------------------------- | -------------------------------------------------------------------------- |
| `AsyncPublishStrategy.Parallel`   | All asynchronous message handlers are executed in parallel.                |
| `AsyncPublishStrategy.Sequential` | Asynchronous message handlers are queued and executed one by one in order. |

### IMessageSubscriber

`IMessageSubscriber<T>` is an interface for subscribing to messages. It provides an extension method `Subscribe()` that accepts an `Action<T>`, allowing you to easily subscribe using lambda expressions. You can unsubscribe by calling `Dispose()` on the returned `IDisposable`.

```cs
IMessageSubscriber<Message> subscriber;

// Subscribe to messages
var subscription = subscriber.Subscribe(x =>
{
    Console.WriteLine(x.Text);
});

// Unsubscribe
subscription.Dispose();
```

You can also perform asynchronous processing within the subscription using `SubscribeAwait()`.

```cs
var subscription = subscriber.SubscribeAwait(async (x, ct) =>
{
    await FooAsync(x, ct);
}, AsyncSubscribeStrategy.Sequential);
```

By specifying `AsyncSubscribeStrategy`, you can change how messages are handled when received during processing.

| `AsyncSubscribeStrategy`            | -                                                            |
| ----------------------------------- | ------------------------------------------------------------ |
| `AsyncSubscribeStrategy.Sequential` | Messages are queued and executed in order.                   |
| `AsyncSubscribeStrategy.Parallel`   | Messages are executed in parallel.                           |
| `AsyncSubscribeStrategy.Switch`     | Cancels the ongoing processing and executes the new message. |
| `AsyncSubscribeStrategy.Drop`       | Ignores new messages during ongoing processing.              |

## Filter

Filters allow you to add processing before and after message handling.

### Creating a Filter

To create a new filter, define a class that implements `IMessageFilter<T>`.

```cs
public class NopFilter<T> : IMessageFilter<T>
{
    public async ValueTask InvokeAsync(T message, CancellationToken cancellationToken, Func<T, CancellationToken, ValueTask> next)
    {
        try
        {
            // Call the next processing step
            await next(message, cancellationToken);
        }
        catch
        {
            throw;
        }
        finally
        {
            
        }
    }
}
```

The definition of `IMessageFilter<T>` adopts the async decorator pattern, which is also used in [ASP.NET Core middleware](https://learn.microsoft.com/ja-jp/aspnet/core/fundamentals/middleware/?view=aspnetcore-8.0).

Here’s an example of a filter that adds logging before and after processing.

```cs
public class LoggingFilter<T> : IMessageFilter<T>
{
    public async ValueTask InvokeAsync(T message, CancellationToken cancellationToken, Func<T, CancellationToken, ValueTask> next)
    {
        Console.WriteLine("Before");
        await next(message, cancellationToken);
        Console.WriteLine("After");
    }
}
```

### Adding Filters

There are several ways to add a created filter.

If adding directly to `MessageBroker<T>`, use `AddFilter<T>()`. The order of filter application will follow the order of addition.

```cs
var broker = new MessageBroker<int>();

// Add a filter
broker.AddFilter<LoggingFilter<int>>();
```

To add a global filter to a publisher in the DI container, configure it within the `AddZeroMessenger()` method.

```cs
Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        services.AddZeroMessenger(messenger =>
        {
            // Specify the type to add
            messenger.AddFilter<LoggingFilter<Message>>();

            // Add with open generics
            messenger.AddFilter(typeof(LoggingFilter<>));
        });
    })
    .Build()
    .Run();
```

To add individual filters when subscribing, you can use the `WithFilter<T>() / WithFilters()` extension methods.

```cs
IMessageSubscriber<Message> subscriber;

subscriber
    .WithFilter<LoggingFilter<Message>>()
    .Subscribe(x =>
    {
        
    });
```

### PredicateFilter

Zero Messenger provides `PredicateFilter<T>`. When you pass a `Predicate<T>` as an argument to `AddFilter<T>()` or `WithFilter<T>()`, a `PredicateFilter<T>` created based on that predicate is automatically added.

```cs
public record struct FooMessage(int Value);

IMessageSubscriber<FooMessage> subscriber;

subscriber
    .WithFilter(x => x.Value >= 0) // Exclude values less than 0
    .Subscribe(x =>
    {
        
    });
```

## R3

Zero Messenger supports integration with [Cysharp/R3](https://github.com/Cysharp/R3). To enable this feature, add the `ZeroMessenger.R3` package.

### .NET CLI

```ps1
dotnet add package ZeroMessenger.R3
```

### Package Manager

```ps1
Install-Package ZeroMessenger.R3
```

By adding ZeroMessenger.R3, you gain access to operators for converting `IMessageSubscriber<T>` to `Observable<T>` and connecting `Observable<T>` to `IMessagePublisher<T>`.

```cs
// Convert IMessageSubscriber<T> to Observable<T>
subscriber.ToObservable()
    .Subscribe(x => { });

// Subscribe to Observable<T> and convert it to IMessagePublisher<T>'s Publish()
observable.SubscribeToPublish(publisher);

// SubscribeAwait to Observable<T> and convert it to IMessagePublisher<T>'s PublishAsync()
observable.SubscribeAwaitToPublish(publisher, AwaitOperation.Sequential, AsyncPublishStrategy.Parallel);
```

## Unity

You can use Zero Messenger in Unity by installing NuGet packages via NugetForUnity.

### Requirements

* Unity 2021.3 or later

### Installation

1. Install [NugetForUnity](https://github.com/GlitchEnzo/NuGetForUnity).

2. Open the NuGet window by selecting `NuGet > Manage NuGet Packages`, search for the `ZeroMessenger` package, and install it.
    ![img](docs/img_nugetforunity.png)

### VContainer

There is also an extension package available for handling Zero Messenger with VContainer's DI container.

To install ZeroMessenger.VContainer, open the Package Manager window by selecting `Window > Package Manager`, then use `[+] > Add package from git URL` and enter the following URL:

```plaintext
https://github.com/AnnulusGames/ZeroMessenger.git?path=src/ZeroMessenger.Unity/Assets/ZeroMessenger.VContainer
```

By introducing ZeroMessenger.VContainer, the `IContainerBuilder` gains the `AddZeroMessenger()` extension method. Calling this method adds Zero Messenger to the DI container, allowing `IMessagePublisher<T>` and `IMessageSubscriber<T>` to be injected.

```cs
using VContainer;
using VContainer.Unity;
using ZeroMessenger.VContainer;

public class ExampleLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Add Zero Messenger
        builder.AddZeroMessenger();
    }
}
```

## License

This library is released under the [MIT License](LICENSE).