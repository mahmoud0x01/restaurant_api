using Mahmoud_Restaurant.Data;
using Mahmoud_Restaurant.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;
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
    private readonly ConcurrentDictionary<string, DateTime> _tokenBlacklist;

    public AuthService(ApplicationDbContext context, string jwtSecret, string adminSecretKey, ConcurrentDictionary<string, DateTime> tokenBlacklist)
    {
        _context = context;
        _jwtSecret = jwtSecret;
        _adminSecretKey = adminSecretKey;
        _tokenBlacklist = tokenBlacklist;
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

        var passwordHash = HashPassword(userDto.Password);

        var user = new User
        {
            PasswordHash = passwordHash,
            FullName = userDto.FullName,
            Email = userDto.Email,
            Address = userDto.Address,
            BirthDate = userDto.BirthDate,
            Gender = userDto.Gender,
            PhoneNumber = userDto.PhoneNumber,
            IsAdmin = isAdmin
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
        if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
            return null;

        var key = GenerateSymmetricSecurityKey(_jwtSecret);

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique token ID
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public void BlacklistToken(string token)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();

        if (jwtTokenHandler.CanReadToken(token))
        {
            var jwtToken = jwtTokenHandler.ReadJwtToken(token);
            var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            var expiry = jwtToken.ValidTo;

            if (!string.IsNullOrEmpty(jti))
            {
                _tokenBlacklist[jti] = expiry;
            }
        }
        else
        {
            throw new ArgumentException("Invalid token.");
        }
    }

    public bool IsTokenBlacklisted(string jti)
    {
        return _tokenBlacklist.ContainsKey(jti);
    }

    private SymmetricSecurityKey GenerateSymmetricSecurityKey(string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        if (keyBytes.Length < 128 / 8)
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
            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hashBytes)}";
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
        var salt = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }
        return salt;
    }
}
