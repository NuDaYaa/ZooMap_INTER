using System.Windows;
using ZooMap.Database;

namespace ZooMap
{
    public partial class RegistrationWindow : Window
    {
        private DatabaseHelper dbHelper;

        public RegistrationWindow()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password;
            string email = txtEmail.Text;

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(email))
            {
                tbMessage.Text = "Заполните все поля";
                return;
            }

            bool success = dbHelper.RegisterUser(username, password, email, "Visitor");

            if (success)
            {
                MessageBox.Show("Регистрация успешна!", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                tbMessage.Text = "Ошибка регистрации. Возможно, пользователь уже существует.";
            }
        }
    }
}