using ProTechTiveGear.Models;
using System;
using System.Linq;

namespace ProTechTiveGear.Services
{
    public class AuthService
    {
        private readonly ProTechTiveGearEntities db;

        public AuthService(ProTechTiveGearEntities context)
        {
            db = context;
        }

        public Customer Login(string username, string password)
        {
            return db.Customers.SingleOrDefault(u => u.Username == username && u.Passwords == password);
        }

        public bool Register(Customer cs)
        {
            var existing = db.Customers.SingleOrDefault(u => u.Username == cs.Username);
            if (existing != null) return false;

            db.Customers.Add(cs);
            db.SaveChanges();
            return true;
        }

        public bool ChangePassword(string username, string oldPass, string newPass)
        {
            var user = db.Customers.SingleOrDefault(u => u.Username == username && u.Passwords == oldPass);
            if (user == null) return false;

            user.Passwords = newPass;
            db.SaveChanges();
            return true;
        }
    }
}
