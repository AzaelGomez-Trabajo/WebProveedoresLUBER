using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using WebProveedoresN.Entities;
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
        public async Task<ActionResult> ListOrders(string searchString = null!, int pageNumber = 1)
        {
            int pageSize = 10;
            var supplierCode = User.FindFirst("SupplierCode")!.Value;
            //ViewBag.Empresa = User.FindFirst("SupplierName")!.Value;
            ViewBag.SupplierCode = supplierCode;

            var orderDetailDTO = new OrderDetailDTO
            {
                Action = 1,
                OrderNumber = 0,
                SupplierCode = supplierCode,
                DocumentType = ""
            };

            var orders = await _orderService.GetOrdersAsync(orderDetailDTO, searchString, pageNumber, pageSize);
            if (!orders.Any() && pageNumber > 1)
            {
                return RedirectToAction("ListOrders", new { pageNumber = 1, searchString });
            }

            int totalRecords = await _orderService.GetTotalOrdersAsync(orderDetailDTO, searchString);
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            ViewBag.TotalPages = totalPages;
            ViewBag.PageNumber = pageNumber;
            ViewBag.SearchString = searchString;

            return View(PaginationDTO<Order>.CreatePagination(orders, pageNumber, pageSize));
        }

        //GET: Orders/Details/5
        [HttpGet]
        public async Task<ActionResult> Details(int orderNumber, string supplierCode, string documentType)
        {
            var orderDetailDTO = new OrderDetailDTO
            {
                Action = 3,
                OrderNumber = orderNumber,
                SupplierCode = supplierCode,
                DocumentType = documentType
            };
            if (orderDetailDTO is null)
            {
                return BadRequest("El número de orden no puede estar vacío.");
            }
            var orderDetails = await _orderService.GetOrderDetailsByOrderNumberAsync(orderDetailDTO);
            if (orderDetails == null)
            {
                return NotFound("No se encontró la orden especificada.");
            }

            var orders = await _orderService.GetOrderByOrderNumberAsync(orderNumber);

            if (orders != null)
            {
                ViewBag.DocumentType = orders.DocumentType;
                ViewBag.Property = orders.Property;
                ViewBag.TotalAmount = orders.TotalAmount;
                ViewBag.OrderNumber = orders.OrderNumber;
                ViewBag.Currency = orders.DocCurOrder;
                ViewBag.OrderDate = orders.OrderDate;
                ViewBag.Invoces = orders.Invoices;
                ViewBag.TotalInvoice = orders.TotalInvoice;
            }

            //var order = orders.FirstOrDefault();
            return View(orderDetails);
        }

        [HttpPost("CloseEmbed")]
        public IActionResult CloseEmbed(int orderNumber)
        {
            return Json(new { success = true });
        }

        //[HttpPost]
        //public IActionResult Details(Order order)
        //{
        //    return View(order);
        //}
    }
}