namespace HttpMessageHandlerStub
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;

	/// <summary>
	/// Defines the interface for setting up the <see cref="HttpMessageHandlerStub"/>.
	/// </summary>
	public interface IHttpMessageHandlerStubSetup
	{
		#region ---------- Methods ----------

		/// <summary>
		/// Specifies a status code to return.
		/// </summary>
		/// <param name="statusCode">
		/// The status code to return.
		/// </param>
		void Returns(HttpStatusCode statusCode);

		/// <summary>
		/// Specifies a data transfer object to return.
		/// </summary>
		/// <param name="dto">
		/// The data transfer object.
		/// </param>
		void Returns(object dto);

		/// <summary>
		/// Specifies an <paramref name="exception"/> to throw.
		/// </summary>
		/// <param name="exception">
		/// The <paramref name="exception"/> to throw.
		/// </param>
		void Throws(Exception exception);

		#endregion
	}
}