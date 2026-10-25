using System.ComponentModel.DataAnnotations;

namespace atlasTaskflow.Models
{
    public class TaskModel
    {
        public int Id { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        public bool IsComplete { get; set; }
    }
}
