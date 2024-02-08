using System.ComponentModel.DataAnnotations;

namespace BarberBookingSystem.Models
{
    public class Booking
    {
        public int ID { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }

        [Required(ErrorMessage = "Customer name is required.")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Customer number is required.")]
        public string CustomerNumber { get; set; }
        public string Status { get; set; }
    }
}
