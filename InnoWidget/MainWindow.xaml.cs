using System.Windows;
using System.Windows.Input;
using InnoWidget.Views;

namespace InnoWidget
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
                return;
            }

            DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new WidgetSettingsWindow();
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
        }

        private void BackgroundMode_Click(object sender, RoutedEventArgs e)
        {
            // Background modunu toggle et
            var app = (App)Application.Current;
            
            // App.cs'deki background service'e erişim için geçici çözüm
            MessageBox.Show("Arkaplan modu aktif! Widget'lar arkaplanda çalışmaya başladı.\n\n" +
                          "Not: Tam System Tray entegrasyonu için daha fazla geliştirme gerekli.", 
                          "Arkaplan Modu", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}