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
            var supplierName = User.FindFirst("SupplierName")?.Value;
            ViewBag.Empresa = supplierName;

            var orders = await _orderService.GetOrdersAsync(supplierName, 1, 0, searchString, pageNumber, pageSize);
            if (!orders.Any() && pageNumber > 1)
            {
                return RedirectToAction("ListOrders", new { pageNumber = 1, searchString });
            }

            int totalRecords = await _orderService.GetTotalOrdersAsync(supplierName, 1, 0, searchString);
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            ViewBag.TotalPages = totalPages;
            ViewBag.PageNumber = pageNumber;
            ViewBag.SearchString = searchString;

            return View(Pagination<OrderDTO>.CreatePagination(orders, pageNumber, pageSize));
        }
    }
}