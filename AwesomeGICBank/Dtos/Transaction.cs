using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwesomeGICBank.Dtos
{
    public class Transaction
    {
        public DateTime Date { get; }
        public string TxnId { get; }
        public string Type { get; } // "D", "W", "I"
        public decimal Amount { get; }

        public Transaction(DateTime date, string txnId, string type, decimal amount)
        {
            if (amount <= 0) throw new ArgumentException("Amount must be greater than zero.");
            if (type != "D" && type != "W" && type != "I") throw new ArgumentException("Invalid transaction type.");

            Date = date;
            TxnId = txnId;
            Type = type;
            Amount = amount;
        }
    }

}
