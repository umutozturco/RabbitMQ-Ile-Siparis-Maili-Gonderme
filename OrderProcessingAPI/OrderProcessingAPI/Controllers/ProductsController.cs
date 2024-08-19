using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using OrderProcessingAPI.Data;
using OrderProcessingAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderProcessingAPI.Controllers
{
    [Route("[controller]")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _memoryCache;

        public ProductsController(ApplicationDbContext context, IMapper mapper, IMemoryCache memoryCache)
        {
            _context = context;
            _mapper = mapper;
            _memoryCache = memoryCache;
        }

        [HttpGet("api")]
        public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetProducts([FromQuery] string category)
        {
            var cacheKey = string.IsNullOrEmpty(category) ? "all_products" : $"products_{category}";
            if (!_memoryCache.TryGetValue(cacheKey, out List<ProductDto> products))
            {
                var productList = string.IsNullOrEmpty(category)
                    ? _context.Products.ToList()
                    : _context.Products.Where(p => p.Category == category).ToList();

                products = _mapper.Map<List<ProductDto>>(productList);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                _memoryCache.Set(cacheKey, products, cacheOptions);
            }

            return Ok(new ApiResponse<List<ProductDto>>
            {
                Status = Status.Success,
                Data = products
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product)
        {
            if (product == null)
            {
                return BadRequest("Product data is null.");
            }

            if (ModelState.IsValid)
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                
            }

            return View("Index");
        }

        [HttpGet]
        public IActionResult Index()
        {
            var products = _context.Products.ToList();
            return View(products);
        }
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return PartialView("_CreateOrderPartial");
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(Order order)
        {
            if (ModelState.IsValid)
            {
                order.OrderDate = DateTime.Now;
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return PartialView("_CreateOrderPartial", order);
        }

        [HttpGet("Edit/{id}")]
        public IActionResult Edit(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return PartialView("_EditProductPartial", product);
        }

        [HttpPost("Edit")]
        public async Task<IActionResult> Edit(Product product)
        {
            if (product == null)
            {
                return BadRequest("Product data is null.");
            }

            if (ModelState.IsValid)
            {
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View("Index");
        }
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok();
        }
        [HttpGet("GetProductName/{id}")]
        public async Task<ActionResult<string>> GetProductName(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound("Ürün bulunamadı.");
            }

            return Ok(product.Description);
        }

    }
}
