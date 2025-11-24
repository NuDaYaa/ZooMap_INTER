using System.Windows;
using ZooMap.Database;
using ZooMap.Models;

namespace ZooMap
{
    public partial class LoginWindow : Window
    {
        private DatabaseHelper dbHelper;
        public User AuthenticatedUser { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                tbMessage.Text = "Заполните все поля";
                return;
            }

            AuthenticatedUser = dbHelper.AuthenticateUser(username, password);

            if (AuthenticatedUser != null)
            {
                DialogResult = true;
                Close();
            }
            else
            {
                tbMessage.Text = "Неверное имя пользователя или пароль";
            }
        }
    }
}