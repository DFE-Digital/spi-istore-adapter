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

            AddLogging(serviceCollection);

            HttpErrorBodyResultProvider httpErrorBodyResultProvider =
                new HttpErrorBodyResultProvider(
                    SystemErrorIdentifier,
                    HttpErrorMessages.ResourceManager);

            serviceCollection
                .AddSingleton<IHttpErrorBodyResultProvider>(httpErrorBodyResultProvider)
                .AddScoped<IHttpSpiExecutionContextManager, HttpSpiExecutionContextManager>()
                .AddScoped<ISpiExecutionContextManager>(x => x.GetService<IHttpSpiExecutionContextManager>())
                .AddScoped<ICensusProcessor, CensusProcessor>();
        }

        private static void AddLogging(IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddScoped<ILogger>(CreateILogger)
                .AddScoped<ILoggerWrapper, LoggerWrapper>();
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