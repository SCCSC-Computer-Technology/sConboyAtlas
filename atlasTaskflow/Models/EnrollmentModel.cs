using System.ComponentModel.DataAnnotations;

namespace atlasTaskflow.Models
{
    public class EnrollmentModel
    {
        public int Id { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;

        public int ClassroomId { get; set; }
        public ClassroomModel? Classroom { get; set; }
    }
}
