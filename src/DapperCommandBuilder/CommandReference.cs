using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperCommandBuilder
{
    public class CommandReference
    {
        private readonly CommandReferenceType _referenceType;
        private readonly string _tableName;
        private readonly Collection<CommandCondition> _onConditions;
        public CommandReference(CommandReferenceType referenceType, string tableName, Collection<CommandCondition> onConditions)
        {
            _tableName = tableName;
            _onConditions = onConditions;
            _referenceType = referenceType;
        }

        public static CommandReference Add(CommandReferenceType referenceType, string tableName, Collection<CommandCondition> onConditions) => new CommandReference(referenceType, tableName, onConditions);

        public CommandReferenceType ReferenceType => _referenceType;
        public string TableName => _tableName;
        public Collection<CommandCondition> OnConditions => _onConditions;
    }
}
