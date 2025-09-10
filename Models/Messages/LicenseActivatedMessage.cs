using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vakilaw.Models.Messages
{
    public class LicenseActivatedMessage
    {
        public bool IsActivated { get; set; }
        public LicenseActivatedMessage(bool activated) { IsActivated = activated; }
    }
}