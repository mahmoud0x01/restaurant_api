using Mahmoud_Restaurant.Data;
using Mahmoud_Restaurant.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

public class AuthService
{
    private readonly ApplicationDbContext _context;
    private readonly string _jwtSecret;
    private readonly string _adminSecretKey;

    public AuthService(ApplicationDbContext context, string jwtSecret,string adminSecretKey)
    {
        _context = context;
        _jwtSecret = jwtSecret;
        _adminSecretKey = adminSecretKey;
    }

    public async Task<User> Register(UserDto userDto, string adminSecretKey = null)
    {
        // Validate email format
        if (!Regex.IsMatch(userDto.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            throw new ArgumentException("Invalid email format.");
        }
        // Check if the username already exists
        var existingUser = await _context.Users.SingleOrDefaultAsync(u => u.Email == userDto.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User already registered with this email.");
        }

        // Validate phone number format
        if (!Regex.IsMatch(userDto.PhoneNumber, @"^\+?[1-9]\d{1,14}$"))
        {
            throw new ArgumentException("Invalid phone number format.");
        }

        // Determine the user role
        var isAdmin = !string.IsNullOrEmpty(adminSecretKey) && adminSecretKey == _adminSecretKey;

        var passwordHash = HashPassword(userDto.Password); // Use SHA-256 for hashing

        var user = new User
        {
            PasswordHash = passwordHash,
            FullName = userDto.FullName,
            Email = userDto.Email,
            Address = userDto.Address,
            BirthDate = userDto.BirthDate,
            Gender = userDto.Gender,
            PhoneNumber = userDto.PhoneNumber,
            IsAdmin = isAdmin // Set the admin status based on the adminSecretKey
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> Authorize(string email)
    {
        var existingUser = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
        if (existingUser == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        return existingUser;

}

        public async Task<string> Login(LoginDto loginDto)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == loginDto.Email);
        if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash)) // Use SHA-256 for verification
            return null;

        var key = GenerateSymmetricSecurityKey(_jwtSecret);

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, user.Email) }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private SymmetricSecurityKey GenerateSymmetricSecurityKey(string secret)
    {
        // Ensure that the key length is at least 128 bits
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        if (keyBytes.Length < 128 / 8) // 128 bits in bytes (16 bytes)
        {
            throw new ArgumentOutOfRangeException(nameof(secret), "Key length must be at least 128 bits.");
        }
        return new SymmetricSecurityKey(keyBytes);
    }

    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var salt = GenerateSalt();
            var saltedPassword = salt.Concat(Encoding.UTF8.GetBytes(password)).ToArray();
            var hashBytes = sha256.ComputeHash(saltedPassword);
            var hash = Convert.ToBase64String(hashBytes);
            return $"{Convert.ToBase64String(salt)}:{hash}"; // Return both salt and hash
        }
    }

    private bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split(':');
        if (parts.Length != 2)
            return false;

        var salt = Convert.FromBase64String(parts[0]);
        var storedHashValue = parts[1];

        using (var sha256 = SHA256.Create())
        {
            var saltedPassword = salt.Concat(Encoding.UTF8.GetBytes(password)).ToArray();
            var computedHash = Convert.ToBase64String(sha256.ComputeHash(saltedPassword));
            return computedHash == storedHashValue;
        }
    }

    private byte[] GenerateSalt()
    {
        var randomNumberGenerator = RandomNumberGenerator.Create();
        var salt = new byte[16];
        randomNumberGenerator.GetBytes(salt);
        return salt;
    }
}
