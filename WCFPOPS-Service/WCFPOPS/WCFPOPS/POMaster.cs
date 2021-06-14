using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace WCFPOPS
{
    [DataContract]
    public class POMaster
    {
        [DataMember]
        public string PurchaseNumber { get; set; }
        [DataMember]
        public DateTime PurchaseDate { get; set; }
        [DataMember]
        public string SupplierNumber { get; set; }
    }
}
