using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using TenmoServer.Models;
using TenmoServer.Security;
using TenmoServer.Security.Models;

namespace TenmoServer.DAO
{
    public class UserSqlDAO : IUserDAO
    {
        private readonly string connectionString;
        const decimal startingBalance = 1000;

        public UserSqlDAO(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }

        /// <summary>
        /// GetUser returns a User object from the db using the username
        /// </summary>
        public User GetUser(string username)
        {
            User returnUser = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT user_id, username, password_hash, salt FROM users WHERE username = @username", conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        returnUser = GetUserFromReader(reader);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return returnUser;
        }

        /// <summary>
        /// GetOtherUser returns a UserInfo object from the db using the user id
        /// </summary>
        public UserInfo GetOtherUser(int userId)
        {
            UserInfo returnUser = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT user_id, username FROM users WHERE user_id = @userId", conn);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        returnUser = GetUserInfoFromReader(reader);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return returnUser;
        }

        /// <summary>
        /// GetUsers returns a list of User objects from the db for all users
        /// </summary>
        public List<User> GetUsers()
        {
            List<User> returnUsers = new List<User>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT user_id, username, password_hash, salt FROM users", conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        User u = GetUserFromReader(reader);
                        returnUsers.Add(u);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return returnUsers;
        }

        /// <summary>
        /// AddUser creates a new user in the db
        /// </summary>
        public User AddUser(string username, string password)
        {
            IPasswordHasher passwordHasher = new PasswordHasher();
            PasswordHash hash = passwordHasher.ComputeHash(password);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("INSERT INTO users (username, password_hash, salt) VALUES (@username, @password_hash, @salt)", conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password_hash", hash.Password);
                    cmd.Parameters.AddWithValue("@salt", hash.Salt);
                    cmd.ExecuteNonQuery();

                    cmd = new SqlCommand("SELECT @@IDENTITY", conn);
                    int userId = Convert.ToInt32(cmd.ExecuteScalar());

                    cmd = new SqlCommand("INSERT INTO accounts (user_id, balance) VALUES (@userid, @startBalance)", conn);
                    cmd.Parameters.AddWithValue("@userid", userId);
                    cmd.Parameters.AddWithValue("@startBalance", startingBalance);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return GetUser(username);
        }

        /// <summary>
        /// GetUserFromReader is a helper method to create a User object from SQL data
        /// </summary>
        private User GetUserFromReader(SqlDataReader reader)
        {
            User u = new User()
            {
                UserId = Convert.ToInt32(reader["user_id"]),
                Username = Convert.ToString(reader["username"]),
                PasswordHash = Convert.ToString(reader["password_hash"]),
                Salt = Convert.ToString(reader["salt"]),
            };

            return u;
        }

        /// <summary>
        /// GetUserInfoFromReader is a helper method to create a UserInfo object from SQL data
        /// </summary>
        private UserInfo GetUserInfoFromReader(SqlDataReader reader)
        {
            int userId = Convert.ToInt32(reader["user_id"]);
            string username = Convert.ToString(reader["username"]);
            UserInfo uI = new UserInfo(userId, username);

            return uI;
        }
    }
}
