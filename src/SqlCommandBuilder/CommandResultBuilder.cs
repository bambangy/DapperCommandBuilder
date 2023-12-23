using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlCommandBuilder
{
    public class CommandResultBuilder
    {
        private class CommandResult : QueryCommandResult
        {
            public CommandResult()
            {
                Parameters = new();
                Script = string.Empty;
            }
        }
        public static IQueryCommandResult Create() => new CommandResult();
    }
}
