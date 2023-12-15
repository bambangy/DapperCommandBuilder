using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperCommandBuilder
{
    public class CommandSort
    {
        public string Column { get; set; }
        public CommandOrderDirection Direction { get; set; }
    }
}
