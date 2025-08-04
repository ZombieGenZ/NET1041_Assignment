using Assignment.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Assignment.Enum;
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
        [HttpGet]
        [Route("statistical")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult Index(string period = "month", string chartPeriod = "month", string categoryPeriod = "month", string productsPeriod = "month", string customersPeriod = "month")
        {
            var model = new StatisticalModel();

            try
            {
                var dateRange = GetDateRange(period);
                var chartDateRange = GetDateRange(chartPeriod);
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
                    for (int i = 0; i < 7; i++)
                    {
                        var dayStart = chartDateRange.startDate.AddDays(i);
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
                    var weeksInMonth = GetWeeksInMonth(chartDateRange.startDate);
                    for (int i = 0; i < weeksInMonth.Count; i++)
                    {
                        var weekRevenue = chartOrdersQuery
                            .Where(o => o.CreatedTime >= weeksInMonth[i].start && o.CreatedTime <= weeksInMonth[i].end)
                            .Sum(o => (decimal)(o.TotalPrice + (o.Fee - o.FeeExcludingTax)));

                        revenueChartData.Labels.Add($"Tuần {i + 1}");
                        revenueChartData.Data.Add(weekRevenue);
                    }
                }
                else if (chartPeriod == "year")
                {
                    for (int i = 1; i <= 12; i++)
                    {
                        var monthStart = new DateTime(chartDateRange.startDate.Year, i, 1);
                        var monthEnd = monthStart.AddMonths(1).AddTicks(-1);

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
            catch
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

        [NonAction]
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

        [NonAction]
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

        [NonAction]
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

        [NonAction]
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