using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public interface IAccountDAO
    {
        Account GetAccount(int userId);
        decimal GetBalance(int accountId);
        bool IncreaseBalance(int accountId, decimal addToBalance);
        bool DecreaseBalance(int accountId, decimal subtractFromBalance);
    }
}
