using System;
using System.Collections.Generic;
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

namespace Happyfeet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static int numItems = 8;
        private static int numColumns = 4;

        private Label[] menuItems;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            menuItems = new Label[numItems];

            for (int i = 0; i < menuItems.Length; i++)
            {
                Label item = new Label();
                item.Content = i + 1;
                item.FontSize = 100;
                item.FontWeight = FontWeights.Bold;
                item.BorderBrush = Brushes.Black;
                item.BorderThickness = new Thickness(2.0);
                item.Height = 200;
                item.Width = 200;
                item.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                item.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                item.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                item.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;

                menuItems[i] = item;
                Grid.SetColumn(item, i % numColumns);
                Grid.SetRow(item, i / numColumns);
                menuGrid.Children.Add(item);
            }
        }
    }
}
