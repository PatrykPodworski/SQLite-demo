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
                Console.ReadKey();
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

        private static void ShowStudents(SQLiteConnection connection, string sortColumn = null)
        {
            var sql = $"SELECT * FROM Students";
            if (sortColumn != null)
            {
                sql += " ORDER BY sortColumn";
            }
            var command = new SQLiteCommand(sql, connection);
            var reader = command.ExecuteReader();

            Console.WriteLine($"| {"Name",-30} | {"Year",-4} | {"LastYearGrade",-13} |");
            Console.WriteLine(String.Empty.PadLeft(57, '-'));
            while (reader.Read())
            {
                var val = reader.GetValues();
                Console.WriteLine($"| {val["Name"],-30} | {val["Year"],-4} | {val["LastYearGrade"],-13} |");
            }
            Console.WriteLine(String.Empty.PadLeft(57, '-'));
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