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
using System.Configuration;

namespace GameDatabase_App
{
    /// <summary>
    /// Логика взаимодействия для EditGameWindow.xaml
    /// </summary>
    public partial class EditGameWindow : Window
    {
        public EditGameWindow(int gameId = 0)
        {
            InitializeComponent();
            Tag = gameId;
            ShowGame(gameId);
        }

        // Загрузка списков, а также информации о игре при редактировании
        private void ShowGame(int id = 0)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["defaultConnection"].ConnectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand() { Connection = connection })
                    {
                        // Отображение списка разработчиков
                        command.CommandText = @"SELECT id, title FROM dbo.Developers";
                        using (SqlDataReader dataReader = command.ExecuteReader())
                        {
                            if (dataReader.HasRows)
                            {
                                while (dataReader.Read())
                                {
                                    CheckBox item = new CheckBox()
                                    {
                                        TabIndex = dataReader.GetInt32(0),
                                        Content = dataReader.GetString(1)
                                    };
                                    GameEditDevelopers.Children.Add(item);
                                }
                            }
                        }

                        // Отображение списка издателей
                        command.CommandText = @"SELECT id, title FROM dbo.Publishers";
                        using (SqlDataReader dataReader = command.ExecuteReader())
                        {
                            if (dataReader.HasRows)
                            {
                                while (dataReader.Read())
                                {
                                    CheckBox item = new CheckBox()
                                    {
                                        TabIndex = dataReader.GetInt32(0),
                                        Content = dataReader.GetString(1)
                                    };
                                    GameEditPublishers.Children.Add(item);
                                }
                            }
                        }

                        // Отображение списка жанров
                        command.CommandText = @"SELECT id, title FROM dbo.Genres";
                        using (SqlDataReader dataReader = command.ExecuteReader())
                        {
                            if (dataReader.HasRows)
                            {
                                while (dataReader.Read())
                                {
                                    CheckBox item = new CheckBox()
                                    {
                                        TabIndex = dataReader.GetInt32(0),
                                        Content = dataReader.GetString(1)
                                    };
                                    GameEditGenres.Children.Add(item);
                                }
                            }
                        }

                        // Отображение списка платформ
                        command.CommandText = @"SELECT id, title FROM dbo.Platforms";
                        using (SqlDataReader dataReader = command.ExecuteReader())
                        {
                            if (dataReader.HasRows)
                            {
                                while (dataReader.Read())
                                {
                                    CheckBox item = new CheckBox()
                                    {
                                        TabIndex = dataReader.GetInt32(0),
                                        Content = dataReader.GetString(1)
                                    };
                                    GameEditPlatforms.Children.Add(item);
                                }
                            }
                        }

                        if (id > 0)
                        {
                            command.Parameters.Add(new SqlParameter("@id", id));
                            // Получение информации о игре
                            command.CommandText = @"SELECT title, website, release_date, summary FROM dbo.Games WHERE id = @id";
                            using (SqlDataReader dataReader = command.ExecuteReader())
                            {
                                if(dataReader.HasRows)
                                {
                                    while (dataReader.Read())
                                    {
                                        EditGameTitle.Text = dataReader.GetString(0);
                                        EditGameOfficial.Text = dataReader.GetString(1);
                                        if (!dataReader.IsDBNull(2))
                                        {
                                            EditGameRelease.SelectedDate = dataReader.GetDateTime(2);
                                        }
                                        else
                                        {
                                            EditGameRelease.SelectedDate = null;
                                        }
                                        EditGameSummary.Text = dataReader.GetString(3);
                                    }
                                }
                            }

                            // Отображение разработчиков игры
                            command.CommandText = @"SELECT developer_id FROM dbo.Games_Developers WHERE game_id = @id";
                            using (SqlDataReader dataReader = command.ExecuteReader())
                            {
                                if (dataReader.HasRows)
                                {
                                    while (dataReader.Read())
                                    {
                                        foreach (CheckBox item in GameEditDevelopers.Children)
                                        {
                                            if (item.TabIndex == dataReader.GetInt32(0))
                                            {
                                                item.IsChecked = true;
                                            }
                                        }
                                    }
                                }
                            }

                            // Отображение издателей игры
                            command.CommandText = @"SELECT publisher_id FROM dbo.Games_Publishers WHERE game_id = @id";
                            using (SqlDataReader dataReader = command.ExecuteReader())
                            {
                                if (dataReader.HasRows)
                                {
                                    while (dataReader.Read())
                                    {
                                        foreach (CheckBox item in GameEditPublishers.Children)
                                        {
                                            if (item.TabIndex == dataReader.GetInt32(0))
                                            {
                                                item.IsChecked = true;
                                            }
                                        }
                                    }
                                }
                            }

                            // Отображение жанров игры
                            command.CommandText = @"SELECT genre_id FROM dbo.Games_Genres WHERE game_id = @id";
                            using (SqlDataReader dataReader = command.ExecuteReader())
                            {
                                if (dataReader.HasRows)
                                {
                                    while (dataReader.Read())
                                    {
                                        foreach (CheckBox item in GameEditGenres.Children)
                                        {
                                            if (item.TabIndex == dataReader.GetInt32(0))
                                            {
                                                item.IsChecked = true;
                                            }
                                        }
                                    }
                                }
                            }

                            // Отображение платформ игры
                            command.CommandText = @"SELECT platform_id FROM dbo.Games_Platforms WHERE game_id = @id";
                            using (SqlDataReader dataReader = command.ExecuteReader())
                            {
                                if (dataReader.HasRows)
                                {
                                    while (dataReader.Read())
                                    {
                                        foreach (CheckBox item in GameEditPlatforms.Children)
                                        {
                                            if (item.TabIndex == dataReader.GetInt32(0))
                                            {
                                                item.IsChecked = true;
                                            }
                                        }
                                    }
                                }
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

        // Отмена изменений
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if(EditGameTitle.Text.Length != 0 && EditGameSummary.Text.Length != 0 && EditGameOfficial.Text.Length != 0)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["defaultConnection"].ConnectionString))
                    {
                        connection.Open();

                        // Добавление/Редактирование игры
                        using (SqlCommand command = new SqlCommand() { Connection = connection })
                        {
                            command.Parameters.Add(new SqlParameter("@id", (int)Tag));
                            command.Parameters.Add(new SqlParameter("@title", EditGameTitle.Text));
                            command.Parameters.Add(new SqlParameter("@release", EditGameRelease.SelectedDate != null ? (object)EditGameRelease.SelectedDate : DBNull.Value));
                            command.Parameters.Add(new SqlParameter("@official", EditGameOfficial.Text));
                            command.Parameters.Add(new SqlParameter("@summary", EditGameSummary.Text));

                            if ((int)Tag > 0)
                            {
                                command.CommandText = @"UPDATE dbo.Games SET title = @title, release_date = @release, website = @official, summary = @summary WHERE id = @id";
                                command.ExecuteNonQuery();
                            }
                            else
                            {
                                command.CommandText = @"INSERT INTO dbo.Games (title, release_date, website, summary) VALUES (@title, @release, @official, @summary)";
                                command.ExecuteNonQuery();
                                command.CommandText = @"SELECT TOP(1) id FROM dbo.Games ORDER BY id DESC";
                                using (SqlDataReader dataReader = command.ExecuteReader())
                                {
                                    dataReader.Read();
                                    Tag = dataReader.GetInt32(0);
                                }
                            }
                        }

                        // Добавление/Удаление разработчиков игры
                        using (SqlCommand command = new SqlCommand(@"SELECT dbo.Games_Developers.game_id, dbo.Games_Developers.developer_id FROM dbo.Games_Developers WHERE game_id = @id AND developer_id = @dev_id", connection))
                        {
                            command.Parameters.Add(new SqlParameter("@id", (int)Tag));
                            command.Parameters.Add(new SqlParameter("@dev_id", 0));
                            foreach (CheckBox item in GameEditDevelopers.Children)
                            {
                                command.Parameters[1].Value = item.TabIndex;
                                using (SqlDataReader dataReader = command.ExecuteReader())
                                {
                                    if ((bool)item.IsChecked)
                                    {
                                        if (!dataReader.HasRows)
                                        {
                                            using (SqlConnection connection1 = new SqlConnection(ConfigurationManager.ConnectionStrings["defaultConnection"].ConnectionString))
                                            {
                                                connection1.Open();
                                                using (SqlCommand insertCommand = new SqlCommand(@"INSERT INTO dbo.Games_Developers (game_id, developer_id) VALUES (@game_id, @dev_id)", connection1))
                                                {
                                                    insertCommand.Parameters.Add(new SqlParameter("@game_id", (int)Tag));
                                                    insertCommand.Parameters.Add(new SqlParameter("@dev_id", item.TabIndex));
                                                    insertCommand.ExecuteNonQuery();
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (dataReader.HasRows)
                                        {
                                            using (SqlConnection connection1 = new SqlConnection(ConfigurationManager.ConnectionStrings["defaultConnection"].ConnectionString))
                                            {
                                                connection1.Open();
                                                using (SqlCommand insertCommand = new SqlCommand(@"DELETE FROM dbo.Games_Developers WHERE game_id = @game_id AND developer_id = @dev_id", connection1))
                                                {
                                                    insertCommand.Parameters.Add(new SqlParameter("@game_id", (int)Tag));
                                                    insertCommand.Parameters.Add(new SqlParameter("@dev_id", item.TabIndex));
                                                    insertCommand.ExecuteNonQuery();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // Добавление/Удаление издателей игры
                        using (SqlCommand command = new SqlCommand(@"SELECT dbo.Games_Publishers.game_id, dbo.Games_Publishers.publisher_id FROM dbo.Games_Publishers WHERE game_id = @id AND publisher_id = @dev_id", connection))
                        {
                            command.Parameters.Add(new SqlParameter("@id", (int)Tag));
                            command.Parameters.Add(new SqlParameter("@dev_id", 0));
                            foreach (CheckBox item in GameEditPublishers.Children)
                            {
                                command.Parameters[1].Value = item.TabIndex;
                                using (SqlDataReader dataReader = command.ExecuteReader())
                                {
                                    if ((bool)item.IsChecked)
                                    {
                                        if (!dataReader.HasRows)
                                        {
                                            using (SqlConnection connection1 = new SqlConnection(ConfigurationManager.ConnectionStrings["defaultConnection"].ConnectionString))
                                            {
                                                connection1.Open();
                                                using (SqlCommand insertCommand = new SqlCommand(@"INSERT INTO dbo.Games_Publishers (game_id, publisher_id) VALUES (@game_id, @dev_id)", connection1))
                                                {
                                                    insertCommand.Parameters.Add(new SqlParameter("@game_id", (int)Tag));
                                                    insertCommand.Parameters.Add(new SqlParameter("@dev_id", item.TabIndex));
                                                    insertCommand.ExecuteNonQuery();
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (dataReader.HasRows)
                                        {
                                            using (SqlConnection connection1 = new SqlConnection(ConfigurationManager.ConnectionStrings["defaultConnection"].ConnectionString))
                                            {
                                                connection1.Open();
                                                using (SqlCommand insertCommand = new SqlCommand(@"DELETE FROM dbo.Games_Publishers WHERE game_id = @game_id AND publisher_id = @dev_id", connection1))
                                                {
                                                    insertCommand.Parameters.Add(new SqlParameter("@game_id", (int)Tag));
                                                    insertCommand.Parameters.Add(new SqlParameter("@dev_id", item.TabIndex));
                                                    insertCommand.ExecuteNonQuery();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // Добавление/Удаление жанров игры
                        using (SqlCommand command = new SqlCommand(@"SELECT dbo.Games_Genres.game_id, dbo.Games_Genres.genre_id FROM dbo.Games_Genres WHERE game_id = @id AND genre_id = @dev_id", connection))
                        {
                            command.Parameters.Add(new SqlParameter("@id", (int)Tag));
                            command.Parameters.Add(new SqlParameter("@dev_id", 0));
                            foreach (CheckBox item in GameEditGenres.Children)
                            {
                                command.Parameters[1].Value = item.TabIndex;
                                using (SqlDataReader dataReader = command.ExecuteReader())
                                {
                                    if ((bool)item.IsChecked)
                                    {
                                        if (!dataReader.HasRows)
                                        {
                                            using (SqlConnection connection1 = new SqlConnection(ConfigurationManager.ConnectionStrings["defaultConnection"].ConnectionString))
                                            {
                                                connection1.Open();
                                                using (SqlCommand insertCommand = new SqlCommand(@"INSERT INTO dbo.Games_Genres (game_id, genre_id) VALUES (@game_id, @dev_id)", connection1))
                                                {
                                                    insertCommand.Parameters.Add(new SqlParameter("@game_id", (int)Tag));
                                                    insertCommand.Parameters.Add(new SqlParameter("@dev_id", item.TabIndex));
                                                    insertCommand.ExecuteNonQuery();
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (dataReader.HasRows)
                                        {
                                            using (SqlConnection connection1 = new SqlConnection(ConfigurationManager.ConnectionStrings["defaultConnection"].ConnectionString))
                                            {
                                                connection1.Open();
                                                using (SqlCommand insertCommand = new SqlCommand(@"DELETE FROM dbo.Games_Genres WHERE game_id = @game_id AND genre_id = @dev_id", connection1))
                                                {
                                                    insertCommand.Parameters.Add(new SqlParameter("@game_id", (int)Tag));
                                                    insertCommand.Parameters.Add(new SqlParameter("@dev_id", item.TabIndex));
                                                    insertCommand.ExecuteNonQuery();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // Добавление/Удаление платформ игры
                        using (SqlCommand command = new SqlCommand(@"SELECT dbo.Games_Platforms.game_id, dbo.Games_Platforms.platform_id FROM dbo.Games_Platforms WHERE game_id = @id AND platform_id = @dev_id", connection))
                        {
                            command.Parameters.Add(new SqlParameter("@id", (int)Tag));
                            command.Parameters.Add(new SqlParameter("@dev_id", 0));
                            foreach (CheckBox item in GameEditPlatforms.Children)
                            {
                                command.Parameters[1].Value = item.TabIndex;
                                using (SqlDataReader dataReader = command.ExecuteReader())
                                {
                                    if ((bool)item.IsChecked)
                                    {
                                        if (!dataReader.HasRows)
                                        {
                                            using (SqlConnection connection1 = new SqlConnection(ConfigurationManager.ConnectionStrings["defaultConnection"].ConnectionString))
                                            {
                                                connection1.Open();
                                                using (SqlCommand insertCommand = new SqlCommand(@"INSERT INTO dbo.Games_Platforms (game_id, platform_id) VALUES (@game_id, @dev_id)", connection1))
                                                {
                                                    insertCommand.Parameters.Add(new SqlParameter("@game_id", (int)Tag));
                                                    insertCommand.Parameters.Add(new SqlParameter("@dev_id", item.TabIndex));
                                                    insertCommand.ExecuteNonQuery();
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (dataReader.HasRows)
                                        {
                                            using (SqlConnection connection1 = new SqlConnection(ConfigurationManager.ConnectionStrings["defaultConnection"].ConnectionString))
                                            {
                                                connection1.Open();
                                                using (SqlCommand insertCommand = new SqlCommand(@"DELETE FROM dbo.Games_Platforms WHERE game_id = @game_id AND platform_id = @dev_id", connection1))
                                                {
                                                    insertCommand.Parameters.Add(new SqlParameter("@game_id", (int)Tag));
                                                    insertCommand.Parameters.Add(new SqlParameter("@dev_id", item.TabIndex));
                                                    insertCommand.ExecuteNonQuery();
                                                }
                                            }
                                        }
                                    }
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
            else
            {
                MessageBox.Show("Поля заполнены некорректно", "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            this.Close();
        }
    }
}
