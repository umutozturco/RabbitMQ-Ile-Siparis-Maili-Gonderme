using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderProcessingAPI.Data;
using OrderProcessingAPI.Models;
using OrderProcessingAPI.Services;
using System.Threading.Tasks;

namespace OrderProcessingAPI.Controllers
{
    [Route("OrdersMvc")]
    public class OrdersMvcController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RabbitMQService _rabbitMQService;

        public OrdersMvcController(ApplicationDbContext context, RabbitMQService rabbitMQService)
        {
            _context = context;
            _rabbitMQService = rabbitMQService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ToListAsync();

            return View(orders);
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromForm] Order order)
        {
            if (!ModelState.IsValid)
            {
                return View(order);
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            
            bool result = await _rabbitMQService.SendMessageAsync(order);
            if (!result)
            {
               
            }

            return RedirectToAction("Index");
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, [FromForm] Order order)
        {
            if (id != order.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(order);
            }

            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
