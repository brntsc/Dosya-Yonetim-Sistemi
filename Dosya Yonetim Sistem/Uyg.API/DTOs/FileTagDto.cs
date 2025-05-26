using System.ComponentModel.DataAnnotations;

namespace Uyg.API.DTOs
{
    public class FileTagDto
    {
        public int Id { get; set; }

        [Required]
        public string TagName { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }
    }

    public class CreateFileTagDto
    {
        [Required]
        public string TagName { get; set; }

        public string Description { get; set; }
    }

    public class UpdateFileTagDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string TagName { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }
    }

    public class FileTagResponseDto
    {
        public int Id { get; set; }
        public string TagName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int FileCount { get; set; }
    }
} 