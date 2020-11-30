using System;
using System.Text;
using Giron.API.Helpers;

namespace Giron.API.Models.Constants
{
    public static class EnvironmentVariables
    {
        private static readonly string PostgresqlHost = 
            Environment.GetEnvironmentVariable("POSTGRES_HOST");
        private static readonly string PostgresqlPort = 
            Environment.GetEnvironmentVariable("POSTGRES_PORT");
        private static readonly string PostgresqlUsername = 
            Environment.GetEnvironmentVariable("POSTGRES_USERNAME");
        private static readonly string PostgresqlPassword = 
            Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
        private static readonly string PostgresqlDatabaseName = 
            Environment.GetEnvironmentVariable("POSTGRES_DB");

        public static readonly string PostgresqlConnectionString =
            $"Host={PostgresqlHost};Port={PostgresqlPort};Username={PostgresqlUsername};Password={PostgresqlPassword};Database={PostgresqlDatabaseName}";

        public static readonly bool IsAdminRegistrationEnabled =
            Environment.GetEnvironmentVariable("ADMIN_REGISTRATION_ENABLED") == "1";
        public static readonly bool IsUserRegistrationEnabled = 
            Environment.GetEnvironmentVariable("USER_REGISTRATION_ENABLED") == "1";
        public static readonly string JwtIssuer = 
            Environment.GetEnvironmentVariable("JWT_ISSUER");
        public static readonly string JwtAudience = 
            Environment.GetEnvironmentVariable("JWT_AUDIENCE");

        public static readonly byte[] JwtAccessSecretKey =
            string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("JWT_ACCESS_SECRET_KEY"))
                ? Generator.GetRandomBytes()
                : Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JWT_ACCESS_SECRET_KEY"));
        public static readonly byte[] JwtRefreshSecretKey =
            string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("JWT_REFRESH_SECRET_KEY"))
                ? Generator.GetRandomBytes()
                : Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JWT_REFRESH_SECRET_KEY"));
    }
}