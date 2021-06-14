using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WCFPOPS
{
    [DataContract]
    public class Supplier
    {
        [DataMember]
        public string SupplierNo { get; set; }
        [DataMember]
        public string SupplierName { get; set; }
        [DataMember]
        public string SupplierAddress { get; set; }
    }
}
