using System.Windows;
using ZooMap.Models;

namespace ZooMap
{
    public partial class EnclosureInfoWindow : Window
    {
        private string enclosureName;
        private User currentUser;
        private bool editMode = false;

        public EnclosureInfoWindow(string name, User user)
        {
            InitializeComponent();
            enclosureName = name;
            currentUser = user;
            LoadEnclosureInfo();
            UpdateEditCapabilities();
        }

        private void LoadEnclosureInfo()
        {
            tbTitle.Text = enclosureName;
            txtDescription.Text = $"Подробное описание вольера {enclosureName}...";
            txtAnimalType.Text = "Разные животные";
        }

        private void UpdateEditCapabilities()
        {
            if (currentUser?.Role == "Admin" || currentUser?.Role == "Worker")
            {
                btnEdit.Visibility = Visibility.Visible;
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            editMode = true;
            txtDescription.IsReadOnly = false;
            txtAnimalType.IsReadOnly = false;
            btnSave.Visibility = Visibility.Visible;
            btnEdit.Visibility = Visibility.Collapsed;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            editMode = false;
            txtDescription.IsReadOnly = true;
            txtAnimalType.IsReadOnly = true;
            btnSave.Visibility = Visibility.Collapsed;
            btnEdit.Visibility = Visibility.Visible;

            MessageBox.Show("Изменения сохранены");
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}