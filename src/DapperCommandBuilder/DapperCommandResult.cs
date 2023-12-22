using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperCommandBuilder
{
    public abstract class DapperCommandResult : IDapperCommandResult
    {
        public string Script { get; set; }
        public Dictionary<string, object?> Parameters { get; set; }
    }
}
