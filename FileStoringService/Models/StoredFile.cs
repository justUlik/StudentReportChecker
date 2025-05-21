using System.ComponentModel.DataAnnotations;

namespace FileStoringService.Models
{
    public class StoredFile
    {
        [Key]
        public Guid Id { get; set; }
        public string FileName { get; set; } = "";
        public string Hash { get; set; } = "";
        public string Location { get; set; } = "";
    }
}