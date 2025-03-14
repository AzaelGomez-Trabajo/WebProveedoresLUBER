using Microsoft.AspNetCore.Mvc;
using WebProveedoresN.Data;
using WebProveedoresN.Models;
using WebProveedoresN.Services;

namespace WebProveedoresN.Controllers
{
    public class OrdersController : Controller
    {
        //GET: Orders/Index
        public ActionResult ListOrders()
        {
            var supplierName = User.FindFirst("SupplierName")?.Value;
            ViewBag.Empresa = supplierName;
            var orders = OrderService.GetOrders(ViewBag.Empresa);
            return View(orders);
        }

    }
}
