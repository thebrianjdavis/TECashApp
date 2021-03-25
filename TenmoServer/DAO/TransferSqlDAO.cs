using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenmoServer.Models;
using TenmoServer.Security;
using TenmoServer.Security.Models;
using System.Data.SqlClient;

namespace TenmoServer.DAO
{
    public class TransferSqlDAO : ITransferDAO
    {
        private readonly string connectionString;
        public TransferSqlDAO(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }

        /// <summary>
        /// CreateTransfer method creates a record of a transfer on the db
        /// </summary>
        public bool CreateTransfer(int userAccountId, int otherAccountId, ClientTransfer clientTransfer)
        {
            int rowsAffected;
            int statusId = 0;
            int typeId = 0;
            int originId = 0;
            int destinationId = 0;
            decimal amount = clientTransfer.Amount;
            if (clientTransfer.IsRequest)
            {
                statusId = 1;
                typeId = 1;
                destinationId = userAccountId;
                originId = otherAccountId;
            }
            else
            {
                statusId = 2;
                typeId = 2;
                originId = userAccountId;
                destinationId = otherAccountId;
            }
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sqlQuery = "INSERT INTO transfers (transfer_type_id, transfer_status_id, account_from, account_to, amount) VALUES (@typeId, @statusId, @accountFrom, @accountTo, @amount);";
                    SqlCommand cmd = new SqlCommand(sqlQuery, conn);
                    cmd.Parameters.AddWithValue("@typeId", typeId);
                    cmd.Parameters.AddWithValue("@statusId", statusId);
                    cmd.Parameters.AddWithValue("@accountFrom", originId);
                    cmd.Parameters.AddWithValue("@accountTo", destinationId);
                    cmd.Parameters.AddWithValue("@amount", amount);
                    rowsAffected = cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return (rowsAffected > 0);
        }

        /// <summary>
        /// UpdatePendingTransfer method updates a transfer on the db as approved or rejected
        /// </summary>
        public bool UpdatePendingTransfer(int accountId, UpdateTransfer transfer)
        {
            Transfer checkTrans = GetTransferDetails(transfer.TransferId);
            int rowsAffected = 0;
            int transferStatus;
            if (transfer.isApproved)
            {
                transferStatus = 2;
            }
            else
            {
                transferStatus = 3;
            }
            if (accountId == checkTrans.AccountFromId)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        string sqlQuery = "UPDATE transfers SET transfer_status_id = @statusId WHERE transfer_id = @transferId;";
                        SqlCommand cmd = new SqlCommand(sqlQuery, conn);
                        cmd.Parameters.AddWithValue("@transferId", transfer.TransferId);
                        cmd.Parameters.AddWithValue("@statusId", transferStatus);
                        rowsAffected = cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            return (rowsAffected > 0);
        }

