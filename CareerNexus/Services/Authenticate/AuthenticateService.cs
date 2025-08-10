using CareerNexus.Common;
using CareerNexus.Models.RequestModel;
using CareerNexus.Models.UserModel;
using CareerNexus.Services.OtpService;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CareerNexus.Services.Authenticate
{
    public class AuthenticateService : IAuthenticate
    {
        private readonly IOTP _otpservice;

        public AuthenticateService(IOTP otpservice)
        {
                _otpservice = otpservice;
        }
        public async Task<ClaimResponseModel> Authenticate(AuthenticationRequestModel request)
        {
            ClaimResponseModel model = new ClaimResponseModel();
            try
            {
                string query = "select * from users where Email = @Email and PasswordHash=@Password";
                SqlCommand cmd = new SqlCommand();
                cmd.Parameters.AddWithValue("@Email", request.Email);
                cmd.Parameters.AddWithValue("@Password", request.Password);
                cmd.CommandText= query;
                cmd.CommandType = CommandType.Text;

               DataTable dt = DBEngine.GetDataTable(cmd, Databaseoperations.Select, query);
                List<UserModel> users =dt.ToModelList<UserModel>();
                if (users == null || users.Count == 0)
                {
                    return null;
                }

                UserModel user = users.First(); // Get the first matched user
                return await _otpservice.GenerateToken(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Authenticate Error: {ex.Message}");
                return model;
            }
        }
        public UserModel Signup(UserModel user)
        {
            string query = "Insert into Users(Username,Email,Fullname,PasswordHash,CreatedOn) values (@Username,@Email,@Fullname,@PasswordHash,GetDate())";
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = query; // 👈 REQUIRED
            cmd.CommandType = CommandType.Text;

            cmd.Parameters.AddWithValue("@Username", user.Username);
            cmd.Parameters.AddWithValue("@Email", user.Email);
            cmd.Parameters.AddWithValue("@Fullname", user.Fullname);
            cmd.Parameters.AddWithValue("@PasswordHash", user.PassswordHash);



            var result = DBEngine.ExecuteNonQuery(cmd,Databaseoperations.Insert,query);
            if (result)
            {
                return user;
            }

            return null;
        }
    }
}
        
    
