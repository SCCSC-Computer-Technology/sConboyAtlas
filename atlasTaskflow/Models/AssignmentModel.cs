using System.ComponentModel.DataAnnotations;

namespace atlasTaskflow.Models
{
    public class AssignmentModel
    {
        public int Id { get; set; }

        public int ClassroomId { get; set; }
        public ClassroomModel? Classroom { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        public ICollection<StudentAssignmentModel> StudentAssignments { get; set; } = new List<StudentAssignmentModel>();
    }
}
