using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoHijauBackend.Application.Dto
{
    public class PayoutDTO
    {
        public string UserId { get; set; } = "";
        public double Amount { get; set; } = Double.MinValue;
    }
}
