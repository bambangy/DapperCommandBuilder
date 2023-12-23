using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlCommandBuilder
{
    public class CommandBuilder
    {
        private class Command : QueryCommand
        {
            public Command()
            {}
        }

        public static IQueryCommand Init() => new Command();
    }
}
