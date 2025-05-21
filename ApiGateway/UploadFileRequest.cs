using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

public class UploadFileRequest
{
    [Required]
    [Display(Name = "file")]
    public IFormFile File { get; set; }
}