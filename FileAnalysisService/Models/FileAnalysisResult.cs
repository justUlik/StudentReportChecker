namespace FileAnalysisService.Models;

public class FileAnalysisResult
{
    public Guid Id { get; set; }
    public int Paragraphs { get; set; }
    public int Words { get; set; }
    public int Characters { get; set; }
}