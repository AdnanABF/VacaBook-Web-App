using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VacaBook.Application.Common.Interfaces;
using VacaBook.Application.Common.Utility;
using VacaBook.Application.Services.Interface;
using VacaBook.Domain.Entities;

namespace VacaBook.Application.Services.Implementation
{
    public class VillaService : IVillaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public VillaService(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public void CreateVilla(Villa villa)
        {
            if (villa.Image != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"UploadImages\Villa");

                using var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create);
                villa.Image.CopyTo(fileStream);

                villa.ImageUrl = @"\UploadImages\Villa\" + fileName;
            }
            else
            {
                villa.ImageUrl = "https://placehold.co/600x400";
            }

            _unitOfWork.Villa.Add(villa);
            _unitOfWork.Save();
        }

        public bool DeleteVilla(int id)
        {
            try
            {
                var villaDetails = _unitOfWork.Villa.Get(x => x.Id == id);
                if (villaDetails is not null)
                {
                    if (!string.IsNullOrEmpty(villaDetails.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, villaDetails.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    _unitOfWork.Villa.Remove(villaDetails);
                    _unitOfWork.Save();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public IEnumerable<Villa> GetAllVillas()
        {
            return _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity");
        }

        public Villa GetVillaById(int id)
        {
            return _unitOfWork.Villa.Get(x => x.Id == id, includeProperties: "VillaAmenity");
        }

        public IEnumerable<Villa> GetVillasAvailabilityByDate(int nights, DateOnly checkInDate)
        {
            var villaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity").ToList();
            var villaNumbersList = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = _unitOfWork.Booking.GetAll(x => x.Status == SD.StatusApproved || x.Status == SD.StatusCheckedIn).ToList();


            foreach (var villa in villaList)
            {
                int roomsAvailable = SD.VillaRoomsAvailable_Count(villa.Id, villaNumbersList, checkInDate, nights, bookedVillas);

                villa.IsAvailable = roomsAvailable > 0 ? true : false;
            }

            return villaList;
        }

        public bool IsVillaAvailableByDate(int villaId, int nights, DateOnly checkInDate)
        {
            var villaNumbersList = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = _unitOfWork.Booking.GetAll(x => x.Status == SD.StatusApproved || x.Status == SD.StatusCheckedIn).ToList();

            int roomAvailable = SD.VillaRoomsAvailable_Count(villaId, villaNumbersList, checkInDate, nights, bookedVillas);

            return roomAvailable > 0;
        }

        public void UpdateVilla(Villa villa)
        {
            if (villa.Image != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"UploadImages\Villa");

                if (!string.IsNullOrEmpty(villa.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, villa.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                using var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create);
                villa.Image.CopyTo(fileStream);

                villa.ImageUrl = @"\UploadImages\Villa\" + fileName;
            }

            _unitOfWork.Villa.Update(villa);
            _unitOfWork.Save();
        }
    }
}
