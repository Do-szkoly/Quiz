namespace Quiz.Models;

public enum QuestionType { Radio, Picker, CheckBox, Switch, Text }

public class Question
{
    public int Id { get; set; }
    public string Text { get; set; } = "";
    public QuestionType Type { get; set; }
    public List<string> Answers { get; set; } = new();
    public List<int> Correct { get; set; } = new();     // puste = brak poprawnych odpowiedzi
    public List<string> Keywords { get; set; } = new(); // "a/b" = alternatywy w jednej grupie
    public int MinKeywords { get; set; }                // ile grup słów kluczowych musi wystąpić
}

public record AnswerResult(Question Question, bool IsCorrect, string UserAnswer, string CorrectAnswer);
