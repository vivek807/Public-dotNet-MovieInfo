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

namespace MovieInfo
{
    [XmlRoot("Movie_list")]
    public class MovieList
    {
        public MovieList() { Items = new List<WebserviceReponse>(); }
        [XmlElement("root")]
        public List<WebserviceReponse> Items { get; set; }
    }
  
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false, ElementName="root")]
	public partial class WebserviceReponse
	{
		private Movie movieField;
		private string responseField;

        public WebserviceReponse(Movie movie)
        {
            movieField = movie;
        }

        public WebserviceReponse() { }

		/// <remarks/>
		public Movie movie
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
	public partial class Movie
	{
        private string titleField = "N/A";
        private string yearField = "";
        private string ratedField = "";
		private string releasedField = "";
		private string runtimeField = "";
		private string genreField = "";
		private string directorField = "";
		private string writerField = "";
		private string actorsField = "";
		private string plotField = "";
		private string languageField = "";
		private string countryField = "";
		private string awardsField = "";
		private string posterField = "";
		private string metascoreField = "";
		private string imdbRatingField = "";
		private string imdbVotesField = "";
		private string imdbIDField = "";
		private string typeField = "";
        private string filePathField = "";
        private string scanDate = DateTime.Now.ToString("dd-MM-yyyy");
        private bool isWatched = false;

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

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string scandate
        {
            get
            {
                return this.scanDate;
            }
            set
            {
                this.scanDate = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool alreadyWatched
        {
            get
            {
                return this.isWatched;
            }
            set
            {
                this.isWatched = value;
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

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string filePath
        {
            get
            {
                return this.filePathField;
            }
            set
            {
                this.filePathField = value;
            }
        }

        private string searchNameField = "";
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string searchName
        {
            get
            {
                return this.searchNameField;
            }
            set
            {
                this.searchNameField = value;
            }
        }

        private string originalNameField = "";
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string originalName
        {
            get
            {
                return this.originalNameField;
            }
            set
            {
                this.originalNameField = value;
            }
        }

	}

	public class IMDBXML
	{
		private WebserviceReponse mWebResponse = null;

        public Movie GetMovieInfo() { return mWebResponse.movie; }

        public IMDBXML(WebserviceReponse response)
        {
            this.mWebResponse = response;
        }

        public bool IsMovieDataAvailable()
        {
            return this.mWebResponse != null && this.mWebResponse.response.ToLower().CompareTo("true") == 0;
        }

        public WebserviceReponse GetResponse() { return mWebResponse; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="originalName"></param>
		/// <param name="movieName"></param>
		/// <param name="year"></param>
        public IMDBXML(String filePath, String originalName, string movieName, string year = "", bool useCache = true)
        {
            // Try first from backup file
            WebserviceReponse cachedResponse = useCache ? UserPreferences.Instance.TryAndGetMovie(originalName) : null;

            if (cachedResponse == null)
            {
                StreamReader reader = Utility.GetMovieData(movieName);

                mWebResponse = (WebserviceReponse)new XmlSerializer(typeof(WebserviceReponse)).Deserialize(reader);
                if (mWebResponse.movie == null) mWebResponse.movie = new Movie();

                // Fill custom fields
                {
                    mWebResponse.movie.originalName = originalName;
                    mWebResponse.movie.searchName = movieName;
                    mWebResponse.movie.filePath = filePath;
                }

                reader.Close();
            }
            else
            {
                if (cachedResponse.movie.filePath != "") cachedResponse.movie.filePath = filePath;

                mWebResponse = cachedResponse;
            }
        }

        public bool AlreadyWatched { get { return mWebResponse.movie.alreadyWatched;} set {mWebResponse.movie.alreadyWatched = value;} }
        public string FileName { get { return mWebResponse.movie.originalName; } set { mWebResponse.movie.originalName= value; } }
        public string Plot { get { return mWebResponse.movie.plot; } set { mWebResponse.movie.plot= value; } }
		public string Year { get { return mWebResponse.movie.year; } set { mWebResponse.movie.year= value; } }
		public string Runtime { get { return mWebResponse.movie.runtime; } set { mWebResponse.movie.runtime= value; } }
		public string Genre { get { return mWebResponse.movie.genre; } set { mWebResponse.movie.genre= value; } }
        public string ImdbRating { get { return mWebResponse.movie.imdbRating; } set { mWebResponse.movie.imdbRating= value; } }
        public string FilePath { get { return mWebResponse.movie.filePath; } set { mWebResponse.movie.filePath= value; } }
        public string Title { get { return mWebResponse.movie.title; } set { mWebResponse.movie.title= value; } } 
        public string Director { get { return mWebResponse.movie.director; } set { mWebResponse.movie.director= value; } }
        public string Actors { get { return mWebResponse.movie.actors; } set { mWebResponse.movie.actors= value; } }
        public string Awards { get { return mWebResponse.movie.awards; } set { mWebResponse.movie.awards= value; } }
        public string Country { get { return mWebResponse.movie.country; } set { mWebResponse.movie.country= value; } }
        public string Released { get { return mWebResponse.movie.released; } set { mWebResponse.movie.released= value; } }
        public string Writer { get { return mWebResponse.movie.writer; } set { mWebResponse.movie.writer= value; } }
        public string Language { get { return mWebResponse.movie.language; } set { mWebResponse.movie.language= value; } }
        public string ScanDate { get { return mWebResponse.movie.scandate; } set { mWebResponse.movie.scandate = value; } }
        #if false
        public string Type { get { return mWebResponse.movie.type; } set { mWebResponse.movie.type= value; } }
        public string Rated { get { return mWebResponse.movie.rated; } set { mWebResponse.movie.rated= value; } }
        public string Poster { get { return mWebResponse.movie.poster; } set { mWebResponse.movie.poster= value; } }
        public string Metascore { get { return mWebResponse.movie.metascore; } set { mWebResponse.movie.metascore= value; } }
        public string ImdbVotes { get { return mWebResponse.movie.imdbVotes; } set { mWebResponse.movie.imdbVotes= value; } }
        public string ImdbID { get { return mWebResponse.movie.imdbID; } set { mWebResponse.movie.imdbID= value; } }
        public string SearchName { get { return mWebResponse.movie.searchName; } set { mWebResponse.movie.searchName= value; } }
        #endif
    }
}