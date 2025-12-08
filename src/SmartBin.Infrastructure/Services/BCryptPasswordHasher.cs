using SmartBin.Application.Services;
using System;
using System.Collections.Generic;
using System.Text;
using BCrypt.Net;
namespace SmartBin.Infrastructure.Services
{
    public class BCryptPasswordHasher : IPasswordHasher
    {
        // Хеширование пароля
        public string HashPassword(string password)
        {
            // Используем BCrypt.HashPassword с автоматической генерацией соли
            // 10 - это стандартный рабочий фактор (cost factor)
            return BCrypt.Net.BCrypt.HashPassword(password, 10);
        }

        // Верификация пароля
        public bool VerifyPassword(string providedPassword, string hashedPassword)
        {
            // BCrypt.Verify автоматически извлекает соль из хеша и сравнивает
            return BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);
        }
    }
}
