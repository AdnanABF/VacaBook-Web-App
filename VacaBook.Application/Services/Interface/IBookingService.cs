using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VacaBook.Domain.Entities;

namespace VacaBook.Application.Services.Interface
{
    public interface IBookingService
    {
        void CreateBooking(Booking booking);
        Booking GetBookingById(int bookingId);
        IEnumerable<Booking> GetAllBookings(string userId = "", string? statusFilterList = "");
        void UpdateStatus(int bookingId, string bookingStatus, int villaNumber);
        void UpdatePaymentId(int bookingId, string sessionId, string paymentIntentId);
        IEnumerable<int> GetCheckedInVillaNumbers(int villaId);
    }
}
