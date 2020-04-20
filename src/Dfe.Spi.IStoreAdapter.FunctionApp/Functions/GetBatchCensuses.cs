using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dfe.Spi.Common.Models;
using Dfe.Spi.IStoreAdapter.Domain.Models;

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
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    
    /// <summary>
    /// Entry class for the <c>GetBatchCensuses</c> function.
    /// </summary>
    public class GetBatchCensuses : FunctionsBase<GetCensusesHttpRequest>
    {
        private readonly ICensusProcessor censusProcessor;
        private readonly IHttpErrorBodyResultProvider httpErrorBodyResultProvider;
        private readonly IHttpSpiExecutionContextManager httpSpiExecutionContextManager;
        private readonly ILoggerWrapper loggerWrapper;

        private CensusIdentifier censusIdentifier;

        /// <summary>
        /// Initialises a new instance of the <see cref="GetSingleCensus" /> class.
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
        public GetBatchCensuses(
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
        /// Entry method for the <c>GetBatchCensuses</c> function.
        /// </summary>
        /// <param name="httpRequest">
        /// An instance of <see cref="HttpContext" />.
        /// </param>
        /// <param name="cancellationToken">
        /// An instance of <see cref="CancellationToken" />.
        /// </param>
        /// <returns>
        /// An instance of type <see cref="IActionResult" />.
        /// </returns>
        [FunctionName(nameof(GetBatchCensuses))]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "censuses")]
            HttpRequest httpRequest,
            CancellationToken cancellationToken)
        {
            return await this.ValidateAndRunAsync(httpRequest, null, cancellationToken);
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
        protected override async Task<IActionResult> ProcessWellFormedRequestAsync(
            GetCensusesHttpRequest request, 
            FunctionRunContext runContext, 
            CancellationToken cancellationToken)
        {
            try
            {
                var censusRequest = new GetCensusesRequest
                {
                    CensusIdentifiers = request.Identifiers.Select(ParseIdentifier).ToArray(),
                    AggregateQueries = request.AggregateQueries,
                };
                
                var response = await this.censusProcessor.GetCensusesAsync(
                    censusRequest,
                    cancellationToken);
                
                var jsonSerializerSettings = JsonConvert.DefaultSettings();

                return jsonSerializerSettings == null 
                    ? new JsonResult(response.Censuses) 
                    : new JsonResult(response.Censuses, jsonSerializerSettings);
            }
            catch (DatasetQueryFileNotFoundException datasetQueryFileNotFoundException)
            {
                return this.GetErrorBody(
                    HttpStatusCode.NotFound,
                    4,
                    datasetQueryFileNotFoundException);
            }
            catch (IncompleteDatasetQueryFileException incompleteDatasetQueryFileException)
            {
                return this.GetErrorBody(
                    HttpStatusCode.ExpectationFailed,
                    5,
                    incompleteDatasetQueryFileException);
            }
            catch (TranslationApiAdapterException translationApiAdapterException)
            {
                return this.GetErrorBody(
                    HttpStatusCode.FailedDependency,
                    6,
                    translationApiAdapterException);
            }
            catch (UnsupportedAggregateColumnRequestException unsupportedAggregateColumnRequestException)
            {
                return this.GetErrorBody(
                    HttpStatusCode.BadRequest,
                    7,
                    unsupportedAggregateColumnRequestException);
            }
            catch (InvalidMappingTypeException invalidMappingTypeException)
            {
                return this.GetErrorBody(
                    HttpStatusCode.ExpectationFailed,
                    8,
                    invalidMappingTypeException);
            }
            catch (SqlFieldValueUnboxingTypeException sqlFieldValueUnboxingTypeException)
            {
                return this.GetErrorBody(
                    HttpStatusCode.ExpectationFailed,
                    9,
                    sqlFieldValueUnboxingTypeException);
            }
            catch (InvalidBetweenValueException invalidBetweenValueException)
            {
                return this.GetErrorBody(
                    HttpStatusCode.BadRequest,
                    10,
                    invalidBetweenValueException);
            }
            catch (InvalidDateTimeFormatException invalidDateTimeFormatException)
            {
                return this.GetErrorBody(
                    HttpStatusCode.BadRequest,
                    11,
                    invalidDateTimeFormatException);
            }
            catch (DataFilterValueUnboxingTypeException dataFilterValueUnboxingTypeException)
            {
                return this.GetErrorBody(
                    HttpStatusCode.BadRequest,
                    12,
                    dataFilterValueUnboxingTypeException);
            }
            catch (UnsupportedSearchParameterException unsupportedSearchParameterException)
            {
                return this.GetErrorBody(
                    HttpStatusCode.BadRequest,
                    13,
                    unsupportedSearchParameterException);
            }
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
    
    public class GetCensusesHttpRequest : RequestResponseBase
    {
        /// <summary>
        /// Gets or sets an array of identifiers
        /// </summary>
        public string[] Identifiers
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the aggregate queries.
        /// </summary>
        [SuppressMessage(
            "Microsoft.Usage",
            "CA2227",
            Justification = "This is a DTO.")]
        public Dictionary<string, AggregateQuery> AggregateQueries
        {
            get;
            set;
        }
    }
}