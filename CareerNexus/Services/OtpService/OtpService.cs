using CareerNexus.Models.UserModel;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CareerNexus.AppConfiguration;
using CareerNexus.Models.Authetication;

namespace CareerNexus.Services.OtpService
{
    public class OtpService : IOTP
    {
        public string BuildToken(long id, string fullName,long RoleId,string RoleName)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                int expiryMinutes = AppConfiguration.AppConfiguration._tokenDurationInMinutes;

                var keyBytes = Encoding.UTF8.GetBytes(AppConfiguration.AppConfiguration._secret);
                var securityKey = new SymmetricSecurityKey(keyBytes);
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
            new Claim(ClaimTypes.PrimarySid, id.ToString()),
            new Claim(ClaimTypes.Name, fullName),
            new Claim(ClaimTypes.Role,RoleId.ToString()),
            new Claim(ClaimTypes.Role,RoleName),

           // new Claim(ClaimTypes.Role, roleId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    IssuedAt = DateTime.UtcNow,
                    NotBefore = DateTime.UtcNow,
                    Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
                    Issuer = AppConfiguration.AppConfiguration._validIssuer,
                    Audience = AppConfiguration.AppConfiguration._validAudience,
                    SigningCredentials = credentials
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BuildToken Error: {ex.Message}");
                return string.Empty;
            }
        }

        public async Task<ClaimResponseModel> GenerateToken(UserModel user)
        {
            var response = new ClaimResponseModel
            {
                Email = user.Email,
                FullName = user.Fullname,
                RoleId= user.RoleId,
                RoleName=user.RoleName,
                RoleType=user.Roletype
                
                //IsTwoFactorEnabled = user.IsTwoFactorEnabled,
                //ProfilePictureURL = user.ProfilePictureURL, // if null, it's okay
                //RoleName = user.RoleName
            };

            var token = BuildToken(user.Id, user.Fullname,user.RoleId,user.RoleName);
            response.Token = token;
            //response.IsSuccess = true;
            return response;
        }
    }
}


        //        public string GenerateToken(string userId, string userName, IConfiguration config)
        //        {
        //            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["AppKeys:Secret"]));
        //            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

//            var claims = new[]
//            {
//    new Claim(ClaimTypes.NameIdentifier, userId),
//    new Claim(ClaimTypes.Name, userName),
//    // add more claims if needed
//};

//            var token = new JwtSecurityToken(
//                issuer: config["App_Keys:ValidIssuer"],
//                audience: config["App_Keys:ValidAudience"],
//                claims: claims,
//                expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(config["App_Keys:TokenDurationInMinutes"])),
//                signingCredentials: creds
//            );

//            return new JwtSecurityTokenHandler().WriteToken(token);
//        }

