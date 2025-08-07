using Assignment.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Assignment.Enum;
using Assignment.Utilities;
using Microsoft.AspNetCore.Authorization;

namespace Assignment.Controllers
{
    public class StatisticalController : Controller
    {
        private readonly ApplicationDbContext _context;
        public StatisticalController(ApplicationDbContext context)
        {
            _context = context;
        }
        [Route("statistical")]
        public IActionResult Index(string period = "month", string chartPeriod = "month", string categoryPeriod = "month", string productsPeriod = "month", string customersPeriod = "month")
        {
            var userId = CookieAuthHelper.GetUserId(HttpContext.User);
            var model = new StatisticalModel();

            try
            {
                var dateRange = GetDateRange(period);
                var chartDateRange = GetDateRangeForChart(chartPeriod);
                var categoryDateRange = GetDateRange(categoryPeriod);
                var productsDateRange = GetDateRange(productsPeriod);
                var customersDateRange = GetDateRange(customersPeriod);

                var ordersQuery = _context.Orders
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                            .ThenInclude(p => p.Category)
                    .Include(o => o.User)
                    .Where(o => o.CreatedTime >= dateRange.startDate &&
                               o.CreatedTime <= dateRange.endDate &&
                               o.Status == OrderStatus.Completed);

                var orders = ordersQuery.ToList();

                model.TotalRevenue = orders.Sum(o => o.TotalPrice + (o.Fee - o.FeeExcludingTax));
                model.TotalProducts = orders.SelectMany(o => o.OrderDetails).Sum(od => od.TotalQuantityPreItems);
                model.TotalOrders = orders.Count();

                var revenueChartData = new ChartData
                {
                    Labels = new List<string>(),
                    Data = new List<decimal>()
                };

                var chartOrdersQuery = _context.Orders
                    .Where(o => o.CreatedTime >= chartDateRange.startDate &&
                               o.CreatedTime <= chartDateRange.endDate &&
                               o.Status == OrderStatus.Completed);

                if (chartPeriod == "week")
                {
                    var today = DateTime.Now.Date;
                    for (int i = 6; i >= 0; i--)
                    {
                        var dayStart = today.AddDays(-i);
                        var dayEnd = dayStart.AddDays(1).AddTicks(-1);

                        var dayRevenue = chartOrdersQuery
                            .Where(o => o.CreatedTime >= dayStart && o.CreatedTime <= dayEnd)
                            .Sum(o => (decimal)(o.TotalPrice + (o.Fee - o.FeeExcludingTax)));

                        revenueChartData.Labels.Add(GetDayOfWeekVietnamese(dayStart.DayOfWeek));
                        revenueChartData.Data.Add(dayRevenue);
                    }
                }
                else if (chartPeriod == "month")
                {
                    var today = DateTime.Now.Date;
                    var startOfCurrentWeek = today.AddDays(-(int)today.DayOfWeek + 1);

                    for (int i = 3; i >= 0; i--)
                    {
                        var weekStart = startOfCurrentWeek.AddDays(-i * 7);
                        var weekEnd = weekStart.AddDays(6).AddDays(1).AddTicks(-1);

                        if (i == 0 && weekEnd > DateTime.Now)
                        {
                            weekEnd = DateTime.Now;
                        }

                        var weekRevenue = chartOrdersQuery
                            .Where(o => o.CreatedTime >= weekStart && o.CreatedTime <= weekEnd)
                            .Sum(o => (decimal)(o.TotalPrice + (o.Fee - o.FeeExcludingTax)));

                        revenueChartData.Labels.Add($"Tuần {4 - i}");
                        revenueChartData.Data.Add(weekRevenue);
                    }
                }
                else if (chartPeriod == "year")
                {
                    var currentYear = DateTime.Now.Year;
                    var currentMonth = DateTime.Now.Month;

                    for (int i = 1; i <= currentMonth; i++)
                    {
                        var monthStart = new DateTime(currentYear, i, 1);
                        var monthEnd = i == currentMonth ? DateTime.Now : monthStart.AddMonths(1).AddTicks(-1);

                        var monthRevenue = chartOrdersQuery
                            .Where(o => o.CreatedTime >= monthStart && o.CreatedTime <= monthEnd)
                            .Sum(o => (decimal)(o.TotalPrice + (o.Fee - o.FeeExcludingTax)));

                        revenueChartData.Labels.Add($"T{i}");
                        revenueChartData.Data.Add(monthRevenue);
                    }
                }

                model.RevenueChart = revenueChartData;

                var categoryOrdersQuery = _context.Orders
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                            .ThenInclude(p => p.Category)
                    .Where(o => o.CreatedTime >= categoryDateRange.startDate &&
                               o.CreatedTime <= categoryDateRange.endDate &&
                               o.Status == OrderStatus.Completed);

                var categoryData = categoryOrdersQuery
                    .SelectMany(o => o.OrderDetails)
                    .GroupBy(od => od.Product.Category.Name)
                    .Select(g => new
                    {
                        CategoryName = g.Key,
                        TotalQuantity = g.Sum(od => od.TotalQuantityPreItems)
                    })
                    .OrderByDescending(x => x.TotalQuantity)
                    .Take(10)
                    .ToList();

                model.CategoryChart = new ChartData
                {
                    Labels = categoryData.Select(x => x.CategoryName).ToList(),
                    Data = categoryData.Select(x => (decimal)x.TotalQuantity).ToList()
                };

                var productsOrdersQuery = _context.Orders
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                            .ThenInclude(p => p.Category)
                    .Where(o => o.CreatedTime >= productsDateRange.startDate &&
                               o.CreatedTime <= productsDateRange.endDate &&
                               o.Status == OrderStatus.Completed);

                var topProducts = productsOrdersQuery
                    .SelectMany(o => o.OrderDetails)
                    .GroupBy(od => new { ProductName = od.Product.Name, CategoryName = od.Product.Category.Name })
                    .Select(g => new TopProduct
                    {
                        Name = g.Key.ProductName,
                        Category = g.Key.CategoryName,
                        Quantity = g.Sum(od => od.TotalQuantityPreItems),
                        Revenue = g.Sum(od => od.TotalPricePreItems)
                    })
                    .OrderByDescending(x => x.Quantity)
                    .Take(10)
                    .ToList();

                model.TopProducts = topProducts;

                var customersOrdersQuery = _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderDetails)
                    .Where(o => o.CreatedTime >= customersDateRange.startDate &&
                               o.CreatedTime <= customersDateRange.endDate &&
                               o.Status == OrderStatus.Completed);

                var customerOrders = customersOrdersQuery
                    .Select(o => new
                    {
                        CustomerName = o.User != null ? o.User.Name : o.Name,
                        UserRank = o.User != null ? o.User.Rank : (UserRankEnum?)null,
                        Revenue = o.TotalPrice + (o.Fee - o.FeeExcludingTax)
                    })
                    .ToList();

                var topCustomers = customerOrders
                    .GroupBy(o => new {
                        CustomerName = o.CustomerName,
                        CustomerType = o.UserRank != null ? GetCustomerTypeVietnamese(o.UserRank.Value) : "Khách vãng lai"
                    })
                    .Select(g => new TopCustomer
                    {
                        Name = g.Key.CustomerName,
                        Type = g.Key.CustomerType,
                        Orders = g.Count(),
                        Revenue = g.Sum(o => o.Revenue)
                    })
                    .OrderByDescending(x => x.Orders)
                    .Take(10)
                    .ToList();

                model.TopCustomers = topCustomers;

            }
            catch (Exception ex)
            {
                model = new StatisticalModel
                {
                    TotalRevenue = 0,
                    TotalProducts = 0,
                    TotalOrders = 0,
                    RevenueChart = new ChartData { Labels = new List<string>(), Data = new List<decimal>() },
                    CategoryChart = new ChartData { Labels = new List<string>(), Data = new List<decimal>() },
                    TopProducts = new List<TopProduct>(),
                    TopCustomers = new List<TopCustomer>()
                };
            }

