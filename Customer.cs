using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekt1
{
    public class Customer
    {
        public string FirstName = "";
        public string LastName = "";
        public string ContactInfo = "";

        public Customer(string firstName, string lastName)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
            this.CreateCustomerDB();
        }

        public void SetContactInfo(string info)
        {
            this.ContactInfo = info;
        }

        private void CreateCustomerDB()
        {
            Database.InsertData("customer", new List<string>() {"firstName", "lastName", "contactInfoId" }, new List<string>() { this.FirstName, this.LastName, this.ContactInfo });
        }
    }
}
