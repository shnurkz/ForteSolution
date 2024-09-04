using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Data.SQLite;
namespace Forte
{
    internal class TableRow
    {
        public int Months { get; set; }
        public decimal Sum { get; set; }
        public DateTime InputDate { get; set; }

        public TableRow(int months, decimal sum, DateTime inputDate)
        {
            Months = months;
            Sum = sum;
            InputDate = inputDate;
        }
    }
}
