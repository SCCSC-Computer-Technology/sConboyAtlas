using System.ComponentModel.DataAnnotations;

namespace atlasTaskflow.Models
{
    public class ClassroomModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Code { get; set; } = string.Empty;

        public int OrganizationId { get; set; }
        public OrganizationModel? Organization { get; set; }

        public int InstructorId { get; set; }
        public InstructorModel? Instructor { get; set; }

        public ICollection<EnrollmentModel> Enrollments { get; set; } = new List<EnrollmentModel>();
        public ICollection<AssignmentModel> Assignments { get; set; } = new List<AssignmentModel>();
    }
}
