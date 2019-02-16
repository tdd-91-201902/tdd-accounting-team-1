using System;
using System.Collections.Generic;
using System.Linq;

namespace Tdd.AccountingPractice
{
    public class Accounting
    {
        private IBudgetRepo budgetRepo;

        public Accounting(IBudgetRepo repo)
        {
            budgetRepo = repo;
        }

        public double TotalAmount(DateTime start, DateTime end)
        {
            var budgets = budgetRepo.GetAll();

            var period = new Period(start, end);
            if (!IsValid(start, end)) return 0;

            if (IsSameMonth(start, end))
            {
                var budget = GetBudget(start, budgets);

                if (budget == null)
                {
                    return 0;
                }

                return budget.DailyAmount() * period.Days();
            }

            var totalAmount = 0;

            totalAmount += GetFirstAndLastTotalAmounts(budgets, period);

            totalAmount += GetMiddleTotalAmounts(period.Start, period.End, budgets);

            return totalAmount;
        }

        private int GetFirstAndLastTotalAmounts(IEnumerable<Budget> budgets, Period period)
        {
            var totalAmount = 0;

            var budgetOfStart = GetTargetBudget(budgets, period.Start);
            int firstAmount = 0;
            if (budgetOfStart != null)
            {
                firstAmount = GetTargetAmount(period.Start, period, budgetOfStart);
            }
            totalAmount += firstAmount;

            var budgetOfEnd = GetTargetBudget(budgets, period.End);
            if (budgetOfEnd != null)
            {
                totalAmount += GetTargetAmount(period.End, period, budgetOfEnd);
            }

            return totalAmount;
        }

        private int GetMiddleTotalAmounts(DateTime start, DateTime end, IEnumerable<Budget> budgets)
        {
            var monthsInTargetRange = GetMonthsInTargetRange(start, end);
            var totalAmount = 0;
            if (monthsInTargetRange > 1)
            {
                for (int i = 1; i < monthsInTargetRange; i++)
                {
                    var searchMonth = start.AddMonths(i);
                    var targetMonthBudget = GetTargetBudget(budgets, searchMonth);
                    if (targetMonthBudget != null)
                    {
                        totalAmount += targetMonthBudget.Amount;
                    }
                }
            }

            return totalAmount;
        }

        private Budget GetTargetBudget(IEnumerable<Budget> budgetList, DateTime targetDateTime)
        {
            return budgetList.FirstOrDefault(x => x.YearMonth == targetDateTime.ToString("yyyyMM"));
        }

        private int GetMonthsInTargetRange(DateTime start, DateTime end)
        {
            var diffMonths = 12 * (end.Year - start.Year) + (end.Month - start.Month);
            return diffMonths;
        }

        private int GetTargetAmount(DateTime targetDateTime, Period period, Budget budget)
        {
            int unitOfDay = budget.DailyAmount();
            int targetAmount = 0;
            if (targetDateTime == period.Start)
            {
                targetAmount = unitOfDay * (budget.Days() - targetDateTime.Day + 1);
            }
            else if (targetDateTime == period.End)
            {
                targetAmount = unitOfDay * targetDateTime.Day;
            }

            return targetAmount;
        }

        private bool IsSameMonth(DateTime start, DateTime end)
        {
            return start.ToString("yyyyMM") == end.ToString("yyyyMM");
        }

        private bool IsValid(DateTime start, DateTime end)
        {
            return start <= end;
        }

        private Budget GetBudget(DateTime start, IEnumerable<Budget> budgets)
        {
            return budgets.FirstOrDefault(x => x.YearMonth == start.ToString("yyyyMM"));
        }

        private int GetDayInTargetMonth(DateTime targetDateTime)
        {
            return DateTime.DaysInMonth(targetDateTime.Year, targetDateTime.Month);
        }
    }
}