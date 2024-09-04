using System;
using System.Collections.Generic;
using System.Globalization;
using System.Data.SQLite;
using Forte;

namespace ForteCred
{
    internal class TableManager
    {
        private readonly DatabaseManager dbManager;
        private List<TableRow> rows;

        public TableManager(DatabaseManager dbManager)
        {
            this.dbManager = dbManager;
            this.rows = new List<TableRow>();
        }

        public void LoadData()
        {
            try
            {
                using (var connection = dbManager.GetConnection())
                {
                    connection.Open();

                    string selectQuery = "SELECT TermMonths, sum, month1, month2, month3 FROM Forte";
                    using (var command = new SQLiteCommand(selectQuery, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int months = reader.GetInt32(0);
                            decimal sum = reader.GetDecimal(1);
                            DateTime inputDate = DateTime.Now;
                            rows.Add(new TableRow(months, sum, inputDate));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке данных: {ex.Message}");
            }
        }

        public void SaveData()
        {
            try
            {
                using (var connection = dbManager.GetConnection())
                {
                    connection.Open();

                    string deleteQuery = "DELETE FROM Forte";
                    using (var command = new SQLiteCommand(deleteQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    foreach (var row in rows)
                    {
                        string insertQuery = @"
                                            INSERT INTO Forte (TermMonths, sum, month1, month2, month3)
                                            VALUES (@TermMonths, @sum, @month1, @month2, @month3)";
                        using (var command = new SQLiteCommand(insertQuery, connection))
                        {
                            command.Parameters.AddWithValue("@TermMonths", row.Months);
                            command.Parameters.AddWithValue("@sum", row.Sum);
                            command.Parameters.AddWithValue("@month1", row.Months >= 1 ? row.Sum / 3 : 0);
                            command.Parameters.AddWithValue("@month2", row.Months >= 2 ? row.Sum / 3 : 0);
                            command.Parameters.AddWithValue("@month3", row.Months == 3 ? row.Sum / 3 : 0);
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении данных: {ex.Message}");
            }
        }

        public void DisplayTable()
        {
            try
            {
                Console.Clear();
                DateTime currentDate = DateTime.Now;
                DateTime date1 = currentDate.Day <= 25 ? new DateTime(currentDate.Year, currentDate.Month, 25) : new DateTime(currentDate.Year, currentDate.Month, 25).AddMonths(1);
                DateTime date2 = date1.AddMonths(1);
                DateTime date3 = date2.AddMonths(1);

                string header = string.Format(
                    "{0,-5} | {1,-10} | {2,-10} | {3,-15} | {4,-15} | {5,-15} | {6,-15}",
                    "№", "Месяцев", "Сумма", "Ежемесячно", date1.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), date2.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), date3.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)
                );

                Console.WriteLine(header);

                if (rows.Count == 0) return;

                decimal totalCol4 = 0;
                decimal totalCol5 = 0;
                decimal totalCol6 = 0;

                for (int i = 0; i < rows.Count; i++)
                {
                    var row = rows[i];
                    decimal monthly = row.Sum / 3;

                    string col4 = row.Months >= 1 ? monthly.ToString("F2", CultureInfo.InvariantCulture) : "";
                    string col5 = row.Months >= 2 ? monthly.ToString("F2", CultureInfo.InvariantCulture) : "";
                    string col6 = row.Months == 3 ? monthly.ToString("F2", CultureInfo.InvariantCulture) : "";

                    totalCol4 += string.IsNullOrEmpty(col4) ? 0 : decimal.Parse(col4, CultureInfo.InvariantCulture);
                    totalCol5 += string.IsNullOrEmpty(col5) ? 0 : decimal.Parse(col5, CultureInfo.InvariantCulture);
                    totalCol6 += string.IsNullOrEmpty(col6) ? 0 : decimal.Parse(col6, CultureInfo.InvariantCulture);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"{i + 1,-5} | ");
                    Console.ResetColor();
                    Console.WriteLine(string.Format(
                        "{0,-10} | {1,-10} | {2,-15} | {3,-15} | {4,-15} | {5,-15}",
                        row.Months, row.Sum.ToString("F2", CultureInfo.InvariantCulture), monthly.ToString("F2", CultureInfo.InvariantCulture), col4, col5, col6
                    ));
                }

                string[] totalRow = new string[]
                {
                                    "Итого", "", "", totalCol4.ToString("F2", CultureInfo.InvariantCulture), totalCol5.ToString("F2", CultureInfo.InvariantCulture), totalCol6.ToString("F2", CultureInfo.InvariantCulture)
                };

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"{"",-5} | ");
                Console.ResetColor();
                Console.WriteLine(string.Format(
                    "{0,-10} | {1,-10} | {2,-15} | {3,-15} | {4,-15} | {5,-15}",
                    totalRow[0], totalRow[1], totalRow[2], totalRow[3], totalRow[4], totalRow[5]
                ));

                SaveTotalColumn4(totalCol4);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отображении таблицы: {ex.Message}");
            }
        }

        public void UpdateRows(DateTime currentDate)
        {
            try
            {
                for (int i = rows.Count - 1; i >= 0; i--)
                {
                    var row = rows[i];
                    if (currentDate >= row.InputDate.AddMonths(1))
                    {
                        row.Months--;
                        if (row.Months <= 0)
                        {
                            rows.RemoveAt(i);
                        }
                        else
                        {
                            row.InputDate = row.InputDate.AddMonths(1);
                        }
                    }
                }

                SaveData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении строк: {ex.Message}");
            }
        }

        public void AddRow()
        {
            try
            {
                Console.Write("Введите количество месяцев (1-3): ");
                int months = int.Parse(Console.ReadLine());

                if (months < 1 || months > 3)
                {
                    Console.WriteLine("Количество месяцев должно быть от 1 до 3.");
                    return;
                }

                Console.Write("Введите сумму: ");
                decimal sum = decimal.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);

                rows.Add(new TableRow(months, sum, DateTime.Now));
                SaveData();
            }
            catch (FormatException)
            {
                Console.WriteLine("Неверный формат ввода. Попробуйте снова.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении строки: {ex.Message}");
            }
        }

        public void RemoveRow()
        {
            try
            {
                Console.Write("Введите номер строки для удаления: ");
                int rowIndex = int.Parse(Console.ReadLine()) - 1;

                if (rowIndex < 0 || rowIndex >= rows.Count)
                {
                    Console.WriteLine("Неверный номер строки.");
                    return;
                }

                rows.RemoveAt(rowIndex);
                SaveData();
            }
            catch (FormatException)
            {
                Console.WriteLine("Неверный формат ввода. Попробуйте снова.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении строки: {ex.Message}");
            }
        }

        public void ClearTable()
        {
            try
            {
                rows.Clear();
                SaveData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при очистке таблицы: {ex.Message}");
            }
        }

        private void SaveTotalColumn4(decimal total)
        {
            try
            {
                using (var connection = dbManager.GetConnection())
                {
                    connection.Open();
                    string updateQuery = "UPDATE Forte SET month1 = @total WHERE TermMonths = 1";
                    using (var command = new SQLiteCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@total", total);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении общего значения: {ex.Message}");
            }
        }
    }
}
