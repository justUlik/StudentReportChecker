using Microsoft.AspNetCore.Mvc;
using FileStoringService.Data;
using FileStoringService.Models;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace FileStoringService.Controllers;

[ApiController]
[Route("[controller]")]
public class FileController : ControllerBase
{
    private readonly FileDbContext _context;
    private readonly IWebHostEnvironment _env;

    public FileController(FileDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpPost("/store")]
    public async Task<IActionResult> StoreFile([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Файл не предоставлен");

        using var md5 = MD5.Create();
        using var stream = file.OpenReadStream();
        var hashBytes = md5.ComputeHash(stream);
        var hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

        var existing = await _context.Files.FirstOrDefaultAsync(f => f.Hash == hash);
        if (existing != null)
            return Ok(existing.Id);

        var id = Guid.NewGuid();
        var storagePath = Path.Combine(_env.ContentRootPath, "Storage");
        Directory.CreateDirectory(storagePath);

        var filePath = Path.Combine(storagePath, id + ".txt");
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        using (var inputStream = file.OpenReadStream())
        {
            await inputStream.CopyToAsync(fileStream);
        }

        var storedFile = new StoredFile
        {
            Id = id,
            FileName = file.FileName,
            Hash = hash,
            Location = filePath
        };

        _context.Files.Add(storedFile);
        await _context.SaveChangesAsync();

        return Ok(id);
    }
    
    [HttpGet("content/{id}")]
    public async Task<IActionResult> GetFileContent(Guid id)
    {
        var file = await _context.Files.FindAsync(id);
        if (file == null)
        {
            Console.WriteLine($"[FileService] File not found: {id}");
            return NotFound();
        }

        Console.WriteLine($"[FileService] File found: {file.FileName} at {file.Location}");

        if (!System.IO.File.Exists(file.Location))
        {
            Console.WriteLine($"[FileService] Physical file missing at: {file.Location}");
            return NotFound("Файл не найден на диске");
        }

        var content = await System.IO.File.ReadAllTextAsync(file.Location);
        return Content(content, "text/plain");
    }
}