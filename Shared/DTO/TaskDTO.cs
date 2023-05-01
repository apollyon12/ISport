using System;

using iSportsRecruiting.Shared.Models;

namespace iSportsRecruiting.Shared.DTO
{
    public class TaskDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Description { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public long TimeStart { get; set; }
        public int Status { get; set; } = 1;
        public int Location { get; set; } = 1;
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Notes { get; set; }
        public int Type { get; set; } = 1;

        public TaskModel ToModel()
        {
            return new TaskModel
            {
                Id = Id,
                User_Id = UserId,
                Description = Description,
                Date_Start = DateStart ?? DateTime.MinValue,
                Date_End = DateEnd ?? DateTime.MinValue.AddDays(1),
                Time_Start = TimeStart,
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
