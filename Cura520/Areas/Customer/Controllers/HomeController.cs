using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Ecommerce.DataAccess;
using Ecommerce.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Areas.Customer.Controllers
{
    [Area("Customer")]
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

        public IActionResult Index(FilterProductVM filterVM , int page = 1 )
        {
            var products  = _context.Products.Include(p=> p.Category).AsQueryable();
            // add filter 
            if (filterVM.Name is not null )
            {
                products = products.Where(p=>p.Name.Contains(filterVM.Name));
                ViewBag.Name = filterVM.Name;
            }
            if (filterVM.MinPrice is not null )
            {
                products = products.Where(p => p.Price - p.Price * (p.Discount / 100) >= filterVM.MinPrice);
                ViewBag.MinPrice = filterVM.MinPrice;
            }
            if (filterVM.MaxPrice is not null)
            {
                products = products.Where(p => p.Price - p.Price * (p.Discount / 100) <= filterVM.MaxPrice);
                ViewBag.MaxPrice = filterVM.MaxPrice;
            }
            if (filterVM.CategoryId is not null)
            {
                products = products.Where(p => p.CategoryId == filterVM.CategoryId);
                ViewBag.CategoryId = filterVM.CategoryId;
            }
            if (filterVM.BrandId is not null)
            {
                products = products.Where(p => p.BrandId == filterVM.BrandId);
                ViewBag.BrandId = filterVM.BrandId;

            }
            if (filterVM.IsHot)
            {
                products = products.Where(p => p.Discount >= 50 );
                ViewBag.IsHot = filterVM.IsHot;
            }
            var categories  = _context.Categories.AsQueryable();
            //ViewData["Categories"] = categories.AsEnumerable();
            ViewBag.Categories = categories.AsEnumerable();

            var brands = _context.Brands.AsQueryable();
            ViewBag.Brands = brands.AsEnumerable();
            

            ViewBag.TotalPages = Math.Ceiling(products.Count() / 8.0);
            ViewBag.CurrentPage = page;

            products = products.Skip((page -1)*8).Take(8);

            return View(products.AsEnumerable());
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public ViewResult Welcome()
        {
            return View();
        }
        public ViewResult PersonalInfo(FilerPersonVM filter )
        {
            var persons = new List<Person>()
            {
                new Person() { Id = 1, Name = "bahaa ", Age = 30, Email = "Person@t.com" } ,
                new Person() { Id = 2, Name = "ahmed", Age = 30, Email = "Person@t.com" } ,
                new Person() { Id = 3, Name = "mona", Age = 30, Email = "Person@t.com" } ,
            };
            var personsDB = persons.AsQueryable();

            personsDB = personsDB.Where(p => p.Id == filter.Id && p.Name.Contains(filter.Name )); 

            var personVm = new PersonVM()
            {
                Persons = personsDB.ToList(),
                Count = personsDB.ToList().Count
            }; 
            return View(personVm);
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
