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
using System.Data.SqlClient;

namespace GameDatabase_App
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(bool isAdmin = false)
        {
            InitializeComponent();
            if(isAdmin)
            {
                AddGameButton.Visibility = Visibility.Visible;
                EditDevelopersButton.Visibility =
                    EditGenresButton.Visibility =
                    EditPublishersButton.Visibility =
                    EditPlatformsButton.Visibility = Visibility.Visible;
            }
            Tag = isAdmin;
            ShowGames();
            UpdateDevelopersList();
            UpdatePublishersList();
            UpdateGenresList();
            UpdatePlatformsList();
        }

        // Поиск
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ShowGames();
        }

        // Очистка параметров поиска
        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            GameTitleSearchTextBlock.Clear();
            GameReleaseFromDatePicker.SelectedDate = null;
            GameReleaseToDatePicker.SelectedDate = null;
            GameScoreFromSlider.Value = 0;
            GameScoreToSlider.Value = 100;
            foreach (CheckBox developer in SearchDevelopersList.Children)
            {
                developer.IsChecked = false;
            }
            foreach (CheckBox publisher in SearchPublishersList.Children)
            {
                publisher.IsChecked = false;
            }
            foreach (CheckBox genre in SearchGenresList.Children)
            {
                genre.IsChecked = false;
            }
            foreach (CheckBox platform in SearchPlatformsList.Children)
            {
                platform.IsChecked = false;
            }
            SortByComboBox.SelectedIndex = 0;
            ShowGames();
        }

        // Значение слайдера не может быть выше значения второго слайдера
        private void GameScoreFromSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue > GameScoreToSlider.Value)
                GameScoreFromSlider.Value = e.OldValue;
        }

        // Значение слайдера не может быть ниже значения первого слайдера
        private void GameScoreToSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue < GameScoreFromSlider.Value)
                GameScoreToSlider.Value = e.OldValue;
        }

        // Открытие окна игры
        private void GameMoreInfoButton_Click(object sender, RoutedEventArgs e)
        {
            GameWindow gameWindow = new GameWindow((int)((Button)sender).Tag, (bool)Tag ? true : false);
            gameWindow.Show();
        }

        // Открытие окна редактирования игры
        private void GameEditButton_Click(object sender, RoutedEventArgs e)
        {
            EditGameWindow window = new EditGameWindow((int)((Button)sender).Tag);
            window.ShowDialog();
            ShowGames();
        }

        // Открытие окна добавления игры
        private void AddGame_Click(object sender, RoutedEventArgs e)
        {
            EditGameWindow window = new EditGameWindow();
            window.ShowDialog();
            ShowGames();
        }

        // Удаление игры
        private void DeleteGame_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show($"Вы действительно хотите удалить {((Label)(((Grid)(((StackPanel)(((Button)sender).Parent)).Parent)).Children[1])).Content}?", "Удаление игры", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(Properties.Settings.Default.userConnection))
                    {
                        connection.Open();

                        using (SqlCommand command = new SqlCommand($@"DELETE FROM dbo.Games WHERE dbo.Games.id = @id", connection))
                        {
                            command.Parameters.Add(new SqlParameter("@id", ((Button)sender).Tag));
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"В процессе обработки данных произошла ошибка:\n{ex}", "Ошибка обработки данных", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            ShowGames();
        }

        // Открытие окна редактирования
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            string id = ((Button)sender).Tag.ToString();
            EditWindow window = new EditWindow(int.Parse(id));
            window.ShowDialog();
            switch (id)
            {
                case "0":
                    UpdateDevelopersList();
                    break;
                case "1":
                    UpdatePublishersList();
                    break;
                case "2":
                    UpdateGenresList();
                    break;
                case "3":
                    UpdatePlatformsList();
                    break;
            }
            ShowGames();
        }


        // Получение списка игр по указанным параметрам поиска  
        private void ShowGames()
        {
            // Очистка списка
            NoResultsTextBlock.Visibility = Visibility.Hidden;
            GamesList.Children.Clear();
            try
            {
                // Подключение
                using (SqlConnection connection = new SqlConnection() { ConnectionString = Properties.Settings.Default.userConnection })
                {
                    // Открытие подключения
                    connection.Open();

                    // Команда sql
                    using (SqlCommand command = GenerateSqlCommand(connection))
                    {
                        // Выполнение запроса
                        using (SqlDataReader dataReader = command.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                // Добавление разделителя строк
                                // -----------------------------------------------
                                if (GamesList.Children.Count > 0)
                                    GamesList.Children.Add(new Separator());
                                // -----------------------------------------------
                                GamesList.Children.Add(
                                    GenerateGameTile(
                                        dataReader.GetInt32(0), 
                                        dataReader.GetString(1), 
                                        dataReader.IsDBNull(3) ? null : (DateTime?)dataReader.GetDateTime(3),
                                        dataReader.IsDBNull(4) ? null : (int?)dataReader.GetInt32(4),
                                        dataReader.GetString(2)));
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"В процессе обработки данных произошла ошибка:\n{ex}", "Ошибка обработки данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            if (GamesList.Children.Count == 0)
                NoResultsTextBlock.Visibility = Visibility.Visible;
        }

        private Grid GenerateGameTile(int id, string title, DateTime? release, int? score, string summary)
        {
            // Создание Grid в который будет компоноваться Tile
            // -----------------------------------------------
            Grid gameTileGrid = new Grid()
            {
                Height = 210,
                Margin = new Thickness(2)
            };
            // Столбцы
            gameTileGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            gameTileGrid.ColumnDefinitions.Add(new ColumnDefinition());
            gameTileGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            // Строки
            gameTileGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            gameTileGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            gameTileGrid.RowDefinitions.Add(new RowDefinition());
            gameTileGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            // -----------------------------------------------

            // Обложка игры
            // -----------------------------------------------
            Border gameCoverBorder = new Border()
            {
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Colors.Black),
                Margin = new Thickness(5),
                Height = 180,
                Width = 120,
                VerticalAlignment = VerticalAlignment.Top
            };
            gameCoverBorder.Child = new Image()
            {
                Stretch = Stretch.Uniform
            };
            gameTileGrid.Children.Add(gameCoverBorder);
            Grid.SetRowSpan(gameCoverBorder, 4);
            // -----------------------------------------------

            // Название игры
            // -----------------------------------------------
            Label gameTitleLabel = new Label()
            {
                Content = title,
                FontSize = 18
            };
            gameTileGrid.Children.Add(gameTitleLabel);
            Grid.SetColumn(gameTitleLabel, 1);
            // -----------------------------------------------

            // Дата выхода игры
            // -----------------------------------------------
            Label gameReleaseDateLabel = new Label()
            {
                Content = $"Дата выхода: " +
                $"{(release == null ? "TBA" : DateTime.Parse(release.ToString()).ToShortDateString())}",
                FontSize = 10
            };
            gameTileGrid.Children.Add(gameReleaseDateLabel);
            Grid.SetColumn(gameReleaseDateLabel, 1);
            Grid.SetRow(gameReleaseDateLabel, 1);
            // -----------------------------------------------

            // Оценка игры
            // -----------------------------------------------
            Border gameScoreBorder = new Border()
            {
                Background = score == null || score <= 50 ? new SolidColorBrush(Colors.Red) : (score <= 70 ? new SolidColorBrush(Colors.Gold) : new SolidColorBrush(Colors.YellowGreen)),
                Margin = new Thickness(5),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Colors.Black),
                Height = 50,
                Width = 50
            };
            gameScoreBorder.Child = new Label()
            {
                FontSize = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Content = score == null ? "-" : score.ToString()
            };
            gameTileGrid.Children.Add(gameScoreBorder);
            Grid.SetColumn(gameScoreBorder, 2);
            Grid.SetRow(gameScoreBorder, 0);
            Grid.SetRowSpan(gameScoreBorder, 2);
            // -----------------------------------------------

            // Описание игры
            // -----------------------------------------------
            TextBlock gameSummaryTextBlock = new TextBlock()
            {
                Text = summary,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(3),
                TextTrimming = TextTrimming.WordEllipsis,
            };
            gameTileGrid.Children.Add(gameSummaryTextBlock);
            Grid.SetColumn(gameSummaryTextBlock, 1);
            Grid.SetRow(gameSummaryTextBlock, 2);
            Grid.SetColumnSpan(gameSummaryTextBlock, 2);
            // -----------------------------------------------

            // Панель кнопок в нижней части плитки
            // -----------------------------------------------
            StackPanel buttons = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(5),
            };
            gameTileGrid.Children.Add(buttons);
            Grid.SetColumn(buttons, 1);
            Grid.SetColumnSpan(buttons, 3);
            Grid.SetRow(buttons, 4);
            // -----------------------------------------------

            if ((bool)Tag)
            {
                // Кнопка удалить
                // -----------------------------------------------
                Button DeleteButton = new Button()
                {
                    Tag = id,
                    Padding = new Thickness(3),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Content = "Удалить"
                };
                DeleteButton.Click += DeleteGame_Click;
                buttons.Children.Add(DeleteButton);
                // -----------------------------------------------

                // Кнопка редактировать
                // -----------------------------------------------
                Button EditButton = new Button()
                {
                    Tag = id,
                    Padding = new Thickness(3),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Content = "Редактировать"
                };
                EditButton.Click += GameEditButton_Click;
                buttons.Children.Add(EditButton);
                // -----------------------------------------------
            }

            // Кнопка подробнее
            // -----------------------------------------------
            Button gameMoreInfoButton = new Button()
            {
                Tag = id,
                Padding = new Thickness(3),
                Content = "Подробнее..."
            };
            gameMoreInfoButton.Click += GameMoreInfoButton_Click;
            buttons.Children.Add(gameMoreInfoButton);
            // -----------------------------------------------
            return gameTileGrid;
        }

        // Формирование поискового запроса для SQL Server
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "<Ожидание>")]
        private SqlCommand GenerateSqlCommand(SqlConnection connection)
        {
            // Основной текст запроса, в случае отсутствие условий поиска выполняется только он
            SqlCommand command = new SqlCommand()
            {
                CommandText =
                " SELECT DISTINCT dbo.Games.id, dbo.Games.title, dbo.Games.summary, dbo.Games.release_date, GameScore.avg_score" +
                " FROM dbo.Games" +
                    " LEFT OUTER JOIN (" +
                        " SELECT game_id, AVG(score) AS avg_score" +
                        " FROM dbo.Reviews" +
                        " GROUP BY dbo.Reviews.game_id) AS GameScore" +
                        " ON  dbo.Games.id = GameScore.game_id",
                Connection = connection
            };

            // Количество выделенных объектов
            int count = 0;

            // Поиск по разработчикам
            foreach (CheckBox item in SearchDevelopersList.Children)
            {
                if ((bool)item.IsChecked)
                {
                    command.Parameters.Add(new SqlParameter($"@developer{item.Tag}", item.Tag));
                    if (count == 0)
                        command.CommandText +=
                            " INNER JOIN dbo.Games_Developers" +
                                " ON dbo.Games_Developers.game_id = dbo.Games.id" +
                               $" AND dbo.Games_Developers.developer_id IN (@developer{item.Tag}";
                    else
                        command.CommandText += $", @developer{item.Tag}";
                    count++;
                }
            }
            if (count != 0)
            {
                command.CommandText += ')';
            }

            count = 0;

            // Поиск по издателям
            foreach (CheckBox item in SearchPublishersList.Children)
            {
                if ((bool)item.IsChecked)
                {
                    command.Parameters.Add(new SqlParameter($"@publisher{item.Tag}", item.Tag));
                    if (count == 0)
                        command.CommandText +=
                            " INNER JOIN dbo.Ganes_Publishers" +
                                " ON dbo.Games_Publishers.game_id = dbo.Games.id" +
                               $" AND dbo.Games_Publishers IN (@publisher{item.Tag}";
                    else
                        command.CommandText += $", @publisher{item.Tag}";
                    count++;
                }
            }
            if (count != 0)
            {
                command.CommandText += ')';
            }

            count = 0;
            
            // Поиск по жанрам
            foreach (CheckBox item in SearchGenresList.Children)
            {
                if ((bool)item.IsChecked)
                {
                    command.Parameters.Add(new SqlParameter($"@genre{item.Tag}", item.Tag));
                    if (count == 0)
                        command.CommandText +=
                            " INNER JOIN dbo.Games_Genres" +
                                " ON dbo.Games_Genres.game_id = dbo.Games.id" +
                               $" AND dbo.Games_Genres.genre_id IN (@genre{item.Tag}";
                    else
                        command.CommandText += $@", @genre{item.Tag}";
                    count++;
                }
            }
            if (count != 0)
            {
                command.CommandText += ')';
            }

            count = 0;
            
            // Поиск по платформам
            foreach (CheckBox item in SearchPlatformsList.Children)
            {
                if ((bool)item.IsChecked)
                {
                    command.Parameters.Add(new SqlParameter($"@platform{item.Tag}", item.Tag));
                    if (count == 0)
                        command.CommandText += " INNER JOIN dbo.Games_Platforms " +
                            " ON dbo.Games_Platforms.game_id = dbo.Games.id" +
                           $" AND dbo.Games_Platforms.platform_id IN (@platform{item.Tag}";
                    else
                        command.CommandText += $@", @platform{item.Tag}";
                    count++;
                }
            }
            if (count != 0)
            {
                command.CommandText += ')';
            }

            // Количество условий поиска
            int terms = 0;

            // Поиск по названию игры
            if (GameTitleSearchTextBlock.Text.Length > 0)
            {
                command.Parameters.Add(new SqlParameter("@title", '%' + GameTitleSearchTextBlock.Text + '%'));
                command.CommandText += 
                    " WHERE dbo.Games.Title LIKE @title";
                terms++;
            }

            // Поиск по дате выхода (с)
            if(GameReleaseFromDatePicker.SelectedDate != null)
            {
                if (terms == 0)
                    command.CommandText += " WHERE";
                else
                    command.CommandText += " AND";
                command.Parameters.Add(new SqlParameter("@releaseDateFrom", GameReleaseFromDatePicker.SelectedDate));
                command.CommandText += " dbo.Games.release_date >= @releaseDateFrom";
                terms++;
            }

            // Поиск по дате выхода (до)
            if (GameReleaseToDatePicker.SelectedDate != null)
            {
                if (terms == 0)
                    command.CommandText += " WHERE";
                else
                    command.CommandText += " AND";
                command.Parameters.Add(new SqlParameter("@releaseDateTo", GameReleaseToDatePicker.SelectedDate));
                command.CommandText += " dbo.Games.release_date <= @releaseDateTo";
                terms++;
            }

            // Поиск по средней оценке (>)
            if (GameScoreFromSlider.Value > 0)
            {
                if (terms == 0)
                    command.CommandText += " WHERE";
                else
                    command.CommandText += " AND";
                command.Parameters.Add(new SqlParameter("@scoreFrom", GameScoreFromSlider.Value));
                command.CommandText += " GameScore.avg_score >= @scoreFrom";
                terms++;
            }

            // Поиск по средней оценке (<)
            if (GameScoreToSlider.Value < 100)
            {
                if (terms == 0)
                    command.CommandText += " WHERE";
                else
                    command.CommandText += " AND";
                command.Parameters.Add(new SqlParameter("@scoreTo", GameScoreToSlider.Value));
                command.CommandText += " GameScore.avg_score <= @scoreTo";
            }

            switch (((ComboBoxItem)SortByComboBox.SelectedItem).Tag)
            {
                case "0":
                    command.CommandText += " ORDER BY dbo.Games.title ASC";
                    break;
                case "1":
                    command.CommandText += " ORDER BY dbo.Games.title DESC";
                    break;
                case "2":
                    command.CommandText += " ORDER BY dbo.Games.release_date ASC";
                    break;
                case "3":
                    command.CommandText += " ORDER BY dbo.Games.release_date DESC";
                    break;
                case "4":
                    command.CommandText += " ORDER BY GameScore.avg_score ASC";
                    break;
                case "5":
                    command.CommandText += " ORDER BY GameScore.avg_score DESC";
                    break;
            }
            return command;
        }

        // Обновление списка разработиков
        private void UpdateDevelopersList()
        {
            try
            {
                // Подключение
                using (SqlConnection connection = new SqlConnection() { ConnectionString = Properties.Settings.Default.userConnection })
                {
                    // Открытие подключения
                    connection.Open();
                    // Отображение списка разработчиков
                    using (SqlCommand command = new SqlCommand("SELECT id, title FROM dbo.Developers ORDER BY title ASC", connection))
                    {
                        // Выполнение запроса   
                        using (SqlDataReader dataReader = command.ExecuteReader())
                        {
                            SearchDevelopersList.Children.Clear();
                            while (dataReader.Read())
                            {
                                CheckBox item = new CheckBox()
                                {
                                    Tag = dataReader.GetInt32(0),
                                    Content = dataReader.GetString(1),
                                    FontSize = 11
                                };
                                SearchDevelopersList.Children.Add(item);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"В процессе получения данных произошла ошибка:\n{ex}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обновление списка издателей
        private void UpdatePublishersList()
        {
            try
            {
                // Подключение
                using (SqlConnection connection = new SqlConnection() { ConnectionString = Properties.Settings.Default.userConnection })
                {
                    // Открытие подключения
                    connection.Open();
                    // Отображение списка разработчиков
                    using (SqlCommand command = new SqlCommand("SELECT id, title FROM dbo.Publishers ORDER BY title ASC", connection))
                    {
                        // Выполнение запроса   
                        using (SqlDataReader dataReader = command.ExecuteReader())
                        {
                            SearchPublishersList.Children.Clear();
                            while (dataReader.Read())
                            {
                                CheckBox item = new CheckBox()
                                {
                                    Tag = dataReader.GetInt32(0),
                                    Content = dataReader.GetString(1),
                                    FontSize = 11
                                };
                                SearchPublishersList.Children.Add(item);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"В процессе получения данных произошла ошибка:\n{ex}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обновление списка жанров
        private void UpdateGenresList()
        {
            try
            {
                // Подключение
                using (SqlConnection connection = new SqlConnection() { ConnectionString = Properties.Settings.Default.userConnection })
                {
                    // Открытие подключения
                    connection.Open();
                    // Отображение списка разработчиков
                    using (SqlCommand command = new SqlCommand("SELECT id, title FROM dbo.Genres ORDER BY title ASC", connection))
                    {
                        // Выполнение запроса   
                        using (SqlDataReader dataReader = command.ExecuteReader())
                        {
                            SearchGenresList.Children.Clear();
                            while (dataReader.Read())
                            {
                                CheckBox item = new CheckBox()
                                {
                                    Tag = dataReader.GetInt32(0),
                                    Content = dataReader.GetString(1),
                                    FontSize = 11
                                };
                                SearchGenresList.Children.Add(item);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"В процессе получения данных произошла ошибка:\n{ex}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обновление списка платформ
        private void UpdatePlatformsList()
        {
            try
            {
                // Подключение
                using (SqlConnection connection = new SqlConnection() { ConnectionString = Properties.Settings.Default.userConnection })
                {
                    // Открытие подключения
                    connection.Open();
                    // Отображение списка разработчиков
                    using (SqlCommand command = new SqlCommand("SELECT id, title FROM dbo.Platforms ORDER BY title ASC", connection))
                    {
                        // Выполнение запроса   
                        using (SqlDataReader dataReader = command.ExecuteReader())
                        {
                            SearchPlatformsList.Children.Clear();
                            while (dataReader.Read())
                            {
                                CheckBox item = new CheckBox()
                                {
                                    Tag = dataReader.GetInt32(0),
                                    Content = dataReader.GetString(1),
                                    FontSize = 11
                                };
                                SearchPlatformsList.Children.Add(item);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"В процессе получения данных произошла ошибка:\n{ex}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
