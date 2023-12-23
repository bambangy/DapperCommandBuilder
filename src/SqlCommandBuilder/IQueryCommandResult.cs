using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlCommandBuilder
{
    public interface IQueryCommandResult
    {
        string Script { get; set; }
        Dictionary<string, object?> Parameters { get; set; }
    }
}
