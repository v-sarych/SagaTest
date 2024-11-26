using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model.Enums
{
    public enum DeliveryStatuses
    {
        Created = 0,
        Aborted = 1,
        Delivered = 2,
        InTransit = 3,
    }
}
