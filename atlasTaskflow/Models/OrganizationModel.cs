using System.ComponentModel.DataAnnotations;

namespace atlasTaskflow.Models
{
    public class OrganizationModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public ICollection<InstructorModel> Instructors { get; set; } = new List<InstructorModel>();
        public ICollection<ClassroomModel> Classrooms { get; set; } = new List<ClassroomModel>();
    }
}
