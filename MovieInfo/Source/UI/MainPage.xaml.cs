using MovieInfo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace MovieInfo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainPage : Page, INotifyPropertyChanged
    {
        #region Data Members
        private DataGrid mGrid = null;
        private int FileCount = 0;
        const double kFixedHeight = 550;
        public ObservableCollection<MovieInfo.IMDBXML> mMovieCollection = new ObservableCollection<MovieInfo.IMDBXML>();
        private HashSet<String> mMovieDistinctCollecion = new HashSet<String>();
        private ICollectionView mCollectionView = null;
        private GroupFilter mFilters = null;
        private MainWindow mParentWindow = null;
        #endregion

        #region Public Methods
        public void SetParentWindow(MainWindow window)
        {
            this.mParentWindow = window;
        }
        #endregion

        #region Constructor
        public MainPage()
        {
            try
            {
                InitializeComponent();
                Init();

                // Create Image directory if not Exits
                System.IO.Directory.CreateDirectory(this.ImageFolder);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[WARNING] " + ex.Message);
                Utility.ExitApplication(ex.Message);
            }
        }

        public void Init()
        {
            this.DataContext = this;
            mCollectionView = CollectionViewSource.GetDefaultView(mMovieCollection);
            mFilters = new GroupFilter();
            //mGrid = mGridUI;
            mGrid = mPaneUI;
            
            this.mMovieGrid.Visibility = System.Windows.Visibility.Collapsed;
            this.mMoviePane.Visibility = System.Windows.Visibility.Visible;

            mGrid.ItemsSource = mCollectionView;
        }

        public void AddRowToCollection(IMDBXML item, bool scrollIntoView = false)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                if (mMovieDistinctCollecion.Add(item.GetMovieInfo().originalName))
                {
                    mMovieCollection.Add(item);
                    if (scrollIntoView) mGrid.ScrollIntoView(item);
                }
            }));
        }

        public void UpdateRowDataInCollection(IMDBXML oldItem, IMDBXML newItem)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                var index = mMovieCollection.IndexOf(mMovieCollection.Where(item => item.FilePath == oldItem.FilePath).FirstOrDefault());

                mMovieCollection[index] = newItem;
                UpdateRowDetails(newItem);
                UserPreferences.Instance.UpdateBackupFile(newItem);
            }));
        }

        public void RemoveRowToCollection(IMDBXML item, bool removefrombackup = false)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                mMovieDistinctCollecion.Remove(item.GetMovieInfo().originalName) ;
                mMovieCollection.Remove(item);

                if (removefrombackup) UserPreferences.Instance.TryAndRemoveMovie(item.FileName);
            }));
        }

        private void InitDataGridCollection(bool mergeResult)
        {
           if (mergeResult == false)
            {
                mMovieDistinctCollecion.Clear();
                mMovieCollection.Clear();
            }
        }
        #endregion

        #region progressBar
        BackgroundWorker worker = null;
        BackgroundWorker nestedWorker = null;

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            if (worker != null && worker.IsBusy)
            {
                worker.CancelAsync();
                Utility.CancelAsyn();
            }
            if (nestedWorker != null && nestedWorker.IsBusy) nestedWorker.CancelAsync();
        }

        private string _message;
        public string ProgressBarText
        {
            private set
            {
                _message = value;
                NotifyPropertyChanged("ProgressBarText");
            }
            get
            {
                return _message;
            }
        }
        protected void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 1) mProgressBar.IsIndeterminate = false;
            float percentage = (float)(e.ProgressPercentage * 100) / this.FileCount;

            ProgressBarText = percentage.ToString("0.0") + "% ( " + e.ProgressPercentage + " / " + this.FileCount + " )";
            mProgressBar.Value = percentage;
        }

        #endregion // ProgressBarText

        #region Grid Even Handling
        private void mGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column is DataGridTextColumn)
            {
                DataGridTextColumn textColumn = e.Column as DataGridTextColumn;

                textColumn.ElementStyle = mGrid.Resources["wordWrapStyle"] as Style;
            }
            else if (e.Column is DataGridCheckBoxColumn)
            {
                DataGridCheckBoxColumn column = e.Column as DataGridCheckBoxColumn;

                e.Column = (DataGridTemplateColumn)mGrid.Resources["AlreadyWatchedHeader"];
                e.Column.Header = "Already Watched";
            }
        }

        private void mGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (mPlayOnDoubleClick.IsChecked == true) this.Play_Click(this, null);
        }
        private void mGrid_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                    this.DeleteMovie_Click(this, null);
                    break;
                case Key.Enter:
                    this.Play_Click(this, null);
                    break;
                case Key.Add:
                    {
                        MouseWheelEventArgs arg = new MouseWheelEventArgs(InputManager.Current.PrimaryMouseDevice, Environment.TickCount, 1);

                        this.mGrid_PreviewMouseWheel(this, arg);
                        break;
                    }
                case Key.Subtract:
                    {
                        MouseWheelEventArgs arg = new MouseWheelEventArgs(InputManager.Current.PrimaryMouseDevice, Environment.TickCount, -1);

                        this.mGrid_PreviewMouseWheel(this, arg);
                        break;
                    }
            }
        }
        private void UpdateSourceData(object sender, RoutedEventArgs e)
        {
            IMDBXML item = mGrid.SelectedItem as IMDBXML;

            item.AlreadyWatched = ((CheckBox)sender).IsChecked == true;
        }
        #endregion

        #region Zooming
        private void mGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                this._zoom.Value += (e.Delta > 0) ? 0.1 : -0.1;
            }
        }
        private void mGrid_PreviewMouseDown(object sender, MouseButtonEventArgs args)
        {
            base.OnPreviewMouseDown(args);

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (args.MiddleButton == MouseButtonState.Pressed)
                {
                    this._zoom.Value = 1.0;
                }
            }
        }
        #endregion Zooming

        #region Context Menu
        private void OpenFileLocation_Click(object sender, RoutedEventArgs e)
        {
            if (mGrid.SelectedItem == null) return;

            var selectedMovie = mGrid.SelectedItem as IMDBXML;

            if (String.IsNullOrEmpty(selectedMovie.FilePath) || !File.Exists(selectedMovie.FilePath))
            {
                MetroMessageBox.ShowErrorMessage(null, "File Path not found",
                    "Error !!",
                    System.Drawing.SystemIcons.Error,
                    MessageBoxButton.OK,
                    MessageBoxResult.OK);
            }
            else
            {
                Process.Start("explorer.exe", @"/select, " + selectedMovie.FilePath);
            }
        }

        private void UpdateInfo_Click(object sender, RoutedEventArgs e)
        {
            if (mGrid.SelectedItem == null) return;

            var selectedMovie = mGrid.SelectedItem as IMDBXML;

            this.ShowEditMovieDialog(selectedMovie);
        }
        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (mGrid.SelectedItem == null) return;

            var selectedMovie = mGrid.SelectedItem as IMDBXML;

            if (selectedMovie == null || String.IsNullOrEmpty(selectedMovie.FilePath) || !File.Exists(selectedMovie.FilePath))
            {
                MetroMessageBox.ShowErrorMessage(null, "File Path not found", "Error !!", System.Drawing.SystemIcons.Error, MessageBoxButton.OK, MessageBoxResult.OK);
            }
            else
            {
                try
                {
                    Process.Start(selectedMovie.FilePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[WARNING] " + ex.Message);
                    MetroMessageBox.ShowErrorMessage(null, "File Path not found", "Error !!", System.Drawing.SystemIcons.Error, MessageBoxButton.OK, MessageBoxResult.OK);
                }
            }
        }
        private void DeleteMovie_Click(object sender, RoutedEventArgs e)
        {
            if (mGrid.SelectedItem == null) return;

            var selectedMovie = mGrid.SelectedItem as IMDBXML;
            bool removeLink = false;

            if (String.IsNullOrEmpty(selectedMovie.FilePath) || !File.Exists(selectedMovie.FilePath))
            {
                MetroMessageBox.ShowErrorMessage(null, "File Path not found", "Error !!", System.Drawing.SystemIcons.Error, MessageBoxButton.OK, MessageBoxResult.OK);
                removeLink = true;
            }
            else if (MetroMessageBox.ShowErrorMessage(null, "Are you sure to delete '" + selectedMovie.FileName + "' movie?" + Environment.NewLine + " This action cannot be undone.", "Are you Sure !!",
                        System.Drawing.SystemIcons.Warning, MessageBoxButton.OKCancel, MessageBoxResult.Cancel) == MessageBoxResult.OK)
            {
                try
                {
                    File.Delete(selectedMovie.FilePath);
                    this.RemoveRowToCollection(selectedMovie, true);
                }
                catch
                {
                    MetroMessageBox.ShowErrorMessage(null, "Unable to delete movie file.", "Error !!", System.Drawing.SystemIcons.Error, MessageBoxButton.OK, MessageBoxResult.OK);
                    removeLink = true;
                }
            }

            if (removeLink && MetroMessageBox.ShowErrorMessage(null, "Want to remove link anyway ?", "Are you Sure !!",
                        System.Drawing.SystemIcons.Warning, MessageBoxButton.OKCancel, MessageBoxResult.Cancel) == MessageBoxResult.OK)
            {
                this.RemoveRowToCollection(selectedMovie, true);
            }
        }
        #endregion Context Menu

        #region ToolBar
        private void subtitle_Click(object sender, RoutedEventArgs e)
        {
            if (mMovieCollection == null)
            {
                MetroMessageBox.ShowErrorMessage(null, "Nothing to find. Sorry !!", "Information !!", System.Drawing.SystemIcons.Information, MessageBoxButton.OK, MessageBoxResult.OK);

                return;
            }
            this.OnInitBackgroundProcess("Collecting files ...");

            int downloadedSubtitles = 0;
            this.worker = new BackgroundWorker();

            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += ProgressChanged;

            worker.DoWork += delegate
            {
                FileCount = mMovieCollection.Count;

                int index = 0;

                foreach (var item in mMovieCollection)
                {
                    if (worker.CancellationPending) break;

                    if (!String.IsNullOrEmpty(item.FilePath) && File.Exists(item.FilePath))
                    {
                        try
                        {
                            if (Utility.sub_return(item.FilePath))
                            {
                                ++downloadedSubtitles;
                            }

                            worker.ReportProgress(++index);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("[WARNING] " + ex.Message);
                        }
                    }
                }
            };

            worker.RunWorkerCompleted += delegate
            {
                this.OnEndBackgroundProcess();
                if (downloadedSubtitles > 0)
                {
                    if (downloadedSubtitles == 1)
                    {
                        MetroMessageBox.ShowErrorMessage(null, downloadedSubtitles + " new subtitle is downloaded in respected folder. Thanks", "Information !!", System.Drawing.SystemIcons.Information, MessageBoxButton.OK, MessageBoxResult.OK);
                    }
                    else
                    {
                        MetroMessageBox.ShowErrorMessage(null, downloadedSubtitles + " new subtitles are downloaded in respected folders. Thanks", "Information !!", System.Drawing.SystemIcons.Information, MessageBoxButton.OK, MessageBoxResult.OK);
                    }
                }
                else
                {
                    MetroMessageBox.ShowErrorMessage(null, "No new subtitles are added.", "Information !!", System.Drawing.SystemIcons.Information, MessageBoxButton.OK, MessageBoxResult.OK);
                }
            };

            worker.RunWorkerAsync();
        }
        private void DuplicateFind_Click(object sender, RoutedEventArgs e)
        {
            if (mMovieCollection.Count > 0) Utility.FindDuplicate(mMovieCollection);
            else MetroMessageBox.ShowErrorMessage(null, "Nothing to search. Sorry !!", "Error !!", System.Drawing.SystemIcons.Error, MessageBoxButton.OK, MessageBoxResult.OK);
#if DEBUG
            if (mMovieCollection.Count > 0) Utility.FindEmptyRecords(mMovieCollection);
#endif // if DEBUG
        }
        private void GroupByScanDate_Checked(object sender, RoutedEventArgs e)
        {
            mCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("ScanDate"));
        }
        private void GroupByScanDate_Unchecked(object sender, RoutedEventArgs e)
        {
            mCollectionView.GroupDescriptions.Clear();
        }
        private void Excel_ExportClick(object sender, RoutedEventArgs e)
        {
            if (mMovieCollection.Count > 0) Utility.ExportToExcel(mMovieCollection.ToList<IMDBXML>());
            else MetroMessageBox.ShowErrorMessage(null, "Nothing to export. Sorry !!", "Error !!", System.Drawing.SystemIcons.Error, MessageBoxButton.OK, MessageBoxResult.OK);
        }
        private void XML_ExportClick(object sender, RoutedEventArgs e)
        {
            if (mMovieCollection.Count > 0) Utility.ExportToXML(mMovieCollection);
            else MetroMessageBox.ShowErrorMessage(null, "Nothing to export. Sorry !!", "Error !!", System.Drawing.SystemIcons.Error, MessageBoxButton.OK, MessageBoxResult.OK);
        }

        private void AddNewMovie_Click(object sender, RoutedEventArgs e)
        {
            this.ShowEditMovieDialog();
        }

        void ShowEditMovieDialog(IMDBXML movie = null)
        {
            try
            {
                EditMovieInfo movieInfo = new EditMovieInfo(movie);
                IMDBXML newMovie = movieInfo.RunDialog();

                if (!movieInfo.isOk) return;

                if (newMovie != null
                    && (newMovie.GetResponse().response == null || newMovie.GetResponse().response.CompareTo("True") == 0))
                {
                    if (movie == null)
                    {
                        this.AddRowToCollection(newMovie, true);
                    }
                    else
                    {
                        this.UpdateRowDataInCollection(movie, newMovie);
                    }

                    MetroMessageBox.ShowErrorMessage(null, "Movie added Successfully!!", "Success",
                        System.Drawing.SystemIcons.Information, MessageBoxButton.OK, MessageBoxResult.OK, true);
                }
                else
                {
                    MetroMessageBox.ShowErrorMessage(null, "No data found!!", "Failed",
                        System.Drawing.SystemIcons.Information, MessageBoxButton.OK, MessageBoxResult.OK, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[WARNING] " + ex.Message);
                /* Gulp the exception */
            }
        }

        private void DeleteWatchedMovie_Click(object sender, RoutedEventArgs e)
        {
            var watchedMovies = mMovieCollection.Where(x => x.AlreadyWatched);

            if (watchedMovies.Count() > 0)
            {
                if (MetroMessageBox.ShowErrorMessage(null, watchedMovies.Count() + " movies found. Are you sure to delete them all." + Environment.NewLine + "This action cannot be undone.", "Are you Sure !!",
                    System.Drawing.SystemIcons.Warning, MessageBoxButton.OKCancel, MessageBoxResult.Cancel) == MessageBoxResult.OK)
                {
                    int count = 0;

                    foreach (var item in watchedMovies)
                    {
                        try
                        {
                            File.Delete(item.GetMovieInfo().filePath);
                            this.RemoveRowToCollection(item);
                            ++count;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("[WARNING] " + ex.Message);
                        }
                    }

                    MetroMessageBox.ShowErrorMessage(null, "Out of " + watchedMovies.Count() + " movies, " + count + "are deleted sucessfully!", "Information !!", System.Drawing.SystemIcons.Information, MessageBoxButton.OK, MessageBoxResult.OK);
                }
            }
            else MetroMessageBox.ShowErrorMessage(null, "Nothing to delete. Sorry !!", "Error !!", System.Drawing.SystemIcons.Error, MessageBoxButton.OK, MessageBoxResult.OK);
        }
            
        #endregion Toolbar

        #region Import/Search
        private void myPC_Click(object sender, RoutedEventArgs e)
        {
            SelectionMade(true);
        }
        private void customeFolder_Click(object sender, RoutedEventArgs e)
        {
            this.SelectionMade();
        }
        private void fromTextFile_Click(object sender, RoutedEventArgs e)
        {
            this.SelectionMade();
        }
        private void importXML_Click(object sender, RoutedEventArgs e)
        {
            this.SelectionMade();
        }
        private void importExcel_Click(object sender, RoutedEventArgs e)
        {
            this.SelectionMade();
        }
        private void reloadLastSession_Click(object sender, RoutedEventArgs e)
        {
            this.GroupByScanDate.IsChecked = false;
            this.OnInitBackgroundProcess("Restoring last session ...");

            Dictionary<string, WebserviceReponse>.ValueCollection lastSession = UserPreferences.Instance.GetLastSession();
            this.worker = new BackgroundWorker();

            this.FileCount = lastSession.Count;
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += ProgressChanged;

            worker.DoWork += delegate
            {
                int percentage = 0;

                foreach (var item in lastSession)
                {
                    if (worker.CancellationPending) break;

                    this.AddRowToCollection(new IMDBXML(item));
                    worker.ReportProgress(++percentage);
                }
            };

            worker.RunWorkerCompleted += delegate
            {
                this.OnEndBackgroundProcess();
            };

            worker.RunWorkerAsync();
        }

        void SelectionMade(bool isPCMode = false)
        {
            mSourceEdit.IsEnabled = !isPCMode;
            mStartBtn.IsEnabled = true;
            mAppendBtn.IsEnabled = true;

            if (isPCMode)
            {
                mStartBtn.Focus();
            }
            else
            {
                mSourceEdit.Focus();
                this.OpenBrowseDialog();
            }

            this.StartPanel.IsExpanded = true;
            //this.SelectionPanel.IsExpanded = true;
        }
        private void OpenBrowseDialog()
        {
            if (customeFolder.IsChecked == true)
            {
                var fsd = new FolderSelectDialog();

                fsd.Title = "Select Movie folder";

                if (fsd.ShowDialog(IntPtr.Zero))
                {
                    this.mSourceEdit.Text = fsd.FileName;
                }
            }
            else
            {
                // Show file dialog
                using (System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog())
                {
                    if (fromTextFile.IsChecked == true)
                    {
                        ofd.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                    }
                    else if (importXML.IsChecked == true)
                    {
                        ofd.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
                    }
                    else if (importExcel.IsChecked == true)
                    {
                        ofd.Filter = "Text files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
                    }

                    if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        this.mSourceEdit.Text = ofd.FileName;
                    }
                }
            }
        }
        private void mStartBtn_Click(object sender, RoutedEventArgs e)
        {
            this.StartSearch(myPC.IsChecked == true, false);
        }
        private void AppendBtn_Click(object sender, RoutedEventArgs e)
        {
            this.StartSearch(myPC.IsChecked == true, true);
        }
        private void StartSearch(bool searchComputer, bool mergeResults)
        {
            // Authenticate Mode on
            Utility.TurnOnAuthentication();
            String inputText = mSourceEdit.Text.Replace("\"", String.Empty);

            this.InitDataGridCollection(mergeResults);
            this.GroupByScanDate.IsChecked = false;

            if (inputText.EndsWith(".xml"))
            {
                try
                {
                    this.ImportXML(inputText, mergeResults);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[WARNING] " + ex.Message);
                    MetroMessageBox.ShowErrorMessage(null, "Invalid XML File", "Error !!", System.Drawing.SystemIcons.Error, MessageBoxButton.OK, MessageBoxResult.OK);
                }
            }
            else if (inputText.EndsWith(".xls") || inputText.EndsWith(".xlsx"))
            {
                try
                {
                    this.ImportExcel(inputText, mergeResults);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[WARNING] " + ex.Message);
                    MetroMessageBox.ShowErrorMessage(null, "Invalid Excel File", "Error !!", System.Drawing.SystemIcons.Error, MessageBoxButton.OK, MessageBoxResult.OK);
                }
            }
            else
            {
                StartSearchImp(searchComputer, inputText.EndsWith(".txt"), mergeResults);
            }
        }
        private void ImportXML(string filePath, bool mergeResults)
        {
            this.OnInitBackgroundProcess("Collecting tags from XML ...");

            worker = new BackgroundWorker();

            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.ProgressChanged += ProgressChanged;

            worker.DoWork += delegate
            {
                using (var reader = new StreamReader(filePath))
                {
                    MovieList movieList = (MovieList)new XmlSerializer(typeof(MovieList)).Deserialize(reader);
                    int count = 0;

                    this.FileCount = movieList.Items.Count;

                    foreach (WebserviceReponse item in movieList.Items)
                    {
                        if (worker.CancellationPending) break;

                        this.AddRowToCollection(new IMDBXML(item), mergeResults);
                        worker.ReportProgress(++count);
                    }
                }
            };

            worker.RunWorkerCompleted += delegate
            {
                this.OnEndBackgroundProcess();
            };

            worker.RunWorkerAsync();
        }
        private void ImportExcel(string inputText, bool mergeResults)
        {
            this.OnInitBackgroundProcess("Collecting files from excel sheet ...");

            String[] folderPath = this.mSourceEdit.Text.Split(';');
            this.worker = new BackgroundWorker();
            this.worker.WorkerSupportsCancellation = true;

            DataTable data = null;

            worker.DoWork += delegate
            {
                var importExcel = new ImportExcel(worker, inputText);

                FileCount = importExcel.GetRowCount();
                data = importExcel.Import();
            };

            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += ProgressChanged;

            worker.RunWorkerCompleted += delegate
            {
                this.OnInitBackgroundProcess("Preparing data ...");
                this.nestedWorker = new BackgroundWorker();

                nestedWorker.WorkerSupportsCancellation = true;
                nestedWorker.WorkerReportsProgress = true;
                nestedWorker.ProgressChanged += ProgressChanged;

                nestedWorker.DoWork += delegate
                {
                    this.FileCount = data.Rows.Count;
                    this.FillListFromDataTable(data, nestedWorker, mergeResults);
                };

                nestedWorker.RunWorkerCompleted += delegate
                {
                    this.OnEndBackgroundProcess();
                };

                nestedWorker.RunWorkerAsync();
            };

            worker.RunWorkerAsync();
        }
        private void FillListFromDataTable(DataTable data, BackgroundWorker worker, bool mergeResults)
        {
            int index = 0;

            foreach (DataRow row in data.Rows)
            {
                if (worker.CancellationPending) break;

                MovieInfo.IMDBXML asset = new MovieInfo.IMDBXML(new WebserviceReponse(new Movie()));
                foreach (PropertyInfo pinfo in asset.GetType().GetProperties())
                {
                    try
                    {
                        string value = row.Field<string>(pinfo.Name);

                        // Check for specific cell .. TODO
                        if (pinfo.Name.CompareTo("AlreadyWatched") == 0)
                        {
                            pinfo.SetValue(asset, Utility.ConvertToBool(value));
                        }
                        else
                        {
                            pinfo.SetValue(asset, value);
                        }
                    }
                    catch (System.ArgumentException ex)
                    {
                        Console.WriteLine("[WARNING] " + ex.Message);
                    }
                }

                this.AddRowToCollection(asset, mergeResults);
                worker.ReportProgress(++index);
            }
        }
        private void OnInitBackgroundProcess(String progressBarText)
        {
            this.StartPanel.IsExpanded = true;

            mProgressBar.IsIndeterminate = true;
            mProgressBar.Value = 0;
            ProgressBarText = progressBarText;

            Storyboard showProgressBar = Application.Current.Resources["ShowAnimation"] as Storyboard;
            Storyboard hideProgressBar = Application.Current.Resources["HideAnimation"] as Storyboard;

            mStartBtnPanel.BeginStoryboard(hideProgressBar);
            mProgressBarGrid.BeginStoryboard(showProgressBar);

            mProgressBarGrid.Visibility = System.Windows.Visibility.Visible;
            mStartBtnPanel.Visibility = System.Windows.Visibility.Hidden;

            this.mParentWindow.ResizeWindow();
        }
        private void OnEndBackgroundProcess()
        {
            Storyboard showProgressBar = Application.Current.Resources["ShowAnimation"] as Storyboard;
            Storyboard hideProgressBar = Application.Current.Resources["HideAnimation"] as Storyboard;

            mStartBtnPanel.BeginStoryboard(showProgressBar);
            mProgressBarGrid.BeginStoryboard(hideProgressBar);

            mStartBtnPanel.Visibility = System.Windows.Visibility.Visible;
            mProgressBarGrid.Visibility = System.Windows.Visibility.Hidden;

            this.SelectionPanel.IsExpanded = false;
            this.StartPanel.IsExpanded = false;
        }
        private void StartSearchImp(bool searchComputer, bool searchTextFile, bool mergeResults)
        {
            this.OnInitBackgroundProcess("Collecting files from " + (searchTextFile ? "text file ...." : "folders ..."));

            String[] folderPath = this.mSourceEdit.Text.Split(';');
            
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += delegate
            {
                if (searchComputer)
                {
                    var traverser = new MovieDirectoryTraverser(worker, this, mergeResults);

                    FileCount = traverser.FilesCount();
                    traverser.Traverse();
                }
                else if (searchTextFile)
                {
                    var traverser = new MovieTextTraverser(worker, folderPath, this, mergeResults);

                    FileCount = traverser.FilesCount();
                    traverser.Traverse();
                }
                else
                {
                    var traverser = new MovieDirectoryTraverser(worker, folderPath, this, mergeResults);

                    FileCount = traverser.FilesCount();
                    traverser.Traverse();
                }
            };

            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += ProgressChanged;

            worker.RunWorkerCompleted += delegate
            {
                this.OnEndBackgroundProcess();
            };

            worker.RunWorkerAsync();
        }
        #endregion

        #region Row Number
        private void mGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
        #endregion

        #region Filters Handling
        private void ClearFilter_Click(object sender, RoutedEventArgs e)
        {
            mFilterText.Clear();
            mShowWatched.IsChecked = false;
            mCollectionView.Filter = null;
            mFilters.ClearFilter();
        }

        private void mShowWatched_Click(object sender, RoutedEventArgs e)
        {
            if (mShowWatched.IsChecked == true)
            {
                mFilters.AddFilter(AlreadyWatchedFilter);
            }
            else
            {
                mFilters.RemoveFilter(AlreadyWatchedFilter);
            }

            mCollectionView.Filter = mFilters.Filter;
        }

        private bool AlreadyWatchedFilter(object item)
        {
            return !(item as IMDBXML).AlreadyWatched;
        }

        private void FilterText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (String.IsNullOrEmpty(mFilterText.Text))
            {
                mFilters.RemoveFilter(DataFilter);
            }
            else
            {
                mFilters.AddFilter(DataFilter);
            }

            mCollectionView.Filter = mFilters.Filter;
        }
        bool CaseInsensitiveContains(String one, String contains)
        {
            bool result = false;

            if (!(String.IsNullOrEmpty(one) || String.IsNullOrEmpty(contains)))
            {
                result = one.IndexOf(contains, StringComparison.OrdinalIgnoreCase) >= 0;
            }

            return result;
        }
        private bool DataFilter(object item)
        {
            Movie movieInfo = (item as IMDBXML).GetMovieInfo();
            string searchString = mFilterText.Text;
            bool result = mShowWatched.IsChecked == true ? this.AlreadyWatchedFilter(item) : true;

            if (mAll.IsChecked == true && result)
            {
                result = CaseInsensitiveContains(movieInfo.originalName, searchString)
                    || CaseInsensitiveContains(movieInfo.plot, searchString)
                    || CaseInsensitiveContains(movieInfo.year, searchString)
                    || CaseInsensitiveContains(movieInfo.runtime, searchString)
                    || CaseInsensitiveContains(movieInfo.genre, searchString)
                    || CaseInsensitiveContains(movieInfo.imdbRating, searchString)
                    || CaseInsensitiveContains(movieInfo.filePath, searchString)
                    || CaseInsensitiveContains(movieInfo.title, searchString)
                    || CaseInsensitiveContains(movieInfo.director, searchString)
                    || CaseInsensitiveContains(movieInfo.actors, searchString)
                    || CaseInsensitiveContains(movieInfo.awards, searchString)
                    || CaseInsensitiveContains(movieInfo.country, searchString)
                    || CaseInsensitiveContains(movieInfo.released, searchString)
                    || CaseInsensitiveContains(movieInfo.writer, searchString)
                    || CaseInsensitiveContains(movieInfo.language, searchString)

#if false
                    || CaseInsensitiveContains(movieInfo.director, searchString)
                    || CaseInsensitiveContains(movieInfo.actors, searchString)
                    || CaseInsensitiveContains(movieInfo.awards, searchString)
                    || CaseInsensitiveContains(movieInfo.country, searchString)
                    || CaseInsensitiveContains(movieInfo.released, searchString)
                    || CaseInsensitiveContains(movieInfo.writer, searchString)
                    || CaseInsensitiveContains(movieInfo.language, searchString)
                    || CaseInsensitiveContains(movieInfo.type, searchString)
                    || CaseInsensitiveContains(movieInfo.rated, searchString)
                    || CaseInsensitiveContains(movieInfo.poster, searchString)
                    || CaseInsensitiveContains(movieInfo.metascore, searchString)
                    || CaseInsensitiveContains(movieInfo.imdbVotes, searchString)
                    || CaseInsensitiveContains(movieInfo.imdbID, searchString)
                    || CaseInsensitiveContains(movieInfo.searchName, searchString)
                    || CaseInsensitiveContains(movieInfo.scandate, searchString)
#endif
;
            }
            else if (mFileNameRadio.IsChecked == true && result)
                result = CaseInsensitiveContains(movieInfo.originalName, searchString);
            else if (mTitleRadio.IsChecked == true && result)
                result = CaseInsensitiveContains(movieInfo.title, searchString);
            else if (mGenreRadio.IsChecked == true && result)
                result = CaseInsensitiveContains(movieInfo.genre, searchString);
            else if (mRatingRadio.IsChecked == true && result)
                result = CaseInsensitiveContains(movieInfo.imdbRating, searchString);
            else if (mYearRadio.IsChecked == true && result)
                result = CaseInsensitiveContains(movieInfo.year, searchString);

            return result;
        }
        private void FilterRadioClicked(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(mFilterText.Text)) FilterText_TextChanged(sender, null);
        }
        #endregion

        #region Show Hide Columns
        public ContextMenu cxMenu = null;
        private void mGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DependencyObject depObj = (DependencyObject)e.OriginalSource;

            while ((depObj != null) && !(depObj is DataGridColumnHeader))
            {
                depObj = VisualTreeHelper.GetParent(depObj);
            }

            if (depObj == null)
            {
                return;
            }

            if (depObj is DataGridColumnHeader)
            {
                DataGridColumnHeader dgColHeader = depObj as DataGridColumnHeader;
                dgColHeader.ContextMenu = cxMenu;
            }
        }

        public ContextMenu SortContextMenu
        {
            get
            {
                if (_sortContextMenu == null) this.GenerateSortContextMenu();
                return _sortContextMenu;
            }
            set
            {
                _sortContextMenu = value;
            }
        }

        private ContextMenu _sortContextMenu = null;
        private BitmapImage UpImage = new BitmapImage(new Uri("/Resources/Images/up.png", UriKind.Relative));
        private BitmapImage DownImage = new BitmapImage(new Uri("/Resources/Images/down.png", UriKind.Relative));

        void GenerateSortContextMenu()
        {
            _sortContextMenu = new ContextMenu();

            MenuItem sortBy = new MenuItem();
            sortBy.Header = "Sort By";

            _sortContextMenu.Items.Add(sortBy);

            foreach (PropertyInfo pinfo in typeof(IMDBXML).GetProperties())
            {
                try
                {
                    MenuItem menuItem = new MenuItem();
                    menuItem.Header = pinfo.Name;

                    sortBy.Items.Add(menuItem);
                    menuItem.Click += new RoutedEventHandler(SortMenuItem_Click);
                }
                catch (System.ArgumentException ex)
                {
                    Console.WriteLine("[WARNING] " + ex.Message);
                }
            }
        }

        void SortMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;

			if (item.Icon == null || ((System.Windows.Controls.Image)(item.Icon)).Source == DownImage)
			{
				item.Icon = new System.Windows.Controls.Image
				{
					Source = UpImage,
                    Width = 20,
                    Height = 20
				};

                mCollectionView.SortDescriptions.Clear();
                mCollectionView.SortDescriptions.Add(new SortDescription(item.Header.ToString(), ListSortDirection.Ascending));
			}
			else
			{
				item.Icon = new System.Windows.Controls.Image
				{
					Source = DownImage,
                    Width = 20,
                    Height = 20
				};

                mCollectionView.SortDescriptions.Clear();
                mCollectionView.SortDescriptions.Add(new SortDescription(item.Header.ToString(), ListSortDirection.Descending));
			}
        }

        private void mGrid_AutoGeneratedColumns(object sender, EventArgs dgData)
        {
            StringCollection strCollection = UserPreferences.Instance.GetSavedMenuCollectionIfAny();
            Dictionary<String, bool> menuItemsMap = new Dictionary<string, bool>();

            if (strCollection != null && strCollection.Count > 0)
            {
                foreach (String header in strCollection)
                {
                    String[] menuString = header.Split(';');

                    menuItemsMap.Add(menuString[0], Boolean.Parse(menuString[1]));
                }
            }

            cxMenu = new ContextMenu();

            foreach (DataGridColumn item in mGrid.Columns)
            {
                MenuItem menuItem = new MenuItem();
                menuItem.Header = item.Header;
                menuItem.IsChecked = true;

                cxMenu.Items.Add(menuItem);

                menuItem.Click += new RoutedEventHandler(menuItem_Click);
                menuItem.Checked += new RoutedEventHandler(menuItem_Checked);
                menuItem.Unchecked += new RoutedEventHandler(menuItem_Unchecked);

                bool menuState = true;

                if (menuItemsMap.TryGetValue(item.Header.ToString(), out menuState))
                {
                    if (!menuState)
                    {
                        DataGridColumn selColumn = mGrid.Columns.First(column => column.Header.ToString().Contains(item.Header.ToString()));

                        menuItem.IsChecked = menuState;
                        selColumn.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }
            }
        }

        void menuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;

            // Raise event
            item.IsChecked = !item.IsChecked;
        }

        void menuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;

            DataGridColumn selColumn = mGrid.Columns.First(column => column.Header.ToString().Contains(item.Header.ToString()));

            selColumn.Visibility = System.Windows.Visibility.Collapsed;
        }

        void menuItem_Checked(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            DataGridColumn selColumn = mGrid.Columns.First(column => column.Header.ToString().Contains(item.Header.ToString()));

            selColumn.Visibility = System.Windows.Visibility.Visible;
        }
        #endregion

        #region Row Details
        public double PlotWidth
        {
            get
            {
                return mDownloadNewPoster.IsChecked == true ? this.ActualWidth * 0.75 : this.ActualWidth * 0.8;
            }
        }

        public double PosterWidth
        {
            get
            {
                return this.mDetailPane.ActualWidth * 0.6;
            }
        }

        String ImageFolder = "Images" + System.IO.Path.DirectorySeparatorChar;

        private void mGrid_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {
            var selectedMovie = e.Row.Item as IMDBXML;

            if (selectedMovie != null)
            {
                Image posterControl = e.DetailsElement.FindName("mPoster") as Image;
                TextBlock nameControl = e.DetailsElement.FindName("mRowDetailsName") as TextBlock;
                TextBlock rankingControl = e.DetailsElement.FindName("mRowDetailsRanking") as TextBlock;
                TextBlock genreControl = e.DetailsElement.FindName("mRowDetailsGenre") as TextBlock;
                TextBlock plotControl = e.DetailsElement.FindName("mRowDetailsPlot") as TextBlock;

                if (nameControl != null) nameControl.Text = selectedMovie.Title;
                if (genreControl != null) genreControl.Text = selectedMovie.Genre;
                if (rankingControl != null) rankingControl.Text = selectedMovie.ImdbRating;
                if (plotControl != null) plotControl.Text = selectedMovie.Plot;

                string url = selectedMovie.GetMovieInfo().poster;

                if (!String.IsNullOrEmpty(url) && posterControl != null)
                {
                    string file = ImageFolder + System.IO.Path.GetFileName(url);

                    if (File.Exists(file) ||
                        (mDownloadNewPoster.IsChecked == true && Utility.DownloadImage(url, file)))
                    {
                        posterControl.Source = new ImageSourceConverter().ConvertFromString(file) as ImageSource;
                    }
                }
            }
        }

        private void MouseLeftButtonUp_Event(object sender, RoutedEventArgs e)
        {
            if (mGrid.CurrentColumn != null && mGrid.CurrentColumn.Header.ToString().CompareTo("AlreadyWatched") != 0)
            {
                var row = (DataGridRow)sender;

                row.DetailsVisibility = row.DetailsVisibility == Visibility.Collapsed ?
                    Visibility.Visible : Visibility.Collapsed;
            }
        }
        #endregion

        #region MoviePaneUI
        private IMDBXML _PaneSelectedItem = null;
        public IMDBXML PaneSelectedItem
        {
            get { return _PaneSelectedItem; }
            set
            {
                _PaneSelectedItem = value;
                NotifyPropertyChanged("PaneSelectedItem");
            }
        }

        private void ScrollToView_Click(object sender, RoutedEventArgs e)
        {
            if (PaneSelectedItem != null)
            {
                mPaneUI.ScrollIntoView(PaneSelectedItem);
            }
        }

        public void UpdateRowDetails(IMDBXML selectedMovie)
        {
            if (mGrid == mPaneUI && selectedMovie != null)
            {
                PaneSelectedItem = selectedMovie;

                if (selectedMovie.IsMovieDataAvailable())
                {
                    mDetailPane.Visibility = System.Windows.Visibility.Visible;
                    mDetailPaneAlternate.Visibility = System.Windows.Visibility.Hidden;
                }
                else
                {
                    mDetailPane.Visibility = System.Windows.Visibility.Hidden;
                    mDetailPaneAlternate.Visibility = System.Windows.Visibility.Visible;
                    return;
                }

                Image posterControl = this.mDetailPanePoster;

                try
                {
                    mDetailPaneRanking_Top.Value = Double.Parse(selectedMovie.ImdbRating);
                }
                catch (Exception)
                {
                    mDetailPaneRanking_Top.Value = 0;
                }

                mDetailPaneRankingTxt_Top.Text = "( " + mDetailPaneRanking_Top.Value + "/10 )";

                string url = selectedMovie.GetMovieInfo().poster;

                if (!String.IsNullOrEmpty(url) && posterControl != null)
                {
                    string file = ImageFolder + System.IO.Path.GetFileName(url);

                    if (File.Exists(file) ||
                        (mDownloadNewPoster.IsChecked == true && Utility.DownloadImage(url, file)))
                    {
                        posterControl.Source = new ImageSourceConverter().ConvertFromString(file) as ImageSource;
                    }
                    else
                    {
                        posterControl.Source = null;
                    }
                }
                else
                {
                    posterControl.Source = null;
                }
            }
        }

        private void ShowDetails_Click(object sender, RoutedEventArgs e)
        {
            UpdateRowDetails(mGrid.SelectedItem as IMDBXML);
        }
       
        public ICollectionView MovieCollectionView {get {return mCollectionView;} }
        
        private void SwitchPane_Click(object sender, RoutedEventArgs e)
        {
            if (mGrid == mGridUI)
            {
                this.mMovieGrid.Visibility = System.Windows.Visibility.Collapsed;
                this.mMoviePane.Visibility = System.Windows.Visibility.Visible;

                mGrid = mPaneUI;
            }
            else
            {
                this.mMovieGrid.Visibility = System.Windows.Visibility.Visible;
                this.mMoviePane.Visibility = System.Windows.Visibility.Collapsed;

                mGrid = mGridUI;
            }

            // Update Source
            mGrid.ItemsSource = mCollectionView;
        }
        #endregion

        private void mDetailPane_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            NotifyPropertyChanged("PosterWidth");
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
        
    }
}