            ViewBag.SelectedPeriod = period;
            ViewBag.ChartPeriod = chartPeriod;
            ViewBag.CategoryPeriod = categoryPeriod;
            ViewBag.ProductsPeriod = productsPeriod;
            ViewBag.CustomersPeriod = customersPeriod;

            return View(model);
        }

        [HttpGet]
        [Route("statistical/shipper/{id:long?}")]
        [Authorize(Policy = "ShipperPolicy")]
        public IActionResult Shipper(long? id, string period = "month", string chartPeriod = "month")
        {
            var model = new ShipperStatisticalModel();

            try
            {
                long? shipperId = null;
                bool isAdminOverview = false;
                var role = CookieAuthHelper.GetRole(HttpContext.User);

                if (role == "Admin")
                {
                    var shippers = _context.Users
                        .Where(u => u.Role == "Shipper")
                        .Select(u => new { u.Id, u.Name })
                        .ToList();
                    ViewBag.Shippers = shippers;
                }

                if (role == "Admin" && !id.HasValue)
                {
                    isAdminOverview = true;
                }
                else if (id.HasValue)
                {
                    shipperId = id.Value;
                }
                else
                {
                    shipperId = CookieAuthHelper.GetUserId(HttpContext.User);
                }

                var dateRange = GetDateRange(period);
                var chartDateRange = GetDateRangeForChart(chartPeriod);

                if (!isAdminOverview)
                {
                    var shipper = _context.Users.FirstOrDefault(u => u.Id == shipperId);
                    if (shipper == null)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    model.ShipperName = shipper.Name;
                    model.ShipperId = shipperId;
                }
                else
                {
                    model.ShipperName = "Tổng quan tất cả shipper";
                    model.ShipperId = null;
                }

                var ordersQuery = _context.Orders
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .Where(o => o.CreatedTime >= dateRange.startDate && o.CreatedTime <= dateRange.endDate);

                if (!isAdminOverview)
                {
                    ordersQuery = ordersQuery.Where(o => o.ShipperId == shipperId);
                }

                var allOrders = ordersQuery.ToList();

                var deliveringOrders = allOrders.Where(o => o.Status == OrderStatus.Delivery).ToList();
                model.DeliveringOrders = deliveringOrders.Count();
                model.DeliveringRevenue = deliveringOrders.Sum(o => o.FeeExcludingTax);

                var completedOrders = allOrders.Where(o => o.Status == OrderStatus.Completed).ToList();
                model.CompletedOrders = completedOrders.Count();
                model.TotalRevenue = completedOrders.Sum(o => o.FeeExcludingTax);

                var revenueChartData = new ChartData
                {
                    Labels = new List<string>(),
                    Data = new List<decimal>()
                };

                var chartOrdersQuery = _context.Orders
                    .Where(o => o.CreatedTime >= chartDateRange.startDate &&
                               o.CreatedTime <= chartDateRange.endDate &&
                               o.Status == OrderStatus.Completed);

                if (!isAdminOverview)
                {
                    chartOrdersQuery = chartOrdersQuery.Where(o => o.ShipperId == shipperId);
                }

                if (chartPeriod == "week")
                {
                    var today = DateTime.Now.Date;
                    for (int i = 6; i >= 0; i--)
                    {
                        var dayStart = today.AddDays(-i);
                        var dayEnd = dayStart.AddDays(1).AddTicks(-1);

                        var dayRevenue = chartOrdersQuery
                            .Where(o => o.CreatedTime >= dayStart && o.CreatedTime <= dayEnd)
                            .Sum(o => (decimal)(o.Fee - o.FeeExcludingTax));

                        revenueChartData.Labels.Add(GetDayOfWeekVietnamese(dayStart.DayOfWeek));
                        revenueChartData.Data.Add(dayRevenue);
                    }
                }
                else if (chartPeriod == "month")
                {
                    var today = DateTime.Now.Date;
                    var startOfCurrentWeek = today.AddDays(-(int)today.DayOfWeek + 1);

                    for (int i = 3; i >= 0; i--)
                    {
                        var weekStart = startOfCurrentWeek.AddDays(-i * 7);
                        var weekEnd = weekStart.AddDays(6).AddDays(1).AddTicks(-1);

                        if (i == 0 && weekEnd > DateTime.Now)
                        {
                            weekEnd = DateTime.Now;
                        }

                        var weekRevenue = chartOrdersQuery
                            .Where(o => o.CreatedTime >= weekStart && o.CreatedTime <= weekEnd)
                            .Sum(o => (decimal)(o.Fee - o.FeeExcludingTax));

                        revenueChartData.Labels.Add($"Tuần {4 - i}");
                        revenueChartData.Data.Add(weekRevenue);
                    }
                }
                else if (chartPeriod == "year")
                {
                    var currentYear = DateTime.Now.Year;
                    var currentMonth = DateTime.Now.Month;

                    for (int i = 1; i <= currentMonth; i++)
                    {
                        var monthStart = new DateTime(currentYear, i, 1);
                        var monthEnd = i == currentMonth ? DateTime.Now : monthStart.AddMonths(1).AddTicks(-1);

                        var monthRevenue = chartOrdersQuery
                            .Where(o => o.CreatedTime >= monthStart && o.CreatedTime <= monthEnd)
                            .Sum(o => (decimal)(o.Fee - o.FeeExcludingTax));

                        revenueChartData.Labels.Add($"T{i}");
                        revenueChartData.Data.Add(monthRevenue);
                    }
                }

                model.RevenueChart = revenueChartData;

                var statusData = allOrders
                    .GroupBy(o => o.Status)
                    .Select(g => new
                    {
                        Status = g.Key,
                        Count = g.Count()
                    })
                    .ToList();

                model.StatusChart = new ChartData
                {
                    Labels = statusData.Select(x => GetOrderStatusVietnamese(x.Status)).ToList(),
                    Data = statusData.Select(x => (decimal)x.Count).ToList()
                };
            }
            catch (Exception ex)
            {
                model = new ShipperStatisticalModel
                {
                    ShipperName = "Unknown",
                    ShipperId = id ?? 0,
                    DeliveringOrders = 0,
                    CompletedOrders = 0,
                    TotalRevenue = 0,
                    DeliveringRevenue = 0,
                    RevenueChart = new ChartData { Labels = new List<string>(), Data = new List<decimal>() },
                    StatusChart = new ChartData { Labels = new List<string>(), Data = new List<decimal>() }
                };
            }

            ViewBag.SelectedPeriod = period;
            ViewBag.ChartPeriod = chartPeriod;

            return View(model);
        }

