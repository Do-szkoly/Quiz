namespace Quiz.Services;

public static class QuizSettings
{
    public const int QuestionsPerTest = 10;
    public const int TimeLimitSeconds = 300; // limit czasu na cały test (5 min)
    public static bool ShowCorrectAnswers { get; } = true; // false = tryb normalnego testu
}
