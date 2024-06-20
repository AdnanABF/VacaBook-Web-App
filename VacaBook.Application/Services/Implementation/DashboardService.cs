using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VacaBook.Application.Common.Interfaces;
using VacaBook.Application.Common.Utility;
using VacaBook.Application.Services.Interface;
using VacaBook.Web.ViewModels;

namespace VacaBook.Application.Services.Implementation
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        static int previousMonth = DateTime.Now.Month == 1 ? 12 : DateTime.Now.Month - 1;
        private DateTime previousMonthStartDate = new(DateTime.Now.Year, previousMonth, 1);
        private DateTime currentMonthStartDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);

        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PieChartDTO> GetBookingPieChartData()
        {
            var totalBooking = _unitOfWork.Booking.GetAll(x => x.BookingDate >= DateTime.Now.AddDays(-30) && (x.Status != SD.StatusPending || x.Status == SD.StatusCancelled));

            var customerWithOneBooking = totalBooking.GroupBy(x => x.UserId).Where(x => x.Count() == 1).Select(x => x.Key).ToList();

            int bookingsByNewCustomer = customerWithOneBooking.Count();
            int bookingsByReturningCustomer = totalBooking.Count() - bookingsByNewCustomer;

            PieChartDTO PieChartDTO = new()
            {
                Labels = new string[] { "New Customer Bookings", "Returning Customer Bookings" },
                Series = new decimal[] { bookingsByNewCustomer, bookingsByReturningCustomer }
            };

            return PieChartDTO;
        }

        public async Task<LineChartDTO> GetMemberAndBookingLineChartData()
        {
            var bookingData = _unitOfWork.Booking.GetAll(x => x.BookingDate >= DateTime.Now.AddDays(-30) && x.BookingDate.Date <= DateTime.Now)
                            .GroupBy(x => x.BookingDate.Date)
                            .Select(u => new
                            {
                                DateTime = u.Key,
                                NewBookingCount = u.Count()
                            });

            var customerData = _unitOfWork.User.GetAll(x => x.CreatedAt >= DateTime.Now.AddDays(-30) && x.CreatedAt.Date <= DateTime.Now)
                .GroupBy(x => x.CreatedAt.Date)
                .Select(u => new
                {
                    DateTime = u.Key,
                    NewCustomerCount = u.Count()
                });

            var leftJoin = bookingData.GroupJoin(customerData, booking => booking.DateTime, customer => customer.DateTime,
                (booking, customer) => new
                {
                    booking.DateTime,
                    booking.NewBookingCount,
                    NewCustomerCount = customer.Select(x => x.NewCustomerCount).FirstOrDefault()
                });

            var rightJoin = customerData.GroupJoin(bookingData, customer => customer.DateTime, booking => booking.DateTime,
                (customer, booking) => new
                {
                    customer.DateTime,
                    NewBookingCount = booking.Select(x => x.NewBookingCount).FirstOrDefault(),
                    customer.NewCustomerCount,
                });

            var mergedData = leftJoin.Union(rightJoin).OrderBy(x => x.DateTime).ToList();

            var newBookingData = mergedData.Select(x => x.NewBookingCount).ToArray();
            var newCustomerData = mergedData.Select(x => x.NewCustomerCount).ToArray();
            var categories = mergedData.Select(x => x.DateTime.ToString("dd/MM/yyyy")).ToArray();

            List<ChartData> chartDataList = new()
            {
                new ChartData
                {
                    Name = "New Bookings",
                    Data = newBookingData
                },
                new ChartData
                {
                    Name = "New Members",
                    Data = newCustomerData
                }
            };

            LineChartDTO LineChartDTO = new()
            {
                Categories = categories,
                Series = chartDataList
            };

            return LineChartDTO;
        }

        public async Task<RadialBarChartDTO> GetRegisteredUserChartData()
        {
            var totalUsers = _unitOfWork.User.GetAll();

            var countByCurrentMonth = totalUsers.Count(x => x.CreatedAt >= currentMonthStartDate && x.CreatedAt <= DateTime.Now);

            var countByPreviousMonth = totalUsers.Count(x => x.CreatedAt >= previousMonthStartDate && x.CreatedAt <= currentMonthStartDate);

            return SD.GetRadiusChartDataModel(totalUsers.Count(), countByCurrentMonth, countByPreviousMonth);
        }

        public async Task<RadialBarChartDTO> GetRevenueChartData()
        {
            var totalBooking = _unitOfWork.Booking.GetAll(x => x.Status != SD.StatusPending || x.Status == SD.StatusCancelled);

            var totalRevenue = Convert.ToInt32(totalBooking.Sum(x => x.TotalCost));

            var countByCurrentMonth = totalBooking.Where(x => x.BookingDate >= currentMonthStartDate && x.BookingDate <= DateTime.Now).Sum(x => x.TotalCost);

            var countByPreviousMonth = totalBooking.Where(x => x.BookingDate >= previousMonthStartDate && x.BookingDate <= currentMonthStartDate).Sum(x => x.TotalCost);

            return SD.GetRadiusChartDataModel(totalRevenue, countByCurrentMonth, countByPreviousMonth);
        }

        public async Task<RadialBarChartDTO> GetTotalBookingRadialChartData()
        {
            var totalBooking = _unitOfWork.Booking.GetAll(x => x.Status != SD.StatusPending || x.Status == SD.StatusCancelled);

            var countByCurrentMonth = totalBooking.Count(x => x.BookingDate >= currentMonthStartDate && x.BookingDate <= DateTime.Now);

            var countByPreviousMonth = totalBooking.Count(x => x.BookingDate >= previousMonthStartDate && x.BookingDate <= currentMonthStartDate);

            return SD.GetRadiusChartDataModel(totalBooking.Count(), countByCurrentMonth, countByPreviousMonth);
        }
    }
}
