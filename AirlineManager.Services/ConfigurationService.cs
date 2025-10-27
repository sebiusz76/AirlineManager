using AirlineManager.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AirlineManager.Services
{
    public interface IConfigurationService
    {
        Task<string?> GetValueAsync(string key);

        Task<T?> GetValueAsync<T>(string key) where T : struct;

        Task<Dictionary<string, string>> GetCategoryAsync(string category);

        Task SetValueAsync(string key, string value, string? modifiedBy = null);
    }

    public class ConfigurationService : IConfigurationService
    {
        private readonly ApplicationDbContext _context;
        private static readonly string EncryptionKey = "AirlineManager2025SecretKey123!"; // Match controller key

        public ConfigurationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string?> GetValueAsync(string key)
        {
            var config = await _context.AppConfigurations
                  .FirstOrDefaultAsync(c => c.Key == key);

            if (config == null) return null;

            if (config.IsEncrypted && !string.IsNullOrEmpty(config.Value))
            {
                try
                {
                    return DecryptString(config.Value);
                }
                catch
                {
                    return null;
                }
            }

            return config.Value;
        }

        public async Task<T?> GetValueAsync<T>(string key) where T : struct
        {
            var value = await GetValueAsync(key);
            if (string.IsNullOrEmpty(value)) return null;

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return null;
            }
        }

        public async Task<Dictionary<string, string>> GetCategoryAsync(string category)
        {
            var configs = await _context.AppConfigurations
   .Where(c => c.Category == category)
           .ToListAsync();

            var result = new Dictionary<string, string>();

            foreach (var config in configs)
            {
                var value = config.Value;

                if (config.IsEncrypted && !string.IsNullOrEmpty(value))
                {
                    try
                    {
                        value = DecryptString(value);
                    }
                    catch
                    {
                        value = string.Empty;
                    }
                }

                result[config.Key] = value;
            }

            return result;
        }

        public async Task SetValueAsync(string key, string value, string? modifiedBy = null)
        {
            var config = await _context.AppConfigurations
                   .FirstOrDefaultAsync(c => c.Key == key);

            if (config == null)
            {
                throw new KeyNotFoundException($"Configuration key '{key}' not found.");
            }

            if (config.IsEncrypted && !string.IsNullOrEmpty(value))
            {
                value = EncryptString(value);
            }

            config.Value = value;
            config.LastModified = DateTime.UtcNow;
            config.LastModifiedBy = modifiedBy ?? "System";

            await _context.SaveChangesAsync();
        }

        private string EncryptString(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(EncryptionKey.PadRight(32).Substring(0, 32));
            aes.IV = new byte[16];

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        private string DecryptString(string cipherText)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(EncryptionKey.PadRight(32).Substring(0, 32));
            aes.IV = new byte[16];

            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }
    }
}