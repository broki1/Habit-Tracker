using Microsoft.Data.Sqlite;
using System.Globalization;

string connectionString = @"Data Source=habit-Tracker.db";
List<string> dbNames = new List<string>();

using (var connection = new  SqliteConnection(connectionString))
{
    connection.Open();

    var dbCmd = connection.CreateCommand();
    dbCmd.CommandText = @"SELECT name FROM sqlite_schema WHERE type='table' AND name NOT LIKE 'sqlite%' ORDER BY name";

    SqliteDataReader reader = dbCmd.ExecuteReader();

    var index = 0;

    Console.WriteLine("Habits:\n\n");

    if (reader.HasRows)
    {
        while (reader.Read())
        {
            var tableName = reader.GetString(0);

            if (!dbNames.Contains(tableName))
            {
                dbNames.Add(tableName);
            }

            var formattedTableName = FormatTableName(tableName);
            Console.WriteLine($"{++index}: {formattedTableName}");
        }
    }

    connection.Close();
}

Console.WriteLine("\n---------------------------------------------------------------------------------------------------------");

Console.WriteLine("\nChoose habit to track (enter corresponding number), or press 0 to create new habit.");

Console.WriteLine("\n---------------------------------------------------------------------------------------------------------\n\n");


using (var connection = new SqliteConnection(connectionString))
{
    connection.Open();
    var tableCmd = connection.CreateCommand();

    tableCmd.CommandText = @"CREATE TABLE IF NOT EXISTS drinking_water (Id INTEGER PRIMARY KEY AUTOINCREMENT, Date TEXT, Quantity INTEGER)";

    tableCmd.ExecuteNonQuery();

    connection.Close();
}



// GetUserInput();

string FormatTableName(string tableName)
{
    string formattedTableName = tableName;

    if (formattedTableName.IndexOf("_") != -1)
    {
        formattedTableName = formattedTableName.Replace("_", " ").ToLower();
        
    }

    char firstLetter = char.ToUpper(formattedTableName[0]);

    formattedTableName = firstLetter + formattedTableName.Substring(1);

    return formattedTableName;
}

void GetUserInput()
{
    Console.Clear();

    bool closeApp = false;

    while (!closeApp)
    {
        Console.WriteLine("\n\nMAIN MENU");
        Console.WriteLine("\nWhat would you like to do?");
        Console.WriteLine("\nType 0 to close the application.");
        Console.WriteLine("Type 1 to View All Records.");
        Console.WriteLine("Type 2 to Insert Record.");
        Console.WriteLine("Type 3 to Update Record.");
        Console.WriteLine("Type 4 to Delete Record.");
        Console.WriteLine("------------------------------------------------");

        string commandInput = Console.ReadLine().Trim();

        switch (commandInput)
        {
            case "0":
                Console.WriteLine("\nGoodbye!\n");
                closeApp = true;
                Environment.Exit(0);
                break;
            case "1":
                GetAllRecords();
                break;
            case "2":
                Insert();
                break;
            case "3":
                Update();
                break;
            case "4":
                Delete();
                break;
            default:
                Console.WriteLine("\nInvalid Command. Please type a number from 0 to 4.\n");
                break;
        }

    }
}

void Insert()
{
    string date = GetDateInput();

    int quantity = GetNumberInput("\n\nPlease insert number of glasses or other measure of your choice (no decimals allowed)\n\n");

    using (var connection = new SqliteConnection(connectionString))
    {
        connection.Open();
        var tableCmd = connection.CreateCommand();
        tableCmd.CommandText = $@"INSERT INTO drinking_water (date, quantity) VALUES ('{date}', {quantity})";

        tableCmd.ExecuteNonQuery();
        connection.Close();
    }
}

