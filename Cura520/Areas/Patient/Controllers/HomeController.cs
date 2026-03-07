using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Cura520.DataAccess;
using Cura520.Models;
using Cura520.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cura520.Areas.Customer.Controllers
{
    [Area("Patient")]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private ApplicationDbContext _context;//= new ApplicationDbContext();
        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        //public IActionResult Index(FilterProductVM filterVM, int page = 1)
        //{
        //    var products = _context.Products.Include(p => p.Category).AsQueryable();
        //    // add filter 
        //    if (filterVM.Name is not null)
        //    {
        //        products = products.Where(p => p.Name.Contains(filterVM.Name));
        //        ViewBag.Name = filterVM.Name;
        //    }
        //    if (filterVM.MinPrice is not null)
        //    {
        //        products = products.Where(p => p.Price - p.Price * (p.Discount / 100) >= filterVM.MinPrice);
        //        ViewBag.MinPrice = filterVM.MinPrice;
        //    }
        //    if (filterVM.MaxPrice is not null)
        //    {
        //        products = products.Where(p => p.Price - p.Price * (p.Discount / 100) <= filterVM.MaxPrice);
        //        ViewBag.MaxPrice = filterVM.MaxPrice;
        //    }
        //    if (filterVM.CategoryId is not null)
        //    {
        //        products = products.Where(p => p.CategoryId == filterVM.CategoryId);
        //        ViewBag.CategoryId = filterVM.CategoryId;
        //    }
        //    if (filterVM.BrandId is not null)
        //    {
        //        products = products.Where(p => p.BrandId == filterVM.BrandId);
        //        ViewBag.BrandId = filterVM.BrandId;

        //    }
        //    if (filterVM.IsHot)
        //    {
        //        products = products.Where(p => p.Discount >= 50);
        //        ViewBag.IsHot = filterVM.IsHot;
        //    }
        //    var categories = _context.Categories.AsQueryable();
        //    //ViewData["Categories"] = categories.AsEnumerable();
        //    ViewBag.Categories = categories.AsEnumerable();

        //    var brands = _context.Brands.AsQueryable();
        //    ViewBag.Brands = brands.AsEnumerable();


        //    ViewBag.TotalPages = Math.Ceiling(products.Count() / 8.0);
        //    ViewBag.CurrentPage = page;

        //    products = products.Skip((page - 1) * 8).Take(8);

        //    return View(products.AsEnumerable());
        //}

        public IActionResult Privacy()
        {
            return View();
        }
        public ViewResult Welcome()
        {
            return View();
        }
        



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
