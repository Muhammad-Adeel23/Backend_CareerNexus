using Azure.Core;
using CareerNexus.Common;
using CareerNexus.Models.Authetication;
using CareerNexus.Models.RequestModel;
using CareerNexus.Models.UserModel;
using CareerNexus.Services.OtpService;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CareerNexus.Services.Authenticate
{
    public class AuthenticateService : IAuthenticate
    {
        private readonly IOTP _otpservice;
        private readonly ILogger<AuthenticateService> __logger;

        public AuthenticateService(IOTP otpservice,ILogger<AuthenticateService>logger)
        {
           __logger = logger;
            _otpservice = otpservice;
        }
       
        public async Task<ClaimResponseModel> Authenticate(AuthenticationRequestModel request)
        {
            ClaimResponseModel model = new ClaimResponseModel();
            try
            {
                string query = @"select U.*,R.RoleName,R.RoleType,UR.RoleId,R.Id  from users U
                                 INNER JOIN UserRoles UR  ON UR.UserId = U.Id
                                 Left JOIN Roles R ON UR.RoleId = R.Id
                                 where Email = @Email and PasswordHash=@Password";
                SqlCommand cmd = new SqlCommand();
                cmd.Parameters.AddWithValue("@Email", request.Email==null? (object)DBNull.Value:request.Email);
                cmd.Parameters.AddWithValue("@Password", request.Password==null?(object)DBNull.Value:Helper.EncryptString(request.Password));
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
        public async Task<long> Register(UserRequestModel user)
        {
            long isinserted = 0;
            try
            {
                string chechQuery = "Select ID from Users Where Email = @Email";
                SqlCommand SqlCmd = new SqlCommand();
                SqlCmd.CommandText = chechQuery;
                SqlCmd.CommandType = CommandType.Text;

                SqlCmd.Parameters.AddWithValue("@Email", user.Email == null ? (object)DBNull.Value : user.Email);
                DataTable check_dt = DBEngine.GetDataTable(SqlCmd, Databaseoperations.Select, chechQuery);
                if (check_dt.Rows.Count > 0)
                {
                    return 0;
                }
                else
                {

                    string query = @" INSERT INTO Users (UserName, Email, PasswordHash, Fullname, IsActive, CreatedOn)
                                   VALUES (@UserName, @Email, @PasswordHash, @Fullname, 1, GETDATE());
                                   SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = query; // 👈 REQUIRED
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.AddWithValue("@UserName", user.Username);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@Fullname", user.Fullname);
                    //cmd.Parameters.AddWithValue("@Fullname", user.Fullname);
                    cmd.Parameters.AddWithValue("@PasswordHash", Helper.EncryptString(user.PassswordHash));
                   

                    isinserted = DBEngine.ExecuteScalar(cmd, Databaseoperations.Insert, query);
                   if(isinserted > 0)
                    {
                        query = "Insert into UserRoles (UserId,RoleId) values(@UserId,@roleid)";
                        cmd = new SqlCommand();
                        cmd.Parameters.AddWithValue("@UserId", isinserted);
                        cmd.Parameters.AddWithValue("@roleid", user.RoleId==0?2:user.RoleId);
                        cmd.CommandText= query;
                        cmd.CommandType= CommandType.Text;
                        DBEngine.ExecuteNonQuery(cmd, Databaseoperations.Insert, query);
  
                        
                    }
                    __logger.LogInformation("User Create Successfully");
                    return isinserted;
                }
            }
            catch (Exception ex)
            {
                __logger.LogWarning($"Create User Error  occured while Creating User,Error:{ex.Message},Stacktrace:{ex.StackTrace}");
            }
            return isinserted;
            }
       
    }
}
        
    
