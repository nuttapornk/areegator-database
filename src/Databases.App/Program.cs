using System.Reflection;
using Databases.App.Models;
using DbUp;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

internal class Program
{
    private static void Main(string[] args)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
        .AddJsonFile("Appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"Appsettings.{env}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build();

        Config config = new();
        configuration.Bind(config);

        if (config.Databases != null && config.Databases.Count > 0)
        {
            foreach (var database in config.Databases)
            {
                Migrate(database);
            }
        }
    }


    private static bool Migrate(Database database)
    {
        var connectionString = GetConnectionString(database);

        EnsureDatabase.For.SqlDatabase(connectionString);

        var upgrader = DeployChanges.To
        .SqlDatabase(connectionString)
        .WithScriptsFromFileSystem($"Scripts\\{database.Name}")
        .LogToConsole()
        .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Migration failed.");
            Console.WriteLine(result.Error);
            Console.ResetColor();
            return false;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Migration successful.");
            Console.ResetColor();
            return true;
        }
    }

    private static string GetConnectionString(Database database)
    {
        SqlConnectionStringBuilder builder = new()
        {
            DataSource = database.Port == 0 ? $"{database.Server},1433" : $"{database.Server},{database.Port}",
            InitialCatalog = database.Name,
            UserID = database.Username,
            Password = database.Password,
            TrustServerCertificate = true
        };
        return builder.ConnectionString;
    }

}