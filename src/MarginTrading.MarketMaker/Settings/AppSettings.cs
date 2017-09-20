﻿using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace MarginTrading.MarketMaker.Settings
{
    public class AppSettings
    {
        public MarginTradingMarketMakerSettings MarginTradingMarketMaker { get; set; }

        [CanBeNull, Optional]
        public SlackNotificationsSettings SlackNotifications { get; set; }

        [CanBeNull, Optional]
        public string AppNameSuffix { get; set; }

    }
}
