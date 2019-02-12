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
using System.Xml.Serialization;

namespace XMLBasedData
{
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
	public partial class root
	{
		private rootMovie movieField;
		private string responseField;

		/// <remarks/>
		public rootMovie movie
		{
			get
			{
				return this.movieField;
			}
			set
			{
				this.movieField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string response
		{
			get
			{
				return this.responseField;
			}
			set
			{
				this.responseField = value;
			}
		}
	}

	/// <remarks/>
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public partial class rootMovie
	{
		private string titleField;

		private string yearField;

		private string ratedField;

		private string releasedField;

		private string runtimeField;

		private string genreField;

		private string directorField;

		private string writerField;

		private string actorsField;

		private string plotField;

		private string languageField;

		private string countryField;

		private string awardsField;

		private string posterField;

		private string metascoreField;

		private string imdbRatingField;

		private string imdbVotesField;

		private string imdbIDField;

		private string typeField;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string title
		{
			get
			{
				return this.titleField;
			}
			set
			{
				this.titleField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string year
		{
			get
			{
				return this.yearField;
			}
			set
			{
				this.yearField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string rated
		{
			get
			{
				return this.ratedField;
			}
			set
			{
				this.ratedField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string released
		{
			get
			{
				return this.releasedField;
			}
			set
			{
				this.releasedField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string runtime
		{
			get
			{
				return this.runtimeField;
			}
			set
			{
				this.runtimeField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string genre
		{
			get
			{
				return this.genreField;
			}
			set
			{
				this.genreField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string director
		{
			get
			{
				return this.directorField;
			}
			set
			{
				this.directorField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string writer
		{
			get
			{
				return this.writerField;
			}
			set
			{
				this.writerField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string actors
		{
			get
			{
				return this.actorsField;
			}
			set
			{
				this.actorsField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string plot
		{
			get
			{
				return this.plotField;
			}
			set
			{
				this.plotField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string language
		{
			get
			{
				return this.languageField;
			}
			set
			{
				this.languageField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string country
		{
			get
			{
				return this.countryField;
			}
			set
			{
				this.countryField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string awards
		{
			get
			{
				return this.awardsField;
			}
			set
			{
				this.awardsField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string poster
		{
			get
			{
				return this.posterField;
			}
			set
			{
				this.posterField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string metascore
		{
			get
			{
				return this.metascoreField;
			}
			set
			{
				this.metascoreField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string imdbRating
		{
			get
			{
				return this.imdbRatingField;
			}
			set
			{
				this.imdbRatingField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string imdbVotes
		{
			get
			{
				return this.imdbVotesField;
			}
			set
			{
				this.imdbVotesField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string imdbID
		{
			get
			{
				return this.imdbIDField;
			}
			set
			{
				this.imdbIDField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string type
		{
			get
			{
				return this.typeField;
			}
			set
			{
				this.typeField = value;
			}
		}
	}

	public class IMDBXML
	{
		public root mMovie;
		public string searchName;
		public FileInfo mFileInfo;

		//Constructor
		public IMDBXML(FileInfo file, string movieName, string year = "")
		{
			mFileInfo = file;
			searchName = movieName;

			var url = getIMDbUrl(System.Uri.EscapeUriString(movieName), year);
			var client = new WebClient();

			client.Proxy.Credentials = CredentialCache.DefaultCredentials;
			StreamReader reader = new StreamReader(client.OpenRead(url));

			mMovie = (root)new XmlSerializer(typeof(root)).Deserialize(reader);
			reader.Close();
		}

		//Get IMDb URL from search results
		private string getIMDbUrl(string movieName, string year)
		{
			// return "http://www.imdb.com/xml/find?json=1&q=" + MovieName;
			return "http://www.omdbapi.com/?t=" + movieName + "&y=" + year + "&plot=full&r=xml";
		}

		// Public fileds
		public string FileName { get { return mFileInfo.Name; } }
		public string Title { get { return mMovie.movie != null ? mMovie.movie.title : ""; } }
		public string Plot { get { return mMovie.movie != null ? mMovie.movie.plot : ""; } }
		public string Year { get { return mMovie.movie != null ? mMovie.movie.year : ""; } }
		public string Rated { get { return mMovie.movie != null ? mMovie.movie.rated : ""; } }
		public string Released { get { return mMovie.movie != null ? mMovie.movie.released : ""; } }
		public string Runtime { get { return mMovie.movie != null ? mMovie.movie.runtime : ""; } }
		public string Genre { get { return mMovie.movie != null ? mMovie.movie.genre : ""; } }
		public string Director { get { return mMovie.movie != null ? mMovie.movie.director : ""; } }
		public string Writer { get { return mMovie.movie != null ? mMovie.movie.writer : ""; } }
		public string Actors { get { return mMovie.movie != null ? mMovie.movie.actors : ""; } }
		public string Language { get { return mMovie.movie != null ? mMovie.movie.language : ""; } }
		public string Country { get { return mMovie.movie != null ? mMovie.movie.country : ""; } }
		public string Awards { get { return mMovie.movie != null ? mMovie.movie.awards : ""; } }
		//public string Poster { get { return mMovie.movie != null ? mMovie.movie.poster : ""; } }
		public string Metascore { get { return mMovie.movie != null ? mMovie.movie.metascore : ""; } }
		public string ImdbRating { get { return mMovie.movie != null ? mMovie.movie.imdbRating : ""; } }
		public string ImdbVotes { get { return mMovie.movie != null ? mMovie.movie.imdbVotes : ""; } }
		public string ImdbID { get { return mMovie.movie != null ? mMovie.movie.imdbID : ""; } }
		public string Type { get { return mMovie.movie != null ? mMovie.movie.type : ""; } }
		public string SearchName { get { return searchName; } }
	}
}