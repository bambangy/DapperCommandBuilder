using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperCommandBuilder
{
    public class CommandResultBuilder
    {
        private class CommandResult : DapperCommandResult
        {
            public CommandResult()
            {
                Parameters = new();
                Script = string.Empty;
            }
        }
        public static IDapperCommandResult Create() => new CommandResult();
    }
}
