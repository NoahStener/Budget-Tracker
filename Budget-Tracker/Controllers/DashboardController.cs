using Budget_Tracker.Data;
using Budget_Tracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Transaction = Budget_Tracker.Models.Transaction;

namespace Budget_Tracker.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }


        public async Task<ActionResult> Index()
        {
            //Last 7 days transactions
            DateTime StartDate = DateTime.Now.AddDays(-6);
            DateTime EndDate = DateTime.Today;

            List<Transaction> SelectedTransaction = await _context.Transactions
                  .Include(x => x.Category)
                  .Where(t => t.Date >= StartDate && t.Date <= EndDate).ToListAsync();


            //Total income
            int TotalIncome = SelectedTransaction
                .Where(i => i.Category.Type == "Income")
                .Sum(i => i.Amount);

            ViewBag.TotalIncome = TotalIncome.ToString("C0");


            //Total expense
            int TotalExpense = SelectedTransaction
                .Where(i => i.Category.Type == "Expense")
                .Sum(i => i.Amount);

            ViewBag.TotalExpense = TotalExpense.ToString("C0");


            //Balance
            int Balance = TotalIncome - TotalExpense;
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-SEK");

            ViewBag.Balance = Balance.ToString("C0", culture);


            //Doughnut chart - Expense by Category

            ViewBag.DoughnutChartData = SelectedTransaction.Where(i => i.Category.Type == "Expense")
                .GroupBy(i => i.Category.CategoryID)
               .Select(k => new
               {
                   CategoryTitleWithIcon = k.First().Category.Icon + " " + k.First().Category.Title,
                   Amount = k.Sum(i => i.Amount),
                   FormattedAmount = k.Sum(j => j.Amount).ToString("C0"),

               }).OrderByDescending(l => l.Amount).ToList();


            //SpLine chart income vs Expense

            //Income
            List<SplineChartData> IncomeSummary = SelectedTransaction.Where(i => i.Category.Type == "Income")
                .GroupBy(j => j.Date).Select(k => new SplineChartData()
                {
                    Day = k.First().Date.ToString("dd-MMM"),
                    Income = k.Sum(l => l.Amount),

                }).ToList();

            //Expense
            List<SplineChartData> ExpenseSummary = SelectedTransaction.Where(i => i.Category.Type == "Expense")
                .GroupBy(j => j.Date).Select(k => new SplineChartData()
                {
                    Day = k.First().Date.ToString("dd-MMM"),
                    Expense = k.Sum(l => l.Amount)

                }).ToList();


            //Combine Income and Expense
            string[] Last7Days = Enumerable.Range(0, 7).Select(i => StartDate.AddDays(i).ToString("dd-MMM")).ToArray();

            return View();
        }
    }

    public class SplineChartData
    {
        public string Day;
        public int Income;
        public int Expense;
    }
}
