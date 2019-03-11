using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Blog.Models
{
    public class SeededAccount
    {
        public string Role { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public SeededAccount(string role, string userName, string password)
        {
            Role = role;
            UserName = userName;
            Password = password;
        }

        public static List<SeededAccount> CreateList()
        {
            List<SeededAccount> Accounts = new List<SeededAccount>()
            {
                new SeededAccount("Admin","admin@blog.com","Password-1"),
                new SeededAccount("Moderator","moderator@blog.com","Password-1")
            };
            return Accounts;
        }
    }
}