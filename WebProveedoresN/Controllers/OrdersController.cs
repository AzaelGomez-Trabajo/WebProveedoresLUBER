using Microsoft.AspNetCore.Mvc;
using WebProveedoresN.Models;
using WebProveedoresN.Services;

namespace WebProveedoresN.Controllers
{
    public class OrdersController : Controller
    {
        //GET: Orders/Index
        public ActionResult ListOrders(string searchString, int pageNumber = 1)
        {
            var supplierName = User.FindFirst("SupplierName")?.Value;
            ViewBag.Empresa = supplierName;
            var orders = OrderService.GetOrders(ViewBag.Empresa, 1, 0);
            if (!string.IsNullOrEmpty(searchString))
            {
                orders = ((IEnumerable<OrderDTO>)orders).Where(o => o.OrderNumber.Contains(searchString)).ToList();
            }

            int pageSize = 10;
            int totalRecords = orders.Count;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            ViewBag.TotalPages = totalPages;
            ViewBag.PageNumber = pageNumber;
            ViewBag.SearchString = searchString;
            return View(Pagination<OrderDTO>.CreatePagination(orders, pageNumber, pageSize));
        }
    }
}