using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoHijauBackend.Application.Dto
{
    public class RateDTO
    {
        public string Id { get; set; } = "";
        public string UserId { get; set; } = "";
        public float CustomerSellingRate { get; set; }
        public float CollectorBuyingRate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
