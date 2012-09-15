namespace SampleTests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Net.Http;

	using HttpMessageHandlerStub;

	using NUnit.Framework;

	using SampleClient;

	[TestFixture]
	public class HttpClientWrapperTests
	{
		#region ---------- Fields ----------

		private const string baseAddress = HttpClientWrapper.BaseAddress;
		private const string stringUri = HttpClientWrapper.StringUri;
		private const string sampleDtoUri = HttpClientWrapper.SampleDtoUri;
		private const int fiveSeconds = 5000;
		private const int pollingEvery100Milliseconds = 100;
		private HttpMessageHandlerStub httpMessageHandlerStub;
		private HttpClientWrapper httpClientWrapper;

		#endregion

		#region ---------- Setup/Teardown ----------

		[SetUp]
		public void SetUp()
		{
			httpMessageHandlerStub = new HttpMessageHandlerStub();
			httpClientWrapper = new HttpClientWrapper(new HttpClient(httpMessageHandlerStub));
		}

		#endregion

		[Test]
		public void GetString_RequestFailed_ThrowsException()
		{
			// Arrange
			httpMessageHandlerStub.
				SetupGet(new Uri(baseAddress + stringUri)).
				Returns(HttpStatusCode.NotFound);

			// Act / Assert
			var exception = Assert.Throws<Exception>(() => httpClientWrapper.GetString());
			Assert.That(exception.Message, Is.EqualTo(HttpStatusCode.NotFound.ToString()));
		}

		[Test]
		public void GetString_RequestSucceeded_ReturnsStringContent()
		{
			// Arrange
			const string expectedResponseContent = "content";
			httpMessageHandlerStub.
				SetupGet(new Uri(baseAddress + stringUri)).
				Returns(expectedResponseContent);

			// Act
			var actualResponseContent = httpClientWrapper.GetString();

			// Assert
			Assert.That(actualResponseContent, Is.EqualTo(expectedResponseContent));
		}

		[Test]
		public void GetStringAsync_RequestThrowsException_SetsLastErrorProperty()
		{
			// Arrange
			var exception = new Exception("Something went wrong.");
			httpMessageHandlerStub.
				SetupGet(new Uri(baseAddress + stringUri)).
				Throws(exception);
			
			// Act
			httpClientWrapper.GetStringAsync(stringContent => { });

			// Assert
			Assert.That(() => httpClientWrapper.LastError, Is.EqualTo(exception).After(fiveSeconds, pollingEvery100Milliseconds));
		}

		[Test]
		public void GetStringAsync_RequestFailed_SetsLastErrorProperty()
		{
			// Arrange
			httpMessageHandlerStub.
				SetupGet(new Uri(baseAddress + stringUri)).
				Returns(HttpStatusCode.NotFound);

			// Act
			httpClientWrapper.GetStringAsync(stringContent => { });

			// Assert
			Assert.That(() => httpClientWrapper.LastError, Is.Not.Null.After(fiveSeconds, pollingEvery100Milliseconds));
			Assert.That(httpClientWrapper.LastError.Message, Is.EqualTo(HttpStatusCode.NotFound.ToString()));
		}

		[Test]
		public void GetStringAsync_RequestSucceeded_CallsGetStringCompletedActionWithStringContent()
		{
			// Arrange
			const string expectedStringContent = "content";
			httpMessageHandlerStub.
				SetupGet(new Uri(baseAddress + stringUri)).
				Returns(expectedStringContent);
			string actualStringContent = null;

			// Act
			httpClientWrapper.GetStringAsync(stringContent => actualStringContent = stringContent);

			// Assert
			Assert.That(() => actualStringContent, Is.EqualTo(expectedStringContent).After(fiveSeconds, pollingEvery100Milliseconds));
		}

		[Test]
		public void GetSampleDtoAsync_RequestSucceeded_CallsGetSampleDtoCompletedActionWithSampleDto()
		{
			// Arrange
			var expectedSampleDto = new SampleDto { Id = 123, Name = "sample" };
			httpMessageHandlerStub.
				SetupGet(new Uri(baseAddress + sampleDtoUri)).
				Returns(expectedSampleDto);
			SampleDto actualSampleDto = null;

			// Act
			httpClientWrapper.GetSampleDtoAsync(sampleDto => actualSampleDto = sampleDto);

			// Assert
			Assert.That(() => actualSampleDto, Is.Not.Null.After(fiveSeconds, pollingEvery100Milliseconds));
			Assert.That(actualSampleDto.Id, Is.EqualTo(expectedSampleDto.Id));
			Assert.That(actualSampleDto.Name, Is.EqualTo(expectedSampleDto.Name));
		}

		[Test]
		public void PostSampleDtoAsync_RequestSucceeded_PassedSampleDtoIsPosted()
		{
			// Arrange
			var sampleDto = new SampleDto { Id = 1 };
			httpMessageHandlerStub.
			    SetupPost<SampleDto>(new Uri(baseAddress + sampleDtoUri), dto => dto.Id == sampleDto.Id).
				Returns(HttpStatusCode.OK);
			var sampleDtoWasPostedSuccessfully = false;

			// Act
			httpClientWrapper.PostSampleDtoAsync(sampleDto, () => sampleDtoWasPostedSuccessfully = true);

			// Assert
			Assert.That(() => sampleDtoWasPostedSuccessfully, Is.True.After(fiveSeconds, pollingEvery100Milliseconds));
		}
	}
}