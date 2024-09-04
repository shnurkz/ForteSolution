using ForteCred;
using System;
using System.IO;

namespace Forte
{
    class Program
    {
        static void Main()
        {
            string dbFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database.db"); // Указываем путь к файлу database.db

            if (!File.Exists(dbFilePath))
            {
                Console.WriteLine("Файл базы данных не найден.");
                return;
            }

            try
            {
                DatabaseManager dbManager = new DatabaseManager(dbFilePath);
                dbManager.InitializeDatabase();

                TableManager tableManager = new TableManager(dbManager);
                tableManager.LoadData();
                tableManager.UpdateRows(DateTime.Now);
                tableManager.DisplayTable();

                ShowMenu(tableManager);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
            }
        }

        static void ShowMenu(TableManager tableManager)
        {
            while (true)
            {
                Console.WriteLine("\nМеню:");
                Console.WriteLine("0 - выход из программы");
                Console.WriteLine("1 - ввод новых строк");
                Console.WriteLine("2 - удаление строки");
                Console.WriteLine("3 - очистка таблицы");
                Console.Write("Выберите пункт меню: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "0":
                        return;
                    case "1":
                        tableManager.AddRow();
                        break;
                    case "2":
                        tableManager.RemoveRow();
                        break;
                    case "3":
                        tableManager.ClearTable();
                        break;
                    default:
                        Console.WriteLine("Неверный выбор. Попробуйте снова.");
                        break;
                }

                tableManager.DisplayTable();
            }
        }
    }
}
