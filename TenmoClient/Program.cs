using System;
using System.Collections.Generic;
using TenmoClient.Data;

namespace TenmoClient
{
    class Program
    {
        private static readonly ConsoleService consoleService = new ConsoleService();
        private static readonly APIService apiService = new APIService();

        static void Main(string[] args)
        {
            Run();
        }

        private static void Run()
        {
            int loginRegister = -1;
            while (loginRegister != 1 && loginRegister != 2)
            {
                Console.WriteLine("Welcome to TEnmo!");
                Console.WriteLine("1: Login");
                Console.WriteLine("2: Register");
                Console.Write("Please choose an option: ");

                if (!int.TryParse(Console.ReadLine(), out loginRegister))
                {
                    Console.WriteLine("Invalid input. Please enter only a number.");
                }
                else if (loginRegister == 1)
                {
                    while (!UserService.IsLoggedIn()) //will keep looping until user is logged in
                    {
                        LoginUser loginUser = consoleService.PromptForLogin();
                        API_User user = apiService.Login(loginUser);
                        if (user != null)
                        {
                            UserService.SetLogin(user);
                        }
                    }
                }
                else if (loginRegister == 2)
                {
                    bool isRegistered = false;
                    while (!isRegistered) //will keep looping until user is registered
                    {
                        LoginUser registerUser = consoleService.PromptForLogin();
                        isRegistered = apiService.Register(registerUser);
                        if (isRegistered)
                        {
                            Console.WriteLine("");
                            Console.WriteLine("Registration successful. You can now log in.");
                            loginRegister = -1; //reset outer loop to allow choice for login
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Invalid selection.");
                }
            }
            Console.Clear();
            MenuSelection();
        }

        private static void MenuSelection()
        {

            int menuSelection = -1;
            while (menuSelection != 0)
            {
                Console.WriteLine("");
                Console.WriteLine("Welcome to TEnmo! Please make a selection: ");
                Console.WriteLine("1: View your current balance");
                Console.WriteLine("2: View your past transfers");
                Console.WriteLine("3: View your pending requests");
                Console.WriteLine("4: Send TE bucks");
                Console.WriteLine("5: Request TE bucks");
                Console.WriteLine("6: Log in as different user");
                Console.WriteLine("0: Exit");
                Console.WriteLine("---------");
                Console.Write("Please choose an option: ");

                if (!int.TryParse(Console.ReadLine(), out menuSelection))
                {
                    Console.WriteLine("Invalid input. Please enter only a number.");
                }
                else if (menuSelection == 1)
                {
                    Console.Clear();
                    decimal balance = apiService.GetBalance();
                    Console.WriteLine($"Your current account balance is: {balance.ToString("C2")}");
                    Console.ReadLine();
                    Console.Clear();
                }
                else if (menuSelection == 2)
                {
                    Console.Clear();
                    List<TransferListObject> transfers = apiService.GetTransfers();
                    Transfer specific = null;
                    int transferId = -1;
                    while (specific == null && transferId == -1)
                    {
                        Console.WriteLine("----------------------------------------");
                        Console.WriteLine("Transfers");
                        Console.WriteLine($"ID\t\tFrom/To\t\tAmount");
                        Console.WriteLine("----------------------------------------");
                        foreach (TransferListObject transfer in transfers)
                        {
                            Console.WriteLine($"{transfer.TransferId}\t\t{transfer.ToFromUser}  \t{transfer.Amount.ToString("C2")}");
                        }
                        transferId = ConsoleService.GetInteger("\nPlease enter transfer ID to view details(0 to cancel): ");
                        Console.Clear();
                        if (transferId != 0)
                        {
                            specific = apiService.GetSpecificTransfer(transferId);
                            if (specific == null)
                            {
                                continue;
                            }
                            Console.WriteLine("----------------------------------------");
                            Console.WriteLine("Transfer Details");
                            Console.WriteLine("----------------------------------------");
                            Console.WriteLine($"Id: {specific.TransferId}");
                            string username = apiService.GetUsernameFromAccountNumber(specific.AccountFromId);
                            Console.WriteLine($"From: {username}");
                            string otherUser = apiService.GetUsernameFromAccountNumber(specific.AccountToId);
                            Console.WriteLine($"To: {otherUser}");

                            Console.WriteLine($"Type: {specific.TransferTypeId}");

                            Console.WriteLine($"Status: {specific.TransferStatusId}");
                            Console.WriteLine($"Amount: {specific.Amount.ToString("C2")}");
                            Console.ReadLine();
                            Console.Clear();
                        }
                        else if (transferId == 0)
                        {
                            break;
                        }
                    }
                }
                else if (menuSelection == 3)
                {
                    Console.Clear();
                    List<Transfer> transfers = apiService.GetPendingTransfers();
                    bool transferExists = false;
                    int transferId = -1;
                    while (!transferExists && transferId != 0)
                    {
                        Console.WriteLine("----------------------------------------");
                        Console.WriteLine("Pending Transfers");
                        Console.WriteLine($"ID\t\tTo\t\tAmount");
                        Console.WriteLine("----------------------------------------");
                        foreach (Transfer transfer in transfers)
                        {
                            string username = apiService.GetUsernameFromAccountNumber(transfer.AccountToId);
                            Console.WriteLine($"{transfer.TransferId}\t\t{username}\t\t{transfer.Amount.ToString("C2")}");
                        }
                        transferId = ConsoleService.GetInteger("\nPlease enter transfer ID to approve / reject(0 to cancel): ");
                        if (transferId == 0)
                        {
                            Console.Clear();
                            break;
                        }
                        foreach (Transfer transfer in transfers)
                        {
                            if (transferId == transfer.TransferId)
                            {
                                transferExists = true;
                            }
                        }
                        Console.Clear();
                    }

                    if (transferId != 0)
                    {
                        int selection = ConsoleService.GetInteger("1: Approve\n2: Reject\n0: Don't approve or reject\n----------------------------------------\nPlease choose an option: ");
                        Console.Clear();
                        UpdateTransfer uT = new UpdateTransfer();

                        bool wasSuccessful = false;

                        if (selection == 1)
                        {
                            uT.TransferId = transferId;
                            uT.isApproved = true;
                            wasSuccessful = apiService.UpdatePendingTransfer(uT);
                        }
                        else if (selection == 2)
                        {
                            uT.TransferId = transferId;
                            uT.isApproved = false;
                            wasSuccessful = apiService.UpdatePendingTransfer(uT);
                        }

                        if (uT.isApproved && wasSuccessful)
                        {
                            Console.Clear();
                            Console.WriteLine("The transaction was approved.");
                        }
                        else if (uT.isApproved)
                        {
                            Console.Clear();
                            Console.WriteLine("Insufficient funds to complete the transaction.");
                        }
                        else if (!uT.isApproved && wasSuccessful)
                        {
                            Console.Clear();
                            Console.WriteLine("The transaction was successfully rejected.");
                        }

                    }
                }
                else if (menuSelection == 4)
                {
                    int otherUserId = -1;
                    decimal amount = -1;
                    bool userExists = false;
                    while (!userExists)
                    {
                        Console.Clear();
                        List<UserInfo> users = apiService.GetAllUsers();
                        Console.WriteLine("----------------------------------------");
                        Console.WriteLine("Users");
                        Console.WriteLine("ID  \t\tName");
                        Console.WriteLine("----------------------------------------");
                        foreach (UserInfo user in users)
                        {
                            Console.WriteLine($"{user.UserId}\t\t{user.Username}");
                        }
                        otherUserId = ConsoleService.GetInteger("Enter ID of user you are sending to (0 to cancel): ");
                        foreach (UserInfo user in users)
                        {
                            if (otherUserId == user.UserId)
                            {
                                userExists = true;
                            }
                        }
                        if (otherUserId == 0)
                        {
                            Console.Clear();
                            break;
                        }
                        else if (userExists)
                        {
                            while (amount < .01M)
                            {
                                amount = ConsoleService.GetDecimal("Enter amount: ");
                            }
                            ClientTransfer cT = new ClientTransfer(otherUserId, amount, false);
                            bool wasSuccessful = apiService.SendMoney(cT);
                            if (wasSuccessful)
                            {
                                Console.Clear();
                                string receiverName = "";
                                foreach (UserInfo user in users)
                                {
                                    if (user.UserId == otherUserId)
                                    {
                                        receiverName = user.Username;
                                    }
                                }
                                Console.WriteLine($"Transfer of {amount.ToString("C2")} to {receiverName} was successful");
                            }
                            else
                            {
                                Console.WriteLine("Transaction was not successful, please try again");
                            }
                        }
                    }
                }
                else if (menuSelection == 5)
                {
                    int otherUserId = -1;
                    decimal amount = -1;
                    bool userExists = false;

                    while (!userExists)
                    {
                        List<UserInfo> users = apiService.GetAllUsers();
                        Console.WriteLine("----------------------------------------");
                        Console.WriteLine("Users");
                        Console.WriteLine("ID  \t\tName");
                        Console.WriteLine("----------------------------------------");
                        foreach (UserInfo user in users)
                        {
                            Console.WriteLine($"{user.UserId}\t\t{user.Username}");
                        }
                        otherUserId = ConsoleService.GetInteger("Enter ID of user you are requesting from (0 to cancel): ");
                        string receiverName = "";
                        foreach (UserInfo user in users)
                        {
                            if (otherUserId == user.UserId)
                            {
                                userExists = true;
                                receiverName = user.Username;
                            }
                        }
                        if (otherUserId == 0)
                        {
                            Console.Clear();
                            break;
                        }
                        else if (userExists)
                        {
                            while (amount < .01M)
                            {
                                amount = ConsoleService.GetDecimal("Enter amount: ");
                            }
                            ClientTransfer cT = new ClientTransfer(otherUserId, amount, true);
                            bool wasSuccessful = apiService.SendMoney(cT);
                            if (wasSuccessful)
                            {
                                Console.Clear();
                                Console.WriteLine($"Transfer of {amount.ToString("C2")} from {receiverName} was created successfully");
                            }
                            else
                            {
                                Console.WriteLine("Transaction was not successful, please try again");
                            }
                        }
                    }
                }
                else if (menuSelection == 6)
                {
                    Console.WriteLine("");
                    UserService.SetLogin(new API_User()); //wipe out previous login info
                    Console.Clear();
                    Run(); //return to entry point
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("Goodbye!");
                    Environment.Exit(0);
                }
            }
        }
    }
}
