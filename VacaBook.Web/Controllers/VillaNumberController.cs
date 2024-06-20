using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VacaBook.Application.Common.Interfaces;
using VacaBook.Application.Services.Implementation;
using VacaBook.Application.Services.Interface;
using VacaBook.Domain.Entities;
using VacaBook.Infrastructure.Data;
using VacaBook.Web.ViewModels;

namespace VacaBook.Web.Controllers
{
    public class VillaNumberController : Controller
    {
        private readonly IVillaService _villaService;
        private readonly IVillaNumberService _villaNumberService;

        public VillaNumberController(IVillaService villaService, IVillaNumberService villaNumberService)
        {
            _villaService = villaService;
            _villaNumberService = villaNumberService;
        }

        public IActionResult Index()
        {
            var villaNumbers = _villaNumberService.GetAllVillaNumbers();
            return View(villaNumbers);
        }

        public IActionResult Create()
        {
            VillaNumberViewModel villaNumberViewModel = new()
            {
                VillaList = _villaService.GetAllVillas().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
            };

            return View(villaNumberViewModel);
        }

        [HttpPost]
        public IActionResult Create(VillaNumberViewModel villaNumberVM)
        {
            var isVillaNumberExists = _villaNumberService.CheckVillaNumberExists(villaNumberVM.VillaNumber.Villa_Number);

            if (ModelState.IsValid && !isVillaNumberExists)
            {
                _villaNumberService.CreateVillaNumber(villaNumberVM.VillaNumber);
                TempData["success"] = "The villa number has been created successfully";
                return RedirectToAction(nameof(Index));
            }
            if (isVillaNumberExists)
            {
                TempData["error"] = "The villa number already exists";
            }

            villaNumberVM.VillaList = _villaService.GetAllVillas().Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });

            return View(villaNumberVM);
        }

        public IActionResult Update(int villaNumberId)
        {
            VillaNumberViewModel villaNumberViewModel = new()
            {
                VillaList = _villaService.GetAllVillas().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }),
                VillaNumber = _villaNumberService.GetVillaNumberById(villaNumberId)
            };

            if (villaNumberViewModel.VillaNumber == null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(villaNumberViewModel);
        }

        [HttpPost]
        public IActionResult Update(VillaNumberViewModel villaNumberVM)
        {
            if (ModelState.IsValid)
            {
                _villaNumberService.UpdateVillaNumber(villaNumberVM.VillaNumber);
                TempData["success"] = "The villa number has been updated successfully";
                return RedirectToAction(nameof(Index));
            }

            villaNumberVM.VillaList = _villaService.GetAllVillas().Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });

            return View(villaNumberVM);
        }

        public IActionResult Delete(int villaNumberId)
        {
            VillaNumberViewModel villaNumberViewModel = new()
            {
                VillaList = _villaService.GetAllVillas().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }),
                VillaNumber = _villaNumberService.GetVillaNumberById(villaNumberId)
            };

            if (villaNumberViewModel.VillaNumber == null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(villaNumberViewModel);
        }

        [HttpPost]
        public IActionResult Delete(VillaNumberViewModel villaNumberVM)
        {
            var villaNumberDetails = _villaNumberService.GetVillaNumberById(villaNumberVM.VillaNumber.Villa_Number);
            if (villaNumberDetails is not null)
            {
                _villaNumberService.DeleteVillaNumber(villaNumberDetails.Villa_Number);
                TempData["success"] = "The villa number has been deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "The villa number could not be deleted";
            return RedirectToAction("Error", "Home");
        }
    }
}
