// -------------------------------------------------------------------------------------------------
// <copyright file="IResponseSetup.cs" company="Start Group">
//   Copyright (c) Start Group. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------
namespace StartGroup.HistorianReplay.TestCommon.HttpMessageHandlerStub
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;

	public interface IResponseSetup
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
		/// Specifies a status code and reason phrase to return.
		/// </summary>
		/// <param name="statusCode">
		/// The status code to return.
		/// </param>
		/// <param name="reasonPhrase">
		/// The reason phrase to return.
		/// </param>
		void Returns(HttpStatusCode statusCode, string reasonPhrase);

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