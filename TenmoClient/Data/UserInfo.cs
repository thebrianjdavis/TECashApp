using System;
using System.Collections.Generic;
using System.Text;

namespace TenmoClient.Data
{
    /// <summary>
    /// UserInfo class mirrors the UserInfo objects sent from the server
    /// to allow them to be deserialized after API calls
    /// </summary>
    public class UserInfo
    {
        public int UserId { get; set; }
        public string Username { get; set; }
    }
}
