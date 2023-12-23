using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlCommandBuilder
{
    public class CommandConditionCollection : Collection<CommandConditionCollection>
    {
        private readonly bool _beginGroup;
        private readonly CommandOperation? _operation;
        private readonly CommandCondition _condition;
        private readonly bool _endGroup;
        public CommandConditionCollection(CommandCondition condition, CommandOperation? operation = null, bool beginGroup = false, bool endGroup = false)
        {
            _condition = condition;
            _operation = operation;
            _beginGroup = beginGroup;
            _endGroup = endGroup;
        }

        public bool BeginGroup => _beginGroup;
        public bool EndGroup => _endGroup;
        public CommandOperation? Operation => _operation;
        public CommandCondition Condition => _condition;
    }
}
