using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperCommandBuilder
{
    public interface IDapperCommandResult
    {
        string Script { get; set; }
        Dictionary<string, object?> Parameters { get; set; }
    }
}
