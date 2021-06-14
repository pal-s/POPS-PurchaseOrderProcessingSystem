using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WCFPOPS
{
    [DataContract]
    public class POException
    {
        [DataMember]
        public string ErrorMessage { get; set; }

        public POException(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }
}
