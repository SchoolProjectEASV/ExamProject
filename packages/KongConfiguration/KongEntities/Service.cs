using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KongSetup.KongEntities
{
    public class Service
    { 
        public string Name { get; set; }
        public string Url { get; set; }
        public Upstream Upstream { get; set; }
        public List<Route> Routes { get; set; }
    }
}
