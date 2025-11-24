using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ZooMap.Database;
using ZooMap.Models;

namespace ZooMap
{
    public partial class MainWindow : Window
    {
        private DatabaseHelper dbHelper;
        private User currentUser;

        public MainWindow()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
            LoadEnclosures();
        }

        private void LoadEnclosures()
        {
            iconsCanvas.Children.Clear();

            // Тестовые данные вместо БД
            CreateEnclosureIcon("Львиный вольер", 300, 400, Colors.Red);
            CreateEnclosureIcon("Обезьянник", 600, 700, Colors.Green);
            CreateEnclosureIcon("Аквариум", 800, 300, Colors.Blue);
        }

        private void CreateEnclosureIcon(string name, double x, double y, Color color)
        {
            Button iconButton = new Button
            {
                Width = 40,
                Height = 40,
                Background = new SolidColorBrush(Colors.Transparent),
                BorderBrush = new SolidColorBrush(color),
                BorderThickness = new Thickness(2),
                Content = new Ellipse
                {
                    Width = 30,
                    Height = 30,
                    Fill = new SolidColorBrush(Color.FromArgb(100, color.R, color.G, color.B)),
                    Stroke = new SolidColorBrush(color),
                    StrokeThickness = 2
                },
                ToolTip = name
            };

            Canvas.SetLeft(iconButton, x - 20);
            Canvas.SetTop(iconButton, y - 20);

            iconButton.Click += (s, e) => ShowEnclosureInfo(name);
            iconsCanvas.Children.Add(iconButton);
        }

        private void ShowEnclosureInfo(string enclosureName)
        {
            EnclosureInfoWindow infoWindow = new EnclosureInfoWindow(enclosureName, currentUser);
            infoWindow.Owner = this;
            infoWindow.ShowDialog();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            if (loginWindow.ShowDialog() == true)
            {
                currentUser = loginWindow.AuthenticatedUser;
                UpdateUI();
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            RegistrationWindow regWindow = new RegistrationWindow();
            regWindow.Owner = this;
            regWindow.ShowDialog();
        }

        private void BtnAdminPanel_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser?.Role == "Admin")
            {
                AdminPanelWindow adminWindow = new AdminPanelWindow();
                adminWindow.Owner = this;
                adminWindow.ShowDialog();
            }
        }

        private void UpdateUI()
        {
            if (currentUser != null)
            {
                tbUserInfo.Text = $"Пользователь: {currentUser.Username} ({currentUser.Role})";
                btnLogin.Content = "Выйти";

                if (currentUser.Role == "Admin" || currentUser.Role == "Worker")
                {
                    btnAdminPanel.Visibility = Visibility.Visible;
                }
            }
            else
            {
                tbUserInfo.Text = "";
                btnLogin.Content = "Вход";
                btnAdminPanel.Visibility = Visibility.Collapsed;
            }
        }
    }
}