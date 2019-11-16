using System;
using System.IO;
using System.Net;
using System.Text;

namespace WebApiUtils
{
	public static class WebApiHelper
	{
		public static string GetResponseString(Uri uri)
		{
			using Stream responseStream = WebRequest.Create(uri).GetResponse().GetResponseStream();
				using var streamReader = new StreamReader(responseStream, Encoding.UTF8);
					return streamReader.ReadToEnd();
		}

		public static string GetResponseString(HttpWebRequest request) => GetResponseString(request, out _);

		public static string GetResponseString(HttpWebRequest request, out WebHeaderCollection headers)
		{
			using WebResponse response = request.GetResponse();
			headers = response.Headers;
			using Stream responseStream = ((HttpWebResponse)response).GetResponseStream();
				using var streamReader = new StreamReader(responseStream, Encoding.UTF8);
					return streamReader.ReadToEnd();
		}

		// REQUEST

		private const string PostMethodName = "POST";

		public static WebRequest GetPostRequest(Uri uri, string requestBody)
		{
			WebRequest request = WebRequest.Create(uri);
			request.Method = PostMethodName;
			using (Stream requestBodyStream = request.GetRequestStream())
			{
				using var streamWriter = new StreamWriter(requestBodyStream);
					streamWriter.Write(requestBody);
			}
			return request;
		}

		public static HttpWebRequest GetPostRequestWithCookies(
			Uri uri, string requestBody, string contentType,
			params RequestHeader[] headers)
		{
			HttpWebRequest request = WebRequest.CreateHttp(uri);
			request.Method = PostMethodName;
			request.ContentType = contentType;
			WebHeaderCollection requestHeaders = request.Headers;
			foreach (RequestHeader header in headers)
			{
				requestHeaders.Add(header.Name, header.Value);
			}
			using (Stream requestBodyStream = request.GetRequestStream())
			{
				using var streamWriter = new StreamWriter(requestBodyStream);
					streamWriter.Write(requestBody);
			}
			return request;
		}

		// COMMON

		public static RequestHeader CreateCookiesString(params (string key, string value)[] cookies)
		{
			const string cookieHeaderName = "Cookie";

			var result = new StringBuilder();
			foreach ((string key, string value) in cookies)
			{
				result.Append(key);
				result.Append('=');
				result.Append(value);
				result.Append("; ");
			}
			return new RequestHeader(cookieHeaderName, result.ToString());
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