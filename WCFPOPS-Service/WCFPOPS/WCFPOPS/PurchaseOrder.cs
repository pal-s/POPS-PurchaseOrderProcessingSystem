using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WCFPOPS
{
    [DataContract]
    public class PurchaseOrder
    {
        [DataMember]
        public string OrderNumber { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public int Quantity { get; set; }

        [DataMember]
        public string SupplierName { get; set; }

        [DataMember]
        public DateTime OrderDate { get; set; }

    }
}
