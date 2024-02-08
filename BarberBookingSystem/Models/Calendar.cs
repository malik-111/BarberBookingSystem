namespace BarberBookingSystem.Models
{
    // CalendarCell.cs
    public class CalendarCell
    {
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public bool IsBooked { get; set; }
        public int? BookingId { get; set; }
    }

}
