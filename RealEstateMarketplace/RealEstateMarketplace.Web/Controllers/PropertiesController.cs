using Microsoft.AspNetCore.Mvc;
using RealEstateMarketplace.BLL.DTOs;
using RealEstateMarketplace.BLL.Services;

namespace RealEstateMarketplace.Web.Controllers;

public class PropertiesController : Controller
{
    private readonly IPropertyService _propertyService;
    private readonly ICategoryService _categoryService;
    private readonly IAmenityService _amenityService;

    public PropertiesController(
        IPropertyService propertyService, 
        ICategoryService categoryService,
        IAmenityService amenityService)
    {
        _propertyService = propertyService;
        _categoryService = categoryService;
        _amenityService = amenityService;
    }

    public async Task<IActionResult> Index(PropertySearchDto? search)
    {
        IEnumerable<PropertyDto> properties;
        
        if (search != null && HasSearchCriteria(search))
        {
            properties = await _propertyService.SearchPropertiesAsync(search);
        }
        else
        {
            properties = await _propertyService.GetAllPropertiesAsync();
            properties = properties.Where(p => p.Status == "Active");
        }
        
        ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
        ViewBag.Search = search ?? new PropertySearchDto();
        
        return View(properties);
    }

    public async Task<IActionResult> Details(int id)
    {
        var property = await _propertyService.GetPropertyByIdAsync(id);
        if (property == null)
        {
            return NotFound();
        }
        
        // Get related properties (same city or type)
        var search = new PropertySearchDto { City = property.City };
        var relatedProperties = (await _propertyService.SearchPropertiesAsync(search))
            .Where(p => p.Id != id)
            .Take(3);
        
        ViewBag.RelatedProperties = relatedProperties;
        
        return View(property);
    }
    
    private bool HasSearchCriteria(PropertySearchDto search)
    {
        return !string.IsNullOrEmpty(search.Keyword) ||
               !string.IsNullOrEmpty(search.PropertyType) ||
               !string.IsNullOrEmpty(search.ListingType) ||
               !string.IsNullOrEmpty(search.City) ||
               search.MinPrice.HasValue ||
               search.MaxPrice.HasValue ||
               search.MinBedrooms.HasValue ||
               search.MaxBedrooms.HasValue;
    }
}
