using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WCFPOPS
{
    [DataContract]
    public class PurchaseOrderSpecial : PurchaseOrder
    {
        [DataMember]
        public string ItemCode { get; set; }

        [DataMember]
        public string SupplierCode { get; set; }
    }
}
