namespace SampleClient
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;

	public class HttpClientWrapper
	{
		#region ---------- Fields ----------

		internal const string BaseAddress = "http://localhost/";
		internal const string StringUri = "stringUri";
		internal const string SampleDtoUri = "sampleDto";
		private readonly HttpClient httpClient;

		#endregion

		#region ---------- Constructors ----------

		public HttpClientWrapper(HttpClient httpClient)
		{
			this.httpClient = httpClient;
			httpClient.BaseAddress = new Uri("http://localhost/");
		}

		#endregion

		#region ---------- Properties ----------

		/// <summary>
		/// Gets the last error which occurred.
		/// </summary>
		public Exception LastError { get; private set; }

		#endregion

		#region ---------- Methods ----------

		public string GetString()
		{
			var response = httpClient.GetAsync(StringUri).Result;
			if (response.IsSuccessStatusCode)
				return response.Content.ReadAsAsync<string>().Result;

			throw new Exception(response.StatusCode.ToString());
		}

		public void GetStringAsync(Action<string> getStringCompletedAction)
		{
			httpClient.
				GetAsync(StringUri).
				ContinueWith(task =>
				             {
				             	try
				             	{
				             		var response = task.Result;
				             		if (response.IsSuccessStatusCode)
				             		{
				             			var stringContent = response.Content.ReadAsAsync<string>().Result;
				             			getStringCompletedAction(stringContent);
				             		}
				             		else
				             			LastError = new Exception(response.StatusCode.ToString());
				             	}
				             	catch (AggregateException aggregateException)
				             	{
				             		LastError = aggregateException.InnerException;
				             	}
				             });
		}

		public void GetSampleDtoAsync(Action<SampleDto> getSampleDtoCompletedAction)
		{
			httpClient.
				GetAsync(SampleDtoUri).
				ContinueWith(task =>
				             {
				             	try
				             	{
				             		var response = task.Result;
				             		if (response.IsSuccessStatusCode)
				             		{
				             			var dto = response.Content.ReadAsAsync<SampleDto>().Result;
										getSampleDtoCompletedAction(dto);
				             		}
				             		else
				             			LastError = new Exception(response.StatusCode.ToString());
				             	}
				             	catch (AggregateException aggregateException)
				             	{
				             		LastError = aggregateException.InnerException;
				             	}
				             });
		}

		public void PostSampleDtoAsync(SampleDto sampleDto, Action postSampleDtoCompletedAction)
		{
			httpClient.
				PostAsJsonAsync(SampleDtoUri, sampleDto).
				ContinueWith(task =>
				             {
				             	try
				             	{
				             		var response = task.Result;
				             		if (!response.IsSuccessStatusCode)
				             			LastError = new Exception(response.StatusCode.ToString());
				             		else
				             			postSampleDtoCompletedAction();
				             	}
				             	catch (AggregateException aggregateException)
				             	{
				             		LastError = aggregateException.InnerException;
				             	}
				             });
		}

		#endregion
	}
}