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
using System.Diagnostics;

namespace GameDatabase_App
{
    /// <summary>
    /// Логика взаимодействия для GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Window
    {
        public GameWindow(int game_id, bool adm = false)
        {
            InitializeComponent();
            Tag = game_id;
            ShowGame();
            if(adm)
            {
                GameEditButton.Visibility = Visibility.Visible;
            }
        }

        private void ShowGame()
        {
            try
            {
                // Подключение
                using (SqlConnection connection = new SqlConnection() { ConnectionString = Properties.Settings.Default.userConnection })
                {
                    // Открытие подключения
                    connection.Open();

                    // Получение списка рецензий на игру
                    ReviewsList.Children.Clear();
                    using (SqlCommand command = new SqlCommand(
                        " SELECT dbo.Reviewers.title, dbo.Reviews.summary, dbo.Reviews.score, dbo.Reviews.web_address" +
                        " FROM dbo.Reviews INNER JOIN dbo.Reviewers ON dbo.Reviews.reviewer_id = dbo.Reviewers.id" +
                        " WHERE game_id = @game_id", connection))
                    {
                        command.Parameters.Add(new SqlParameter("@game_id", (int)Tag));
                        // Выполнение запроса   
                        using (SqlDataReader dataReader = command.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                Grid reviewTileGrid = new Grid()
                                {
                                    //ShowGridLines = true
                                };
                                ReviewsList.Children.Add(reviewTileGrid);
                                ReviewsList.Children.Add(new Separator());
                                reviewTileGrid.ColumnDefinitions.Add(new ColumnDefinition());
                                reviewTileGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                                reviewTileGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                                reviewTileGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                                reviewTileGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                                // Название рецензента
                                // ---------------------------------------
                                Label reviewerLabel = new Label()
                                {
                                    Content = dataReader.GetString(0),
                                    FontSize = 18
                                };
                                reviewTileGrid.Children.Add(reviewerLabel);
                                // ---------------------------------------

                                // Краткое содержание рецензии
                                // ---------------------------------------
                                TextBlock reviewSummaryTextBox = new TextBlock()
                                {
                                    Text = dataReader.GetString(1),
                                    TextWrapping = TextWrapping.Wrap,
                                    Margin = new Thickness(3)
                                };
                                Grid.SetRow(reviewSummaryTextBox, 1);
                                reviewTileGrid.Children.Add(reviewSummaryTextBox);
                                // ---------------------------------------

                                // Оценка
                                // ---------------------------------------
                                Border reviewScoreBorder = new Border()
                                {
                                    Background = dataReader.IsDBNull(2) || dataReader.GetByte(2) <= 50 ? new SolidColorBrush(Colors.Red) : (dataReader.GetByte(2) <= 70 ? new SolidColorBrush(Colors.Gold) : new SolidColorBrush(Colors.YellowGreen)),
                                    Margin = new Thickness(5),
                                    BorderThickness = new Thickness(1),
                                    BorderBrush = new SolidColorBrush(Colors.Black),
                                    Height = 50,
                                    Width = 50,
                                    VerticalAlignment = VerticalAlignment.Top
                                };
                                reviewScoreBorder.Child = new Label()
                                {
                                    FontSize = 24,
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    VerticalAlignment = VerticalAlignment.Center,
                                    Content = dataReader.IsDBNull(2) ? "-" : dataReader.GetByte(2).ToString()
                                };
                                Grid.SetColumn(reviewScoreBorder, 1);
                                Grid.SetRowSpan(reviewScoreBorder, 2);
                                reviewTileGrid.Children.Add(reviewScoreBorder);
                                // ---------------------------------------

                                // Ссылка на рецензию
                                // ---------------------------------------
                                Label moreInfoLabel = new Label
                                {
                                    Content = new Hyperlink()
                                    {
                                        NavigateUri = new Uri(dataReader.GetString(3), UriKind.Relative)
                                    }
                                };
                                ((Hyperlink)(moreInfoLabel.Content)).Inlines.Add("Читать полностью");
                                ((Hyperlink)(moreInfoLabel.Content)).RequestNavigate += Hyperlink_RequestNavigate;
                                reviewTileGrid.Children.Add(moreInfoLabel);
                                Grid.SetColumnSpan(moreInfoLabel, 2);
                                Grid.SetRow(moreInfoLabel, 2);
                                // ---------------------------------------

                            }
                        }
                    }

                    // Получение информации о игре
                    using (SqlCommand command = new SqlCommand(
                        " SELECT dbo.Games.title, dbo.Games.website, dbo.Games.summary, dbo.Games.release_date, GameScore.avg_score" +
                        " FROM dbo.Games" +
                            " LEFT OUTER JOIN ( SELECT game_id, AVG(score) AS avg_score FROM dbo.Reviews GROUP BY dbo.Reviews.game_id) AS GameScore" +
                                " ON  dbo.Games.id = GameScore.game_id" +
                        " WHERE dbo.Games.id = @game_id", connection))
                    {
                        command.Parameters.Add(new SqlParameter("@game_id", (int)Tag));
                        // Выполнение запроса   
                        using (SqlDataReader dataReader = command.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                // Заголовок окна
                                Title = dataReader.GetString(0);
                                // Название игры
                                GameTitle.Content = dataReader.GetString(0);
                                // Дата выхода игры
                                GameReleaseDate.Content = "Дата выхода: " +
                                    $"{ (dataReader.IsDBNull(3) ? "TBA" : DateTime.Parse(dataReader.GetDateTime(3).ToString()).ToShortDateString())}";
                                // Средняя оценка игры
                                GameAvgScore.Content = dataReader.IsDBNull(4) ? "-" : dataReader.GetInt32(4).ToString();
                                // Цвет поля со средней оценкой
                                GameAvgScoreBorder.Background = dataReader.IsDBNull(4) || dataReader.GetInt32(4) <= 50 ? new SolidColorBrush(Colors.Red) : (dataReader.GetInt32(4) <= 70 ? new SolidColorBrush(Colors.Gold) : new SolidColorBrush(Colors.YellowGreen));
                                // Описание игры
                                GameSummary.Text = dataReader.GetString(2);
                                // Официальный сайт игры
                                GameOfficial.NavigateUri = new Uri(dataReader.GetString(1), UriKind.Relative);
                                GameOfficial.Inlines.Clear();
                                GameOfficial.Inlines.Add(dataReader.GetString(1));
                            }
                        }
                    }

                    // Разработчики игры
                    using (SqlCommand command = new SqlCommand(
                        " SELECT dbo.Developers.title" +
                        " FROM dbo.Games_Developers INNER JOIN dbo.Developers" +
                            " ON dbo.Games_Developers.developer_id = dbo.Developers.id" +
                        " WHERE dbo.Games_Developers.game_id = @game_id", connection))
                    {
                        command.Parameters.Add(new SqlParameter("@game_id", (int)Tag));
                        // Выполнение запроса   
                        using (SqlDataReader dataReader = command.ExecuteReader())
                        {
                            int count = 0;
                            while (dataReader.Read())
                            {
                                if (count == 0)
                                    GameDevelopers.Text = dataReader.GetString(0);
                                else
                                    GameDevelopers.Text += $", {dataReader.GetString(0)}";
                                count++;
                            }
                        }
                    }

                    // Издатели игры
                    using (SqlCommand command = new SqlCommand(
                        " SELECT dbo.Publishers.title" +
                        " FROM dbo.Games_Publishers INNER JOIN dbo.Publishers" +
                            " ON dbo.Games_Publishers.publisher_id = dbo.Publishers.id" +
                        " WHERE dbo.Games_Publishers.game_id = @game_id", connection))
                    {
                        command.Parameters.Add(new SqlParameter("@game_id", (int)Tag));
                        // Выполнение запроса   
                        using (SqlDataReader dataReader = command.ExecuteReader())
                        {
                            int count = 0;
                            while (dataReader.Read())
                            {
                                if (count == 0)
                                    GamePublishers.Text = dataReader.GetString(0);
                                else
                                    GamePublishers.Text += $", {dataReader.GetString(0)}";
                                count++;
                            }
                        }
                    }

                    // Жанры игры
                    using (SqlCommand command = new SqlCommand(
                        " SELECT dbo.Genres.title" +
                        " FROM dbo.Games_Genres INNER JOIN dbo.Genres " +
                            " ON dbo.Games_Genres.genre_id = dbo.Genres.id" +
                        " WHERE dbo.Games_Genres.game_id = @game_id", connection))
                    {
                        command.Parameters.Add(new SqlParameter("@game_id", (int)Tag));
                        // Выполнение запроса   
                        using (SqlDataReader dataReader = command.ExecuteReader())
                        {
                            int count = 0;
                            while (dataReader.Read())
                            {
                                if (count == 0)
                                    GameGenres.Text = dataReader.GetString(0);
                                else
                                    GameGenres.Text += $", {dataReader.GetString(0)}";
                                count++;
                            }
                        }
                    }

                    // Платформы игры
                    using (SqlCommand command = new SqlCommand(
                        " SELECT dbo.Platforms.title" +
                        " FROM dbo.Games_Platforms INNER JOIN dbo.Platforms" +
                            " ON dbo.Games_Platforms.platform_id = dbo.Platforms.id" +
                        " WHERE dbo.Games_Platforms.game_id = @game_id", connection))
                    {
                        command.Parameters.Add(new SqlParameter("@game_id", (int)Tag));
                        // Выполнение запроса   
                        using (SqlDataReader dataReader = command.ExecuteReader())
                        {
                            int count = 0;
                            while (dataReader.Read())
                            {
                                if (count == 0)
                                    GamePlatforms.Text = dataReader.GetString(0);
                                else
                                    GamePlatforms.Text += $", {dataReader.GetString(0)}";
                                count++;
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

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.IsAbsoluteUri ? e.Uri.AbsoluteUri : "http://" + e.Uri.OriginalString));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"В процессе загрузки сайта произошла ошибка:{ex}", "Ошибка загрузки", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Открытие окна редактирования игры
        private void GameEditButton_Click(object sender, RoutedEventArgs e)
        {
            EditGameWindow window = new EditGameWindow((int)Tag);
            window.ShowDialog();
            ShowGame();
        }
    }
}
