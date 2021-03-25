using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TenmoServer.Models
{
    /// <summary>
    /// model to create a transfer in the database
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

    /// <summary>
    /// model to create a list of transfers searchable by client
    /// </summary>
    public class TransferListObject
    {
        public int TransferId { get; set; }
        public int AccountId { get; set; }
        public string? ToFromUser { get; set; }
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// Client transfer allows user to send funds while keeping account
    /// numbers and balances secure on server
    /// </summary>
    public class ClientTransfer
    {
        public int? TransferId { get; set; }
        public int OtherUserId { get; set; }
        public decimal Amount { get; set; }
        public bool IsRequest { get; set; }
    }

    /// <summary>
    /// UpdateTransfer class protects information while allowing a specific transaction
    /// to be approved by the from account
    /// </summary>
    public class UpdateTransfer
    {
        public int TransferId { get; set; }
        public bool isApproved { get; set; }
    }
}
