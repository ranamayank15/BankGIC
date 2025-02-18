using AwesomeGICBank.Dtos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class Program
{
    //uncomment this code in order to save data in file, otherwise stored in-memory
    //static string transactionsFile = "transactions.txt";
    //static string interestRulesFile = "interest_rules.txt";
    static Dictionary<string, Account> accounts = new Dictionary<string, Account>();
    static List<InterestRule> interestRules = new List<InterestRule>();

    static void Main()
    {
        //LoadData(); //this will load the data from file
        while (true)
        {
            Console.WriteLine("\nWelcome to AwesomeGIC Bank! What would you like to do?");
            Console.WriteLine("[T] Input transactions");
            Console.WriteLine("[I] Define interest rules");
            Console.WriteLine("[P] Print statement");
            Console.WriteLine("[Q] Quit");
            Console.Write("> ");
            string? choice = Console.ReadLine()?.Trim().ToUpper();

            switch (choice)
            {
                case "T":
                    InputTransaction();
                    break;
                case "I":
                    DefineInterestRule();
                    break;
                case "P":
                    PrintStatement();
                    break;
                case "Q":
                  //S  SaveData(); //saving data in file, in order to keep the memory
                    Console.WriteLine("Thank you for banking with AwesomeGIC Bank. Have a nice day!");
                    return;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }

    public static void InputTransaction()
    {
        while (true)
        {
            Console.Write("\nEnter transaction details <Date YYYYMMdd> <Account> <Type (D/W)> <Amount> (or blank to go back): ");
            Console.Write("> \n");
            string? input = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(input)) return;

            string[] parts = input.Split(' ');
            // Validating input and parsing values
            if (parts.Length != 4 || !DateTime.TryParseExact(parts[0], "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime date) ||
                (parts[2].ToUpper() != "D" && parts[2].ToUpper() != "W") || !decimal.TryParse(parts[3], out decimal amount) || amount <= 0)
            {
                Console.WriteLine("Invalid input. Try again.");
                continue;
            }

            string accountId = parts[1];
            string type = parts[2].ToUpper();

            // Creating an account if it does not exist
            if (!accounts.ContainsKey(accountId))
                accounts[accountId] = new Account(accountId);

            try
            {
                string txnId = $"{date:yyyyMMdd}-{(accounts[accountId].Transactions.Count + 1):D2}";
                accounts[accountId].AddTransaction(new Transaction(date, txnId, type, amount));

                Console.WriteLine("Transaction added successfully!");
                PrintAccountStatement(accountId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    static void DefineInterestRule()
    {
        while (true)
        {
            Console.Write("\nEnter interest rule <Date YYYYMMdd> <RuleId> <Rate%> (or blank to go back): ");
            Console.Write("> \n");
            string? input = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(input)) return;

            string[] parts = input.Split(' ');
            // Validating input and parsing values
            if (parts.Length != 3 || !DateTime.TryParseExact(parts[0], "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime date) ||
                !decimal.TryParse(parts[2], out decimal rate) || rate <= 0 || rate >= 100)
            {
                Console.WriteLine("Invalid input. Try again.");
                continue;
            }

            string ruleId = parts[1];

            // Remove old rule on the same date
            interestRules.RemoveAll(r => r.StartDate == date);
            interestRules.Add(new InterestRule(date, ruleId, rate));
            interestRules = interestRules.OrderBy(r => r.StartDate).ToList();

            Console.WriteLine("Interest rule added successfully!");
            PrintInterestRules();
        }
    }

    public static void PrintStatement()
    {
        Console.Write("\nEnter account and month <Account> <YYYYMM>: ");
        Console.Write("> \n");
        string? input = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(input)) return;

        string[] parts = input.Split(' ');
        // Validating input
        if (parts.Length != 2 || !accounts.ContainsKey(parts[0]) || parts[1].Length != 6 || !int.TryParse(parts[1], out _))
        {
            Console.WriteLine("Invalid input. Try again.");
            return;
        }

        string accountId = parts[0];
        string yearMonth = parts[1];

        accounts[accountId].ApplyMonthlyInterest(interestRules, yearMonth);
        PrintAccountStatement(accountId);
    }

    public static void PrintAccountStatement(string accountId)
    {
        Console.WriteLine($"\nAccount: {accountId}");
        Console.WriteLine("| Date     | Txn Id      | Type | Amount | Balance |");
        decimal balance = 0;
        // Iterating transactions in chronological order
        foreach (var txn in accounts[accountId].Transactions.OrderBy(t => t.Date))
        {
            balance += txn.Type == "W" ? -txn.Amount : +txn.Amount;
            Console.WriteLine($"| {txn.Date:yyyyMMdd} | {txn.TxnId,-10} | {txn.Type}    | {txn.Amount,6:F2} | {balance,8:F2} |");
        }
    }

    public static void PrintInterestRules()
    {
        Console.WriteLine("\nInterest Rules:");
        Console.WriteLine("| Date     | RuleId | Rate (%) |");
        // Displaying interest rules in order
        foreach (var rule in interestRules)
        {
            Console.WriteLine($"| {rule.StartDate:yyyyMMdd} | {rule.RuleId,-6} | {rule.Rate,8:F2} |");
        }
    }

    //Code for saving data in file

    //static void SaveData()
    //{
    //    File.WriteAllLines(transactionsFile, accounts.SelectMany(a => a.Value.Transactions.Select(t =>
    //        $"{t.Date:yyyyMMdd} {a.Key} {t.TxnId} {t.Type} {t.Amount:F2}")));
    //    File.WriteAllLines(interestRulesFile, interestRules.Select(r => $"{r.StartDate:yyyyMMdd} {r.RuleId} {r.Rate:F2}"));
    //}

    //Code for opening data from file

    //static void LoadData()
    //{
    //    if (File.Exists(transactionsFile))
    //    {
    //        foreach (var line in File.ReadAllLines(transactionsFile))
    //        {
    //            var parts = line.Split(' ');
    //            var date = DateTime.ParseExact(parts[0], "yyyyMMdd", null);
    //            var accountId = parts[1];
    //            var txnId = parts[2];
    //            var type = parts[3];
    //            var amount = decimal.Parse(parts[4]);
    //            if (!accounts.ContainsKey(accountId))
    //                accounts[accountId] = new Account(accountId);

    //            accounts[accountId].AddTransaction(new Transaction(date, txnId, type, amount));
    //        }
    //    }

    //    if (File.Exists(interestRulesFile))
    //    {
    //        foreach (var line in File.ReadAllLines(interestRulesFile))
    //        {
    //            var parts = line.Split(' ');
    //            var date = DateTime.ParseExact(parts[0], "yyyyMMdd", null);
    //            var ruleId = parts[1];
    //            var rate = decimal.Parse(parts[2]);
    //            interestRules.Add(new InterestRule(date, ruleId, rate));
    //        }
    //    }
    //}
}
