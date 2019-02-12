using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;

namespace MovieInfo
{
	/// <summary>
	/// Interaction logic for Welcome.xaml
	/// </summary>
	public partial class MetroMessageBox : System.Windows.Controls.UserControl
	{
		#region Data members
        private static Window currentContext = null;
        const Double kMaxWidth = 600;
        const Double kMinWidth = 424;
		public MessageBoxResult result;

        static void CloseWindow()
        {
            if (currentContext != null)
            {
                currentContext.Close();
                currentContext = null;
            }
        }
		private void CancelBtn_Click(object sender, RoutedEventArgs e)
		{
			result = MessageBoxResult.Cancel;
            CloseWindow(); 
		}

		private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
		{
			result = MessageBoxResult.OK;
            CloseWindow();
		}

		public String MessageString
		{
			get
			{
				return (String)this.ErrorMessage.Text;
			}

			set
			{
				this.ErrorMessage.Text = value;
			}
		}

		#endregion

		#region Constructor

		public MetroMessageBox(String errorMsg,
			String title,
			Icon icon,
			MessageBoxButton button = MessageBoxButton.OKCancel,
			MessageBoxResult msgDefault = MessageBoxResult.No)
		{
			InitializeComponent();

			this.ErrorMessage.Text = errorMsg;
			this.title.Content = title;
			this.MgsImage.Source = icon.ToImageSource();

			if (button == MessageBoxButton.OK)
			{
				this.CancelBtn.Visibility = System.Windows.Visibility.Hidden;
                this.ConfirmBtn.Content = "OK";
			}

			if (msgDefault == MessageBoxResult.No || msgDefault == MessageBoxResult.Cancel)
			{
				this.CancelBtn.IsDefault = true;
			}
			else
			{
				this.ConfirmBtn.IsDefault = true;
			}
		}

		#endregion

		#region MessageBox
        public static MessageBoxResult ShowErrorMessage(Window parent, String errorMsg,
			String title,
			Icon icon,
			MessageBoxButton button,
			MessageBoxResult msgDefault,
            bool fixedWidth = false)
		{
            MetroMessageBox messageWin = null;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (parent == null)
                {
                    parent = Application.Current.MainWindow;
                }

                Double width = parent != null ? parent.ActualWidth - 100 : 0;

                if (width > kMaxWidth) width = kMaxWidth;
                else if (width < kMinWidth) width = kMinWidth;

                messageWin = new MetroMessageBox(errorMsg, title, icon, button, msgDefault);

                Window window = new Window
                {
                    Content = messageWin,
                    Owner = parent,
                    SizeToContent = SizeToContent.Height,
                    WindowStyle = System.Windows.WindowStyle.None,
                    ResizeMode = ResizeMode.NoResize,
                    WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,
                    Width = width
                };

                currentContext = window;
                window.KeyUp += new System.Windows.Input.KeyEventHandler(window_KeyUp);
                window.ShowDialog();
            }));

            return messageWin.result;
		}

		static void window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Escape)
			{
				((MetroMessageBox)((Window)sender).Content).result = MessageBoxResult.Cancel;
                CloseWindow(); 
			}
		}
		#endregion
	}
}