using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using Projekt1;

namespace Projekt1
{
    public class Program
    {
        static bool MainLoop = true;
        static Dictionary<string, bool> CommandList = new Dictionary<string, bool>()
        {
            ["help"] = true,
            ["exit"] = true,
        };

        static void Main(string[] args)
        {
            Program.Help();
            string line = "";
            while (Program.MainLoop == true)
            {
                line = Console.ReadLine();
                int indexSpace = line.IndexOf(" ");

                if (indexSpace != -1)
                {
                    string lineCommand = line.Substring(0, indexSpace);
                    line = line.Replace(lineCommand, "").TrimStart();

                    indexSpace = line.IndexOf(" ");

                    if (indexSpace != -1)
                    {
                        string lineCommandType = line.Substring(0, indexSpace);
                        line = line.Replace(lineCommandType, "");
                        line = line.Trim();

                        Program.MainSwitch(line, lineCommand, lineCommandType);
                    }
                    else if (Program.CommandList.ContainsKey(lineCommand) == true)
                    {
                        if (line.Trim().Equals("exit"))
                            return;

                        Program.MainSwitch("", lineCommand, "");
                    }
                    else
                    {
                        Program.CommandNotFound();
                    }
                }
                else if (Program.CommandList.ContainsKey(line.Trim()) == true)
                {
                    if (line.Trim().Equals("exit"))
                        return;

                    Program.MainSwitch("", line.Trim(), "");
                }
                else
                {
                    Program.CommandNotFound();
                }
            }
        }

        static private void CommandNotFound()
        {
            Console.WriteLine("Command not found, please try again or write help to see available commands." + System.Environment.NewLine);
        }

        static private void MainSwitch(string line, string lineCommand, string lineCommandType)
        {
            switch (lineCommand.ToLower())
            {
                case "help":
                    Program.Help();
                    break;

                case "new":
                    List<string> values = new List<string>(line.Split(","));
                    switch (lineCommandType.ToLower())
                    {
                        case "customer":
                            if (values.Count > 1)
                            {
                                Customer customer = new Customer(values[0], values[1]);
                            }
                            break;

                        case "booking":
                            if (values.Count > 5)
                            {
                                int dateRowId = Database.InsertData("bookingDates", new List<string>() { "checkInDate", "checkOutDate" }, new List<string>() { values[4], values[5] });
                                List<Dictionary<string, string>> foundRoom = Database.GetData("room", values[1], new List<string>() { "roomNumber" });
                                List<Dictionary<string, string>> foundCustomer = Database.GetData("customer", values[0], new List<string>() { "Id" });
                                List<Dictionary<string, string>> foundBookings = Database.GetData("bookings", foundCustomer[0]["Id"], new List<string>() { "customerId" });
                                int bookingId = Database.InsertData("booking", new List<string>() { "bookingsId", "customerId", "roomId", "paymentMethod", "bookingDatesId" }, new List<string>() { foundBookings[0]["Id"], foundCustomer[0]["Id"], foundRoom[0]["Id"], values[3], dateRowId.ToString() });
                                Console.WriteLine(bookingId);
                            }
                            break;
                    }
                    break;

                case "remove":
                    switch (lineCommandType.ToLower())
                    {
                        case "customer":
                            Database.UpdateData("customer", new List<string>() { "firstName", "lastName" }, new List<string>() { "", "" }, line, "Id");
                            break;

                        case "booking":
                            Database.UpdateData("booking", new List<string>() { "bookingValid" }, new List<string>() { "0" }, line, "Id");
                            break;
                    }
                    break;

                case "find":
                    switch (lineCommandType.ToLower())
                    {
                        case "customer":
                            List<Dictionary<string, string>> foundValues = Database.GetData("customer", line, new List<string>() { "firstName", "lastName" });

                            if (foundValues.Count == 0)
                            {
                                Console.WriteLine("No customer found with query: " + line);
                            }
                            else
                            {
                                foreach (Dictionary<string, string> dict in foundValues)
                                {
                                    foreach (KeyValuePair<string, string> value in dict)
                                    {
                                        Console.WriteLine(value.Key + " - " + value.Value);
                                    }
                                    Console.WriteLine(System.Environment.NewLine);
                                }
                            }
                            break;

                        case "all":
                            List<Dictionary<string, string>> foundAllValues = new List<Dictionary<string, string>>();
                            switch (line.ToLower())
                            {
                                case "customer":
                                    foundAllValues = Database.GetData("customer", "*", new List<string>() { });
                                    foreach (Dictionary<string, string> dict in foundAllValues)
                                    {
                                        foreach (KeyValuePair<string, string> value in dict)
                                        {
                                            Console.WriteLine(value.Key + " - " + value.Value);
                                        }
                                        Console.WriteLine(System.Environment.NewLine);
                                    }
                                    break;
                                case "booking":
                                    foundAllValues = Database.GetData("booking", "*", new List<string>() { });
                                    foreach (Dictionary<string, string> dict in foundAllValues)
                                    {
                                        foreach (KeyValuePair<string, string> value in dict)
                                        {
                                            Console.WriteLine(value.Key + " - " + value.Value);
                                        }
                                        Console.WriteLine();
                                    }
                                    break;
                            }
                            break;
                    }
                    break;
            }
        }

        static public void Help()
        {
            Console.WriteLine("Command: help displays this information again.");
            Console.WriteLine("Command: exit exists the console.");
            Console.WriteLine("Command: new booking customerId, room, floor, paymentMethod, checkInDate, checkOutDate");
            Console.WriteLine("Command: remove booking bookingId");
            Console.WriteLine("Command: new customer FirstName, LastName - Example: new customer lasse, bergslagen");
            Console.WriteLine("Command: remove customer CustomerID");
            Console.WriteLine("Command: find customer query");
            Console.WriteLine("Command: find all query - Example: find all customer");
            Console.WriteLine();
        }
    }
}
