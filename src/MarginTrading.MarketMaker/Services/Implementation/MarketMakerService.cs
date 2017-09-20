﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using MarginTrading.MarketMaker.Enums;
using MarginTrading.MarketMaker.HelperServices;
using MarginTrading.MarketMaker.Messages;
using MarginTrading.MarketMaker.Models;
using MarginTrading.MarketMaker.Settings;

namespace MarginTrading.MarketMaker.Services.Implementation
{
    internal class MarketMakerService : IMarketMakerService
    {
        private const int OrdersVolume = 1000000;

        private readonly IAssetPairsSettingsService _assetPairsSettingsService;
        private readonly Lazy<IMessageProducer<OrderCommandsBatchMessage>> _messageProducer;
        private readonly ISystem _system;
        private readonly MarginTradingMarketMakerSettings _settings;
        private readonly ISpotOrderCommandsGeneratorService _spotOrderCommandsGeneratorService;

        public MarketMakerService(IAssetPairsSettingsService assetPairsSettingsService,
            MarginTradingMarketMakerSettings marginTradingMarketMakerSettings,
            IRabbitMqService rabbitMqService,
            ISystem system,
            MarginTradingMarketMakerSettings settings,
            ISpotOrderCommandsGeneratorService spotOrderCommandsGeneratorService)
        {
            _assetPairsSettingsService = assetPairsSettingsService;
            _system = system;
            _settings = settings;
            _spotOrderCommandsGeneratorService = spotOrderCommandsGeneratorService;
            _messageProducer = new Lazy<IMessageProducer<OrderCommandsBatchMessage>>(() =>
                CreateRabbitMqMessageProducer(marginTradingMarketMakerSettings, rabbitMqService));
        }

        public Task ProcessNewExternalOrderbookAsync(ExternalExchangeOrderbookMessage orderbook)
        {
            var quotesSource = _assetPairsSettingsService.GetAssetPairQuotesSource(orderbook.AssetPairId);
            if (quotesSource.SourceType != AssetPairQuotesSourceTypeEnum.External ||
                !string.Equals(orderbook.Source, quotesSource.ExternalExchange, StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            var commands = new List<OrderCommand>
            {
                new OrderCommand {CommandType = OrderCommandTypeEnum.DeleteOrder}
            };

            foreach (var bid in orderbook.Bids)
            {
                commands.Add(new OrderCommand
                {
                    CommandType = OrderCommandTypeEnum.SetOrder,
                    Direction = OrderDirectionEnum.Buy,
                    Price = bid.Price,
                    Volume = bid.Volume
                });
            }

            foreach (var ask in orderbook.Asks)
            {
                commands.Add(new OrderCommand
                {
                    CommandType = OrderCommandTypeEnum.SetOrder,
                    Direction = OrderDirectionEnum.Sell,
                    Price = ask.Price,
                    Volume = ask.Volume
                });
            }

            return SendOrderCommandsAsync(orderbook.AssetPairId, commands);
        }

        public Task ProcessNewSpotOrderBookDataAsync(SpotOrderbookMessage orderbook)
        {
            var quotesSource = _assetPairsSettingsService.GetAssetPairQuotesSource(orderbook.AssetPair)
                .SourceType;
            if (quotesSource != AssetPairQuotesSourceTypeEnum.Spot)
            {
                return Task.CompletedTask;
            }

            var commands = _spotOrderCommandsGeneratorService.GenerateOrderCommands(orderbook.AssetPair, orderbook.IsBuy,
                orderbook.Prices[0].Price, OrdersVolume);

            return SendOrderCommandsAsync(orderbook.AssetPair, commands);
        }

        public async Task ProcessAssetPairSettingsAsync(AssetPairSettingsModel model)
        {
            AssetPairQuotesSourceTypeEnum? quotesSourceType;
            if (model.SetNewQuotesSourceType != null)
            {
                quotesSourceType = model.SetNewQuotesSourceType.Value;
                await _assetPairsSettingsService.SetAssetPairQuotesSource(model.AssetPairId,
                    model.SetNewQuotesSourceType.Value, model.SetNewQuotesExternalExhange);
            }
            else
            {
                quotesSourceType = _assetPairsSettingsService.GetAssetPairQuotesSource(model.AssetPairId).SourceType;
            }

            if (quotesSourceType == AssetPairQuotesSourceTypeEnum.Manual && model.PriceForBuyOrder != null &&
                model.PriceForSellOrder != null)
            {
                var commands = new[]
                {
                    new OrderCommand {CommandType = OrderCommandTypeEnum.DeleteOrder},
                    new OrderCommand
                    {
                        CommandType = OrderCommandTypeEnum.SetOrder,
                        Direction = OrderDirectionEnum.Sell,
                        Price = model.PriceForSellOrder.Value,
                        Volume = OrdersVolume
                    },
                    new OrderCommand
                    {
                        CommandType = OrderCommandTypeEnum.SetOrder,
                        Direction = OrderDirectionEnum.Buy,
                        Price = model.PriceForBuyOrder.Value,
                        Volume = OrdersVolume
                    },
                };
                await SendOrderCommandsAsync(model.AssetPairId, commands);
            }
        }

        private static IMessageProducer<OrderCommandsBatchMessage> CreateRabbitMqMessageProducer(
            MarginTradingMarketMakerSettings marginTradingMarketMakerSettings, IRabbitMqService rabbitMqService)
        {
            return rabbitMqService.CreateProducer<OrderCommandsBatchMessage>(
                marginTradingMarketMakerSettings.RabbitMq.OrderCommandsConnectionSettings, false);
        }

        private Task SendOrderCommandsAsync(string assetPairId, IReadOnlyList<OrderCommand> commands)
        {
            if (commands.Count == 0)
            {
                return Task.CompletedTask;
            }

            return _messageProducer.Value.ProduceAsync(new OrderCommandsBatchMessage
            {
                AssetPairId = assetPairId,
                Timestamp = _system.UtcNow,
                Commands = commands,
                MarketMakerId = _settings.MarketMakerId,
            });
        }
    }
}