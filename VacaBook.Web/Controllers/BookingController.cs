﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;
using VacaBook.Application.Common.Interfaces;
using VacaBook.Application.Common.Utility;
using VacaBook.Domain.Entities;

namespace VacaBook.Web.Controllers
{
    public class BookingController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public BookingController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult FinalizeBooking(int villaId, DateOnly checkInDate, int nights)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ApplicationUser user = _unitOfWork.User.Get(x => x.Id == userId);

            Booking booking = new()
            {
                VillaId = villaId,
                Villa = _unitOfWork.Villa.Get(x => x.Id == villaId, includeProperties: "VillaAmenity"),
                CheckInDate = checkInDate,
                Nights = nights,
                CheckOutDate = checkInDate.AddDays(nights),
                UserId = userId,
                Phone = user.PhoneNumber,
                Email = user.Email,
                Name = user.Name
            };
            booking.TotalCost = booking.Villa.Price * nights;
            return View(booking);
        }

        [Authorize]
        [HttpPost]
        public IActionResult FinalizeBooking(Booking booking)
        {
            var villa = _unitOfWork.Villa.Get(x => x.Id == booking.VillaId);
            booking.TotalCost = villa.Price * booking.Nights;

            booking.Status = SD.StatusPending;
            booking.BookingDate = DateTime.Now;

            var villaNumbersList = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = _unitOfWork.Booking.GetAll(x => x.Status == SD.StatusApproved || x.Status == SD.StatusCheckedIn).ToList();

            int roomAvailable = SD.VillaRoomsAvailable_Count(villa.Id, villaNumbersList, booking.CheckInDate, booking.Nights, bookedVillas);

            if (roomAvailable == 0)
            {
                TempData["error"] = "Room has been sold out";
                return RedirectToAction(nameof(FinalizeBooking), new
                {
                    villaId = booking.VillaId,
                    checkInDtae = booking.CheckInDate,
                    nights = booking.Nights
                });
            }

            _unitOfWork.Booking.Add(booking);
            _unitOfWork.Save();

            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                Currency = "aed",
                BillingAddressCollection = "required",
                SuccessUrl = domain + $"booking/BookingConfirmation?bookingId={booking.Id}",
                CancelUrl = domain + $"booking/FinalizeBooking?villaId={booking.VillaId}&checkInDate={booking.CheckInDate}&nights={booking.Nights}",
            };

            options.LineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(booking.TotalCost * 100),
                    Currency = "aed",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = villa.Name,
                        //Images = new List<string> { domain + villa.ImageUrl }
                    }
                },
                Quantity = 1
            });

            var service = new SessionService();
            Session session = service.Create(options);

            _unitOfWork.Booking.UpdatePaymentId(booking.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        [Authorize]
        public IActionResult BookingConfirmation(int bookingId)
        {
            Booking bookingFromDb = _unitOfWork.Booking.Get(x => x.Id == bookingId, includeProperties: "User,Villa");

            if (bookingFromDb.Status == SD.StatusPending)
            {
                //this is a pending order
                var service = new SessionService();
                Session session = service.Get(bookingFromDb.SessionId);

                if (session.PaymentStatus == "paid")
                {
                    _unitOfWork.Booking.UpdateStatus(bookingFromDb.Id, SD.StatusApproved, 0);
                    _unitOfWork.Booking.UpdatePaymentId(bookingFromDb.Id, session.Id, session.PaymentIntentId);
                    _unitOfWork.Save();
                }
            }

            return View(bookingId);
        }

        [Authorize]
        public IActionResult BookingDetails(int bookingId)
        {
            Booking bookingFromDb = _unitOfWork.Booking.Get(x => x.Id == bookingId, includeProperties: "User,Villa");

            if (bookingFromDb.VillaNumber == 0 && bookingFromDb.Status == SD.StatusApproved)
            {
                var availableVillaNumber = AssignAvailableVillaNumberByVilla(bookingFromDb.VillaId);

                bookingFromDb.VillaNumbers = _unitOfWork.VillaNumber.GetAll(x => x.VillaId == bookingFromDb.VillaId
                && availableVillaNumber.Any(u => u == x.Villa_Number)).ToList();
            }

            return View(bookingFromDb);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckIn(Booking booking)
        {
            _unitOfWork.Booking.UpdateStatus(booking.Id, SD.StatusCheckedIn, booking.VillaNumber);
            _unitOfWork.Save();
            TempData["success"] = "Booking Updated Successfully";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckOut(Booking booking)
        {
            _unitOfWork.Booking.UpdateStatus(booking.Id, SD.StatusCompleted, booking.VillaNumber);
            _unitOfWork.Save();
            TempData["success"] = "Booking Completed Successfully";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CancelBooking(Booking booking)
        {
            _unitOfWork.Booking.UpdateStatus(booking.Id, SD.StatusCancelled, 0);
            _unitOfWork.Save();
            TempData["success"] = "Booking Cancelled Successfully";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        private List<int> AssignAvailableVillaNumberByVilla(int villaId)
        {
            List<int> availableVillaNumbers = new();

            var villaNumbers = _unitOfWork.VillaNumber.GetAll(x => x.VillaId == villaId);

            var checkedInVilla = _unitOfWork.Booking.GetAll(x => x.VillaId == villaId && x.Status == SD.StatusCheckedIn).Select(x => x.VillaNumber);

            foreach (var villaNumber in villaNumbers)
            {
                if (!checkedInVilla.Contains(villaNumber.Villa_Number))
                {
                    availableVillaNumbers.Add(villaNumber.Villa_Number);
                }
            }

            return availableVillaNumbers;
        }

        #region API Calls
        [HttpGet]
        [Authorize]
        public IActionResult GetAll(string status)
        {
            IEnumerable<Booking> objBookings;

            if (User.IsInRole(SD.Role_Admin))
            {
                objBookings = _unitOfWork.Booking.GetAll(includeProperties: "User,Villa");
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                objBookings = _unitOfWork.Booking.GetAll(x => x.UserId == userId, includeProperties: "User,Villa");
            }

            if (!string.IsNullOrEmpty(status))
            {
                objBookings = objBookings.Where(x => x.Status.ToLower().Equals(status.ToLower()));
            }
            return Json(new { data = objBookings });
        }

        #endregion
    }
}
