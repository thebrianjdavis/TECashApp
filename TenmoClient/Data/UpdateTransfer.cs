using System;
using System.Collections.Generic;
using System.Text;

namespace TenmoClient.Data
{
    /// <summary>
    /// UpdateTransfer class mirrors the UpdateTransfer objects to allow them
    /// to be serialized and sent to the server via the API
    /// </summary>
    public class UpdateTransfer
    {
        public int TransferId { get; set; }
        public bool isApproved { get; set; }
    }
}
