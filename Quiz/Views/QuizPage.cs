using Quiz.Models;
using Quiz.Services;

namespace Quiz.Views;

public class QuizPage : ContentPage
{
    readonly List<Question> _questions;
    readonly List<AnswerResult> _results = new();
    int _index, _remaining = QuizSettings.TimeLimitSeconds;
    bool _finished, _started;
    IDispatcherTimer? _timer;

    readonly Label _progress = new() { VerticalOptions = LayoutOptions.Center };
    readonly Label _timerLabel = new() { FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.End };
    readonly Label _question = new() { FontSize = 20, FontAttributes = FontAttributes.Bold };
    readonly Label _hint = new() { FontSize = 13, Opacity = 0.7 };
    readonly VerticalStackLayout _answers = new() { Spacing = 10 };
    readonly Button _next = new();

    // delegat odczytujący aktualną odpowiedź użytkownika (zależny od typu pytania)
    Func<(HashSet<int> Selected, string Text)> _read = () => (new(), "");

    public QuizPage(List<Question> questions)
    {
        _questions = questions;
        NavigationPage.SetHasNavigationBar(this, false);

        var header = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        header.Add(_progress, 0);
        header.Add(_timerLabel, 1);

        var top = new VerticalStackLayout { Spacing = 10, Children = { header, _question, _hint } };
        _next.Clicked += (_, _) => OnNext();

        var root = new Grid
        {
            Padding = 20, RowSpacing = 15,
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) }
        };
        root.Add(top, 0, 0);
        root.Add(new ScrollView { Content = _answers }, 0, 1);
        root.Add(_next, 0, 2);
        Content = root;

        UpdateTimer();
        ShowQuestion();
    }

    protected override bool OnBackButtonPressed() => true; // brak wyjścia w trakcie testu

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_started || _finished) return;
        _started = true;
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += (_, _) =>
        {
            _remaining--;
            UpdateTimer();
            if (_remaining <= 0) Finish(timeUp: true);
        };
        _timer.Start();
    }

    void UpdateTimer()
    {
        _timerLabel.Text = $"⏱ {_remaining / 60:00}:{_remaining % 60:00}";
        _timerLabel.TextColor = _remaining <= 30 ? Colors.Red : (Application.Current?.RequestedTheme == AppTheme.Dark ? Colors.White : Colors.Black);
    }

    void ShowQuestion()
    {
        var q = _questions[_index];
        _progress.Text = $"Pytanie {_index + 1}/{_questions.Count}";
        _question.Text = q.Text;
        _next.Text = _index == _questions.Count - 1 ? "Zakończ test" : "Dalej";
        _answers.Children.Clear();

        var order = Enumerable.Range(0, q.Answers.Count).OrderBy(_ => Random.Shared.Next()).ToList(); // losowa kolejność odpowiedzi
        (_hint.Text, _read) = q.Type switch
        {
            QuestionType.Radio => ("Wybierz jedną odpowiedź (RadioButton).", BuildRadio(q, order)),
            QuestionType.Picker => ("Wybierz jedną odpowiedź z listy rozwijanej (Picker).", BuildPicker(q, order)),
            QuestionType.CheckBox => ("Zaznacz wszystkie poprawne odpowiedzi – może być jedna, kilka lub żadna (CheckBox).", BuildChecks(q, order)),
            QuestionType.Switch => ("Włącz przełącznik przy każdej poprawnej odpowiedzi – może być jedna, kilka lub żadna (Switch).", BuildSwitches(q, order)),
            _ => ("Wpisz odpowiedź własnymi słowami. Zostanie sprawdzona pod kątem słów kluczowych.", BuildText())
        };

        if (QuizSettings.ShowCorrectAnswers)
            _answers.Add(new Label
            {
                Text = CorrectAnswerText(q),
                TextColor = Colors.Green,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(0, 10, 0, 0)
            });
    }

    Func<(HashSet<int>, string)> BuildRadio(Question q, List<int> order)
    {
        var group = $"q{_index}";
        var buttons = order.Select(i => new RadioButton { Content = q.Answers[i], GroupName = group, Value = i }).ToList();
        foreach (var b in buttons) _answers.Add(b);
        return () => (buttons.Where(b => b.IsChecked).Select(b => (int)b.Value).ToHashSet(), "");
    }

    Func<(HashSet<int>, string)> BuildPicker(Question q, List<int> order)
    {
        var picker = new Picker { Title = "Wybierz odpowiedź...", ItemsSource = order.Select(i => q.Answers[i]).ToList() };
        _answers.Add(picker);
        return () => (picker.SelectedIndex >= 0 ? new HashSet<int> { order[picker.SelectedIndex] } : new HashSet<int>(), "");
    }

    Func<(HashSet<int>, string)> BuildChecks(Question q, List<int> order)
    {
        var boxes = new List<(CheckBox Box, int Idx)>();
        foreach (var i in order)
        {
            var cb = new CheckBox();
            var lbl = new Label { Text = q.Answers[i], VerticalOptions = LayoutOptions.Center };
            lbl.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => cb.IsChecked = !cb.IsChecked) });
            _answers.Add(new HorizontalStackLayout { Spacing = 8, Children = { cb, lbl } });
            boxes.Add((cb, i));
        }
        return () => (boxes.Where(b => b.Box.IsChecked).Select(b => b.Idx).ToHashSet(), "");
    }

    Func<(HashSet<int>, string)> BuildSwitches(Question q, List<int> order)
    {
        var switches = new List<(Switch Sw, int Idx)>();
        foreach (var i in order)
        {
            var sw = new Switch();
            var row = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 10 };
            row.Add(new Label { Text = q.Answers[i], VerticalOptions = LayoutOptions.Center }, 0);
            row.Add(sw, 1);
            _answers.Add(row);
            switches.Add((sw, i));
        }
        return () => (switches.Where(s => s.Sw.IsToggled).Select(s => s.Idx).ToHashSet(), "");
    }

    Func<(HashSet<int>, string)> BuildText()
    {
        var editor = new Editor { Placeholder = "Twoja odpowiedź...", HeightRequest = 150 };
        _answers.Add(editor);
        return () => (new HashSet<int>(), editor.Text ?? "");
    }

    static string CorrectAnswerText(Question q)
    {
        if (q.Type == QuestionType.Text)
            return "Odpowiedź powinna zawierać słowa kluczowe: " +
                   string.Join(", ", q.Keywords.Select(k => k.Replace("/", " / "))) +
                   $" (wymagane co najmniej {q.MinKeywords})";

        return q.Correct.Count == 0
            ? "Poprawne odpowiedzi: żadna"
            : "Poprawne odpowiedzi: " + string.Join("; ", q.Correct.Select(i => q.Answers[i]));
    }

    void Commit()
    {
        var q = _questions[_index];
        var (selected, text) = _read();

        if (q.Type == QuestionType.Text)
        {
            bool ok = AnswerChecker.CountKeywords(q, text) >= q.MinKeywords;
            var expected = "przykładowe słowa kluczowe: " + string.Join(", ", q.Keywords.Select(k => k.Split('/')[0])) +
                           $" (wymagane co najmniej {q.MinKeywords})";
            _results.Add(new AnswerResult(q, ok, string.IsNullOrWhiteSpace(text) ? "(brak odpowiedzi)" : text.Trim(), expected));
        }
        else
        {
            string Join(IEnumerable<int> idx, string empty) =>
                idx.Any() ? string.Join("; ", idx.OrderBy(i => i).Select(i => q.Answers[i])) : empty;
            _results.Add(new AnswerResult(q, AnswerChecker.IsCorrect(q, selected),
                Join(selected, "(nic nie zaznaczono)"),
                Join(q.Correct, "(żadna odpowiedź nie jest poprawna)")));
        }
    }

    void OnNext()
    {
        if (_finished) return;
        Commit();
        _index++;
        if (_index >= _questions.Count) Finish(false);
        else ShowQuestion();
    }

    void Finish(bool timeUp)
    {
        if (_finished) return;
        _finished = true;
        _timer?.Stop();

        if (timeUp)
        {
            Commit();
            _index++;
            for (; _index < _questions.Count; _index++)
                _results.Add(new AnswerResult(_questions[_index], false, "(brak odpowiedzi)", ""));
        }
        Application.Current!.Windows[0].Page = new NavigationPage(new ResultPage(_results, timeUp));
    }
}
