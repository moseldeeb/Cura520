using Ecommerce.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using System.Xml.Schema;

namespace Ecommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE} ,{SD.ADMIN_ROLE} ,{SD.EMPLOYEE_ROLE} ")]


    public class ProductController : Controller
    {
        ApplicationDbContext _context;// = new ApplicationDbContext();
        IProductRepository _productRepository;// = new ProductRepository();
        IRepository<Category> _categoryRepository;// = new Repository<Category>();
        IRepository<Brand> _brandRepository;// = new Repository<Brand>();
        IRepository<ProductSubImage> _productSubImageRepository;// = new Repository<ProductSubImage>(); 
        IRepository<ProductColor> _productColorRepository;// = new Repository<ProductColor>(); 

        public ProductController(ApplicationDbContext context, IProductRepository productRepository, IRepository<Category> categoryRepository, IRepository<Brand> brandRepository, IRepository<ProductSubImage> productSubImageRepository, IRepository<ProductColor> productColorRepository)
        {
            _context = context;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _productSubImageRepository = productSubImageRepository;
            _productColorRepository = productColorRepository;
        }

        public async Task<IActionResult> Index()
        {
            //var products = _context.Products.Include(p=>p.Category).Include(p=>p.Brand).AsQueryable();
            var products = await _productRepository.GetAsync(includes: [p => p.Category , p => p.Brand]);
            return View(products.AsEnumerable());
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View( new ProductVM()
            {
                //Categories = _context.Categories.ToList(),
                Categories = await _categoryRepository.GetAsync(),
                //Brands = _context.Brands.ToList()
                Brands = await _brandRepository.GetAsync()
            } );
        }
        [HttpPost]
        public async Task<IActionResult> Create(Product product ,IFormFile img , List<IFormFile> SubImages , List<string> Colors)
        {
            if (img is not null )
            {
                if(img.Length >0 )
                { 
                    //var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                    var fileName = Guid.NewGuid().ToString() + "-" + img.FileName;
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images\\", fileName);
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        img.CopyTo(stream);
                    }
                    product.MainImg = fileName;
                }
            }
            //var AddedProduct = _context.Products.Add(product);
            var AddedProduct = await _productRepository.AddAsync(product);
            //_context.SaveChanges();
            await _productRepository.CommitAsync(); 
            if (SubImages is not null)
            {
                foreach(var item in SubImages)
                {
                    if (item.Length > 0)
                    {
                        //var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                        var SubImageName = Guid.NewGuid().ToString() + "-" + item.FileName;
                        var SubImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images\\product_sub_images", SubImageName);
                        using (var stream = System.IO.File.Create(SubImagePath))
                        {
                            item.CopyTo(stream);
                        }
                        var productSubImage = new ProductSubImage()
                        {
                            Img = SubImageName,
                            ProductId = AddedProduct.Entity.Id
                        };  
                        //_context.ProductSubImages.Add(productSubImage);
                        await _productSubImageRepository.AddAsync(productSubImage);
                        //_context.SaveChanges();
                        await _productSubImageRepository.CommitAsync();
                    }
                }
                
            }
            if (Colors is not null && Colors.Count > 0 )
            {
                Colors = Colors.Distinct().ToList(); 
                foreach (var item in Colors)
                {
                    var productColor = new ProductColor()
                    {
                        Color = item,
                        ProductId = AddedProduct.Entity.Id
                    };
                    //_context.ProductColors.Add(productColor);
                    await _productColorRepository.AddAsync(productColor);
                }
                //_context.SaveChanges();
                await _productColorRepository.CommitAsync();

            }

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            //var product = _context.Products.Include(p=>p.ProductColors).Include(p=>p.ProductSubImages).FirstOrDefault(c=>c.Id == id);
            var product = await _productRepository.GetOneAsync(c => c.Id == id , includes: [p => p.ProductColors , p => p.ProductSubImages]);

            if (product is null)
                return RedirectToAction("NotFoundPage" , "Home" );
            //return View(product);
            return View(new ProductVM()
            {
                //Categories = _context.Categories.ToList(),
                Categories = await _categoryRepository.GetAsync(),

                //Brands = _context.Brands.ToList() , 
                Brands = await _brandRepository.GetAsync(),
                Product = product
            });
        }
        [HttpPost]
        public async Task<IActionResult> Update(Product product, IFormFile img  ,List<IFormFile> SubImages , List<string> Colors)
        {
            //var productInDB = _context.Products.AsNoTracking().FirstOrDefault(c => c.Id == product.Id);
            var productInDB = await _productRepository.GetOneAsync(c => c.Id == product.Id , trackd: false);

            //var productColorsDB = _context.ProductColors.Where(p => p.ProductId == product.Id); 
            var productColorsDB = await _productColorRepository.GetAsync(p => p.ProductId == product.Id); 
            if (img is not null) {
                if (img.Length > 0)
                {
                    //var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                    var fileName = Guid.NewGuid().ToString() + "-" + img.FileName;
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images\\", fileName);
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        img.CopyTo(stream);
                    }
                    product.MainImg = fileName;
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images\\", productInDB.MainImg);

                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }
            }
            else
            {
                product.MainImg = productInDB.MainImg;
            }

            if (SubImages is not null)
            {
                foreach (var item in SubImages)
                {
                    if (item.Length > 0)
                    {
                        //var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                        var SubImageName = Guid.NewGuid().ToString() + "-" + item.FileName;
                        var SubImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images\\product_sub_images", SubImageName);
                        using (var stream = System.IO.File.Create(SubImagePath))
                        {
                            item.CopyTo(stream);
                        }
                        var productSubImage = new ProductSubImage()
                        {
                            Img = SubImageName,
                            ProductId = product.Id
                        };
                        //_context.ProductSubImages.Add(productSubImage);
                        await _productSubImageRepository.AddAsync(productSubImage);
                        //_context.SaveChanges();
                        await _productSubImageRepository.CommitAsync();

                    }
                }

            }

            if (Colors is not null && Colors.Count > 0)
            {
                foreach(var item in productColorsDB)
                {
                    //_context.ProductColors.Remove(item);
                    _productColorRepository.Delete(item);   
                }
                Colors = Colors.Distinct().ToList();
                foreach (var item in Colors)
                {
                    var productColor = new ProductColor()
                    {
                        Color = item,
                        ProductId = product.Id
                    };
                    //_context.ProductColors.Add(productColor);
                    await _productColorRepository.AddAsync(productColor);
                }
                //_context.SaveChanges();
                await _productColorRepository.CommitAsync();

            }
            //_context.Products.Update(product);
            _productRepository.Update(product);
            //_context.SaveChanges();
            await _productRepository.CommitAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int id)
        {
            //var product = _context.Products.Include(p=>p.ProductSubImages).FirstOrDefault(c => c.Id == id);
            var product = await _productRepository.GetOneAsync(c => c.Id == id , includes: [ p => p.ProductSubImages ] );
            if (product is null)
                return RedirectToAction("NotFoundPage", "Home");

            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images\\", product.MainImg);

            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }
            if (product.ProductSubImages.Count > 0 )
            {
                foreach (var img in product.ProductSubImages)
                {
                    var subImgPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images\\product_sub_images", img.Img);
                    if (System.IO.File.Exists(subImgPath))
                    {
                        System.IO.File.Delete(subImgPath);
                    }
                }
            }
            //_context.Products.Remove(product);
            _productRepository.Delete(product);
            //_context.SaveChanges();
            await _productRepository.CommitAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task< IActionResult> DeleteSubImage(int productId , string img)
        {
            //var productSubImage = _context.ProductSubImages.FirstOrDefault(p => p.ProductId == productId && p.Img == img);
            var productSubImage = await _productSubImageRepository.GetOneAsync(p => p.ProductId == productId && p.Img == img);

            if (productSubImage is null)
                return RedirectToAction("NotFoundPage", "Home");
            //_context.ProductSubImages.Remove(productSubImage);
            _productSubImageRepository.Delete(productSubImage);
            await _productSubImageRepository.CommitAsync();
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images\\product_sub_images", productSubImage.Img);
            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }

            //_context.SaveChanges();
            await _productRepository.CommitAsync();



            return RedirectToAction(nameof(Update) , new { id = productId } );

        }
    }

}


