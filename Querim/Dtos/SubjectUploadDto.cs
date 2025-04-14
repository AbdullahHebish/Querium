using System.ComponentModel.DataAnnotations;

namespace Querim.Dtos
{
    public class SubjectUploadDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public int AcademicYear { get; set; }

        [Required]
        public string Semester { get; set; }

        public List<ChapterUploadDto> Chapters { get; set; }
    }

    public class ChapterUploadDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public string PdfPath { get; set; }
    }
}
