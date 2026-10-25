using System.ComponentModel.DataAnnotations;

namespace atlasTaskflow.Models
{
    public class StudentAssignmentModel
    {
        public int Id { get; set; }

        public int AssignmentId { get; set; }
        public AssignmentModel? Assignment { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;

        public bool IsComplete { get; set; }
    }
}
