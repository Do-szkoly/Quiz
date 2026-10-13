using System.Globalization;
using System.Text;
using Quiz.Models;

namespace Quiz.Services;

public static class AnswerChecker
{
    public static string Normalize(string s)
    {
        var d = s.ToLowerInvariant().Replace('ł', 'l').Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in d)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    public static int CountKeywords(Question q, string text)
    {
        var t = Normalize(text);
        return q.Keywords.Count(group =>
            group.Split('/').Any(k => !string.IsNullOrWhiteSpace(k) && t.Contains(Normalize(k.Trim()))));
    }

    public static bool IsCorrect(Question q, ISet<int> selected) => selected.SetEquals(q.Correct);
}
