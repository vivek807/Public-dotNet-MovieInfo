using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Windows;

namespace JSONBasedData
{
	public class Movies
	{
		public List<Movie> title_exact { get; set; }
	}

	public class Movie
	{
		public string id { get; set; }
		public string title { get; set; }
		public string name { get; set; }
		public string title_description { get; set; }
		public string episode_title { get; set; }
		public string description { get; set; }
	}

	public class RequestState
	{
		public WebRequest Request;
		public string MovieTitle;
		public RequestState()
		{
			Request = null;
			MovieTitle = string.Empty;
		}
	}

	public class IMDbJSON
	{
		//Search Engine URLs
		private string GoogleSearch = "http://www.google.com/search?q=imdb+";
		public Movies mMovies;

		//Constructor
		public IMDbJSON(string MovieName)
		{
			mMovies = new JavaScriptSerializer().Deserialize<Movies>(getUrlData(MovieName));
		}

		//Get IMDb URL from search results
		private string getIMDbUrl(string MovieName)
		{
			return "http://www.imdb.com/xml/find?json=1&q=" + MovieName;
		}

		private string getUrlData(string movieTitle)
		{
			var url = getIMDbUrl(System.Uri.EscapeUriString(movieTitle));
			var client = new WebClient();

			client.Proxy.Credentials = CredentialCache.DefaultCredentials;

			return new StreamReader(client.OpenRead(url)).ReadToEnd().ToString();
		}

		//Get URL Data
		private void fetchURLData(string movieTitle)
		{
			var url = getIMDbUrl(System.Uri.EscapeUriString(movieTitle));
			var Wr = WebRequest.Create(url);
			Wr.Timeout = 15000;//15 secs
			Wr.Proxy.Credentials = CredentialCache.DefaultCredentials;

			Wr.BeginGetResponse(new AsyncCallback(ResponseCallback), new RequestState() { Request = Wr, MovieTitle = movieTitle });
		}

		void ResponseCallback(IAsyncResult result)
		{
			try
			{
				RequestState state = (RequestState)result.AsyncState;
				WebRequest request = (WebRequest)state.Request;
				// get the Response
				HttpWebResponse Response = (HttpWebResponse)request.EndGetResponse(result);
				
				String json = new StreamReader(Response.GetResponseStream()).ReadToEnd();

				mMovies = new JavaScriptSerializer().Deserialize<Movies>(json);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

	}
}