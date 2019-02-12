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
    public partial class MainWindow : Window
    {
        #region Data Members
        const double kFixedHeight = 550;
        MainPage mMainPage = null;
        #endregion

        #region Constructor
        
        public MainWindow()
        {
            try
            {
                InitializeComponent(); 
                InitializeCommands();

                // Load Preferences
                UserPreferences.Instance.LoadPreferences(this);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[WARNING] " + ex.Message);
                Utility.ExitApplication(ex.Message);
            }
        }

     #endregion

        #region Commands
        private void InitializeCommands()
        {
            this.CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, OnCloseWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, OnMaximizeWindow, OnCanResizeWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, OnMinimizeWindow, OnCanMinimizeWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, OnRestoreWindow, OnCanResizeWindow));
        }

        private void OnCanResizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ResizeMode == ResizeMode.CanResize || this.ResizeMode == ResizeMode.CanResizeWithGrip;
        }

        private void OnCanMinimizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ResizeMode != ResizeMode.NoResize;
        }

        private void OnCloseWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void OnMaximizeWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        private void OnMinimizeWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void OnRestoreWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }
        #endregion

        #region Window
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = UserPreferences.Instance.QuitMainWindow(mMainPage);
        }
        #endregion Window

        private void mFrame_LoadCompleted(object sender, NavigationEventArgs e)
        {
            if (mFrame.Content is MainPage)
            {
                mMainPage = (MainPage)mFrame.Content;
                mMainPage.SetParentWindow(this);
                //mFrame.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            }
            else
            {
                //mFrame.NavigationUIVisibility = NavigationUIVisibility.Visible;
            }
        }

        public void ResizeWindow()
        {
            if (this.Height < kFixedHeight)
            {
                Storyboard increaseHeight = Application.Current.Resources["increaseSizeAnimation"] as Storyboard;

                this.BeginStoryboard(increaseHeight);
            }
        }
    }
}
