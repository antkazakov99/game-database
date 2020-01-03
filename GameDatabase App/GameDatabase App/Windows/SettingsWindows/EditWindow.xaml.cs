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
    /// Логика взаимодействия для EditWindow.xaml
    /// </summary>
    public partial class EditWindow : Window
    {
        public EditWindow(int Type)
        {
            InitializeComponent();
            Tag = Type;
            Show(Type);
        }

        private void Show(int type)
        {
            Items.Items.Clear();
            try
            {
                using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.userConnection))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand() { Connection = connection})
                    {
                        switch (type)
                        {
                            case 0:
                                command.CommandText = @"SELECT id, title FROM dbo.Developers";
                                break;
                            case 1:
                                command.CommandText = @"SELECT id, title FROM dbo.Publishers";
                                break;
                            case 2:
                                command.CommandText = @"SELECT id, title FROM dbo.Genres";
                                break;
                            case 3:
                                command.CommandText = @"SELECT id, title FROM dbo.Platforms";
                                break;
                            case 4:
                                command.CommandText = @"SELECT id, title FROM dbo.Reviewers";
                                break;
                        }
                        using (SqlDataReader dataReader = command.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                ListBoxItem item = new ListBoxItem()
                                {
                                    Tag = dataReader.GetInt32(0),
                                    Content = dataReader.GetString(1)
                                };
                                Items.Items.Add(item);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"В процессе обработки данных произошла ошибка:\n{ex}", "Ошибка обработки данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if(Items.SelectedItem != null)
            {
                foreach (ListBoxItem item in Items.SelectedItems)
                {
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.userConnection))
                        {
                            connection.Open();

                            using (SqlCommand command = new SqlCommand() { Connection = connection })
                            {
                                command.Parameters.Add(new SqlParameter("@id", (int)(item.Tag)));
                                switch ((int)Tag)
                                {
                                    case 0:
                                        command.CommandText = @"DELETE FROM dbo.Developers WHERE id = @id";
                                        break;
                                    case 1:
                                        command.CommandText = @"DELETE FROM dbo.Publishers WHERE id = @id";
                                        break;
                                    case 2:
                                        command.CommandText = @"DELETE FROM dbo.Genres WHERE id = @id";
                                        break;
                                    case 3:
                                        command.CommandText = @"DELETE FROM dbo.Platforms WHERE id = @id";
                                        break;
                                    case 4:
                                        command.CommandText = @"DELETE FROM dbo.Reviewers WHERE id = @id";
                                        break;
                                }
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show($"В процессе обработки данных произошла ошибка:\n{ex}", "Ошибка обработки данных", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            Show((int)Tag);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (AddItem.Text != "")
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.userConnection))
                    {
                        connection.Open();

                        using (SqlCommand command = new SqlCommand() { Connection = connection })
                        {
                            command.Parameters.Add(new SqlParameter("@title", AddItem.Text));
                            switch ((int)Tag)
                            {
                                case 0:
                                    command.CommandText = @"INSERT INTO dbo.Developers (title) VALUES (@title)";
                                    break;
                                case 1:
                                    command.CommandText = @"INSERT INTO dbo.Publishers (title) VALUES (@title)";
                                    break;
                                case 2:
                                    command.CommandText = @"INSERT INTO dbo.Genres (title) VALUES (@title)";
                                    break;
                                case 3:
                                    command.CommandText = @"INSERT INTO dbo.Platforms (title) VALUES (@title)";
                                    break;
                                case 4:
                                    command.CommandText = @"INSERT INTO dbo.Reviewers (title) VALUES (@title)";
                                    break;
                            }
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"В процессе обработки данных произошла ошибка:\n{ex}", "Ошибка обработки данных", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                Show((int)Tag);
            }
        }
    }
}
