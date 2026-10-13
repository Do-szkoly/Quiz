using Quiz.Services;

namespace Quiz.Views;

public class StartPage : ContentPage
{
    readonly Label _info = new() { HorizontalTextAlignment = TextAlignment.Center };

    public StartPage()
    {
        NavigationPage.SetHasNavigationBar(this, false);
        var start = new Button { Text = "Rozpocznij test", FontSize = 18 };
        start.Clicked += async (_, _) =>
        {
            start.IsEnabled = false;
            try
            {
                var questions = await QuestionRepository.DrawAsync(QuizSettings.QuestionsPerTest);
                await Navigation.PushAsync(new QuizPage(questions));
            }
            catch (Exception ex) { await DisplayAlertAsync("Błąd", ex.Message, "OK"); }
            finally { start.IsEnabled = true; }
        };

        Content = new VerticalStackLayout
        {
            Padding = 30, Spacing = 20, VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Label { Text = "Aplikacja do testów", FontSize = 30, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center },
                new Label { Text = $"Test: {QuizSettings.QuestionsPerTest} losowych pytań\nLimit czasu: {QuizSettings.TimeLimitSeconds / 60} min", HorizontalTextAlignment = TextAlignment.Center },
                _info,
                start
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try { _info.Text = $"Liczba pytań w bazie: {(await QuestionRepository.LoadAsync()).Count}"; }
        catch (Exception ex) { _info.Text = "Nie udało się wczytać pytań: " + ex.Message; }
    }
}
