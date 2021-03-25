using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public interface ITransferDAO
    {
        bool CreateTransfer(int userAccountId, int otherAccountId, ClientTransfer clientTransfer);
        bool UpdatePendingTransfer(int accountId, UpdateTransfer transfer);
        List<Transfer> GetPendingTransfers(int userId);
        List<TransferListObject> GetTransfers(int userId);
        Transfer GetTransferDetails(int transferId);
        string GetUsernameForAccount(int accountId);
    }
}