        private string GetOrderStatusVietnamese(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Confirmed => "Chờ nhận",
                OrderStatus.Delivery => "Đang giao",
                OrderStatus.Completed => "Đã giao",
                OrderStatus.Cancelled => "Đã hủy",
                _ => "Không xác định"
            };
        }

        private (DateTime startDate, DateTime endDate) GetDateRange(string periodType)
        {
            var now = DateTime.Now;
            var startDate = DateTime.MinValue;
            var endDate = now;

            switch (periodType?.ToLower())
            {
                case "today":
                    startDate = now.Date;
                    endDate = now;
                    break;
                case "week":
                    startDate = now.Date.AddDays(-7);
                    endDate = now;
                    break;
                case "month":
                    startDate = now.Date.AddDays(-30);
                    endDate = now;
                    break;
                case "year":
                    startDate = now.Date.AddDays(-365);
                    endDate = now;
                    break;
                case "all":
                default:
                    startDate = DateTime.MinValue;
                    endDate = DateTime.MaxValue;
                    break;
            }

            return (startDate, endDate);
        }

        private (DateTime startDate, DateTime endDate) GetDateRangeForChart(string periodType)
        {
            var now = DateTime.Now;
            var startDate = DateTime.MinValue;
            var endDate = now;

            switch (periodType?.ToLower())
            {
                case "week":
                    startDate = now.Date.AddDays(-6);
                    break;
                case "month":
                    startDate = now.Date.AddDays(-27);
                    break;
                case "year":
                    startDate = new DateTime(now.Year, 1, 1);
                    break;
                default:
                    startDate = now.Date.AddDays(-30);
                    break;
            }

            return (startDate, endDate);
        }

        private string GetDayOfWeekVietnamese(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "T2",
                DayOfWeek.Tuesday => "T3",
                DayOfWeek.Wednesday => "T4",
                DayOfWeek.Thursday => "T5",
                DayOfWeek.Friday => "T6",
                DayOfWeek.Saturday => "T7",
                DayOfWeek.Sunday => "CN",
                _ => ""
            };
        }

        private string GetCustomerTypeVietnamese(UserRankEnum rank)
        {
            return rank switch
            {
                UserRankEnum.None => "Khách hàng tìm năng",
                UserRankEnum.Copper => "Khách hàng đồng",
                UserRankEnum.Silver => "Khách hàng bạc",
                UserRankEnum.Gold => "Khách hàng vàng",
                UserRankEnum.Diamond => "Khách hàng kim cương",
                _ => "Khách hàng tìm năng"
            };
        }

        private List<(DateTime start, DateTime end)> GetWeeksInMonth(DateTime month)
        {
            var weeks = new List<(DateTime start, DateTime end)>();
            var firstDay = new DateTime(month.Year, month.Month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            var current = firstDay;
            while (current <= lastDay)
            {
                var weekEnd = current.AddDays(6);
                if (weekEnd > lastDay)
                    weekEnd = lastDay;

                weeks.Add((current, weekEnd.AddDays(1).AddTicks(-1)));
                current = current.AddDays(7);
            }

            return weeks;
        }
    }
}