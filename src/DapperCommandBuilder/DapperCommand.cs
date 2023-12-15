using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperCommandBuilder
{
    public sealed class DapperCommand
    {
        public CommandAdapter Adapter { get; set; }
        public CommandType Type { get; set; }
        public string TableName { get; set; }
        public string[] Selections { get; set; }
        public Collection<CommandCondition> Conditions { get; set; }
        public Collection<CommandReference> References { get; set; }
        public Collection<CommandSort> Sorts { get; set; }
        public string[] Groupings { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public Dictionary<string, object?> Bindings { get; set; }
        public static DapperCommand Init() => new()
        {
            Adapter = CommandAdapter.SqlServer,
            Limit = 0,
            Offset = 0,
            Conditions = new Collection<CommandCondition>(),
            References = new Collection<CommandReference>(),
            Sorts = new Collection<CommandSort>(),
            Bindings = new Dictionary<string, object?>()
        };
    }
}
