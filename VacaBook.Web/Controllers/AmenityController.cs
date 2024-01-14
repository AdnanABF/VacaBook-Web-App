using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VacaBook.Application.Common.Interfaces;
using VacaBook.Application.Common.Utility;
using VacaBook.Domain.Entities;
using VacaBook.Infrastructure.Data;
using VacaBook.Web.ViewModels;

namespace VacaBook.Web.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class AmenityController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AmenityController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var amenities = _unitOfWork.Amenity.GetAll(includeProperties: "Villa");
            return View(amenities);
        }

        public IActionResult Create()
        {
            AmenityViewModel amenityViewModel = new()
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
            };

            return View(amenityViewModel);
        }

        [HttpPost]
        public IActionResult Create(AmenityViewModel amenityViewModel)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Amenity.Add(amenityViewModel.Amenity);
                _unitOfWork.Save();
                TempData["success"] = "The amenity has been created successfully";
                return RedirectToAction(nameof(Index));
            }

            amenityViewModel.VillaList = _unitOfWork.Villa.GetAll().Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });

            return View(amenityViewModel);
        }

        public IActionResult Update(int amenityId)
        {
            AmenityViewModel amenityViewModel = new()
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }),
                Amenity = _unitOfWork.Amenity.Get(x => x.Id == amenityId)
            };

            if (amenityViewModel.Amenity == null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(amenityViewModel);
        }

        [HttpPost]
        public IActionResult Update(AmenityViewModel amenityViewModel)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Amenity.Update(amenityViewModel.Amenity);
                _unitOfWork.Save();
                TempData["success"] = "The amenity has been updated successfully";
                return RedirectToAction(nameof(Index));
            }

            amenityViewModel.VillaList = _unitOfWork.Villa.GetAll().Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });

            return View(amenityViewModel);
        }

        public IActionResult Delete(int amenityId)
        {
            AmenityViewModel amenityViewModel = new()
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }),
                Amenity = _unitOfWork.Amenity.Get(x => x.Id == amenityId)
            };

            if (amenityViewModel.Amenity == null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(amenityViewModel);
        }

        [HttpPost]
        public IActionResult Delete(AmenityViewModel amenityViewModel)
        {
            var villaNumberDetails = _unitOfWork.Amenity.Get(x => x.Id == amenityViewModel.Amenity.Id);
            if (villaNumberDetails is not null)
            {
                _unitOfWork.Amenity.Remove(villaNumberDetails);
                _unitOfWork.Save();
                TempData["success"] = "The amenity has been deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "The amenity could not be deleted";
            return RedirectToAction("Error", "Home");
        }
    }
}
