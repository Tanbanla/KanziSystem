using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRJ_WAREHOUSE_BIVN.Common
{
    public class ConnectionStringOptions
    {
        // Match appsettings.json key: "ConnectionStrings:CostManagerConnection"
        public string CostManagerConnection { get; set; }
        public string WorkingControlConnection { get; set; }
        public string AgentConnection { get; set; }
    }
}
