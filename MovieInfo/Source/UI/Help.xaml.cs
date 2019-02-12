using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MovieInfo.Source.UI
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Help : Page
    {
        const string BlogURL = @"https://handshakingwithdigitalworld.blogspot.in/2016/06/fetch-information-about-movies-from.html";
        BackgroundWorker worker = new BackgroundWorker();

        public Help()
        {
            InitializeComponent();
            LoadData();
        }

        void LoadData()
        {
            HideScriptErrors(HelpBrowser, true);
            HelpBrowser.Navigate(BlogURL);

            //worker.DoWork += delegate
            //{
            //    HideScriptErrors(HelpBrowser, true);
            //    HelpBrowser.Navigate(BlogURL);
            //};

            //worker.RunWorkerAsync();
        }

        public void HideScriptErrors(WebBrowser wb, bool hide)
        {
            var fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fiComWebBrowser == null) return;
            var objComWebBrowser = fiComWebBrowser.GetValue(wb);
            if (objComWebBrowser == null)
            {
                wb.Loaded += (o, s) => HideScriptErrors(wb, hide); //In case we are to early
                return;
            }
            objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { hide });
        }
    }
}
