using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderProcessingAPI.Data;
using OrderProcessingAPI.Models;
using OrderProcessingAPI.Services;
using System.Linq;
using System.Threading.Tasks;

namespace OrderProcessingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly RabbitMQService _rabbitMQService;

        public OrdersApiController(ApplicationDbContext context, RabbitMQService rabbitMQService)
        {
            _context = context;
            _rabbitMQService = rabbitMQService;
        }

        
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound(new ApiResponse<string>
                {
                    Status = Status.Failed,
                    ResultMessage = "Order not found.",
                    ErrorCode = "404"
                });
            }

            return Ok(new ApiResponse<Order>
            {
                Status = Status.Success,
                ResultMessage = "Order retrieved successfully.",
                Data = order
            });
        }

        
        [HttpPost("edit")]
        public async Task<IActionResult> EditOrder([FromBody] Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Status = Status.Failed,
                    ResultMessage = "Invalid order data.",
                    ErrorCode = "400"
                });
            }

            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent(); // 204 No Content
        }

        // POST: api/orders/CreateOrder
        [HttpPost("CreateOrder")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Status = Status.Failed,
                    ResultMessage = "Invalid request data.",
                    ErrorCode = "400"
                });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {              
                var order = new Order
                {
                    CustomerName = request.CustomerName,
                    CustomerEmail = request.CustomerEmail,
                    CustomerGSM = request.CustomerGSM,
                    TotalAmount = request.ProductDetails.Sum(pd => pd.UnitPrice * pd.Amount),
                    OrderDate = DateTime.Now
                };

                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync();

             
                foreach (var productDetail in request.ProductDetails)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.Id, 
                        ProductId = productDetail.ProductId,
                        UnitPrice = productDetail.UnitPrice,
                        Amount = productDetail.Amount
                    };

                    await _context.OrderDetails.AddAsync(orderDetail);
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                await _rabbitMQService.SendMessageAsync(new EmailSendingService.EmailMessage
                {
                    CustomerName = order.CustomerName,
                    CustomerEmail = order.CustomerEmail,
                    Products = request.ProductDetails.Select(pd => new EmailSendingService.ProductDetail
                    {
                        Description = _context.Products.FirstOrDefault(p => p.Id == pd.ProductId)?.Description,
                        UnitPrice = pd.UnitPrice,
                        Quantity = pd.Amount
                    }).ToList(),
                    TotalAmount = order.TotalAmount
                });

                return Ok(new ApiResponse<int>
                {
                    Status = Status.Success,
                    ResultMessage = "Order created successfully.",
                    Data = order.Id
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new ApiResponse<string>
                {
                    Status = Status.Failed,
                    ResultMessage = "An error occurred while creating the order.",
                    ErrorCode = "500",
                    Data = ex.Message
                });
            }
        }

        [HttpGet("GetOrderDetails/{id}")]
        public async Task<IActionResult> GetOrderDetails(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound(new ApiResponse<string>
                {
                    Status = Status.Failed,
                    ResultMessage = "Order not found.",
                    ErrorCode = "404"
                });
            }

            return Ok(new ApiResponse<Order>
            {
                Status = Status.Success,
                ResultMessage = "Order details retrieved successfully.",
                Data = order
            });
        }
    }
}
