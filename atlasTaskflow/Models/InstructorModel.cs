namespace atlasTaskflow.Models
{
    public class InstructorModel
    {
        public int Id {get; set;}
        public string? UserId { get; set; }
        public string? FirstName {get; set;}
        public string? LastName {get; set;}
        public string? Email {get; set;}
        public string? Subject {get; set;}
        public int? OrganizationId { get; set; }
        public OrganizationModel? Organization { get; set; }
        public ICollection<ClassroomModel> Classrooms { get; set; } = new List<ClassroomModel>();
    }
}
