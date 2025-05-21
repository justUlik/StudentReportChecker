using FileAnalysisService.Data;
using FileAnalysisService.Models;
using FileAnalysisService.Services;
using Microsoft.AspNetCore.Mvc;

namespace FileAnalysisService.Controllers;

[ApiController]
[Route("[controller]")]
public class FileAnalysisController : ControllerBase
{
    private readonly AnalysisDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;

    public FileAnalysisController(AnalysisDbContext context, IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _config = config;
    }

    [HttpGet("/analyze/{id}")]
    public async Task<IActionResult> Analyze(Guid id)
    {
        var existing = await _context.Results.FindAsync(id);
        if (existing != null)
            return Ok(existing);

        string content;
        try
        {
            content = await GetFileContentFromFileService(id);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"[Analysis] FileStoringService недоступен: {ex.Message}");
            return StatusCode(503, "FileStoringService временно недоступен");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Analysis] Ошибка получения файла: {ex.Message}");
            return StatusCode(500, "Ошибка при получении файла");
        }

        (int paragraphs, int words, int characters) analysis;
        try
        {
            analysis = TextAnalyzer.Analyze(content);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Analysis] Ошибка анализа текста: {ex.Message}");
            return StatusCode(500, "Ошибка анализа файла");
        }

        var result = new FileAnalysisResult
        {
            Id = id,
            Paragraphs = analysis.paragraphs,
            Words = analysis.words,
            Characters = analysis.characters
        };

        try
        {
            _context.Results.Add(result);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Analysis] Ошибка сохранения в БД: {ex.Message}");
            return StatusCode(500, "Не удалось сохранить результаты анализа");
        }

        return Ok(result);
    }

    private async Task<string> GetFileContentFromFileService(Guid id)
    {
        var fileServiceUrl = _config["Services:FileStoring"];
        var http = _httpClientFactory.CreateClient();

        var response = await http.GetAsync($"{fileServiceUrl}/file/content/{id}");
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Ошибка получения файла: код {response.StatusCode}");

        return await response.Content.ReadAsStringAsync();
    }
}