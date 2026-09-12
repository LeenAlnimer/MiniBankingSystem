using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniBankingSystem.Domain.Entities
{
    public  class Transaction
    {

        public Guid Id { get; set; } 
        public Guid AccountId {  get; set; }
        public string Type { get; set; }= string.Empty;
        public  decimal Amount { get; set; }
        public string Currency { get; set; } = "JOD";
        public DateTime CreatedAt { get; set; }
        public Account Account { get; set; } = null!;

    }
}
