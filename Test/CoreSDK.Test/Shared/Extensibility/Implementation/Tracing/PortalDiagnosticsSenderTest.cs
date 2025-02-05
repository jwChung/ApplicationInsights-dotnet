﻿namespace Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing
{
    using System;
    using System.Collections.Generic;
#if CORE_PCL || NET45 || NET46
    using System.Diagnostics.Tracing;
#endif
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing.Mocks;
#if NET40
    using Microsoft.Diagnostics.Tracing;
#endif
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestFramework;

    [TestClass]
    public class PortalDiagnosticsSenderTest
    {
        private readonly IList<ITelemetry> sendItems = new List<ITelemetry>();
        private readonly PortalDiagnosticsSender nonThrottlingPortalSender;

        private readonly PortalDiagnosticsSender throttlingPortalSender;

        private readonly IDiagnoisticsEventThrottlingManager throttleAllManager
            = new DiagnoisticsEventThrottlingManagerMock(true);

        private readonly IDiagnoisticsEventThrottlingManager dontThrottleManager
            = new DiagnoisticsEventThrottlingManagerMock(false);

        public PortalDiagnosticsSenderTest()
        {
            var configuration =
                new TelemetryConfiguration
                {
                    TelemetryChannel = new StubTelemetryChannel { OnSend = item => this.sendItems.Add(item) },
                    InstrumentationKey = Guid.NewGuid().ToString()
                };

            this.nonThrottlingPortalSender = new PortalDiagnosticsSender(
                configuration, 
                this.dontThrottleManager);

            this.throttlingPortalSender = new PortalDiagnosticsSender(
                configuration,
                this.throttleAllManager); 
        }

        [TestMethod]
        public void TestSendingOfEvent()
        {
            var evt = new TraceEvent
            {
                MetaData = new EventMetaData
                {
                    EventId = 10,
                    Keywords = 0x20,
                    Level = EventLevel.Warning,
                    MessageFormat = "Error occured at {0}, {1}"
                },
                Payload = new[] { "My function", "some failure" }
            };

            this.nonThrottlingPortalSender.Send(evt);

            Assert.AreEqual(1, this.sendItems.Count);
            var trace = this.sendItems[0] as TraceTelemetry;
            Assert.IsNotNull(trace);
            Assert.AreEqual(
                "AI (Internal): Error occured at My function, some failure", 
                trace.Message);
            Assert.AreEqual(2, trace.Properties.Count);
        }

        [TestMethod]
        public void TestSendingWithSeparateInstrumentationKey()
        {
            var diagnosticsInstrumentationKey = Guid.NewGuid().ToString();
            this.nonThrottlingPortalSender.DiagnosticsInstrumentationKey = diagnosticsInstrumentationKey;
            var evt = new TraceEvent
            {
                MetaData = new EventMetaData
                {
                    EventId = 10,
                    Keywords = 0x20,
                    Level = EventLevel.Warning,
                    MessageFormat = "Error occured at {0}, {1}"
                },
                Payload = new[] { "My function", "some failure" }
            };

            this.nonThrottlingPortalSender.Send(evt);

            Assert.AreEqual(1, this.sendItems.Count);
            var trace = this.sendItems[0] as TraceTelemetry;
            Assert.IsNotNull(trace);
            Assert.AreEqual(diagnosticsInstrumentationKey, trace.Context.InstrumentationKey);
        }

        [TestMethod]
        public void TestSendingEmptyPayload()
        {
            var evt = new TraceEvent
            {
                MetaData = new EventMetaData
                {
                    EventId = 10,
                    Keywords = 0x20,
                    Level = EventLevel.Warning,
                    MessageFormat = "Something failed"
                },
                Payload = null
            };

            this.nonThrottlingPortalSender.Send(evt);

            Assert.AreEqual(1, this.sendItems.Count);
            var trace = this.sendItems[0] as TraceTelemetry;
            Assert.IsNotNull(trace);
            Assert.AreEqual(
                "AI (Internal): Something failed",
                trace.Message);
            Assert.AreEqual(0, trace.Properties.Count);
        }

        [TestMethod]
        public void SendNotFailIfChannelNotInitialized()
        {
            var configuration = new TelemetryConfiguration();
            var portalSenderWithDefaultCOnfiguration = new PortalDiagnosticsSender(
                configuration,
                this.dontThrottleManager);

            var evt = new TraceEvent
            {
                MetaData = new EventMetaData
                {
                    EventId = 10,
                    Keywords = 0x20,
                    Level = EventLevel.Warning,
                    MessageFormat = "Something failed"
                },
                Payload = null
            };

            portalSenderWithDefaultCOnfiguration.Send(evt);
        }
    }
}
