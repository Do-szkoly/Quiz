using System.Text.Json.Serialization;
using System.Text.Json;
using Quiz.Models;

namespace Quiz.Services;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true, UseStringEnumConverter = true)]
[JsonSerializable(typeof(List<Question>))]
internal partial class QuizJsonContext : JsonSerializerContext { }

public static class QuestionRepository
{
    static List<Question>? _all;

    public static async Task<List<Question>> LoadAsync()
    {
        if (_all != null) return _all;
        await using var stream = await FileSystem.OpenAppPackageFileAsync("questions.json");
        _all = await JsonSerializer.DeserializeAsync(stream, QuizJsonContext.Default.ListQuestion) ?? new();
        return _all;
    }

    public static async Task<List<Question>> DrawAsync(int count)
    {
        var all = await LoadAsync();
        return all.OrderBy(_ => Random.Shared.Next()).Take(count).ToList();
    }
}
