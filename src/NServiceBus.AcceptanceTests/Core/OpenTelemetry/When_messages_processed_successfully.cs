namespace NServiceBus.AcceptanceTests.Core.OpenTelemetry;

using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.AcceptanceTesting;
using NUnit.Framework;
using Conventions = AcceptanceTesting.Customization.Conventions;

public class When_messages_processed_successfully : OpenTelemetryAcceptanceTest
{
    [Test]
    public async Task Should_report_successful_message_metric()
    {
        using var metricsListener = TestingMetricListener.SetupNServiceBusMetricsListener();

        _ = await Scenario.Define<Context>()
            .WithEndpoint<EndpointWithMetrics>(b => b
                .When(async (session, ctx) =>
                {
                    for (var x = 0; x < 5; x++)
                    {
                        await session.SendLocal(new OutgoingMessage());
                    }
                }))
            .Done(c => c.OutgoingMessagesReceived == 5)
            .Run();

        metricsListener.AssertMetric("nservicebus.messaging.successes", 5);
        metricsListener.AssertMetric("nservicebus.messaging.fetches", 5);
        metricsListener.AssertMetric("nservicebus.messaging.failures", 0);

        var successEndpoint = metricsListener.AssertTagKeyExists("nservicebus.messaging.successes", "nservicebus.queue");
        var successType = metricsListener.AssertTagKeyExists("nservicebus.messaging.successes", "nservicebus.message_type");
        var fetchedEndpoint = metricsListener.AssertTagKeyExists("nservicebus.messaging.fetches", "nservicebus.queue");
        var fetchedType = metricsListener.AssertTagKeyExists("nservicebus.messaging.fetches", "nservicebus.message_type").ToString();

        Assert.AreEqual(Conventions.EndpointNamingConvention(typeof(EndpointWithMetrics)), successEndpoint);
        Assert.AreEqual(Conventions.EndpointNamingConvention(typeof(EndpointWithMetrics)), fetchedEndpoint);
        Assert.AreEqual(typeof(OutgoingMessage).FullName, successType);
        Assert.AreEqual(typeof(OutgoingMessage).FullName, fetchedType);
    }

    [Test]
    public async Task Should_only_tag_most_concrete_type_on_metric()
    {
        using var metricsListener = TestingMetricListener.SetupNServiceBusMetricsListener();

        _ = await Scenario.Define<Context>()
            .WithEndpoint<EndpointWithMetrics>(b => b
                .When(async (session, ctx) =>
                {
                    for (var x = 0; x < 5; x++)
                    {
                        await session.SendLocal(new OutgoingWithComplexHierarchyMessage());
                    }
                }))
            .Done(c => c.ComplexOutgoingMessagesReceived == 5)
            .Run();

        metricsListener.AssertMetric("nservicebus.messaging.successes", 5);
        metricsListener.AssertMetric("nservicebus.messaging.fetches", 5);
        metricsListener.AssertMetric("nservicebus.messaging.failures", 0);

        var successEndpoint = metricsListener.AssertTagKeyExists("nservicebus.messaging.successes", "nservicebus.queue");
        var successType = metricsListener.AssertTagKeyExists("nservicebus.messaging.successes", "nservicebus.message_type");
        var fetchedEndpoint = metricsListener.AssertTagKeyExists("nservicebus.messaging.fetches", "nservicebus.queue");
        var fetchedType = metricsListener.AssertTagKeyExists("nservicebus.messaging.fetches", "nservicebus.message_type").ToString();

        Assert.AreEqual(Conventions.EndpointNamingConvention(typeof(EndpointWithMetrics)), successEndpoint);
        Assert.AreEqual(Conventions.EndpointNamingConvention(typeof(EndpointWithMetrics)), fetchedEndpoint);
        Assert.AreEqual(typeof(OutgoingWithComplexHierarchyMessage).FullName, successType);
        Assert.AreEqual(typeof(OutgoingWithComplexHierarchyMessage).FullName, fetchedType);
    }

    class Context : ScenarioContext
    {
        public int OutgoingMessagesReceived;
        public int ComplexOutgoingMessagesReceived;
    }

    class EndpointWithMetrics : EndpointConfigurationBuilder
    {
        public EndpointWithMetrics() => EndpointSetup<OpenTelemetryEnabledEndpoint>();

        class MessageHandler : IHandleMessages<OutgoingMessage>
        {
            readonly Context testContext;

            public MessageHandler(Context testContext) => this.testContext = testContext;

            public Task Handle(OutgoingMessage message, IMessageHandlerContext context)
            {
                Interlocked.Increment(ref testContext.OutgoingMessagesReceived);
                return Task.CompletedTask;
            }
        }

        class ComplexMessageHandler : IHandleMessages<OutgoingWithComplexHierarchyMessage>
        {
            readonly Context testContext;

            public ComplexMessageHandler(Context testContext) => this.testContext = testContext;

            public Task Handle(OutgoingWithComplexHierarchyMessage message, IMessageHandlerContext context)
            {
                Interlocked.Increment(ref testContext.ComplexOutgoingMessagesReceived);
                return Task.CompletedTask;
            }
        }
    }

    public class OutgoingMessage : IMessage
    {
    }

    public class BaseOutgoingMessage : IMessage
    {
    }

    public class OutgoingWithComplexHierarchyMessage : BaseOutgoingMessage
    {
    }
}