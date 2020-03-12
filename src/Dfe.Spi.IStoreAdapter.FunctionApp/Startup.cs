namespace Dfe.Spi.IStoreAdapter.FunctionApp
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Dfe.Spi.Common.Context.Definitions;
    using Dfe.Spi.Common.Http.Server;
    using Dfe.Spi.Common.Http.Server.Definitions;
    using Dfe.Spi.Common.Logging;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.IStoreAdapter.Application;
    using Dfe.Spi.IStoreAdapter.Application.Definitions;
    using Dfe.Spi.IStoreAdapter.Application.Definitions.Factories;
    using Dfe.Spi.IStoreAdapter.Application.Definitions.SettingsProvider;
    using Dfe.Spi.IStoreAdapter.Application.Factories;
    using Dfe.Spi.IStoreAdapter.Application.Models;
    using Dfe.Spi.IStoreAdapter.Domain.Definitions;
    using Dfe.Spi.IStoreAdapter.Domain.Definitions.SettingsProviders;
    using Dfe.Spi.IStoreAdapter.FunctionApp.SettingsProviders;
    using Dfe.Spi.IStoreAdapter.Infrastructure.AzureStorage;
    using Dfe.Spi.IStoreAdapter.Infrastructure.SqlServer;
    using Dfe.Spi.IStoreAdapter.Infrastructure.TranslationApi;
    using Microsoft.Azure.Functions.Extensions.DependencyInjection;
    using Microsoft.Azure.WebJobs.Logging;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Functions startup class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Startup : FunctionsStartup
    {
        private const string SystemErrorIdentifier = "ISA";

        /// <inheritdoc />
        public override void Configure(
            IFunctionsHostBuilder functionsHostBuilder)
        {
            if (functionsHostBuilder == null)
            {
                throw new ArgumentNullException(nameof(functionsHostBuilder));
            }

            // camelCase, if you please.
            JsonConvert.DefaultSettings =
                () => new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore,
                };

            IServiceCollection serviceCollection =
                functionsHostBuilder.Services;

            AddAdapters(serviceCollection);
            AddFactories(serviceCollection);
            AddLogging(serviceCollection);
            AddSettingsProviders(serviceCollection);
            AddManagers(serviceCollection);
            AddProcessors(serviceCollection);

            HttpErrorBodyResultProvider httpErrorBodyResultProvider =
                new HttpErrorBodyResultProvider(
                    SystemErrorIdentifier,
                    HttpErrorMessages.ResourceManager);

            serviceCollection
                .AddSingleton<IHttpErrorBodyResultProvider>(httpErrorBodyResultProvider)
                .AddSingleton<AggregationFieldsCache>();
        }

        private static void AddAdapters(
            IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddScoped<ICensusAdapter, CensusAdapter>()
                .AddScoped<IDatasetQueryFilesStorageAdapter, DatasetQueryFileStorageAdapter>()
                .AddScoped<ITranslationApiAdapter, TranslationApiAdapter>();
        }

        private static void AddFactories(
            IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddScoped<IAggregatorFactory, AggregatorFactory>();
        }

        private static void AddLogging(IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddScoped<ILogger>(CreateILogger)
                .AddScoped<ILoggerWrapper, LoggerWrapper>();
        }

        private static void AddManagers(IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddScoped<IHttpSpiExecutionContextManager, HttpSpiExecutionContextManager>()
                .AddScoped<ISpiExecutionContextManager>(x => x.GetService<IHttpSpiExecutionContextManager>());
        }

        private static void AddProcessors(IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddScoped<ICensusProcessor, CensusProcessor>();
        }

        private static void AddSettingsProviders(
            IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<ICensusProcessorSettingsProvider, CensusProcessorSettingsProvider>()
                .AddSingleton<IDatasetQueryFilesStorageAdapterSettingsProvider, DatasetQueryFilesStorageAdapterSettingsProvider>()
                .AddSingleton<ITranslationApiAdapterSettingsProvider, TranslationApiAdapterSettingsProvider>();
        }

        private static ILogger CreateILogger(IServiceProvider serviceProvider)
        {
            ILogger toReturn = null;

            ILoggerFactory loggerFactory =
                serviceProvider.GetService<ILoggerFactory>();

            string categoryName = LogCategories.CreateFunctionUserCategory(
                nameof(Dfe.Spi.IStoreAdapter));

            toReturn = loggerFactory.CreateLogger(categoryName);

            return toReturn;
        }
    }
}