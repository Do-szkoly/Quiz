using Microsoft.Maui.Controls.Shapes;
using Quiz.Models;

namespace Quiz.Views;

public class ResultPage : ContentPage
{
    public ResultPage(IReadOnlyList<AnswerResult> results, bool timeUp)
    {
        NavigationPage.SetHasNavigationBar(this, false);
        int score = results.Count(r => r.IsCorrect), total = results.Count;
        double pct = total == 0 ? 0 : 100.0 * score / total;
        string grade = pct >= 90 ? "Doskonale!" : pct >= 70 ? "Dobry wynik!" : pct >= 50 ? "Zaliczone, ale można lepiej." : "Warto jeszcze poćwiczyć.";

        var stack = new VerticalStackLayout { Padding = 20, Spacing = 12 };
        stack.Add(new Label { Text = "Wynik testu", FontSize = 26, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center });
        stack.Add(new Label { Text = $"{score} / {total}", FontSize = 48, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center });
        stack.Add(new Label { Text = $"{pct:0}% – {grade}", FontSize = 18, HorizontalTextAlignment = TextAlignment.Center });
        if (timeUp)
            stack.Add(new Label { Text = "Czas minął – test zakończono automatycznie.", TextColor = Colors.OrangeRed, HorizontalTextAlignment = TextAlignment.Center });
        stack.Add(new Label { Text = "Szczegóły odpowiedzi", FontSize = 20, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 10, 0, 0) });

        int n = 1;
        foreach (var r in results)
        {
            var card = new VerticalStackLayout { Spacing = 4 };
            card.Add(new Label { Text = $"{n++}. {(r.IsCorrect ? "✔" : "✘")} {r.Question.Text}", FontAttributes = FontAttributes.Bold });
            card.Add(new Label { Text = $"Twoja odpowiedź: {r.UserAnswer}" });
            if (!r.IsCorrect)
                card.Add(new Label { Text = $"Poprawnie: {r.CorrectAnswer}", TextColor = Colors.Green });
            stack.Add(new Border
            {
                Stroke = r.IsCorrect ? Colors.Green : Colors.Red, StrokeThickness = 2, Padding = 12,
                StrokeShape = new RoundRectangle { CornerRadius = 10 }, Content = card
            });
        }

        var again = new Button { Text = "Zacznij nowy test", Margin = new Thickness(0, 10, 0, 20) };
        again.Clicked += (_, _) => Application.Current!.Windows[0].Page = new NavigationPage(new StartPage());
        stack.Add(again);

        Content = new ScrollView { Content = stack };
    }
}
