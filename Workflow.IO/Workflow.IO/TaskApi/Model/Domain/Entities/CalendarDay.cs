using System;
using System.ComponentModel.DataAnnotations;

namespace TaskApi.Model.Domain.Entities
{
    public class CalendarDay
    {
        [Key]
        public DateTime Date { get; set; }

        public bool IsWorkingDay { get; set; }
    }
}
