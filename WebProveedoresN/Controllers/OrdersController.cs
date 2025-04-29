using Microsoft.AspNetCore.Mvc;
using WebProveedoresN.DTOs;
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

        public IActionResult ListOrders()
        {
            return View();
        }

        //GET: Orders/Index
        [HttpGet]
        public async Task<ActionResult> ListOrders(string searchString = null!, int pageNumber = 1)
        {
            int pageSize = 10;
            var supplierCode = User.FindFirst("SupplierCode")!.Value;
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
        [HttpGet("Details")]
        public IActionResult Details()
        {
            return View();
        }

        [HttpPost("DetailsOrder")]
        public async Task<ActionResult> DetailsOrder(OrderDetailsDTO orderDetailsDTO)
        {
            if (orderDetailsDTO == null || orderDetailsDTO.OrderNumber == 0)
            {
                return BadRequest("El número de orden no puede estar vacío.");
            }
            ViewBag.OrderNumber = orderDetailsDTO.OrderNumber;
            ViewBag.SupplierCode = orderDetailsDTO.SupplierCode;
            ViewBag.DocumentType = orderDetailsDTO.DocumentType;

            var orderDetailDTO = new OrderDetailDTO
            {
                Action = 3,
                OrderNumber = orderDetailsDTO.OrderNumber,
                SupplierCode = orderDetailsDTO.SupplierCode,
                DocumentType = orderDetailsDTO.DocumentType
            };
            if (orderDetailDTO is null)
            {
                return BadRequest("El número de orden no puede estar vacío.");
            }
            var orderDetails = await _orderService.GetOrderDetailsByOrderNumberAsync(orderDetailDTO);
            if (orderDetails == null)
            {
                return NotFound("No se encontró la Orden de Compra especificada.");
            }

            var orders = await _orderService.GetOrderByOrderNumberAsync(orderDetailsDTO.OrderNumber);

            if (orders != null)
            {
                ViewBag.DocumentType = orders.DocumentType;
                ViewBag.Property = orders.Property;
                ViewBag.TotalAmount = orders.TotalAmount;
                ViewBag.OrderNumber = orders.OrderNumber;
                ViewBag.Currency = orders.DocCurOrder;
                ViewBag.OrderDate = orders.OrderDate;
                ViewBag.Invoices = orders.Invoices;
                ViewBag.TotalInvoices = orders.TotalInvoice;
            }

            //var order = orders.FirstOrDefault();
            return View(orderDetails);
        }

        [HttpPost("CloseEmbed")]
        public IActionResult CloseEmbed()
        {
            return Json(new { success = true });
        }
    }
}