using System;

namespace Heat_Production_Optimization.Models
{
    public readonly struct HourSlot
    {
        public DateOnly Date { get; }
        public int Hour { get; }

        public HourSlot(DateOnly date, int hour)
        {
            if (hour < 0 || hour > 23)
                throw new ArgumentOutOfRangeException(nameof(hour), "Hour must be 0-23.");

            Date = date;
            Hour = hour;
        }

        public DateTime ToDateTime()
        {
            return Date.ToDateTime(new TimeOnly(Hour, 0));
        }

        public override string ToString()
        {
            return $"{Date:dd.MM.yyyy} {Hour:00}:00";
        }
    }
}