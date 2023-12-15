using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperCommandBuilder
{
    public class DapperCommandResult
    {
        public string Script { get; set; }
        public Dictionary<string, object?> Parameters { get; set; }
    }
}
