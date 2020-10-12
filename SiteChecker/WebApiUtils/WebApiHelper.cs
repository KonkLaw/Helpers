using System;
using System.IO;
using System.Net;

namespace WebApiUtils
{
	public readonly struct PostRequestOptions
	{
		public readonly string ContentType;
		public readonly DecompressionMethods DecompressionMethods;
		public readonly RequestHeader[]? Headers;

		public PostRequestOptions(
			string contentType,
			DecompressionMethods decompressionMethods,
			RequestHeader[]? headers = null)
		{
			ContentType = contentType;
			DecompressionMethods = decompressionMethods;
			Headers = headers;
		}
	}

	public static class WebApiHelper
	{
		public static string GetRequestGetBody(Uri uri)
		{
			using WebResponse webResponse = WebRequest.Create(uri).GetResponse();
			using Stream responseStream = webResponse.GetResponseStream();
			using var streamReader = new StreamReader(responseStream);
			return streamReader.ReadToEnd();
		}

		public static WebHeaderCollection GetRequestGetHeaders(Uri uri)
		{
			using WebResponse webResponse = WebRequest.Create(uri).GetResponse();
			return webResponse.Headers;
		}

		public static string PostRequestResponseBody(
			Uri uri, string requestBody, in PostRequestOptions postRequestOptions)
		{
			HttpWebRequest request = WebRequest.CreateHttp(uri);
			request.AutomaticDecompression = postRequestOptions.DecompressionMethods;
			const string postMethodName = "POST";
			request.Method = postMethodName;
			request.ContentType = postRequestOptions.ContentType;
			WebHeaderCollection requestHeaders = request.Headers;
			if (postRequestOptions.Headers != null)
				foreach (RequestHeader header in postRequestOptions.Headers)
				{
					requestHeaders.Add(header.Name, header.Value);
				}
			using (Stream requestBodyStream = request.GetRequestStream())
			{
				using var streamWriter = new StreamWriter(requestBodyStream);
				streamWriter.Write(requestBody);
			}

			using WebResponse response = request.GetResponse();
			using Stream responseStream = ((HttpWebResponse)response).GetResponseStream();
			using var streamReader = new StreamReader(responseStream);
			return streamReader.ReadToEnd();
		}
	}

	public readonly struct RequestHeader
	{
		public readonly string Name;
		public readonly string Value;

		public RequestHeader(string headerName, string headerValue)
		{
			Name = headerName;
			Value = headerValue;
		}
	}
}