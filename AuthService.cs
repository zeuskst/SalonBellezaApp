using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SalonBellezaApp
{
    public static class AuthService
    {
        private static readonly string AppDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SalonBellezaApp");
        private static readonly string CredentialsFile = Path.Combine(AppDir, "credentials.txt");
        private static readonly string LogFile = Path.Combine(AppDir, "login_attempts.log");
        private static readonly object _fileLock = new object();

        
        private class LockInfo
        {
            public int FailedCount { get; set; }
            public DateTime? LockoutEnd { get; set; }
            public DateTime LastAttemptUtc { get; set; }
        }

        private static readonly Dictionary<string, LockInfo> _locks = new Dictionary<string, LockInfo>(StringComparer.OrdinalIgnoreCase);
        private static readonly object _locksLock = new object();

        private const int DefaultIterations = 100_000;
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

        static AuthService()
        {
            try
            {
                Directory.CreateDirectory(AppDir);
                EnsureDefaultCredential();
            }
            catch
            {
            }
        }

        private static void EnsureDefaultCredential()
        {
            
            var sysUser = Environment.UserName;
            lock (_fileLock)
            {
                if (!File.Exists(CredentialsFile))
                {
                    AddOrUpdateUserPassword(sysUser, "admin");
                    return;
                }

                var lines = File.ReadAllLines(CredentialsFile);
                var exists = lines.Any(l => !string.IsNullOrWhiteSpace(l) && l.Split('|').Length >= 4 && l.Split('|')[0].Equals(sysUser, StringComparison.OrdinalIgnoreCase));
                if (!exists)
                {
                    AddOrUpdateUserPassword(sysUser, "admin");
                }
            }
        }

        public static void AddOrUpdateUserPassword(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));
            if (password == null) throw new ArgumentNullException(nameof(password));

            var salt = new byte[SaltSize];
            RandomNumberGenerator.Fill(salt);

            using var derive = new Rfc2898DeriveBytes(password, salt, DefaultIterations, HashAlgorithmName.SHA256);
            var hash = derive.GetBytes(HashSize);

            var line = string.Join('|', new[] { username, DefaultIterations.ToString(), Convert.ToBase64String(salt), Convert.ToBase64String(hash) });

            lock (_fileLock)
            {
                var lines = File.Exists(CredentialsFile) ? File.ReadAllLines(CredentialsFile).ToList() : new List<string>();
                var idx = lines.FindIndex(l => !string.IsNullOrWhiteSpace(l) && l.Split('|')[0].Equals(username, StringComparison.OrdinalIgnoreCase));
                if (idx >= 0) lines[idx] = line; else lines.Add(line);
                File.WriteAllLines(CredentialsFile, lines);
            }
        }

        public static bool VerifyUserPassword(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || password == null) return false;

            lock (_fileLock)
            {
                if (!File.Exists(CredentialsFile)) return false;
                var lines = File.ReadAllLines(CredentialsFile);
                var line = lines.FirstOrDefault(l => !string.IsNullOrWhiteSpace(l) && l.Split('|').Length >= 4 && l.Split('|')[0].Equals(username, StringComparison.OrdinalIgnoreCase));
                if (line == null) return false;

                var parts = line.Split('|');
                if (parts.Length < 4) return false;

                if (!int.TryParse(parts[1], out int iterations)) iterations = DefaultIterations;
                var salt = Convert.FromBase64String(parts[2]);
                var storedHash = Convert.FromBase64String(parts[3]);

                using var derive = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
                var computed = derive.GetBytes(storedHash.Length);
                return CryptographicOperations.FixedTimeEquals(computed, storedHash);
            }
        }

        public static void LogAttempt(string username, bool success, string reason = null)
        {
            try
            {
                Directory.CreateDirectory(AppDir);
                var entry = new StringBuilder();
                entry.Append(DateTime.UtcNow.ToString("o"));
                entry.Append('\t');
                entry.Append("Machine=").Append(Environment.MachineName);
                entry.Append('\t');
                entry.Append("UserInput=").Append(username ?? string.Empty);
                entry.Append('\t');
                entry.Append("Success=").Append(success);
                if (!string.IsNullOrEmpty(reason)) { entry.Append('\t'); entry.Append("Reason=").Append(reason); }
                entry.AppendLine();

                lock (_fileLock)
                {
                    File.AppendAllText(LogFile, entry.ToString());
                }
            }
            catch
            {
                
            }
        }

        public static bool IsLockedOut(string username, out TimeSpan remaining)
        {
            remaining = TimeSpan.Zero;
            if (string.IsNullOrWhiteSpace(username)) return false;

            lock (_locksLock)
            {
                if (_locks.TryGetValue(username, out var info))
                {
                    if (info.LockoutEnd.HasValue && info.LockoutEnd.Value > DateTime.UtcNow)
                    {
                        remaining = info.LockoutEnd.Value - DateTime.UtcNow;
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IncreaseFailedAttempt(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;
            lock (_locksLock)
            {
                if (!_locks.TryGetValue(username, out var info)) info = new LockInfo();
                
                if (info.LockoutEnd.HasValue && info.LockoutEnd.Value > DateTime.UtcNow)
                {
                    _locks[username] = info;
                    return true;
                }

                info.FailedCount++;
                info.LastAttemptUtc = DateTime.UtcNow;
                if (info.FailedCount >= MaxFailedAttempts)
                {
                    info.LockoutEnd = DateTime.UtcNow.Add(LockoutDuration);
                    info.FailedCount = 0;
                    _locks[username] = info;
                    return true; 
                }

                _locks[username] = info;
                return false;
            }
        }

        public static void ResetFailedAttempts(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return;
            lock (_locksLock)
            {
                if (_locks.ContainsKey(username)) _locks.Remove(username);
            }
        }
    }
}
