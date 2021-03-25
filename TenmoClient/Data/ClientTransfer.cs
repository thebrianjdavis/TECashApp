using System;
using System.Collections.Generic;
using System.Text;

namespace TenmoClient.Data
{
    /// <summary>
    /// ClientTransfer is almost a full Transfer object, but only includes
    /// the properties necessary to create a new Transfer on the server
    /// </summary>
    public class ClientTransfer
    {
        public int? TransferId { get; set; }
        public int OtherUserId { get; set; }
        public decimal Amount { get; set; }
        public bool IsRequest { get; set; }
        public ClientTransfer(int otherUserId, decimal amount, bool isRequest)
        {
            OtherUserId = otherUserId;
            Amount = amount;
            IsRequest = isRequest;
        }
    }
}