void GetAllRecords()
{
    using (var connection = new SqliteConnection(connectionString))
    {
        connection.Open();
        var tableCmd = connection.CreateCommand();
        tableCmd.CommandText = "SELECT * FROM drinking_water";

        List<DrinkingWater> tableData = new();

        // tableCmd is executed, and returns a Reader object that can read the data retrieved
        SqliteDataReader reader = tableCmd.ExecuteReader();

        if (reader.HasRows)
        {
            while (reader.Read())
            {
                tableData.Add(
                    new DrinkingWater
                    {
                        Id = reader.GetInt32(0),
                        Date = DateTime.ParseExact(reader.GetString(1), "dd-MM-yy", new CultureInfo("en-US")),
                        Quantity = reader.GetInt32(2)
                    });
            }
        }

        else
        {
            Console.WriteLine("No rows found.");
        }

        connection.Close();

        Console.WriteLine("----------------------------------------\n");

        foreach (var dw in tableData)
        {

            Console.WriteLine($"{dw.Id} - {dw.Date.ToString("dd-MM-yyyy")} - Quantity: {dw.Quantity}");

        }

        Console.WriteLine("----------------------------------------\n");

    }
}

void Update()
{

    GetAllRecords();

    var recordId = GetNumberInput("\n\nPlease type Id of the record would like to update. Type 0 to return to main manu.\n\n");

    using (var connection = new SqliteConnection(connectionString))
    {
        connection.Open();

        var checkCmd = connection.CreateCommand();
        checkCmd.CommandText = $"SELECT EXISTS(SELECT 1 FROM drinking_water WHERE Id = {recordId})";
        int checkQuery = Convert.ToInt32(checkCmd.ExecuteScalar());

        if (checkQuery == 0)
        {
            Console.WriteLine($"\n\nRecord with Id {recordId} doesn't exist.\n\n");
            connection.Close();
            Update();
        }

        string date = GetDateInput();

        int quantity = GetNumberInput("\n\nPlease insert number of glasses or other measure of your choice (no decimals allowed)\n\n");

        var tableCmd = connection.CreateCommand();
        tableCmd.CommandText = $"UPDATE drinking_water SET date = '{date}', quantity = {quantity} WHERE Id = {recordId}";

        tableCmd.ExecuteNonQuery();

        connection.Close();

    }
}

void Delete()
{
    // Console.Clear();
    GetAllRecords();

    var recordId = GetNumberInput("\n\nPlease type the Id of the record you want to delete or type 0 to go back to Main Menu\n\n");

    using (var connection = new SqliteConnection(connectionString))
    {
        connection.Open();
        var tableCmd = connection.CreateCommand();

        tableCmd.CommandText = $"DELETE from drinking_water WHERE Id = '{recordId}'";

        int rowCount = tableCmd.ExecuteNonQuery();

        if (rowCount == 0)
        {
            Console.WriteLine($"\n\nRecord with Id {recordId} doesn't exist. \n\n");
            Delete();
        }

    }

    Console.WriteLine($"\n\nRecord with Id {recordId} was deleted. \n\n");

    GetUserInput();
}

int GetNumberInput(string message)
{
    Console.WriteLine(message);

    string numberInput = Console.ReadLine();

    if (numberInput == "0") GetUserInput();

    while (!Int32.TryParse(numberInput, out _) || Convert.ToInt32(numberInput) < 0)
    {
        Console.WriteLine("\n\nInvalid number. Try again.\n\n");
        numberInput = Console.ReadLine();
    }

    int finalInput = Convert.ToInt32(numberInput);

    return finalInput;
}

string GetDateInput()
{
    Console.WriteLine("\n\nPlease insert the date: (Format: dd-mm-yy). Type 0 to return to main manu.\n\n");

    string dateInput = Console.ReadLine();

    if (dateInput == "0") GetUserInput();

    while (!DateTime.TryParseExact(dateInput, "dd-MM-yy", new CultureInfo("en-US"), DateTimeStyles.None, out _))
    {
        Console.WriteLine("\n\nInvalid date. (Format: dd-mm-yy). Type 0 to return to main manu or try again:\n\n");
        dateInput = Console.ReadLine();
    }

    return dateInput;
}

public class DrinkingWater
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int Quantity { get; set; }
}