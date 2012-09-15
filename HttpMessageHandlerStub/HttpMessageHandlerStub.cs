namespace HttpMessageHandlerStub
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Runtime.Serialization;
	using System.Threading;
	using System.Threading.Tasks;

	using Newtonsoft.Json;

	/// <summary>
	/// Defines a mock HttpMessageHandler for unit testing HttpClient behaviors.
	/// </summary>
	public class HttpMessageHandlerStub : HttpMessageHandler
	{
		#region ---------- Fields ----------

		private const string jsonMediaType = "application/json";
		private const string xmlMediaType = "application/xml";
		private readonly HttpMessageHandlerStubSetup setup;
		private Uri requestUri;
		private HttpMethod httpMethod;

		private Delegate isContentValidFunction;
		private Type requestDtoType;

		#endregion

		#region ---------- Constructors ----------

		/// <summary>
		/// Initializes a new instance of the <see cref="HttpMessageHandlerStub"/> class.  
		/// </summary>
		public HttpMessageHandlerStub()
		{
			setup = new HttpMessageHandlerStubSetup();
		}

		#endregion

		#region ---------- Methods ----------

		/// <summary>
		/// Used to set up a GET behavior for a particular request URI.
		/// </summary>
		/// <param name="requestUri">
		/// The request URI.
		/// </param>
		/// <returns>
		/// A setup object used to specify the desired behavior.
		/// </returns>
		public IHttpMessageHandlerStubSetup SetupGet(Uri requestUri)
		{
			this.requestUri = requestUri;
			httpMethod = HttpMethod.Get;
			return setup;
		}

		/// <summary>
		/// Used to set up a POST behavior for a particular request URI.
		/// </summary>
		/// <typeparam name="T">
		/// The type of the content to be posted.
		/// </typeparam>
		/// <param name="requestUri">
		/// The request URI.
		/// </param>
		/// <param name="isContentValidFunction">
		/// A function used to determine whether the posted content is valid.
		/// </param>
		/// <returns>
		/// A setup object used to specify the desired behavior.
		/// </returns>
		public IHttpMessageHandlerStubSetup SetupPost<T>(Uri requestUri, Func<T, bool> isContentValidFunction)
		{
			this.requestUri = requestUri;
			httpMethod = HttpMethod.Post;
			this.isContentValidFunction = isContentValidFunction;
			requestDtoType = typeof(T);
			return setup;
		}

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			if (requestDtoType != null)
			{
				var content = request.Content.ReadAsAsync(requestDtoType).Result;
				if (!(bool)isContentValidFunction.DynamicInvoke(content))
				{
					throw new InvalidOperationException("The validation function passed in during setup returned false for the request content.");
				}
			}

			if (httpMethod == null || requestUri == null)
				throw new InvalidOperationException("HttpMessageHandlerStub was not set up.");

			if (request.RequestUri != requestUri)
				throw new InvalidOperationException(string.Format("Incorrect request URI. Expected {0}, but was {1}.", requestUri, request.RequestUri));

			if (request.Method != httpMethod)
				throw new InvalidOperationException(string.Format("Incorrect HTTP method. Expected {0}, but was {1}.", httpMethod, request.Method));

			var responseTask = new Task<HttpResponseMessage>(() => CreateResponse(request), cancellationToken);
			responseTask.Start();
			return responseTask;
		}

		private HttpResponseMessage CreateResponse(HttpRequestMessage request)
		{
			if (setup.Exception != null)
				throw setup.Exception;

			if (setup.StatusCode != null)
				return new HttpResponseMessage(setup.StatusCode.Value) { RequestMessage = request };

			if (setup.ResponseObject != null)
			{
				var responseMessage = new HttpResponseMessage(HttpStatusCode.OK) { RequestMessage = request };
				if (request.Headers.Accept.Any(headerValue => headerValue.MediaType == jsonMediaType) ||
				    request.Headers.Accept.Count == 0)
				{
					responseMessage.Content = new StringContent(JsonConvert.SerializeObject(setup.ResponseObject));
					responseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(jsonMediaType);
				}
				else if (request.Headers.Accept.Any(headerValue => headerValue.MediaType == xmlMediaType))
				{
					var dataContractSerializer = new DataContractSerializer(setup.ResponseObject.GetType());
					using (var memoryStream = new MemoryStream())
					using (var streamReader = new StreamReader(memoryStream))
					{
						dataContractSerializer.WriteObject(memoryStream, setup.ResponseObject);
						memoryStream.Seek(0, SeekOrigin.Begin);
						responseMessage.Content = new StringContent(streamReader.ReadToEnd());
					}

					responseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(xmlMediaType);
				}
				else
				{
					throw new InvalidOperationException(string.Format("Unknown media type specified in accept header: {0}", request.Headers.Accept));
				}

				return responseMessage;
			}

			throw new InvalidOperationException("HttpMessageHandlerStub was not set up.");
		}

		#endregion

		#region ---------- HttpMessageHandlerStubSetup Class ----------

		private class HttpMessageHandlerStubSetup : IHttpMessageHandlerStubSetup
		{
			#region ---------- Properties ----------

			/// <summary>
			/// Gets the status code to return if it has been set up, otherwise null.
			/// </summary>
			public HttpStatusCode? StatusCode { get; private set; }

			/// <summary>
			/// Gets the response object to return if it has been set up, otherwise null.
			/// </summary>
			public object ResponseObject { get; private set; }

			/// <summary>
			/// Gets the exception to throw if it has been set up, otherwise null.
			/// </summary>
			public Exception Exception { get; private set; }

			#endregion

			#region ---------- IHttpMessageHandlerStubSetup Implementation ----------

			/// <summary>
			/// Specifies a status code to return.
			/// </summary>
			/// <param name="statusCode">
			/// The status code to return.
			/// </param>
			public void Returns(HttpStatusCode statusCode)
			{
				StatusCode = statusCode;
			}

			/// <summary>
			/// Specifies a data transfer object to return.
			/// </summary>
			/// <param name="dto">
			/// The data transfer object.
			/// </param>
			public void Returns(object dto)
			{
				ResponseObject = dto;
			}

			/// <summary>
			/// Specifies an <paramref name="exception"/> to throw.
			/// </summary>
			/// <param name="exception">
			/// The <paramref name="exception"/> to throw.
			/// </param>
			public void Throws(Exception exception)
			{
				Exception = exception;
			}

			#endregion
		}

		#endregion
	}
}