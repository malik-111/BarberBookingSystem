using BarberBookingSystem.Data;
using BarberBookingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

// BookingController.cs
public class BookingController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public BookingController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    
    [Authorize]
    public IActionResult Index()
    {
        var bookings = _dbContext.Bookings.ToList();
        var calendarCells = GenerateCalendar(bookings);

        return View(calendarCells); // Pass List<CalendarCell> to the view
    }


    private List<CalendarCell> GenerateCalendar(List<Booking> bookings)
    {
        var calendarCells = new List<CalendarCell>();
        var currentDate = DateTime.Now.Date;
        var bookingIdCounter = bookings.Any() ? bookings.Max(b => b.ID) + 1 : 1; // Increment from the highest existing booking ID

        for (int i = 0; i < 7; i++)
        {
            for (int j = 9; j <= 21; j++)
            {
                var timeSlot = $"{j}:00";
                var isBooked = bookings.Any(b => b.Date.Date == currentDate.AddDays(i) && b.Time == timeSlot);
                var bookingId = isBooked
                    ? bookings.First(b => b.Date.Date == currentDate.AddDays(i) && b.Time == timeSlot).ID
                    : bookingIdCounter++;

                var cell = new CalendarCell
                {
                    Date = currentDate.AddDays(i),
                    Time = timeSlot,
                    IsBooked = isBooked,
                    BookingId = bookingId
                };

                calendarCells.Add(cell);
            }
        }

        return calendarCells;
    }


    [Authorize]
    public IActionResult Create(DateTime date, string time)
    {
        // Display a form to create a booking for the selected date and time
        var model = new Booking
        {
            Date = date,
            Time = time
        };

        return View(model);
    }

    [Authorize]
    [HttpPost]
    public IActionResult Create(Booking model)
    {
        if (ModelState.IsValid)
        {
            // Check if the selected time slot is available
            var isSlotAvailable = !_dbContext.Bookings.Any(b =>
                b.Date.Date == model.Date.Date &&
                b.Time == model.Time &&
                b.CustomerName == model.CustomerName &&
                b.CustomerNumber == model.CustomerNumber);

            if (isSlotAvailable)
            {
                _dbContext.Bookings.Add(model);
                _dbContext.SaveChanges();

                // Redirect to the Index action after successfully creating a booking
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "The selected time slot is already booked. Please choose another time.");
            }
        }

        // If ModelState is not valid or the time slot is not available, return to the create view with validation errors
        return View(model);
    }


    // Add other actions for updating and deleting bookings

    [Authorize(Roles = "Admin")] // Add the necessary role for admin access
    public IActionResult Edit(int id)
    {
        // Display a form to edit a booking for the specified ID
        var booking = _dbContext.Bookings.Find(id);

        if (booking == null)
        {
            return NotFound();
        }

        return View(booking);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public IActionResult Edit(int id, Booking model)
    {
        // Action to handle the form submission and update the booking in the database
        if (id != model.ID)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _dbContext.Bookings.Update(model);
                _dbContext.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookingExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction("Index");
        }

        // If ModelState is not valid, return to the edit view with validation errors
        return View(model);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Delete(int id)
    {
        // Display a confirmation page to delete a booking for the specified ID
        var booking = _dbContext.Bookings.Find(id);

        if (booking == null)
        {
            return NotFound();
        }

        return View(booking);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(int id)
    {
        // Action to handle the confirmation and delete the booking from the database
        var booking = _dbContext.Bookings.Find(id);

        if (booking == null)
        {
            return NotFound();
        }

        _dbContext.Bookings.Remove(booking);
        _dbContext.SaveChanges();

        return RedirectToAction("Index");
    }

    private bool BookingExists(int id)
    {
        return _dbContext.Bookings.Any(e => e.ID == id);
    }

    [Authorize]
    public IActionResult BookingConfirmed(int id)
    {
        var booking = _dbContext.Bookings.Find(id);

        if (booking == null)
        {
            return NotFound();
        }

        return View(booking);
    }

    [Authorize]
    [HttpGet] // Add this attribute to allow GET requests
    public IActionResult Book(int id, string date, string time)
    {
        if (id <= 0)
        {
            // Invalid booking ID, handle accordingly (e.g., redirect to an error page)
            return BadRequest();
        }

        var booking = _dbContext.Bookings.Find(id);

        if (booking == null)
        {
            // No existing booking found with the specified ID, proceed with creating a new booking
            var newBooking = new Booking
            {
                Date = DateTime.Parse(date),
                Time = time,
                CustomerName = "ExampleName", // replace with the actual name
                CustomerNumber = "ExampleNumber", // replace with the actual number
            };

            // Pass the necessary information to the view
            ViewData["Date"] = newBooking.Date.ToString("yyyy-MM-dd");
            ViewData["Time"] = newBooking.Time;
            ViewData["Name"] = newBooking.CustomerName;
            ViewData["Number"] = newBooking.CustomerNumber;
            ViewData["BookingId"] = id;

            return View(newBooking);
        }

        if (booking.Status == "Confirmed")
        {
            // The existing booking is already confirmed, redirect to the BookingConfirmed action
            return RedirectToAction("BookingConfirmed", new { id = booking.ID });
        }

        // An existing booking with the specified ID was found and it's not confirmed,
        // set the time dynamically based on the selected time slot
        ViewData["Date"] = booking.Date.ToString("yyyy-MM-dd");

        // Update the Time property dynamically based on the selected time slot
        ViewData["Time"] = time;

        ViewData["BookingId"] = id;

        // Redirect to the BookingConfirmed action with the booking ID
        return RedirectToAction("BookingConfirmed", new { id = booking.ID });
    }

    [Authorize]
    [HttpPost]
    public IActionResult Book(int id, Booking model)
    {
        if (id <= 0)
        {
            // Invalid booking ID, handle accordingly (e.g., redirect to an error page)
            return BadRequest();
        }

        var booking = _dbContext.Bookings.Find(id);

        if (booking == null)
        {
            // No existing booking found with the specified ID, proceed with creating a new booking
            var newBooking = new Booking
            {
                Date = model.Date,
                Time = model.Time,
                CustomerName = model.CustomerName,
                CustomerNumber = model.CustomerNumber,
                Status = "Pending" // Set a default status value
            };

            _dbContext.Bookings.Add(newBooking);
            _dbContext.SaveChanges();

            // Redirect to the BookingConfirmed action after successfully creating a booking
            return RedirectToAction("BookingConfirmed", new { id = newBooking.ID });
        }

        // An existing booking with the specified ID was found and it's not confirmed,
        // set the time dynamically based on the selected time slot
        ViewData["Date"] = booking.Date.ToString("dd-MM-yyyy");

        // Update the Time property dynamically based on the selected time slot
        ViewData["Time"] = model.Time;

        ViewData["BookingId"] = id;

        // Redirect to the BookingConfirmed action with the booking ID
        return RedirectToAction("BookingConfirmed", new { id = booking.ID });
    }

}

