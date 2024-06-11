using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;
using System.Runtime;
using System.Security.AccessControl;
using System.Transactions;
using VacaBook.Application.Common.Interfaces;
using VacaBook.Application.Common.Utility;
using VacaBook.Web.ViewModels;

namespace VacaBook.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        static int previousMonth = DateTime.Now.Month == 1 ? 12 : DateTime.Now.Month - 1;
        private DateTime previousMonthStartDate = new(DateTime.Now.Year, previousMonth, 1);
        private DateTime currentMonthStartDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);

        public DashboardController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetTotalBookingRadialChartData()
        {
            var totalBooking = _unitOfWork.Booking.GetAll(x => x.Status != SD.StatusPending || x.Status == SD.StatusCancelled);

            var countByCurrentMonth = totalBooking.Count(x => x.BookingDate >= currentMonthStartDate && x.BookingDate <= DateTime.Now);

            var countByPreviousMonth = totalBooking.Count(x => x.BookingDate >= previousMonthStartDate && x.BookingDate <= currentMonthStartDate);

            return Json(GetRadiusChartDataModel(totalBooking.Count(), countByCurrentMonth, countByPreviousMonth));
        }

        public async Task<IActionResult> GetRegisteredUserChartData()
        {
            var totalUsers = _unitOfWork.User.GetAll();

            var countByCurrentMonth = totalUsers.Count(x => x.CreatedAt >= currentMonthStartDate && x.CreatedAt <= DateTime.Now);

            var countByPreviousMonth = totalUsers.Count(x => x.CreatedAt >= previousMonthStartDate && x.CreatedAt <= currentMonthStartDate);

            return Json(GetRadiusChartDataModel(totalUsers.Count(), countByCurrentMonth, countByPreviousMonth));
        }

        public async Task<IActionResult> GetRevenueChartData()
        {
            var totalBooking = _unitOfWork.Booking.GetAll(x => x.Status != SD.StatusPending || x.Status == SD.StatusCancelled);

            var totalRevenue = Convert.ToInt32(totalBooking.Sum(x => x.TotalCost));

            var countByCurrentMonth = totalBooking.Where(x => x.BookingDate >= currentMonthStartDate && x.BookingDate <= DateTime.Now).Sum(x => x.TotalCost);

            var countByPreviousMonth = totalBooking.Where(x => x.BookingDate >= previousMonthStartDate && x.BookingDate <= currentMonthStartDate).Sum(x => x.TotalCost);

            return Json(GetRadiusChartDataModel(totalRevenue, countByCurrentMonth, countByPreviousMonth));
        }

        public async Task<IActionResult> GetBookingPieChartData()
        {
            var totalBooking = _unitOfWork.Booking.GetAll(x => x.BookingDate >= DateTime.Now.AddDays(-30) && (x.Status != SD.StatusPending || x.Status == SD.StatusCancelled));

            var customerWithOneBooking = totalBooking.GroupBy(x => x.UserId).Where(x => x.Count() == 1).Select(x => x.Key).ToList();

            int bookingsByNewCustomer = customerWithOneBooking.Count();
            int bookingsByReturningCustomer = totalBooking.Count() - bookingsByNewCustomer;

            PieChartViewModel pieChartViewModel = new()
            {
                Labels = new string[] { "New Customer Bookings", "Returning Customer Bookings" },
                Series = new decimal[] { bookingsByNewCustomer, bookingsByReturningCustomer }
            };

            return Json(pieChartViewModel);
        }

        private static RadialBarChartViewModel GetRadiusChartDataModel(int totalCount, double currentMonthCount, double prevMonthCount)
        {
            RadialBarChartViewModel radialBarChartViewModel = new();

            int increaseDecreaseRatio = 100;

            if (prevMonthCount != 0)
            {
                increaseDecreaseRatio = Convert.ToInt32((currentMonthCount - prevMonthCount) / prevMonthCount * 100);
            }

            radialBarChartViewModel.TotalCount = totalCount;
            radialBarChartViewModel.CountInCurrentMonth = Convert.ToInt32(currentMonthCount);
            radialBarChartViewModel.HasRatioIncreased = currentMonthCount > prevMonthCount;
            radialBarChartViewModel.Series = new int[] { increaseDecreaseRatio };

            return radialBarChartViewModel;
        }
    }
}
