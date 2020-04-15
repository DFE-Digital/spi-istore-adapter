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
    using Dfe.Spi.IStoreAdapter.FunctionApp.Definitions.SettingsProviders;
    using Dfe.Spi.IStoreAdapter.FunctionApp.SettingsProviders;
    using Dfe.Spi.IStoreAdapter.Infrastructure.AzureStorage;
    using Dfe.Spi.IStoreAdapter.Infrastructure.SqlServer;
    using Dfe.Spi.IStoreAdapter.Infrastructure.TranslationApi;
    using Microsoft.Azure.Functions.Extensions.DependencyInjection;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.Services.AppAuthentication;
    using Microsoft.Azure.WebJobs.Logging;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.AzureKeyVault;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using static Microsoft.Azure.Services.AppAuthentication.AzureServiceTokenProvider;

    /// <summary>
    /// Functions startup class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Startup : FunctionsStartup
    {
        private const string SystemErrorIdentifier = "ISA";

        public static Exception StartupException
        {
            get;
            set;
        }

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
            AddConfigurationProvider(serviceCollection);
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

        private static void AddConfigurationProvider(
            IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(BuildConfiguration);
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
                .AddSingleton<ICensusAdapterSettingsProvider, CensusAdapterSettingsProvider>()
                .AddSingleton<ICensusProcessorSettingsProvider, CensusProcessorSettingsProvider>()
                .AddSingleton<IDatasetQueryFilesStorageAdapterSettingsProvider, DatasetQueryFilesStorageAdapterSettingsProvider>()
                .AddSingleton<ITranslationApiAdapterSettingsProvider, TranslationApiAdapterSettingsProvider>()
                .AddSingleton<IStartupSettingsProvider, StartupSettingsProvider>();
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

        [SuppressMessage(
            "Microsoft.Reliability",
            "CA2000",
            Justification = "Disoposing of object may impact functionality. Setting up provider as outlined in: https://docs.microsoft.com/en-us/aspnet/core/security/key-vault-configuration?view=aspnetcore-3.1#use-managed-identities-for-azure-resources")]
        private static IConfiguration BuildConfiguration(
            IServiceProvider serviceProvider)
        {
            IConfiguration toReturn = null;

            ConfigurationBuilder configurationBuilder =
                new ConfigurationBuilder();

            // Go to environment variables first...
            configurationBuilder.AddEnvironmentVariables();

            IStartupSettingsProvider startupSettingsProvider =
                serviceProvider.GetService<IStartupSettingsProvider>();

            string keyVaultInstanceName =
                startupSettingsProvider.KeyVaultInstanceName;

            if (!string.IsNullOrEmpty(keyVaultInstanceName))
            {
                string vault =
                    $"https://{keyVaultInstanceName}.vault.azure.net/";

                AzureServiceTokenProvider azureServiceTokenProvider =
                    new AzureServiceTokenProvider();

                TokenCallback keyVaultTokenCallback =
                    azureServiceTokenProvider.KeyVaultTokenCallback;

                KeyVaultClient.AuthenticationCallback authenticationCallback =
                    new KeyVaultClient.AuthenticationCallback(
                        keyVaultTokenCallback);

                KeyVaultClient keyVaultClient = new KeyVaultClient(
                    authenticationCallback);

                DefaultKeyVaultSecretManager defaultKeyVaultSecretManager =
                    new DefaultKeyVaultSecretManager();

                // Otherwise, KeyVault.
                configurationBuilder.AddAzureKeyVault(
                    vault,
                    keyVaultClient,
                    defaultKeyVaultSecretManager);
            }

            try
            {
                try
                {
                    toReturn = configurationBuilder.Build();
                }
                catch (AzureServiceTokenProviderException azureServiceTokenProviderException)
                {
                    throw new Exception(
                        $"This is likely happening because you're debugging, " +
                        $"and you haven't used the Azure CLI 2.0 tools to 'az " +
                        $"login'. Because KeyVault uses Managed Service " +
                        $"Identities, you need to do this first. If you'd " +
                        $"rather fall back to environment variables only, make " +
                        $"the setting value for " +
                        $"{nameof(IStartupSettingsProvider.KeyVaultInstanceName)} " +
                        $"null (or just omit it completely).",
                        azureServiceTokenProviderException);
                }
            }
            catch (Exception exception)
            {
                StartupException = exception;


                configurationBuilder =
                    new ConfigurationBuilder();

                // Go to environment variables first...
                configurationBuilder.AddEnvironmentVariables();

                toReturn = configurationBuilder.Build();
            }

            return toReturn;
        }
    }
}