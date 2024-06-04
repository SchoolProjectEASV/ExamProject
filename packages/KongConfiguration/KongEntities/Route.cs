using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KongSetup.KongEntities
{
    public class Route
    {
        public string Path { get; set; }
        public string[] Methods { get; set; }
        public bool BypassAuth { get; set; }
    }
}
