using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;

namespace SqliteDemo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            SQLiteConnection.CreateFile("MyDatabase.sqlite");
            var connection = new SQLiteConnection("Data Source=MyDatabase.sqlite;Version=3;");
            try
            {
                connection.Open();
                CreateStudentsTable(connection);
                InsertStudents(connection);
                ShowStudents(connection);
                RunTheApplication(connection);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        private static void RunTheApplication(SQLiteConnection connection)
        {
            var isRunning = true;
            while (isRunning)
            {
                try
                {
                    Console.WriteLine("Provide a command:");
                    var input = Console.ReadLine().Split(' ');

                    if (input.Length == 1)
                    {
                        var command = input[0];
                        if (HandleCommand(command, ref isRunning))
                        {
                            continue;
                        }
                    }
                    if (input.Length == 2)
                    {
                        var command = input[0];
                        var argument = input[1];
                        if (HandleCommand(command, argument, connection))
                        {
                            continue;
                        }
                    }
                    Console.WriteLine("Command is invalid.");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private static bool HandleCommand(string command, ref bool isRunning)
        {
            if (command == "q")
            {
                Console.WriteLine("Closing the application.");
                isRunning = false;
                return true;
            }
            if (command == "h")
            {
                Console.WriteLine("s [column] - sort by column");
                Console.WriteLine("sd [column] - sort by column descending");
                Console.WriteLine("d [column]:[value] - delete rows by value in column");
                Console.WriteLine("dn [column] - delete rows where NULL in column");
                Console.WriteLine("h - help");
                Console.WriteLine("q - quit");
                return true;
            }

            return false;
        }

        private static bool HandleCommand(string command, string argument, SQLiteConnection connection)
        {
            switch (command)
            {
                case "s":
                    ShowStudents(connection, argument);
                    return true;

                case "sd":
                    ShowStudents(connection, argument, true);
                    return true;

                case "d":
                    var result = DeleteStudents(connection, argument);
                    ShowStudents(connection);
                    return result;

                case "dn":
                    DeleteNullStudents(connection, argument);
                    ShowStudents(connection);
                    return true;

                default:
                    return false;
            }
        }

        private static void DeleteNullStudents(SQLiteConnection connection, string column)
        {
            var sql = $"DELETE FROM Students WHERE {column} IS NULL";
            var command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        private static bool DeleteStudents(SQLiteConnection connection, string argument)
        {
            var arguments = argument.Split(':');
            if (arguments.Length != 2)
            {
                return false;
            }

            var column = arguments[0];
            var value = arguments[1];

            var sql = $"DELETE FROM Students WHERE {column} == {value}";
            var command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();
            return true;
        }

        private static void ShowStudents(SQLiteConnection connection, string sortColumn = null, bool descending = false)
        {
            string sql = GetSelectQuery(sortColumn, descending);

            var command = new SQLiteCommand(sql, connection);
            var reader = command.ExecuteReader();

            Console.WriteLine(String.Empty.PadLeft(57, '-'));
            Console.WriteLine($"| {"Name",-30} | {"Year",-4} | {"LastYearGrade",-13} |");
            Console.WriteLine(String.Empty.PadLeft(57, '-'));
            while (reader.Read())
            {
                var val = reader.GetValues();
                Console.WriteLine($"| {val["Name"],-30} | {val["Year"],-4} | {val["LastYearGrade"],-13} |");
            }
            Console.WriteLine(String.Empty.PadLeft(57, '-'));
        }

        private static string GetSelectQuery(string sortColumn, bool descending)
        {
            var sql = $"SELECT * FROM Students";
            if (sortColumn != null)
            {
                sql += $" ORDER BY {sortColumn}";
                if (descending)
                {
                    sql += $" DESC";
                }
            }

            return sql;
        }

        private static void CreateStudentsTable(SQLiteConnection connection)
        {
            var sql = "CREATE TABLE Students (name TEXT, year INTEGER, lastYearGrade REAL)";
            var command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        private static void InsertStudents(SQLiteConnection connection)
        {
            var sql = "INSERT INTO Students VALUES {0}";
            var valueQuery = "(\"{0}\", {1}, {2})";
            var values = GetValues(valueQuery);

            var query = string.Format(CultureInfo.InvariantCulture, sql, string.Join(", ", values));
            var command = new SQLiteCommand(query, connection);
            command.ExecuteNonQuery();
        }

        private static List<string> GetValues(string valueQuery)
        {
            var values = new List<string>();

            var students = GetStudents();
            foreach (var student in students)
            {
                var query = string.Format(CultureInfo.InvariantCulture, valueQuery, student.Name, student.Year, student.LastYearGrade);
                values.Add(query);
            }

            var nullQuery = string.Format(CultureInfo.InvariantCulture, valueQuery, "Michał Pierwszak", 1, "NULL");
            values.Add(nullQuery);
            var textQuery = string.Format(CultureInfo.InvariantCulture, valueQuery, "Janusz Starszak", 7, "\"czwórka\"");
            values.Add(textQuery);

            return values;
        }

        private static List<Student> GetStudents()
        {
            return new List<Student>
            {
                new Student
                {
                    Name = "Jan Kowalski",
                    Year = 2,
                    LastYearGrade = 3.87
                },
                new Student
                {
                    Name = "Kazimierz Nowak",
                    Year = 5,
                    LastYearGrade = 2.94
                }
            };
        }
    }

    public class Student
    {
        public string Name { get; set; }
        public int Year { get; set; }
        public double LastYearGrade { get; set; }
    }
}