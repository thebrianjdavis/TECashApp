using System.Collections.Generic;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public interface IUserDAO
    {
        User GetUser(string username);
        UserInfo GetOtherUser(int userId);
        User AddUser(string username, string password);
        List<User> GetUsers();
    }
}
