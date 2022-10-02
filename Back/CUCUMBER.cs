using MassTransit;

// ReSharper disable InconsistentNaming

namespace Back;

public static class CucumberExtensions
{
    public static IConsumerRegistrationConfigurator<T> AddCucumber<T>(this IBusRegistrationConfigurator config)
        where T : class, ICucumber =>
        config.AddConsumer<T>();
}

public interface ICucumber : IConsumer
{
}

public interface ICucumber<T> : IConsumer<T>, ICucumber where T : class
{
}