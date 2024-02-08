using BarberBookingSystem.Data;
using BarberBookingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;

public class BookingRepository
{
    private readonly ApplicationDbContext _dbContext;

    public BookingRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public List<Booking> GetAllBookings()
    {
        return _dbContext.Bookings.ToList();
    }

    public void AddBooking(Booking booking)
    {
        _dbContext.Bookings.Add(booking);
        _dbContext.SaveChanges();
    }
}
