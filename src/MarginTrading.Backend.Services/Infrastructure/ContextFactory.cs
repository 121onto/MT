﻿using MarginTrading.Backend.Core.Settings;
using MarginTrading.Common.Services.Telemetry;

namespace MarginTrading.Backend.Services.Infrastructure
{
    public class ContextFactory : IContextFactory
    {
        private readonly ITelemetryPublisher _telemetryPublisher;
        private readonly MarginSettings _marginSettings;

        public ContextFactory(ITelemetryPublisher telemetryPublisher, MarginSettings marginSettings)
        {
            _telemetryPublisher = telemetryPublisher;
            _marginSettings = marginSettings;
        }

        public TradingSyncContext GetReadSyncContext(string source)
        {
            return new TradingSyncContext(TelemetryConstants.ReadTradingContext, source, _telemetryPublisher, _marginSettings);
        }

        public TradingSyncContext GetWriteSyncContext(string source)
        {
            return new TradingSyncContext(TelemetryConstants.WriteTradingContext, source, _telemetryPublisher, _marginSettings);
        }
    }
}
