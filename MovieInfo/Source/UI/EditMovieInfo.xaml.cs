using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Windows.Media.Animation;

namespace MovieInfo
{
	/// <summary>
	/// Interaction logic for Welcome.xaml
	/// </summary>
    public partial class EditMovieInfo : Window
	{
		#region Data members
        IMDBXML mMovieInfo;
        IMDBXML result = null;
        public bool isOk = false;
        BackgroundWorker worker = null;

    	private void CancelBtn_Click(object sender, RoutedEventArgs e)
		{
            isOk = false;
            result = null;
            this.Close();
		}

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            if (worker != null)
            {
                worker.CancelAsync();
                Utility.CancelAsyn();
            }
        }

		private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
		{
            Init();

            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;

            worker.DoWork += delegate
            {
               result = mMovieInfo != null
                    ? new IMDBXML(mMovieInfo.FilePath, mMovieInfo.FileName, MovieName, Year, false)
                    : new IMDBXML("", MovieName, MovieName, Year);
            };

            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.RunWorkerAsync();
		}

        void Init()
        {
            isOk = true;

            mButtonPanel.Visibility = System.Windows.Visibility.Collapsed;
            mProgressBar.Visibility = System.Windows.Visibility.Visible;
        }

        void DeInit()
        {
            isOk = false;
            result = null;

            mButtonPanel.Visibility = System.Windows.Visibility.Visible;
            mProgressBar.Visibility = System.Windows.Visibility.Collapsed;
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                this.DeInit();
            }
            else
            {
                this.Close();
            }
        }
         

        public String MovieName { get; set; }
        public String Year { get; set; }

		#endregion

		#region Constructor

        public EditMovieInfo(IMDBXML movie)
		{
			InitializeComponent();
            this.Owner = App.Current.MainWindow;
            this.mMovieInfo = movie;

            DataContext = this;

            if (movie != null)
            {
                MovieName = movie.GetMovieInfo().searchName;
                Year = movie.Year;
            }
            else
            {
                mTitle.Content = "Add Movie information manually";
            }
		}

        public IMDBXML RunDialog()
        {
            //DoubleAnimation animFadeIn = new DoubleAnimation();

            //animFadeIn.From = 0;
            //animFadeIn.To = 1;
            //animFadeIn.Duration = new Duration(TimeSpan.FromSeconds(2));
            
            //this.BeginAnimation(Window.OpacityProperty, animFadeIn);
            this.ShowDialog();

            return result;
        }

		#endregion
	}
}