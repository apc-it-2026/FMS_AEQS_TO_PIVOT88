using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compal.FMS
{
    public interface FileMonitorClient
    {
        void Execute(object vfilePath);
    }
}
