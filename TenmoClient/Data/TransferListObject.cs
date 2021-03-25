using System;
using System.Collections.Generic;
using System.Text;

namespace TenmoClient.Data
{
    /// <summary>
    /// TransferListObject class mirrors the TLOs created on the server
    /// to allow them to be deserialized after API calls
    /// </summary>
    public class TransferListObject
    {
        public int TransferId { get; set; }
        public int AccountId { get; set; }
        public string? ToFromUser { get; set; }
        public decimal Amount { get; set; }
    }
}
