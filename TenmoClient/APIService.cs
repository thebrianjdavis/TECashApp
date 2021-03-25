using RestSharp;
using RestSharp.Authenticators;
using System;
using TenmoClient.Data;
using System.Collections.Generic;

namespace TenmoClient
{
    public class APIService
    {
        private readonly static string API_BASE_URL = "https://localhost:44315/";
        private readonly static string API_URL = "https://localhost:44315/transaction/";
        private readonly IRestClient client = new RestClient();

        public decimal GetBalance()
        {
            RestRequest request = new RestRequest(API_URL + "balance");
            IRestResponse<decimal> response = client.Get<decimal>(request);

            if (response.ResponseStatus != ResponseStatus.Completed)
            {
                throw new Exception("Error occurred - unable to reach server.", response.ErrorException);
            }
            else if (!response.IsSuccessful)
            {
                throw new Exception("Error occurred - received non-success response: " + (int)response.StatusCode);
            }
            else
            {
                return response.Data;
            }
        }
        public List<TransferListObject> GetTransfers()
        {
            RestRequest request = new RestRequest(API_URL + "transfers");
            IRestResponse<List<TransferListObject>> response = client.Get<List<TransferListObject>>(request);

            if (response.ResponseStatus != ResponseStatus.Completed || !response.IsSuccessful)
            {
                //ProcessErrorResponse(response);
            }
            else
            {
                return response.Data;
            }
            return null;
        }

        public Transfer GetSpecificTransfer(int transferId)
        {
            RestRequest request = new RestRequest(API_URL + "transfers/" + transferId);
            IRestResponse<Transfer> response = client.Get<Transfer>(request);

            if (response.ResponseStatus != ResponseStatus.Completed || !response.IsSuccessful)
            {
                //ProcessErrorResponse(response);
            }
            else
            {
                return response.Data;
            }
            return null;
        }
        public List<Transfer> GetPendingTransfers()
        {
            RestRequest request = new RestRequest(API_URL + "pending");
            IRestResponse<List<Transfer>> response = client.Get<List<Transfer>>(request);

            if (response.ResponseStatus != ResponseStatus.Completed || !response.IsSuccessful)
            {
                //ProcessErrorResponse(response);
            }
            else
            {
                return response.Data;
            }
            return null;
        }

        public bool UpdatePendingTransfer(UpdateTransfer updateTransfer)
        {
            RestRequest request = new RestRequest(API_URL + "request");
            request.AddJsonBody(updateTransfer);
            IRestResponse<bool> response = client.Put<bool>(request);

            if (response.ResponseStatus != ResponseStatus.Completed || !response.IsSuccessful)
            {
                //ProcessErrorResponse(response);
            }
            else
            {
                return response.Data;
            }
            return false;
        }

        public string GetUsernameFromAccountNumber(int accountId)
        {
            RestRequest request = new RestRequest(API_URL + "accounts/" + accountId);
            IRestResponse<string> response = client.Get<string>(request);

            if (response.ResponseStatus != ResponseStatus.Completed || !response.IsSuccessful)
            {
                //ProcessErrorResponse(response);
            }
            else
            {
                return response.Data;
            }
            return null;
        }

        public List<UserInfo> GetAllUsers()
        {
            RestRequest request = new RestRequest(API_URL + "users");
            IRestResponse<List<UserInfo>> response = client.Get<List<UserInfo>>(request);

            if (response.ResponseStatus != ResponseStatus.Completed || !response.IsSuccessful)
            {
                //ProcessErrorResponse(response);
            }
            else
            {
                return response.Data;
            }
            return null;
        }
        public bool SendMoney(ClientTransfer transfer)
        {
            RestRequest request = new RestRequest(API_URL + "request");
            request.AddJsonBody(transfer);
            IRestResponse<bool> response = client.Post<bool>(request);

            if (response.ResponseStatus != ResponseStatus.Completed || !response.IsSuccessful)
            {
                //ProcessErrorResponse(response);
            }
            else
            {
                return response.Data;
            }
            return false;
        }

        //login endpoints
        public bool Register(LoginUser registerUser)
        {
            RestRequest request = new RestRequest(API_BASE_URL + "login/register");
            request.AddJsonBody(registerUser);
            IRestResponse<API_User> response = client.Post<API_User>(request);

            if (response.ResponseStatus != ResponseStatus.Completed)
            {
                Console.WriteLine("An error occurred communicating with the server.");
                return false;
            }
            else if (!response.IsSuccessful)
            {
                if (!string.IsNullOrWhiteSpace(response.Data.Message))
                {
                    Console.WriteLine("An error message was received: " + response.Data.Message);
                }
                else
                {
                    Console.WriteLine("An error response was received from the server. The status code is " + (int)response.StatusCode);
                }
                return false;
            }
            else
            {
                return true;
            }
        }
        public API_User Login(LoginUser loginUser)
        {
            RestRequest request = new RestRequest(API_BASE_URL + "login");
            request.AddJsonBody(loginUser);
            IRestResponse<API_User> response = client.Post<API_User>(request);

            if (response.ResponseStatus != ResponseStatus.Completed)
            {
                Console.WriteLine("An error occurred communicating with the server.");
                return null;
            }
            else if (!response.IsSuccessful)
            {
                if (!string.IsNullOrWhiteSpace(response.Data.Message))
                {
                    Console.WriteLine("An error message was received: " + response.Data.Message);
                }
                else
                {
                    Console.WriteLine("An error response was received from the server. The status code is " + (int)response.StatusCode);
                }
                return null;
            }
            else
            {
                client.Authenticator = new JwtAuthenticator(response.Data.Token);
                return response.Data;
            }
        }
    }
}
