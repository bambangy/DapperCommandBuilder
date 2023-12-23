using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlCommandBuilder
{
    public class CommandCondition
    {
        private readonly bool _beginGroup = false;
        private readonly bool _endGroup = false;
        private readonly CommandOperation? _operation;
        private readonly string _column;
        private readonly CommandMatchType _match;
        private readonly object? _value;
        public CommandCondition(string column, CommandMatchType match, object? value, CommandOperation? operation = null, bool beginGroup = false, bool endGroup = false)
        {
            _column = column;
            _match = match;
            _value = value;
            _beginGroup = beginGroup;
            _endGroup = endGroup;
            _operation = operation;
        }

        public static CommandCondition Add(string column, CommandMatchType match, object? value, CommandOperation? operation = null, bool beginGroup = false, bool endGroup = false) => new(column, match, value, operation, beginGroup, endGroup);

        public string Column => _column;
        public CommandMatchType Match => _match;
        public object? Value => _value;
        public bool BeginGroup => _beginGroup;
        public bool EndGroup => _endGroup;
        public CommandOperation? Operation => _operation;
    }
}
