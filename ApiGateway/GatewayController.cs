using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace ApiGateway.Controllers;

[ApiController]
[Route("gateway")]
public class GatewayController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public GatewayController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient();
        _configuration = configuration;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadFile([FromForm] UploadFileRequest request)
    {
        var file = request.File;

        if (file == null || file.Length == 0 || !file.FileName.EndsWith(".txt"))
            return BadRequest("Файл не предоставлен или имеет неверный формат");

        try
        {
            var fileServiceUrl = _configuration["Services:FileStoring"];

            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
            content.Add(fileContent, "file", file.FileName);

            var response = await _httpClient.PostAsync($"{fileServiceUrl}/store", content);
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Ошибка при передаче файла");

            var fileId = await response.Content.ReadAsStringAsync();
            return Ok(new { fileId });
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"[Gateway] FileStoringService недоступен: {ex.Message}");
            return StatusCode(503, "FileStoringService временно недоступен");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Gateway] Внутренняя ошибка при загрузке файла: {ex.Message}");
            return StatusCode(500, "Внутренняя ошибка");
        }
    }

    [HttpGet("analyze/{fileId}")]
    public async Task<IActionResult> Analyze(string fileId)
    {
        try
        {
            var analysisServiceUrl = _configuration["Services:Analysis"];
            var response = await _httpClient.GetAsync($"{analysisServiceUrl}/analyze/{fileId}");
            var result = await response.Content.ReadAsStringAsync();

            return StatusCode((int)response.StatusCode, result);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"[Gateway] AnalysisService недоступен: {ex.Message}");
            return StatusCode(503, "AnalysisService временно недоступен");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Gateway] Внутренняя ошибка анализа: {ex.Message}");
            return StatusCode(500, "Внутренняя ошибка");
        }
    }

    [HttpGet("content/{fileId}")]
    public async Task<IActionResult> GetFileContent(string fileId)
    {
        try
        {
            var fileServiceUrl = _configuration["Services:FileStoring"];
            var response = await _httpClient.GetAsync($"{fileServiceUrl}/file/content/{fileId}");
            var content = await response.Content.ReadAsStringAsync();

            return StatusCode((int)response.StatusCode, content);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"[Gateway] FileStoringService недоступен: {ex.Message}");
            return StatusCode(503, "FileStoringService временно недоступен");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Gateway] Внутренняя ошибка при получении файла: {ex.Message}");
            return StatusCode(500, "Внутренняя ошибка");
        }
    }
}