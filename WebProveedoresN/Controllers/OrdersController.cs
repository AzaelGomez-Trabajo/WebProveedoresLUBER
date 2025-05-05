using Microsoft.AspNetCore.Mvc;
using WebProveedoresN.DTOs;
using WebProveedoresN.Interfaces;
using WebProveedoresN.Models;
using WebProveedoresN.ViewModel;

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
            var orders = await _orderService.GetOrderByOrderNumberAsync(orderDetailsDTO.OrderNumber);
            if (orderDetailsDTO.DocumentType == "Pedido")
            {
                var orderInvoices = await _orderService.GetOrderInvoicesByOrderNumberAsync(orderDetailsDTO.OrderNumber);
                var orderDetails = await _orderService.GetOrderDetailsByOrderNumberAsync(orderDetailDTO);
                var orderDetailsGoodsReceipt = await _orderService.GetOrderDetailsGoodsReceiptByOrderNumberAsync(orderDetailDTO);
                var viewModel = new CombinedDetailsOrderViewModel
                {
                    Orders = orders,
                    OrderDetails = orderDetails,
                    OrderDetailsGoodsReceipt = orderDetailsGoodsReceipt,
                    OrderInvoices = orderInvoices,
                };
                if (viewModel == null)
                {
                    return NotFound("No se encontró la Orden de Compra especificada.");
                }
                return View("DetailsOrder", viewModel);
            }
            else
            {
                var orderDetailsOffer = await _orderService.GetOrderDetailsOfferByOrderNumberAsync(orderDetailDTO);
                var viewModel = new CombinedDetailsOrderOfferViewModel
                {
                    Orders = orders,
                    OrderDetailsOffer = orderDetailsOffer,
                };
                if (viewModel == null)
                {
                    return NotFound("No se encontró la Orden de Compra especificada.");
                }
                return View("OrderDetailsOffer", viewModel);
            }
        }

        [HttpPost("CloseEmbed")]
        public IActionResult CloseEmbed()
        {
            return Json(new { success = true });
        }
    }
}