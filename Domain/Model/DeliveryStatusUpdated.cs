using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Model.Enums;

namespace Domain.Model
{
    public class DeliveryStatusUpdated
    {
        public Guid OrderId { get; set; }
        public DeliveryStatuses DeliveryStatus { get; set; }
    }
}
