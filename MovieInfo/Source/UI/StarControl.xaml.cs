using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MovieInfo
{
    /// <summary>
    /// Interaction logic for StarControl.xaml
    /// </summary>
    public partial class StarControl : UserControl
    {
        #region Ctor
        public StarControl()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public Path GetPath() { return this.starPath; }

        public void Fill(double percentage)
        {
            LinearGradientBrush brush = new LinearGradientBrush();

            brush.StartPoint = new Point(0, 1);
            brush.EndPoint = new Point(1, 1);

            brush.GradientStops.Add(new GradientStop(Colors.Orange, 0));
            brush.GradientStops.Add(new GradientStop(Colors.Orange, percentage));

            brush.GradientStops.Add(new GradientStop(Colors.White, percentage));

            this.starPath.Fill = brush;
        }
        #endregion
     }
}
