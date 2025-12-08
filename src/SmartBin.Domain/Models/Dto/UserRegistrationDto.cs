using System;
using System.Collections.Generic;
using System.Text;

namespace SmartBin.Domain.Models.Dto
{
    public class UserRegistrationDto
    {
        // Используется как логин/уникальный идентификатор
        public string Nickname { get; set; }

        // Пароль в открытом виде для хеширования
        public string Password { get; set; }

        // Используется для поля FullName в модели User
        public string FullName { get; set; }
    }
    public class UserLoginDto
    {
        // Используется для поиска пользователя в БД (соответствует полю Nickname в User)
        public string Nickname { get; set; }

        // Пароль, введенный пользователем (будет хешироваться и сравниваться с PasswordHash)
        public string Password { get; set; }
    }
}
