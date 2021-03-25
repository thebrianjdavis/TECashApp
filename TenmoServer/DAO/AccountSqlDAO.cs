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
    public class AccountSqlDAO : IAccountDAO
    {
        private readonly string connectionString;
        public AccountSqlDAO(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }

        /// <summary>
        /// GetAccount accepts a userid and returns an account object from db
        /// (object remains on the server to protect information)
        /// </summary>
        public Account GetAccount(int userId)
        {
            Account returnAccount = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sqlQuery = "SELECT account_id, user_id, balance FROM accounts WHERE user_id = @userid;";
                    SqlCommand cmd = new SqlCommand(sqlQuery, conn);
                    cmd.Parameters.AddWithValue("@userid", userId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        returnAccount = GetAccountFromReader(reader);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return returnAccount;

        }

        /// <summary>
        /// IncreaseBalance updates a balance on the db for a specified account
        /// </summary>
        public bool IncreaseBalance(int accountId, decimal addToBalance)
        {
            int rowsAffected;

            decimal balance = GetBalance(accountId);
            balance += addToBalance;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sqlQuery = "UPDATE accounts SET balance = @balance WHERE account_id = @account_id;";
                    SqlCommand cmd = new SqlCommand(sqlQuery, conn);
                    cmd.Parameters.AddWithValue("@account_id", accountId);
                    cmd.Parameters.AddWithValue("@balance", balance);
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
        /// DecreaseBalance updates a balance on the db for a specified account
        /// - fund availability is confirmed before this method is invoked
        /// </summary>
        public bool DecreaseBalance(int accountId, decimal subtractFromBalance)
        {
            int rowsAffected;
            decimal balance = GetBalance(accountId);
            balance -= subtractFromBalance;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sqlQuery = "UPDATE accounts SET balance = @balance WHERE account_id = @account_id;";
                    SqlCommand cmd = new SqlCommand(sqlQuery, conn);
                    cmd.Parameters.AddWithValue("@account_id", accountId);
                    cmd.Parameters.AddWithValue("@balance", balance);
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
        /// GetBalance returns a balance from the db
        /// - this can only be called for the logged in user
        /// </summary>
        public decimal GetBalance (int accountId)
        {
            Account returnAccount = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sqlQuery = "SELECT account_id, user_id, balance FROM accounts WHERE account_id = @account_id;";
                    SqlCommand cmd = new SqlCommand(sqlQuery, conn);
                    cmd.Parameters.AddWithValue("@account_id", accountId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        returnAccount = GetAccountFromReader(reader);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }
            return returnAccount.Balance;
        }

        /// <summary>
        /// GetAccountFromReader is a helper method to create an object from SQL data
        /// </summary>
        private Account GetAccountFromReader(SqlDataReader reader)
        {
            Account a = new Account()
            {
                AccountId = Convert.ToInt32(reader["account_id"]),
                UserId = Convert.ToInt32(reader["user_id"]),
                Balance = Convert.ToDecimal(reader["balance"]),
            };

            return a;
        }
    }
}
