using System;
using System.Collections.Generic;
using System.Text;

namespace TenmoClient.Data
{
    /// <summary>
    /// Transfer class mirrors the transfer objects created on the server
    /// to allow them to be deserialized after API calls
    /// </summary>
    public class Transfer
    {
        public int TransferId { get; set; }
        public int TransferTypeId { get; set; }
        public int TransferStatusId { get; set; }
        public int AccountFromId { get; set; }
        public int AccountToId { get; set; }
        public decimal Amount { get; set; }
    }
}
