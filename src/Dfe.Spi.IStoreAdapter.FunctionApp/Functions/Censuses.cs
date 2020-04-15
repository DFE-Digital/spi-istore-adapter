namespace Dfe.Spi.IStoreAdapter.FunctionApp.Functions
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Extensions;
    using Dfe.Spi.Common.Http.Server;
    using Dfe.Spi.Common.Http.Server.Definitions;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.IStoreAdapter.Application.Definitions;
    using Dfe.Spi.IStoreAdapter.Application.Exceptions;
    using Dfe.Spi.IStoreAdapter.Application.Models.Processors;
    using Dfe.Spi.IStoreAdapter.Domain.Exceptions;
    using Dfe.Spi.Models.Entities;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Newtonsoft.Json;

    /// <summary>
    /// Entry class for the <c>censuses</c> function.
    /// </summary>
    public class Censuses : FunctionsBase<GetCensusRequest>
    {
        private readonly ICensusProcessor censusProcessor;
        private readonly IHttpErrorBodyResultProvider httpErrorBodyResultProvider;
        private readonly IHttpSpiExecutionContextManager httpSpiExecutionContextManager;
        private readonly ILoggerWrapper loggerWrapper;

        private CensusIdentifier censusIdentifier;

        /// <summary>
        /// Initialises a new instance of the <see cref="Censuses" /> class.
        /// </summary>
        /// <param name="censusProcessor">
        /// An instance of type <see cref="ICensusProcessor" />.
        /// </param>
        /// <param name="httpErrorBodyResultProvider">
        /// An instance of type <see cref="IHttpErrorBodyResultProvider" />.
        /// </param>
        /// <param name="httpSpiExecutionContextManager">
        /// An instance of type <see cref="IHttpSpiExecutionContextManager" />.
        /// </param>
        /// <param name="loggerWrapper">
        /// An instance of type <see cref="ILoggerWrapper" />.
        /// </param>
        public Censuses(
            ICensusProcessor censusProcessor,
            IHttpErrorBodyResultProvider httpErrorBodyResultProvider,
            IHttpSpiExecutionContextManager httpSpiExecutionContextManager,
            ILoggerWrapper loggerWrapper)
            : base(httpSpiExecutionContextManager, loggerWrapper)
        {
            this.censusProcessor = censusProcessor;
            this.httpErrorBodyResultProvider = httpErrorBodyResultProvider;
            this.httpSpiExecutionContextManager = httpSpiExecutionContextManager;
            this.loggerWrapper = loggerWrapper;
        }

        /// <summary>
        /// Entry method for the <c>censuses</c> function.
        /// </summary>
        /// <param name="httpRequest">
        /// An instance of <see cref="HttpContext" />.
        /// </param>
        /// <param name="id">
        /// The id of the requested census.
        /// </param>
        /// <param name="cancellationToken">
        /// An instance of <see cref="CancellationToken" />.
        /// </param>
        /// <returns>
        /// An instance of type <see cref="IActionResult" />.
        /// </returns>
        [FunctionName("censuses")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "GET", "POST", Route = "censuses/{id}")]
            HttpRequest httpRequest,
            string id,
            CancellationToken cancellationToken)
        {
            IActionResult toReturn = null;

            // TODO: REMOVE
            if (Startup.StartupException != null)
            {
                this.loggerWrapper.Error("The startup exception is...", Startup.StartupException);

                throw Startup.StartupException;
            }

            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            // Parse and validate the id to a CensusIdentifier.
            this.censusIdentifier = ParseIdentifier(id);

            FunctionRunContext functionRunContext = new FunctionRunContext();

            if (this.censusIdentifier != null)
            {
                switch (httpRequest.Method)
                {
                    case "POST":
                        toReturn = await this.ValidateAndRunAsync(
                            httpRequest,
                            functionRunContext,
                            cancellationToken)
                            .ConfigureAwait(false);
                        break;

                    case "GET":
                        IHeaderDictionary headerDictionary =
                            httpRequest.Headers;

                        this.httpSpiExecutionContextManager.SetContext(
                            headerDictionary);

                        toReturn = await this.ProcessWellFormedRequestAsync(
                            null,
                            functionRunContext,
                            cancellationToken)
                            .ConfigureAwait(false);
                        break;
                }
            }
            else
            {
                toReturn =
                    this.httpErrorBodyResultProvider.GetHttpErrorBodyResult(
                        HttpStatusCode.BadRequest,
                        3);
            }

            return toReturn;
        }

        /// <inheritdoc />
        protected override HttpErrorBodyResult GetMalformedErrorResponse(
            FunctionRunContext functionRunContext)
        {
            HttpErrorBodyResult toReturn =
                this.httpErrorBodyResultProvider.GetHttpErrorBodyResult(
                    HttpStatusCode.BadRequest,
                    1);

            return toReturn;
        }

        /// <inheritdoc />
        protected override HttpErrorBodyResult GetSchemaValidationResponse(
            JsonSchemaValidationException jsonSchemaValidationException,
            FunctionRunContext functionRunContext)
        {
            HttpErrorBodyResult toReturn = null;

            if (jsonSchemaValidationException == null)
            {
                throw new ArgumentNullException(
                    nameof(jsonSchemaValidationException));
            }

            string message = jsonSchemaValidationException.Message;

            toReturn =
                this.httpErrorBodyResultProvider.GetHttpErrorBodyResult(
                    HttpStatusCode.BadRequest,
                    2,
                    message);

            return toReturn;
        }

        /// <inheritdoc />
        protected async override Task<IActionResult> ProcessWellFormedRequestAsync(
            GetCensusRequest getCensusRequest,
            FunctionRunContext functionRunContext,
            CancellationToken cancellationToken)
        {
            IActionResult toReturn = null;

            if (getCensusRequest == null)
            {
                getCensusRequest = new GetCensusRequest();
            }

            getCensusRequest.CensusIdentifier = this.censusIdentifier;

            try
            {
                GetCensusResponse getCensusResponse =
                    await this.censusProcessor.GetCensusAsync(
                        getCensusRequest,
                        cancellationToken)
                        .ConfigureAwait(false);

                Census census = getCensusResponse.Census;

                JsonSerializerSettings jsonSerializerSettings =
                    JsonConvert.DefaultSettings();

                if (jsonSerializerSettings == null)
                {
                    toReturn = new JsonResult(census);
                }
                else
                {
                    toReturn = new JsonResult(census, jsonSerializerSettings);
                }
            }
            catch (DatasetQueryFileNotFoundException datasetQueryFileNotFoundException)
            {
                toReturn = this.GetErrorBody(
                    HttpStatusCode.NotFound,
                    4,
                    datasetQueryFileNotFoundException);
            }
            catch (IncompleteDatasetQueryFileException incompleteDatasetQueryFileException)
            {
                toReturn = this.GetErrorBody(
                    HttpStatusCode.ExpectationFailed,
                    5,
                    incompleteDatasetQueryFileException);
            }
            catch (TranslationApiAdapterException translationApiAdapterException)
            {
                toReturn = this.GetErrorBody(
                    HttpStatusCode.FailedDependency,
                    6,
                    translationApiAdapterException);
            }
            catch (UnsupportedAggregateColumnRequestException unsupportedAggregateColumnRequestException)
            {
                toReturn = this.GetErrorBody(
                    HttpStatusCode.BadRequest,
                    7,
                    unsupportedAggregateColumnRequestException);
            }
            catch (InvalidMappingTypeException invalidMappingTypeException)
            {
                toReturn = this.GetErrorBody(
                    HttpStatusCode.ExpectationFailed,
                    8,
                    invalidMappingTypeException);
            }
            catch (SqlFieldValueUnboxingTypeException sqlFieldValueUnboxingTypeException)
            {
                toReturn = this.GetErrorBody(
                    HttpStatusCode.ExpectationFailed,
                    9,
                    sqlFieldValueUnboxingTypeException);
            }
            catch (InvalidBetweenValueException invalidBetweenValueException)
            {
                toReturn = this.GetErrorBody(
                    HttpStatusCode.BadRequest,
                    10,
                    invalidBetweenValueException);
            }
            catch (InvalidDateTimeFormatException invalidDateTimeFormatException)
            {
                toReturn = this.GetErrorBody(
                    HttpStatusCode.BadRequest,
                    11,
                    invalidDateTimeFormatException);
            }
            catch (DataFilterValueUnboxingTypeException dataFilterValueUnboxingTypeException)
            {
                toReturn = this.GetErrorBody(
                    HttpStatusCode.BadRequest,
                    12,
                    dataFilterValueUnboxingTypeException);
            }
            catch (UnsupportedSearchParameterException unsupportedSearchParameterException)
            {
                toReturn = this.GetErrorBody(
                    HttpStatusCode.BadRequest,
                    13,
                    unsupportedSearchParameterException);
            }

            return toReturn;
        }

        private static CensusIdentifier ParseIdentifier(
            string censusIdentifierStr)
        {
            CensusIdentifier toReturn = null;

            string[] identifierParts = censusIdentifierStr.Split(
                '-',
                StringSplitOptions.RemoveEmptyEntries);

            if (identifierParts.Length == 3)
            {
                toReturn = new CensusIdentifier()
                {
                    DatasetQueryFileId = identifierParts[0],
                    ParameterName = identifierParts[1],
                    ParameterValue = identifierParts[2],
                };
            }

            return toReturn;
        }

        private IActionResult GetErrorBody(
            HttpStatusCode httpStatusCode,
            int errorId,
            Exception exception)
        {
            IActionResult toReturn = null;

            string message = exception.Message;

            Type exceptionType = exception.GetType();

            this.loggerWrapper.Error(
                $"An exception of type {exceptionType.Name} was thrown: " +
                $"{message}",
                exception);

            toReturn =
                this.httpErrorBodyResultProvider.GetHttpErrorBodyResult(
                    httpStatusCode,
                    errorId,
                    message);

            return toReturn;
        }
    }
}