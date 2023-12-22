using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperCommandBuilder
{
    public class CommandBuilder
    {
        private class Command : DapperCommand
        {
            public Command()
            {}
        }

        public static IDapperCommand Init() => new Command();
    }
}
