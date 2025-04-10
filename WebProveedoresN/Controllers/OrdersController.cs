using Microsoft.AspNetCore.Mvc;
using WebProveedoresN.Interfaces;
using WebProveedoresN.Models;

namespace WebProveedoresN.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        //GET: Orders/Index
        public async Task<ActionResult> ListOrders(string searchString = null, int pageNumber = 1)
        {
            int pageSize = 10;
            var supplierCode = User.FindFirst("SupplierCode").Value;
            ViewBag.Empresa = User.FindFirst("SupplierName").Value;

            var orders = await _orderService.GetOrdersAsync(supplierCode, 1, 0, searchString, pageNumber, pageSize);
            if (!orders.Any() && pageNumber > 1)
            {
                return RedirectToAction("ListOrders", new { pageNumber = 1, searchString });
            }

            int totalRecords = await _orderService.GetTotalOrdersAsync(supplierCode, 1, 0, searchString);
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            ViewBag.TotalPages = totalPages;
            ViewBag.PageNumber = pageNumber;
            ViewBag.SearchString = searchString;

            return View(PaginationDTO<OrderDTO>.CreatePagination(orders, pageNumber, pageSize));
        }

        [HttpPost]
        public IActionResult UploadFile(int orderId, IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var filePath = Path.Combine("wwwroot/UploadedFiles", file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                // Lógica adicional para asociar el archivo con la orden
            }
            return RedirectToAction("Details", new { id = orderId });
        }

        [HttpGet]
        public IActionResult GetDocuments(int orderId)
        {
            var documents = new List<string>
            {
                "document1.pdf",
                "document2.pdf"
            };
            return Json(new { success = true, documents });
        }

        //GET: Orders/Details/5
        [HttpGet]
        public async Task<ActionResult> Details(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        [HttpPost]
        public IActionResult CloseEmbed(int orderId)
        {
            return Json(new { success = true });
        }


        [HttpPost]
        public IActionResult Details(OrderDTO order)
        {
            return View(order);
        }
    }
}