using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CityInfo.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private IConfiguration _configuration;

        // This is a placeholder for the authentication endpoint. In a real application, you would implement proper authentication logic here.
        public class AuthenticationRequestBody
        {
            public string? UserName { get; set; }
            public string? Password { get; set; }
        }

        private class CityInfoUser // This is a placeholder for a user class. In a real application, you would have a proper user class with more properties and methods.
        {
            public int UserId { get; set; }
            public string UserName { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string City { get; set; }

            public CityInfoUser(int userId, string userName, string firstName, string lastName, string city) // This is a constructor for the CityInfoUser class.
                                                                                                             // In a real application, you would have a proper constructor and other methods for this class.
            {
                UserId = userId;
                UserName = userName;
                FirstName = firstName;
                LastName = lastName;
                City = city;
            }
        }

        public AuthenticationController(IConfiguration configuration) // This is the constructor for the AuthenticationController class.
                                                                      // It takes an IConfiguration object as a parameter, which is used to access the configuration settings for the application.
        {
            _configuration = configuration;
        }   

        [HttpPost("authenticate")]
        public ActionResult<string> Authenticate(AuthenticationRequestBody authenticationRequestBody) // This is the action method for the authentication endpoint. It takes an AuthenticationRequestBody object as a parameter, which contains the username and password for the authentication request.
        {
            // Step 1: validate the username and password (this is just a placeholder, you should implement proper validation)
            var user = ValidateUserCredentials(authenticationRequestBody.UserName, authenticationRequestBody.Password);
            if (user == null)
            {
                return Unauthorized(); // Return 401 Unauthorized if the credentials are invalid
            }

            // Step 2: create a JWT token (this is just a placeholder, you should implement proper JWT token creation)
            var securityKey = new SymmetricSecurityKey(
                Convert.FromBase64String(_configuration["Authentication:SecretForKey"]!));

            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256); // This creates the signing credentials for the JWT token using the secret key from the configuration and the HMAC SHA256 algorithm.

            var claimsForToken = new List<Claim>();
            claimsForToken.Add(new Claim("sub", user.UserId.ToString())); // This adds a claim for the user ID to the list of claims for the JWT token.
            claimsForToken.Add(new Claim("given_name", user.FirstName)); // This adds a claim for the user's first name to the list of claims for the JWT token.
            claimsForToken.Add(new Claim("family_name", user.LastName)); // This adds a claim for the user's last name to the list of claims for the JWT token.
            claimsForToken.Add(new Claim("city", user.City)); // This adds a claim for the user's city to the list of claims for the JWT token.

            var jwtSecurityToken = new JwtSecurityToken(
                _configuration["Authentication:Issuer"],
                _configuration["Authentication:Audience"],
                claimsForToken,
                DateTime.UtcNow,
                DateTime.UtcNow.AddHours(1),
                signingCredentials);

            var tokenToReturn = new JwtSecurityTokenHandler()
                .WriteToken(jwtSecurityToken); // This creates a JWT token handler and uses it to write the JWT token to a string.

            return Ok(tokenToReturn); // Return the JWT token as the response to the authentication request.
        }

        private CityInfoUser ValidateUserCredentials(string? userName, string? password)
        {
            // In a real application, you would validate the username and password against a database or other user store.
            // For this example, we'll just return a hardcoded user if the username and password are not empty.
            return new CityInfoUser(
                1,
                userName ?? "",
                "Bob",
                "Tomato",
                "Antwerp"
                );
        }
    }
}
