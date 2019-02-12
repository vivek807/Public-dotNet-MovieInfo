using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Xml.Serialization;

namespace MovieInfo
{
    sealed class UserPreferences
    {
        #region Singleton Handling

        private static readonly UserPreferences instance = new UserPreferences();
        private static string kBackupConfigFile = "backupfile";
        private static Dictionary<string, WebserviceReponse> mBackupFileMap = new Dictionary<string, WebserviceReponse>();
        private static StringCollection mColumns;
        private UserPreferences() { }

        public static UserPreferences Instance
        {
            get
            {
                return instance;
            }
        }

        #endregion

        #region public function

        public StringCollection GetSavedMenuCollectionIfAny()
        {
            return mColumns;
        }

        public bool TryAndRemoveMovie(String name)
        {
            return mBackupFileMap.Remove(name);
        }

        public WebserviceReponse TryAndGetMovie(string name)
        {
            WebserviceReponse result = null;

            try
            {
                mBackupFileMap.TryGetValue(name, out result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[WARNING] " + ex.Message);
            }

            return result;
        }

        public Dictionary<string, WebserviceReponse>.ValueCollection GetLastSession()
        {
            return mBackupFileMap.Values;
        }

        public void UpdateBackupFile(IMDBXML item)
        {
            mBackupFileMap[item.GetMovieInfo().originalName] = item.GetResponse();
        }

        public void LoadPreferences(MainWindow _mainWindow)
        {
            try
            {
#if false
			_mainWindow.Top = Properties.Windows.Default.WindowTop;
			_mainWindow.Left = Properties.Windows.Default.WindowLeft;
			_mainWindow.Height = Properties.Windows.Default.WindowHeight;
			_mainWindow.Width = Properties.Windows.Default.WindowWidth;
			_mainWindow.WindowState = Properties.Windows.Default.WindowState;
#endif
                // Color Settings
                System.Windows.Application.Current.Resources["AccentColor"] = (Color)ColorConverter.ConvertFromString(Properties.Windows.Default.AccentColor);

                // Menu Strings
                mColumns = Properties.Windows.Default.Columns;

                // Backupfile
                if (File.Exists(kBackupConfigFile))
                {
                    using (var reader = new StreamReader(kBackupConfigFile))
                    {
                        MovieList backupList = (MovieList)new XmlSerializer(typeof(MovieList)).Deserialize(reader);

                        foreach (WebserviceReponse item in backupList.Items)
                        {
                            mBackupFileMap.Add(item.movie.originalName, item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[WARNING] " + ex.Message);
            }
        }

        public void SavePreferences(MainPage _mainWindow)
        {
            try
            {
#if false
			if (_mainWindow.WindowState != System.Windows.WindowState.Minimized)
			{
				if (_mainWindow.WindowState == System.Windows.WindowState.Maximized)
				{
					// Use the RestoreBounds as the current values will be 0, 0 and the size of the screen
					Properties.Windows.Default.WindowTop = _mainWindow.RestoreBounds.Top;
					Properties.Windows.Default.WindowLeft = _mainWindow.RestoreBounds.Left;
					Properties.Windows.Default.WindowHeight = _mainWindow.RestoreBounds.Height;
					Properties.Windows.Default.WindowWidth = _mainWindow.RestoreBounds.Width;
				}
				else
				{
					Properties.Windows.Default.WindowTop = _mainWindow.Top;
					Properties.Windows.Default.WindowLeft = _mainWindow.Left;
					Properties.Windows.Default.WindowHeight = _mainWindow.Height;
					Properties.Windows.Default.WindowWidth = _mainWindow.Width;
				}

				Properties.Windows.Default.WindowState = _mainWindow.WindowState;
				Properties.Windows.Default.Save();
			}
#endif
                Color color = (Color)System.Windows.Application.Current.Resources["AccentColor"];
                Properties.Windows.Default.AccentColor = color.ToString();

                if (_mainWindow.cxMenu != null)
                {
                    Properties.Windows.Default.Columns = new StringCollection();

                    foreach (System.Windows.Controls.MenuItem item in _mainWindow.cxMenu.Items)
                    {
                        Properties.Windows.Default.Columns.Add(item.Header + ";" + item.IsChecked);
                    }
                }

                // Create back up
                if (_mainWindow.mMovieCollection.Count > 0)
                {
                    // Update list and export
                    foreach (var item in _mainWindow.mMovieCollection)
                    {
                        WebserviceReponse res = item.GetResponse();
                        WebserviceReponse found;

                        mBackupFileMap.TryGetValue(res.movie.originalName, out found);

                        if (found == null)
                        {
                            mBackupFileMap.Add(res.movie.originalName, res);
                        }
                    }

                    FileInfo fi = null;

                    if (File.Exists(kBackupConfigFile))
                    {
                        fi = new FileInfo(kBackupConfigFile);
                        fi.Attributes = FileAttributes.Normal;
                    }

                    Utility.ExportToXML(mBackupFileMap.Values, kBackupConfigFile, false);

                    if (fi == null) fi = new FileInfo(kBackupConfigFile);
                    fi.Attributes = FileAttributes.Hidden;
                }



                Properties.Windows.Default.Save();
            }
            catch (Exception ex)
            {
                Console.WriteLine("[WARNING] " + ex.Message);
            }
        }

        public bool QuitMainWindow(MainPage _mainWindow)
        {
            bool cancel = true;

            if (MetroMessageBox.ShowErrorMessage(null, "Are you sure to quit the Application ?",
                   "Information !!",
                   System.Drawing.SystemIcons.Warning,
                   MessageBoxButton.OKCancel,
                   MessageBoxResult.Cancel) == System.Windows.MessageBoxResult.OK)
            {
                cancel = false;
                UserPreferences.Instance.SavePreferences(_mainWindow); // Save UI data
            }

            return cancel;
        }

        #endregion // public functions
    }
}
