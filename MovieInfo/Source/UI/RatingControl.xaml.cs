using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MovieInfo
{
      public partial class RatingControl : UserControl
    {
        public Double Value
        {
            get { return (Double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(Double), typeof(RatingControl),
            new FrameworkPropertyMetadata(0.0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(RatingChanged)));

        private static Brush GetBrustBasedOnPercentage(double percentage)
        {
            LinearGradientBrush brush = new LinearGradientBrush();

            brush.StartPoint = new Point(0, 1);
            brush.EndPoint = new Point(1, 1);
 
            brush.GradientStops.Add(new GradientStop(Colors.Orange, 0));
            brush.GradientStops.Add(new GradientStop(Colors.Orange, percentage));

            brush.GradientStops.Add(new GradientStop(Colors.White, percentage));

            return brush;
         }

        //private int _max = 10;
        private static void RatingChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            RatingControl item = sender as RatingControl;
            Double valProp = (Double)e.NewValue;
            UIElementCollection childs = ((Grid)(item.Content)).Children;
            StarControl button = null;

            for (int i = 0; i < childs.Count; ++i, --valProp) {
                button = childs[i] as StarControl;

                if (button == null) break;

                if (valProp >= 1) {
                    button.GetPath().Fill = new SolidColorBrush(Colors.Orange);
                }
                else if (valProp < 1 && valProp > 0) {
                    button.Fill(valProp);
                }
                else {
                    button.GetPath().Fill = new SolidColorBrush(Colors.White);
                }
            }
        }
       
        public RatingControl()
        {
            InitializeComponent();
        }
    }
}
