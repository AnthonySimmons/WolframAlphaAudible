
using System;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using WolframAlphaAPI;

namespace WolframAlphaAudible
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WolframAlphaClient _wolframClient = new WolframAlphaClient();

        private const string QueryTextTitle = "Enter Query...";

        private const string AnswerTextTitle = "Answer...";

        public MainWindow()
        {
            InitializeComponent();        
        }

        private void SetProgressBar()
        {
            var thread = new Thread(SetProgressBarThread);
            thread.Start();
        }

        private void SetProgressBarThread()
        {
            Dispatcher.Invoke(() => searchProgressBar.Visibility = Visibility.Visible);
            while(_wolframClient.QueryInProgress)
            {
                Dispatcher.Invoke(() =>
                {
                    searchProgressBar.Value += 1;
                    if (searchProgressBar.Value >= searchProgressBar.Maximum)
                    {
                        searchProgressBar.Value = 0;
                    }
                });
                Thread.Sleep(25);
            }
            Dispatcher.Invoke(() => searchProgressBar.Visibility = Visibility.Hidden);
        }

        private async void SendQueryAsync()
        {
            try
            {
                SetProgressBar();
                var query = textBoxQuery.Text;
                string answer = await _wolframClient.SendQueryAsync(query);

                SetCurrentAnswer();
            }
            catch(InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
            
        }


        private void SetCurrentAnswer()
        {
            buttonNext.IsEnabled = _wolframClient.HasAnswers;
            buttonPrevious.IsEnabled = _wolframClient.HasAnswers;

            textBoxAnswer.Text = _wolframClient.CurrentAnswer;
            imageAnswer.Source = new BitmapImage(new Uri(_wolframClient.CurrentImageUrl));
        }


        private void textBoxQuery_GotFocus(object sender, RoutedEventArgs e)
        {
            if (textBoxQuery.Text == QueryTextTitle)
            {
                textBoxQuery.Text = string.Empty;
            }
        }
        

        private void textBoxQuery_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Enter)
            {
                SendQueryAsync();
            }
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            SendQueryAsync();
        }

        private void buttonPrevious_Click(object sender, RoutedEventArgs e)
        {
            _wolframClient.CurrentAnswerIndex = Math.Max(--_wolframClient.CurrentAnswerIndex, 0);
            SetCurrentAnswer();
        }

        private void buttonNext_Click(object sender, RoutedEventArgs e)
        {
            _wolframClient.CurrentAnswerIndex = Math.Min(++_wolframClient.CurrentAnswerIndex, _wolframClient.Answers.Count - 1);
            SetCurrentAnswer();
        }
    }
}