        /// <summary>
        /// GetTransfers returns a list of TransferListObjects where the user account matches either
        /// the account to or account from on the transfers table in db
        /// </summary>
        public List<TransferListObject> GetTransfers(int userId)
        {
            List<TransferListObject> transfers = new List<TransferListObject>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sqlQuery = "SELECT t.transfer_id, t.account_to, t.amount FROM users u JOIN accounts a ON u.user_id = a.user_id JOIN transfers t ON a.account_id = t.account_from WHERE u.user_id = @userid";
                    SqlCommand cmd = new SqlCommand(sqlQuery, conn);
                    cmd.Parameters.AddWithValue("@userid", userId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        TransferListObject t = GetTLOTFromReader(reader);
                        string toName = GetUsernameForAccount(t.AccountId);
                        t.ToFromUser = $"To: {toName}";
                        transfers.Add(t);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sqlQuery = "SELECT t.transfer_id, t.account_from, t.amount FROM users u JOIN accounts a ON u.user_id = a.user_id JOIN transfers t ON a.account_id = t.account_to WHERE u.user_id = @userid";
                    SqlCommand cmd = new SqlCommand(sqlQuery, conn);
                    cmd.Parameters.AddWithValue("@userid", userId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        TransferListObject t = GetTLOFFromReader(reader);
                        string fromName = GetUsernameForAccount(t.AccountId);
                        t.ToFromUser = $"From: {fromName}";
                        transfers.Add(t);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return transfers;
        }

        /// <summary>
        /// GetPendingTransfers returns a list of Transfers where the user account matches
        /// the account from and the status is pending on the transfers table in db
        /// </summary>
        public List<Transfer> GetPendingTransfers(int userId)
        {
            List<Transfer> transfers = new List<Transfer>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sqlQuery = "SELECT t.transfer_id, t.transfer_type_id, t.transfer_status_id, t.account_from, t.account_to, t.amount FROM users u JOIN accounts a ON u.user_id = a.user_id JOIN transfers t ON a.account_id = t.account_from WHERE u.user_id = @userid AND t.transfer_type_id = 1 AND t.transfer_status_id = 1";
                    SqlCommand cmd = new SqlCommand(sqlQuery, conn);
                    cmd.Parameters.AddWithValue("@userid", userId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        Transfer t = GetTransferFromReader(reader);
                        transfers.Add(t);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return transfers;
        }

        /// <summary>
        /// GetTransferDetails returns a specific Transfer from the transfers table in db
        /// </summary>
        public Transfer GetTransferDetails(int transferId)
        {
            Transfer transfer = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sqlQuery = "SELECT transfer_id, transfer_type_id, transfer_status_id, account_from, account_to, amount FROM transfers WHERE transfer_id = @transferid;";
                    SqlCommand cmd = new SqlCommand(sqlQuery, conn);
                    cmd.Parameters.AddWithValue("@transferid", transferId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        transfer = GetTransferFromReader(reader);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return transfer;
        }

        /// <summary>
        /// GetUsernameForAccount returns a string value for the name of the account holder
        /// using the account id
        /// </summary>
        public string GetUsernameForAccount(int accountId)
        {
            string username = "";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sqlQuery = "SELECT u.username FROM accounts a JOIN users u ON a.user_id = u.user_id WHERE a.account_id = @accountId;";
                    SqlCommand cmd = new SqlCommand(sqlQuery, conn);
                    cmd.Parameters.AddWithValue("@accountId", accountId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        username = Convert.ToString(reader["username"]);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }
            return username;
        }

        /// <summary>
        /// GetTransferFromReader is a helper method to create a Transfer object from SQL data
        /// </summary>
        private Transfer GetTransferFromReader(SqlDataReader reader)
        {
            Transfer t = new Transfer()
            {
                TransferId = Convert.ToInt32(reader["transfer_id"]),
                TransferTypeId = Convert.ToInt32(reader["transfer_type_id"]),
                TransferStatusId = Convert.ToInt32(reader["transfer_status_id"]),
                AccountFromId = Convert.ToInt32(reader["account_from"]),
                AccountToId = Convert.ToInt32(reader["account_to"]),
                Amount = Convert.ToDecimal(reader["amount"]),
            };

            return t;
        }

        /// <summary>
        /// GetTLOTFromReader is a helper method to create a TransferListObject from SQL data
        /// TLOT - TransferListObject To
        /// </summary>
        private TransferListObject GetTLOTFromReader(SqlDataReader reader)
        {
            TransferListObject t = new TransferListObject()
            {
                TransferId = Convert.ToInt32(reader["transfer_id"]),
                AccountId = Convert.ToInt32(reader["account_to"]),
                Amount = Convert.ToDecimal(reader["amount"]),
            };

            return t;
        }

        /// <summary>
        /// GetTLOFFromReader is a helper method to create a TransferListObject from SQL data
        /// TLOF - TransferListObject From
        /// </summary>
        private TransferListObject GetTLOFFromReader(SqlDataReader reader)
        {
            TransferListObject t = new TransferListObject()
            {
                TransferId = Convert.ToInt32(reader["transfer_id"]),
                AccountId = Convert.ToInt32(reader["account_from"]),
                Amount = Convert.ToDecimal(reader["amount"]),
            };

            return t;
        }
    }
}
