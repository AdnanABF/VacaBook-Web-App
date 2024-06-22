using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using VacaBook.Application.Common.Interfaces;
using VacaBook.Application.Common.Utility;
using VacaBook.Application.Services.Interface;
using VacaBook.Domain.Entities;

namespace VacaBook.Application.Services.Implementation
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BookingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void CreateBooking(Booking booking)
        {
            _unitOfWork.Booking.Add(booking);
            _unitOfWork.Save();
        }

        public IEnumerable<Booking> GetAllBookings(string userId = "", string? statusFilterList = "")
        {
            IEnumerable<string> statusList = statusFilterList.ToLower().Split(",");
            if (!string.IsNullOrEmpty(statusFilterList) && !string.IsNullOrEmpty(userId))
            {
                return _unitOfWork.Booking.GetAll(x => statusList.Contains(x.Status.ToLower()) && x.UserId == userId, includeProperties: "User,Villa");
            }
            else
            {
                if (!string.IsNullOrEmpty(statusFilterList))
                {
                    return _unitOfWork.Booking.GetAll(x => statusList.Contains(x.Status.ToLower()), includeProperties: "User,Villa");
                }
                if (!string.IsNullOrEmpty(userId))
                {
                    return _unitOfWork.Booking.GetAll(x => x.UserId == userId, includeProperties: "User,Villa");
                }
            }
            return _unitOfWork.Booking.GetAll(includeProperties: "User,Villa");
        }

        public Booking GetBookingById(int bookingId)
        {
            return _unitOfWork.Booking.Get(x => x.Id == bookingId, includeProperties: "User,Villa");
        }

        public IEnumerable<int> GetCheckedInVillaNumbers(int villaId)
        {
            return _unitOfWork.Booking.GetAll(x => x.VillaId == villaId && x.Status == SD.StatusCheckedIn).Select(x => x.VillaNumber);
        }

        public void UpdatePaymentId(int bookingId, string sessionId, string paymentIntentId)
        {
            var bookingFromDb = _unitOfWork.Booking.Get(x => x.Id == bookingId, tracked: true);
            if (bookingFromDb != null)
            {
                if (!string.IsNullOrEmpty(sessionId))
                {
                    bookingFromDb.SessionId = sessionId;
                }
                if (!string.IsNullOrEmpty(paymentIntentId))
                {
                    bookingFromDb.PaymentIntentId = paymentIntentId;
                    bookingFromDb.PaymentDate = DateTime.Now;
                    bookingFromDb.IsPaymentSuccessful = true;
                }
            }
            _unitOfWork.Save();
        }

        public void UpdateStatus(int bookingId, string bookingStatus, int villaNumber = 0)
        {
            var bookingFromDb = _unitOfWork.Booking.Get(x => x.Id == bookingId, tracked: true);
            if (bookingFromDb != null)
            {
                bookingFromDb.Status = bookingStatus;
                if (bookingStatus == SD.StatusCheckedIn)
                {
                    bookingFromDb.VillaNumber = villaNumber;
                    bookingFromDb.ActualCheckInDate = DateTime.Now;
                }
                if (bookingStatus == SD.StatusCompleted)
                {
                    bookingFromDb.ActualCheckOutDate = DateTime.Now;
                }
            }
            _unitOfWork.Save();
        }
    }
}
