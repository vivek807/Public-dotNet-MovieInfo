using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace MovieInfo
{
    class Utility
    {
        public static string[] filters = { ".avi", ".mkv", ".mp4", ".m4v", ".rmvb", ".mov", ".dat" };
        private static readonly string duplicateTempFileName = "Duplicate Files.txt";

        static Utility()
        {
            client.Proxy.Credentials = CredentialCache.DefaultCredentials;
        }

        static public bool ConvertToBool(string value)
        {
            return value.ToLower().CompareTo("true") == 0 ? true : false;
        }

        static public DateTime GetCurrentDateFromNet()
        {
            try
            {
                var myHttpWebRequest = (HttpWebRequest)WebRequest.Create("http://www.google.co.in");

                myHttpWebRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;

                var response = myHttpWebRequest.GetResponse();
                string todaysDates = response.Headers["date"];

                return DateTime.ParseExact(todaysDates, "ddd, dd MMM yyyy HH:mm:ss 'GMT'", CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AssumeUniversal);
            } catch(Exception ex)
            {
                MetroMessageBox.ShowErrorMessage(null, "Internet connection is required to run the app properly. Some of the movies will not be loaded !!", "Check internet connection !!", System.Drawing.SystemIcons.Error, MessageBoxButton.OK, MessageBoxResult.OK);
                throw ex;
            }
        }

        static public StreamReader GetURLData(string url)
        {
            return new StreamReader(client.OpenRead(url));
        }

        static public string GetCleanName(string fileName)
        {
            String result = fileName.Trim().ToLower();

            // #0 panic words
            if (Regex.IsMatch(result, panic_words)) { return null; }

            // #1 remove all string after year (if found)
            result = Regex.Replace(result, @"([\(\{\[])?\d{4}([\)\}\]])?(.*)$", "");

            // #2 remove year and stuff following it
            result = Regex.Replace(result, reject_words, "");           

            // #3 replace dot(.) into space
            result = Regex.Replace(result, @"\.|-|_|~", " ");

            return String.IsNullOrWhiteSpace(result) ? fileName : result.Normalize();
        }

         public static void TurnOnAuthentication()
        {
#if !DEBUG
            if (authenticate && isLastAuthenticateFailed)
                authenticate = false;
#endif
        }
#if !DEBUG
        private static bool authenticate = false;
        private static bool isLastAuthenticateFailed = true;

        public static void TurnOffAuthentication()
        {
            authenticate = true;
        }
#endif
        const string emailId = @"approach2vivek@gmail.com";

        public static StreamReader GetMovieData(string movieName, string year = "")
        {
#if !DEBUG
            try
            {
                if (!authenticate)
                {
                    DateTime currentDate = Utility.GetCurrentDateFromNet();
                    DateTime expiryDate = DateTime.ParseExact("31/12/2017", "dd/MM/yyyy", CultureInfo.InvariantCulture);

                    if (currentDate.CompareTo(expiryDate) > 0)
                    {
                        String msg = @"Package expired. Please email to developer (" + emailId + ") for new package or check for latest package online. It's free !!";
                        Utility.ExitApplication(msg);
                    }

                    isLastAuthenticateFailed = false;
                }
            }
            catch (System.Net.WebException) {
                isLastAuthenticateFailed = true;
                /* gulp exception */
            }
            catch (Exception ex)
            {
                Console.WriteLine("[WARNING] " + ex.Message);
                isLastAuthenticateFailed = true;
                MetroMessageBox.ShowErrorMessage(null, ex.Message, "Error !!", System.Drawing.SystemIcons.Error, MessageBoxButton.OK, MessageBoxResult.OK);
            }

            authenticate = true;
#endif // if RELEASE
            return GetURLData(GetIMDbUrl(movieName, year));
        }

        public static void ExportToExcel(List<IMDBXML> movieList)
        {
            ExportToExcel<MovieInfo.IMDBXML, List<MovieInfo.IMDBXML>> export = new ExportToExcel<IMDBXML, List<IMDBXML>>();

            export.dataToPrint = movieList;
            export.GenerateReport();
        }

        public static void ExportToXML(ObservableCollection<MovieInfo.IMDBXML> movieInfoList, string fileName = "movies.xml", bool show = true)
        {
            MovieList list = new MovieList();

            foreach (IMDBXML xmlData in movieInfoList)
            {
                list.Items.Add(xmlData.GetResponse());
            }

            Utility.ExportToXML(list, fileName, show);
        }

        public static void ExportToXML(Dictionary<string, WebserviceReponse>.ValueCollection valueCollection, string fileName, bool show)
        {
           MovieList list = new MovieList();

           list.Items.AddRange(valueCollection);
           Utility.ExportToXML(list, fileName, show);
        }

        public static void ExportToXML(MovieList list, string fileName = "movies.xml", bool show = true)
        {
            using (var writer = new StreamWriter(fileName))
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(MovieList));

                deserializer.Serialize(writer, list);
            }

            if (show) Process.Start(fileName);
        }

        public static void FindDuplicate(ObservableCollection<MovieInfo.IMDBXML> list)
        {
            var movieList = new List<IMDBXML>(list);
            var dups = movieList.GroupBy(x => x.Title)
                                .Where(x => x.Count() > 1)
                                .Where(grp => grp.Key != "N/A")
                                .Select(grp => new { key = grp.Key, duplciateItems = grp.ToList() })
                                .ToList();

            if (dups.Count > 0)
            {
                File.WriteAllLines(duplicateTempFileName, dups.Select(x => "[ " + x.key + " ]" + Environment.NewLine + "    " + String.Join(Environment.NewLine + "    ", x.duplciateItems.Select(y => y.FilePath).ToArray())).ToArray());

                Process.Start(duplicateTempFileName);
            }
            else
            {
                MetroMessageBox.ShowErrorMessage(null, "No duplicate entry found.", "Information !!", System.Drawing.SystemIcons.Information, MessageBoxButton.OK, MessageBoxResult.OK);
            }
        }

        public static void FindEmptyRecords(ObservableCollection<MovieInfo.IMDBXML> movieList)
        {
            int count = movieList.Count(x => x.Title == "N/A");

            MetroMessageBox.ShowErrorMessage(null, "Empty Records: " + count + " / " + movieList.Count, "Information !!", System.Drawing.SystemIcons.Information, MessageBoxButton.OK, MessageBoxResult.OK);
        }

        private static string ImdbIDRegex = @"tt[0-9]+";
        private static string GetIMDbUrl(string movieName, string year)
        {
            if (movieName == null) return "";

            // return "http://www.imdb.com/xml/find?json=1&q=" + MovieName;
			// http://img.omdbapi.com/?apikey=[yourkey]&
            string url = "http://www.omdbapi.com/?apikey=39a7bbd3&";

            if (Regex.IsMatch(movieName, ImdbIDRegex))
            {
                url += "i=";
            }
            else
            {
                url += "t=";
            } 

            url += movieName + "&y=" + year + "&plot=full&r=xml";
            return url;
        }

        private static WebClient client = new WebClient();
        private static string reject_words = @"yify|e-sub|bluray|dual audio|dual.audio|dual_audio|stylishsalh|filmywap|dvdscr|ac3|dvdrip|dvd|xvid|hdtv|bdrip|etrg|webrip|hdrip|brrip|r5|480p|720p|1080p|uncut|moviexinema|usabit.com|hd 720p|720|dubby|divx|playxd|1cdrip|cdrip|unrated|aac|x264|klaxxon|axxo|br_300|300mb|cd1|cd2|\s*?(?:\(.*?\)|\[.*?\]|\{.*?\})|[ ]{2,}";
        private static string panic_words = @"utorrentpartfile|^sample$|^etrg$";


        public static void CancelAsyn()
        {
            if (client.IsBusy) client.CancelAsync();
        }
        public static void OpenHelpPage()
        {
            Process.Start(new ProcessStartInfo(@"https://handshakingwithdigitalworld.blogspot.in/2016/06/fetch-information-about-movies-from.html"));
        }
        public static bool DownloadImage(String url, String filePath)
        {
            bool result = false;

            try
            {
                client.DownloadFile(url, filePath);
                result = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[WARNING] " + ex.Message);
            }

            return result;
        }
        public static void ExitApplication(string message)
        {
            try
            {
                MetroMessageBox.ShowErrorMessage(null, message, "Information !!", System.Drawing.SystemIcons.Information, MessageBoxButton.OK, MessageBoxResult.OK);
            }
            catch(Exception)
            {
                System.Windows.Forms.MessageBox.Show(message);
            }
            finally
            {
#if !DEBUG
                Utility.OpenHelpPage();
                Environment.Exit(0);
#endif
            }
        }

        static string hash_compute(string file_path) //compute MD5 hash of first and last 64kb of the video file
        {
            try
            {
                using (BinaryReader b = new BinaryReader(File.Open(file_path, FileMode.Open)))
                {
                    int required = 64 * 1024;

                    b.BaseStream.Seek(0, SeekOrigin.Begin);

                    byte[] by = b.ReadBytes(required);

                    b.BaseStream.Seek(-required, SeekOrigin.End);

                    byte[] bye = b.ReadBytes(required);
                    byte[] final = new byte[by.Length + bye.Length];

                    by.CopyTo(final, 0);
                    Array.Copy(bye, 0, final, by.Length, bye.Length);

                    MD5CryptoServiceProvider a = new MD5CryptoServiceProvider();
                    byte[] result = a.ComputeHash(final);

                    StringBuilder sb = new StringBuilder();

                    foreach (byte item in result)
                    {
                        sb.Append(item.ToString("X2"));
                    }

                    return sb.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[WARNING] " + ex.Message + "\n" + ex.TargetSite);
                return "err";
            }
        }

        public static Boolean sub_return(string file_path)
        {
            Boolean subtitleDownloaded = false;
            String subTitleFile = Path.GetDirectoryName(file_path) + Path.DirectorySeparatorChar
                            + Path.GetFileNameWithoutExtension(file_path)
                            + ".srt";

            if (File.Exists(subTitleFile)) return subtitleDownloaded;

            string final_hash = hash_compute(file_path); //compute hash function

            if (!final_hash.Equals("err"))
            {
                using (var client = new WebClient())
                {
                    client.Headers.Add("user-agent", "SubDB/1.0 (SubIt/0.9; https://handshakingwithdigitalworld.blogspot.in/)"); //required header where subdb/1.0 needs to be intact
                    string URL = "http://api.thesubdb.com/?action=download&language=en&hash=" + final_hash; //URL with hash and language of subtitle required

                    try
                    {
                        client.Proxy.Credentials = CredentialCache.DefaultCredentials;

                        Stream stream = client.OpenRead(URL);
                        StreamReader read_stream = new StreamReader(stream); //response stream

                        if (read_stream != null)
                        {
                            File.WriteAllText(subTitleFile, read_stream.ReadToEnd());
                            subtitleDownloaded = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("[INFO] Subtitle not found.");
                        Console.WriteLine("[WARNING] " + ex.Message);
                    }
                }
            }
            else
            {
                Console.WriteLine("Some program is accessing the file. Please close that program and try again.");
                Console.ReadLine();
            }

            return subtitleDownloaded;
        }
    }
}
