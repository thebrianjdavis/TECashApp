using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TenmoServer.DAO;
using TenmoServer.Models;
using TenmoServer.Security;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace TenmoServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class TransactionController : ControllerBase
    {
        private readonly IAccountDAO accountDAO;
        private readonly ITransferDAO transferDAO;
        private readonly IUserDAO userDAO;

        public TransactionController(IAccountDAO _accountDAO, ITransferDAO _transferDAO, IUserDAO _userDAO)
        {
            accountDAO = _accountDAO;
            transferDAO = _transferDAO;
            userDAO = _userDAO;
        }

        /// <summary>
        /// GetBalance method returns a decimal for the balance based upon the token
        /// for the logged in user
        /// </summary>
        [HttpGet("balance")]
        public ActionResult<decimal> GetBalance()
        {
            int userId = int.Parse(User.FindFirst("sub")?.Value);
            Account userAccount = accountDAO.GetAccount(userId);
            return Ok(userAccount.Balance);
        }
        
        /// <summary>
        /// GetAllUsers method returns a list of UserInfo (UserId and Username only)
        /// objects so that password hashes, salts and emails are not sent to client
        /// devices - logged in user is not included in the list
        /// </summary>
        [HttpGet("users")]
        public ActionResult<List<UserInfo>> GetAllUsers()
        {
            List<UserInfo> userInfoList = new List<UserInfo>();
            List<User> userList = userDAO.GetUsers();
            foreach (User user in userList)
            {
                if (user.UserId == int.Parse(User.FindFirst("sub")?.Value))
                {

                }
                else
                {
                    UserInfo uI = new UserInfo(user.UserId, user.Username);
                    userInfoList.Add(uI);
                }
            }
            return Ok(userInfoList);
        }

        /// <summary>
        /// GetUsernameFromAccount returns the username for the user associated with
        /// a specific account
        /// </summary>
        [HttpGet("accounts/{accountId}")]
        public ActionResult<string> GetUsernameFromAccount(int accountId)
        {
            string username = transferDAO.GetUsernameForAccount(accountId);
            return Ok(username);
        }

        /// <summary>
        /// SendMoney returns true if a transfer was successfully logged in the db
        /// a "send" transfer is true only if funds were sufficient and logged on db
        /// a "request" transfer is true if it is logged in the db
        /// </summary>
        [HttpPost("request")]
        public ActionResult<bool> SendMoney(ClientTransfer clientTransfer)
        {
            int userId = int.Parse(User.FindFirst("sub")?.Value);
            Account userAccount = accountDAO.GetAccount(userId);
            Account destAccount = accountDAO.GetAccount(clientTransfer.OtherUserId);
            if (clientTransfer.IsRequest)
            {
                return Ok(transferDAO.CreateTransfer(userAccount.AccountId, destAccount.AccountId, clientTransfer));
            }
            else
            {
                if (userAccount.Balance >= clientTransfer.Amount)
                {
                    accountDAO.DecreaseBalance(userAccount.AccountId, clientTransfer.Amount);
                    accountDAO.IncreaseBalance(destAccount.AccountId, clientTransfer.Amount);
                    
                    return Ok(transferDAO.CreateTransfer(userAccount.AccountId, destAccount.AccountId, clientTransfer));
                }
                else
                {
                    return BadRequest("Insufficient funds to complete the transaction");
                }
            }
        }

        /// <summary>
        /// GetUserTransfers returns a list of TransferListObjects
        /// </summary>
        [HttpGet("transfers")]
        public ActionResult<List<TransferListObject>> GetUserTransfers()
        {
            int userId = int.Parse(User.FindFirst("sub")?.Value);
            return Ok(transferDAO.GetTransfers(userId));
        }

        /// <summary>
        /// GetSpecificTransfer returns a transfer only if the account for the
        /// logged in user is in either the account from or account to fields
        /// </summary>
        [HttpGet("transfers/{transferId}")]
        public ActionResult<Transfer> GetSpecificTransfer(int transferId)
        {
            int userId = int.Parse(User.FindFirst("sub")?.Value);
            List<TransferListObject> listTransfers = transferDAO.GetTransfers(userId);
            Transfer xfer = null;
            bool isInList = false;

            foreach (TransferListObject transfer in listTransfers)
            {
                if (transferId == transfer.TransferId)
                {
                    isInList = true;
                    break;
                }
            }

            if (isInList)
            {
                return Ok(xfer = transferDAO.GetTransferDetails(transferId));
            }
            else
            {
                return Unauthorized();
            }
        }

        /// <summary>
        /// GetPendingTransfers returns a list of transfers where the logged in
        /// user is the party sending money (account from)
        /// </summary>
        [HttpGet("pending")]
        public List<Transfer> GetPendingTransfers()
        {
            int userId = int.Parse(User.FindFirst("sub")?.Value);
            return transferDAO.GetPendingTransfers(userId);
        }
        
        /// <summary>
        /// UpdateTransfer updates a transfer in the db where the logged in
        /// user is the party sending money (account from)
        /// The transaction cannot be approved if the user has insufficient
        /// funds - but can be rejected in any circumstance
        /// </summary>
        [HttpPut("request")]
        public ActionResult<bool> UpdateTransfer(UpdateTransfer transfer)
        {
            bool wasSuccessful = false;

            int userId = int.Parse(User.FindFirst("sub")?.Value);
            Transfer pending = transferDAO.GetTransferDetails(transfer.TransferId);
            Account userAccount = accountDAO.GetAccount(userId);
            if (userAccount.Balance >= pending.Amount && transfer.isApproved == true)
            {
                accountDAO.DecreaseBalance(userAccount.AccountId, pending.Amount);
                accountDAO.IncreaseBalance(pending.AccountToId, pending.Amount);
                wasSuccessful = transferDAO.UpdatePendingTransfer(userAccount.AccountId, transfer);
                return Ok(wasSuccessful);
            }
            else if (transfer.isApproved == true)
            {
                return BadRequest("Insufficient funds to complete the transfer");
            }
            else
            {
                wasSuccessful = transferDAO.UpdatePendingTransfer(userAccount.AccountId, transfer);
                return Ok("The request was succesfully rejected");
            }
        }
    }
}
