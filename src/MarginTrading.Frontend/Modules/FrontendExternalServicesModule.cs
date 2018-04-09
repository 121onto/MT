﻿using Autofac;
using Autofac.Extensions.DependencyInjection;
using Lykke.ClientGenerator;
using Lykke.Service.ClientAccount.Client;
using Lykke.SettingsReader;
using MarginTrading.Backend.Contracts.DataReaderClient;
using MarginTrading.Backend.Contracts.Infrastructure;
using MarginTrading.Frontend.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;

namespace MarginTrading.Frontend.Modules
{
    public class FrontendExternalServicesModule : Module
    {
        private readonly IReloadingManager<ApplicationSettings> _settings;

        public FrontendExternalServicesModule(IReloadingManager<ApplicationSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var services = new ServiceCollection();

            services.RegisterMtDataReaderClientsPair(
                ClientProxyGenerator.CreateDefault(
                    _settings.CurrentValue.MtDataReaderDemoServiceClient.ServiceUrl,
                    _settings.CurrentValue.MtDataReaderLiveServiceClient.ServiceUrl), 
                ClientProxyGenerator.CreateDefault(
                    _settings.CurrentValue.MtDataReaderDemoServiceClient.ApiKey,
                    _settings.CurrentValue.MtDataReaderLiveServiceClient.ApiKey));
            
            builder.RegisterLykkeServiceClient(_settings.CurrentValue.ClientAccountServiceClient.ServiceUrl);
            
            builder.Populate(services);
        }
    }
}