using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MovieInfo
{
    public partial class WindowStyle
    {
        #region Footer
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            Utility.OpenHelpPage();
        }
        #endregion
    }
}
