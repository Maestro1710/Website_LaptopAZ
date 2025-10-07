using ClosedXML.Excel;
using ProTechTiveGear.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ProTechTiveGear.Services
{
    public class AdminService
    {
        private readonly ProTechTiveGearEntities db;

        public AdminService(ProTechTiveGearEntities context)
        {
            db = context;   
        }

        public Admin Login(string username, string password)
        {
            return db.Admins.SingleOrDefault(a => a.Username == username && a.Passwords == password);
        }

        public bool CreateAdmin(Admin admin)
        {
            var existing = db.Admins.SingleOrDefault(a => a.Username == admin.Username);
            if (existing != null) return false;

            db.Admins.Add(admin);
            db.SaveChanges();
            return true;
        }

        public IEnumerable<OrderEntity> GetAllOrders()
        {
            // Lấy toàn bộ Order từ DB
            var orders = db.Orders.ToList();

            // Chuyển từng Order thành OrderEntity
            var orderEntities = orders.Select(o => new OrderEntity(o)).ToList();

            return orderEntities;
        }


        public IEnumerable<OrderEntity> GetOrders()
        {
            var orders = db.Orders.Where(o => o.Status != 1).ToList();
            var orderEntities = orders.Select(o => new OrderEntity(o)).ToList();
            return orderEntities;
        }


        public IEnumerable<OrderDetailEntity> GetOrderDetails(long id)
        {
            return db.OrderDetails
                .Where(d => d.OrderID.HasValue && d.OrderID.Value == id)
                .ToList()
                .Select(d => new OrderDetailEntity(d))
                .ToList();
        }

        public bool ConfirmOrder(long id, int status)
        {
            var order = db.Orders.SingleOrDefault(o => o.ID == id);
            if (order == null) return false;

            order.Status = status;
            if (status == 1)
                order.Deliverydate = System.DateTime.Now;

            db.SaveChanges();
            return true;
        }

        // Nghiệp vụ lấy báo cáo số lượng bán trong tháng

        public byte[] ExportSoldOrdersToExcel(int month, int year)
        {
            // Lọc đơn hàng đã bán theo tháng và năm
            var soldOrders = db.Orders
                .Where(o => o.Status == 1 &&
                            o.Orderdate.HasValue &&
                            o.Orderdate.Value.Month == month &&
                            o.Orderdate.Value.Year == year)
                .ToList();

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add($"Đơn hàng {month}-{year}");

                // Tiêu đề cột
                ws.Cell(1, 1).Value = "Mã đơn hàng";
                ws.Cell(1, 2).Value = "Khách hàng";
                ws.Cell(1, 3).Value = "Ngày đặt";
                ws.Cell(1, 4).Value = "Ngày giao";
                ws.Cell(1, 5).Value = "Tổng tiền";

                int row = 2;
                foreach (var order in soldOrders)
                {
                    ws.Cell(row, 1).Value = order.ID;

                    ws.Cell(row, 2).Value = order.Customer != null
                        ? order.Customer.Name ?? order.Customer.Username
                        : "Khách vãng lai";

                    ws.Cell(row, 3).Value = order.Orderdate?.ToString("dd/MM/yyyy");
                    ws.Cell(row, 4).Value = order.Deliverydate?.ToString("dd/MM/yyyy");
                    ws.Cell(row, 5).Value = order.Totalprice ?? 0;

                    row++;
                }

                ws.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

    }

}
