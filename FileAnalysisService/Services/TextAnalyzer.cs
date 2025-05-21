namespace FileAnalysisService.Services;

public class TextAnalyzer
{
    public static (int paragraphs, int words, int characters) Analyze(string content)
    {
        var paragraphs = content.Split('\n').Count(l => !string.IsNullOrWhiteSpace(l));
        var words = content.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        var characters = content.Length;
        return (paragraphs, words, characters);
    }
}