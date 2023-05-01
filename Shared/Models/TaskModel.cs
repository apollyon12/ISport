using System;

using iSportsRecruiting.Shared.DTO;

namespace iSportsRecruiting.Shared.Models
{
    public class TaskModel
    {
        public int Id { get; set; }
        public int User_Id { get; set; }
        public string Description { get; set; }
        public DateTime Date_Start { get; set; }
        public DateTime Date_End { get; set; }
        public long Time_Start { get; set; }
        public int Status { get; set; }
        public int Location { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Notes { get; set; }
        public int Type { get; set; }

        public TaskDTO ToDTO()
        {
            return new TaskDTO
            {
                Id = Id,
                UserId = User_Id,
                Description = Description,
                DateStart = Date_Start,
                DateEnd = Date_End,
                TimeStart = Time_Start,
                Status = Status,
                Location = Location,
                Email = Email,
                Phone = Phone,
                Notes = Notes,
                Type = Type
            };
        }
    }
}
