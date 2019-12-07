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
using System.Windows.Shapes;
using System.Data.SqlClient;

namespace GameDatabase_App
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SqlConnectionStringBuilder sqlConnectionString = new SqlConnectionStringBuilder(Properties.Settings.Default.userConnection)
                {
                    UserID = Login.Text,
                    Password = Password.Password
                };
                using (SqlConnection connection = new SqlConnection(sqlConnectionString.ConnectionString))
                {
                    connection.Open();
                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        Properties.Settings.Default.userConnection = sqlConnectionString.ConnectionString;
                        using (SqlCommand command = new SqlCommand("SELECT PERMISSIONS(OBJECT_ID('Games', 'U'))", connection))
                        {
                            int value = (int)command.ExecuteScalar();
                            if (value > 20000)
                            {
                                OpenMainWindow(true);
                            }
                            else
                            {
                                OpenMainWindow();
                            }
                        }
                    }
                }
            }
            catch (SqlException)
            {
                InvalidLogOrPassHint.Visibility = Visibility.Visible;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SqlConnectionStringBuilder sqlConnectionString = new SqlConnectionStringBuilder(Properties.Settings.Default.userConnection)
            {
                IntegratedSecurity = true
            };
            Properties.Settings.Default.userConnection = sqlConnectionString.ConnectionString;
            OpenMainWindow(false);
        }

        private void OpenMainWindow(bool isAdmin = false)
        {
            MainWindow window = new MainWindow(isAdmin);
            window.Show();
            Close();
        }
    }
}
