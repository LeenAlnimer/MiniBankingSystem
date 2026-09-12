using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniBankingSystem.Domain.Entities
{
    public  class Account
    {

        public Guid Id { get; set; } 
        public Guid CustomerId { get; set; }    
        public  string AccountNumber {  get; set; } =string.Empty;

        public  decimal Balance { get; set; }
        public string Currency { get; set; } = "JOD";
        public DateTime CreatedAt { get; set; }
        public Customer Customer { get; set; } = null!;
        // means  that  account  have  one  customer  
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        // means  account can  have  many  transactions


    }
}
