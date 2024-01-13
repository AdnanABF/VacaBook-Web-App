using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VacaBook.Domain.Entities;
using VacaBook.Infrastructure.Data;
using VacaBook.Web.ViewModels;

namespace VacaBook.Web.Controllers
{
    public class VillaNumberController : Controller
    {
        private readonly ApplicationDbContext _DbContext;

        public VillaNumberController(ApplicationDbContext context)
        {
            _DbContext = context;
        }
        public IActionResult Index()
        {
            var villaNumbers = _DbContext.VillaNumbers.Include(x => x.Villa).ToList();
            return View(villaNumbers);
        }

        public IActionResult Create()
        {
            VillaNumberViewModel villaNumberViewModel = new()
            {
                VillaList = _DbContext.Villas.ToList().Select(x => new SelectListItem
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
            var isVillaNumberExists = _DbContext.VillaNumbers.Any(x => x.Villa_Number == villaNumberVM.VillaNumber.Villa_Number);

            if (ModelState.IsValid && !isVillaNumberExists)
            {
                _DbContext.VillaNumbers.Add(villaNumberVM.VillaNumber);
                _DbContext.SaveChanges();
                TempData["success"] = "The villa number has been created successfully";
                return RedirectToAction(nameof(Index));
            }
            if (isVillaNumberExists)
            {
                TempData["error"] = "The villa number already exists";
            }

            villaNumberVM.VillaList = _DbContext.Villas.ToList().Select(x => new SelectListItem
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
                VillaList = _DbContext.Villas.ToList().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }),
                VillaNumber = _DbContext.VillaNumbers.FirstOrDefault(x=>x.Villa_Number == villaNumberId)
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
                _DbContext.VillaNumbers.Update(villaNumberVM.VillaNumber);
                _DbContext.SaveChanges();
                TempData["success"] = "The villa number has been updated successfully";
                return RedirectToAction(nameof(Index));
            }

            villaNumberVM.VillaList = _DbContext.Villas.ToList().Select(x => new SelectListItem
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
                VillaList = _DbContext.Villas.ToList().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }),
                VillaNumber = _DbContext.VillaNumbers.FirstOrDefault(x => x.Villa_Number == villaNumberId)
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
            var villaNumberDetails = _DbContext.VillaNumbers.FirstOrDefault(x => x.Villa_Number == villaNumberVM.VillaNumber.Villa_Number);
            if (villaNumberDetails is not null)
            {
                _DbContext.VillaNumbers.Remove(villaNumberDetails);
                _DbContext.SaveChanges();
                TempData["success"] = "The villa number has been deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "The villa number could not be deleted";
            return RedirectToAction("Error", "Home");
        }
    }
}
