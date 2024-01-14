using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using VacaBook.Domain.Entities;

namespace VacaBook.Web.ViewModels
{
    public class AmenityViewModel
    {
        public Amenity? Amenity { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem>? VillaList { get; set; }
    }
}
