using System;
using System.Collections.Generic;
using System.Linq;
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
    public partial class Settings : Page
    {
        public const string KeyAccentColor = "AccentColor";
        public const string KeyAccent = "Accent";

        public Settings()
        {
            InitializeComponent();
            this.DataContext = this;
            this.SelectedAccentColor = (Color)Application.Current.Resources[KeyAccentColor];
            this.CustomValue.Text = this.SelectedAccentColor.ToString();
        }

        private void ApplyAccentColor(Color accentColor)
        {
            this.CustomValue.Text = accentColor.ToString();

            // set accent color and brush resources
            Application.Current.Resources[KeyAccentColor] = accentColor;
            Application.Current.Resources[KeyAccent] = new SolidColorBrush(accentColor);

            ReApplyTheme();
        }

        private ResourceDictionary GetThemeDictionary()
        {
            // determine the current theme by looking at the app resources and return the first dictionary having the resource key 'WindowBackground' defined.
            return (from dict in Application.Current.Resources.MergedDictionaries
                    where dict.Contains("WindowBackground")
                    select dict).FirstOrDefault();
        }

        void ReApplyTheme()
        {
            var source = GetThemeDictionary();
            var dictionaries = Application.Current.Resources.MergedDictionaries;
            var themeDict = new ResourceDictionary { Source = source.Source};

            dictionaries.Add(themeDict);
            dictionaries.Remove(source);
        }

        #region Color Pallete
        private Color[] mAccentColors = new Color[] {
            Color.FromRgb(0xa4, 0xc4, 0x00),   // lime
            Color.FromRgb(0x60, 0xa9, 0x17),   // green
            Color.FromRgb(0x00, 0x8a, 0x00),   // emerald
            Color.FromRgb(0x00, 0xab, 0xa9),   // teal
            Color.FromRgb(0x1b, 0xa1, 0xe2),   // cyan
            Color.FromRgb(0x00, 0x50, 0xef),   // cobalt
            Color.FromRgb(0x6a, 0x00, 0xff),   // indigo
            Color.FromRgb(0xaa, 0x00, 0xff),   // violet
            Color.FromRgb(0xf4, 0x72, 0xd0),   // pink
            Color.FromRgb(0xd8, 0x00, 0x73),   // magenta
            Color.FromRgb(0xa2, 0x00, 0x25),   // crimson
            Color.FromRgb(0xe5, 0x14, 0x00),   // red
            Color.FromRgb(0xfa, 0x68, 0x00),   // orange
            Color.FromRgb(0xf0, 0xa3, 0x0a),   // amber
            Color.FromRgb(0xe3, 0xc8, 0x00),   // yellow
            Color.FromRgb(0x82, 0x5a, 0x2c),   // brown
            Color.FromRgb(0x6d, 0x87, 0x64),   // olive
            Color.FromRgb(0x64, 0x76, 0x87),   // steel
            Color.FromRgb(0x76, 0x60, 0x8a),   // mauve
            Color.FromRgb(0x87, 0x79, 0x4e),   // taupe
        };

        public Color[] AccentColors
        {
            get { return this.mAccentColors; }
        }

        public SolidColorBrush ColorBrush
        {
            get { return new SolidColorBrush(this.selectedAccentColor); }
        }

        private Color selectedAccentColor;
        public Color SelectedAccentColor
        {
            get { return this.selectedAccentColor; }
            set
            {
                if (this.selectedAccentColor != value)
                {
                    this.selectedAccentColor = value;
                    ApplyAccentColor(this.selectedAccentColor);
                }
            }
        }
        #endregion

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Color color = (Color)ColorConverter.ConvertFromString(CustomValue.Text);

                ApplyAccentColor(color);
            } catch(Exception)
            {
                MetroMessageBox.ShowErrorMessage(null, "Invalid Color Selection. Try again.",
                   "Error !!",
                   System.Drawing.SystemIcons.Error,
                   MessageBoxButton.OK,
                   MessageBoxResult.OK);
            }
        }

    }
}
