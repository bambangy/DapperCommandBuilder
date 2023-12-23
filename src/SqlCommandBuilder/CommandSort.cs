using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlCommandBuilder
{
    public class CommandSort
    {
        public string Column { get; set; }
        public CommandOrderDirection Direction { get; set; }
        public static CommandSort Add(string column, CommandOrderDirection direction) => new CommandSort()
        {
            Column = column,
            Direction = direction
        };
    }
}
