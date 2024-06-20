using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VacaBook.Application.Common.Interfaces;
using VacaBook.Application.Services.Interface;
using VacaBook.Domain.Entities;
using VacaBook.Infrastructure.Data;

namespace VacaBook.Web.Controllers
{
    [Authorize]
    public class VillaController : Controller
    {
        private readonly IVillaService _villaService;

        public VillaController(IVillaService villaService)
        {
            _villaService = villaService;
        }

        public IActionResult Index()
        {
            var villas = _villaService.GetAllVillas();
            return View(villas);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Villa villa)
        {
            if (villa.Name == villa.Description)
            {
                ModelState.AddModelError("description", "The description cannot exactly match the name");
            }
            if (ModelState.IsValid)
            {
                _villaService.CreateVilla(villa);
                TempData["success"] = "The villa has been created successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(villa);
        }

        public IActionResult Update(int villaId)
        {
            var villa = _villaService.GetVillaById(villaId);
            if (villa == null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(villa);
        }

        [HttpPost]
        public IActionResult Update(Villa villa)
        {
            if (ModelState.IsValid)
            {
                _villaService.UpdateVilla(villa);
                TempData["success"] = "The villa has been updated successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(villa);
        }

        public IActionResult Delete(int villaId)
        {
            var villa = _villaService.GetVillaById(villaId);
            if (villa is null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(villa);
        }

        [HttpPost]
        public IActionResult Delete(Villa villa)
        {
            bool deleted = _villaService.DeleteVilla(villa.Id);
            if (deleted)
            {
                TempData["success"] = "The villa has been deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["error"] = "Failed to delete villa";
            }
            return View();
        }
    }
}
