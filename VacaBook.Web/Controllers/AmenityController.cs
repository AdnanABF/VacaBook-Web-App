using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VacaBook.Application.Common.Interfaces;
using VacaBook.Application.Common.Utility;
using VacaBook.Application.Services.Interface;
using VacaBook.Domain.Entities;
using VacaBook.Infrastructure.Data;
using VacaBook.Web.ViewModels;

namespace VacaBook.Web.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class AmenityController : Controller
    {
        private readonly IAmenityService _amenityService;
        private readonly IVillaService _villaService;

        public AmenityController(IAmenityService amenityService, IVillaService villaService)
        {
            _amenityService = amenityService;
            _villaService = villaService;
        }

        public IActionResult Index()
        {
            var amenities = _amenityService.GetAllAmenities();
            return View(amenities);
        }

        public IActionResult Create()
        {
            AmenityViewModel amenityViewModel = new()
            {
                VillaList = _villaService.GetAllVillas().Select(x => new SelectListItem
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
                _amenityService.CreateAmenity(amenityViewModel.Amenity);
                TempData["success"] = "The amenity has been created successfully";
                return RedirectToAction(nameof(Index));
            }

            amenityViewModel.VillaList = _villaService.GetAllVillas().Select(x => new SelectListItem
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
                VillaList = _villaService.GetAllVillas().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }),
                Amenity = _amenityService.GetAmenityById(amenityId)
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
                _amenityService.UpdateAmenity(amenityViewModel.Amenity);
                TempData["success"] = "The amenity has been updated successfully";
                return RedirectToAction(nameof(Index));
            }

            amenityViewModel.VillaList = _villaService.GetAllVillas().Select(x => new SelectListItem
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
                VillaList = _villaService.GetAllVillas().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }),
                Amenity = _amenityService.GetAmenityById(amenityId)
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
            var villaNumberDetails = _amenityService.GetAmenityById(amenityViewModel.Amenity.Id);
            if (villaNumberDetails is not null)
            {
                _amenityService.DeleteAmenity(villaNumberDetails.Id);
                TempData["success"] = "The amenity has been deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "The amenity could not be deleted";
            return RedirectToAction("Error", "Home");
        }
    }
}
