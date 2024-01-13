using Microsoft.AspNetCore.Mvc;
using VacaBook.Domain.Entities;
using VacaBook.Infrastructure.Data;

namespace VacaBook.Web.Controllers
{
    public class VillaController : Controller
    {
        private readonly ApplicationDbContext _DbContext;

        public VillaController(ApplicationDbContext context)
        {
            _DbContext = context;
        }
        public IActionResult Index()
        {
            var villas = _DbContext.Villas.ToList();
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
                _DbContext.Villas.Add(villa);
                _DbContext.SaveChanges();
                TempData["success"] = "The villa has been created successfully";
                return RedirectToAction("Index");
            }
            return View(villa);
        }

        public IActionResult Update(int villaId)
        {
            var villa = _DbContext.Villas.FirstOrDefault(x => x.Id == villaId);
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
                _DbContext.Villas.Update(villa);
                _DbContext.SaveChanges();
                TempData["success"] = "The villa has been updated successfully";
                return RedirectToAction("Index");
            }
            return View(villa);
        }

        public IActionResult Delete(int villaId)
        {
            var villa = _DbContext.Villas.FirstOrDefault(x => x.Id == villaId);
            if (villa is null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(villa);
        }

        [HttpPost]
        public IActionResult Delete(Villa villa)
        {
            var villaDetails = _DbContext.Villas.FirstOrDefault(x => x.Id == villa.Id);
            if (villaDetails is not null)
            {
                _DbContext.Villas.Remove(villaDetails);
                _DbContext.SaveChanges();
                TempData["success"] = "The villa has been deleted successfully";
                return RedirectToAction("Index");
            }
            TempData["error"] = "The villa could not be deleted";
            return RedirectToAction("Error", "Home");
        }
    }
}
