namespace Dfe.Spi.IStoreAdapter.FunctionApp.Functions
{
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Dfe.Spi.Common.Http.Server;
    using Dfe.Spi.Common.Http.Server.Definitions;
    using Dfe.Spi.Common.Logging.Definitions;
    using Dfe.Spi.IStoreAdapter.Application.Definitions;
    using Dfe.Spi.IStoreAdapter.Application.Models.Processors;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;

    /// <summary>
    /// Entry class for the <c>census</c> function.
    /// </summary>
    public class Census : FunctionsBase<GetCensusRequest>
    {
        private readonly ICensusProcessor censusProcessor;
        private readonly IHttpErrorBodyResultProvider httpErrorBodyResultProvider;

        /// <summary>
        /// Initialises a new instance of the <see cref="Census" /> class.
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
        public Census(
            ICensusProcessor censusProcessor,
            IHttpErrorBodyResultProvider httpErrorBodyResultProvider,
            IHttpSpiExecutionContextManager httpSpiExecutionContextManager,
            ILoggerWrapper loggerWrapper)
            : base(httpSpiExecutionContextManager, loggerWrapper)
        {
            this.censusProcessor = censusProcessor;
            this.httpErrorBodyResultProvider = httpErrorBodyResultProvider;
        }

        /// <summary>
        /// Entry method for the <c>census</c> function.
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
        [FunctionName("census")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "POST", Route = null)]
            HttpRequest httpRequest,
            CancellationToken cancellationToken)
        {
            IActionResult toReturn = await this.ValidateAndRunAsync(
                httpRequest,
                cancellationToken)
                .ConfigureAwait(false);

            return toReturn;
        }

        /// <inheritdoc />
        protected override HttpErrorBodyResult GetMalformedErrorResponse()
        {
            HttpErrorBodyResult toReturn =
                this.httpErrorBodyResultProvider.GetHttpErrorBodyResult(
                    HttpStatusCode.BadRequest,
                    1);

            return toReturn;
        }

        /// <inheritdoc />
        protected override HttpErrorBodyResult GetSchemaValidationResponse(
            string message)
        {
            HttpErrorBodyResult toReturn =
                this.httpErrorBodyResultProvider.GetHttpErrorBodyResult(
                    HttpStatusCode.BadRequest,
                    2);

            return toReturn;
        }

        /// <inheritdoc />
        protected async override Task<IActionResult> ProcessWellFormedRequestAsync(
            GetCensusRequest getCensusRequest,
            CancellationToken cancellationToken)
        {
            IActionResult toReturn = null;

            GetCensusResponse getCensusResponse =
                await this.censusProcessor.GetCensusAsync(
                    getCensusRequest,
                    cancellationToken)
                    .ConfigureAwait(false);

            // TODO: Wire up to processor response.
            toReturn = new OkResult();

            return toReturn;
        }
    }
}