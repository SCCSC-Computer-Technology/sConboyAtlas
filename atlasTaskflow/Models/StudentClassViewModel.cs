namespace atlasTaskflow.Models
{
    public class StudentClassViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public int AssignmentCount { get; set; }
    }
}
