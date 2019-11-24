using System;
using System.Data.SQLite;

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

        private static void CreateStudentsTable(SQLiteConnection connetion)
        {
            var sql = "CREATE TABLE Students (name TEXT, year INTEGER, lastYearGrade REAL)";
            var command = new SQLiteCommand(sql, connetion);
            command.ExecuteNonQuery();
        }
    }
}