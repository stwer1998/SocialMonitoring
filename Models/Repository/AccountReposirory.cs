using Models.DAL;
using Models.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Models.Repository
{
    public class AccountReposirory : IAccountRepository
    {
        private ApplicationContext db;
        public AccountReposirory(ApplicationContext application)
        {
            db = application;
        }
        public void AddUser(User User)
        {
            db.Users.Add(User);
            db.SaveChanges();
        }

        public bool GetLogin(string login)
        {
            var user = db.Users.FirstOrDefault(x => x.Login == login);
            if (user != null) { return true; }
            else return false;
        }

        public bool GetUser(string login, string password)
        {
            var user = db.Users.FirstOrDefault(x => x.Login == login && x.Password == password);
            if (user != null) { return true; }
            else return false;
        }
    }
}
