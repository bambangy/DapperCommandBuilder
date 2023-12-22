using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperCommandBuilder
{
    public abstract class DapperCommand : IDapperCommand
    {
        private CommandAdapter? adapter;
        private CommandType? type;
        private string tableName;
        private Collection<string> selections;
        private Collection<CommandCondition> conditions { get; set; }
        private Collection<CommandReference> references { get; set; }
        private Collection<CommandSort> sorts { get; set; }
        private Collection<string> groupings { get; set; }
        private int limit { get; set; }
        private int offset { get; set; }
        private Dictionary<string, object?> bindings { get; set; }

        public DapperCommand()
        {
            adapter = null;
            type = null;
            tableName = string.Empty;
            selections = new Collection<string>();
            conditions = new Collection<CommandCondition>();
            references = new Collection<CommandReference>();
            sorts = new Collection<CommandSort>();
            groupings = new Collection<string>();
            limit = 0;
            offset = 0;
            bindings = new Dictionary<string, object?>();
        }

        public IDapperCommand InitDelete(string tableName)
        {
            this.tableName = tableName;
            this.type = CommandType.DELETE;
            return this;
        }

        public IDapperCommand AddGroupBy(string column)
        {
            this.groupings.Add(column);
            return this;
        }

        public IDapperCommand InitInsert(string tableName, Dictionary<string, object?> parameters)
        {
            this.tableName = tableName;
            bindings = parameters;
            type = CommandType.INSERT;
            return this;
        }

        public IDapperCommand AddReference(CommandReferenceType type, string tableName, ICollection<(CommandOperation operation, string column, CommandMatchType matchType, object? value)> onConditions)
        {
            Collection<CommandCondition> innerOnCondtions = new Collection<CommandCondition>(onConditions.Select(t => CommandCondition.Add(t.column, t.matchType, t.value, operation: t.operation)).ToList());
            references.Add(CommandReference.Add(type, tableName, innerOnCondtions));
            return this;
        }

        public IDapperCommand AddField(string column)
        {
            selections.Add(column);
            return this;
        }

        public IDapperCommand InitSelect(string tableName, string[] columns)
        {
            this.tableName = tableName;
            selections = new Collection<string>(columns);
            type = CommandType.SELECT;
            return this;
        }

        public IDapperCommand SetAdapter(CommandAdapter adapter)
        {
            this.adapter = adapter;
            return this;
        }

        public IDapperCommand SetSkip(int offset)
        {
            this.offset = offset;
            return this;
        }

        public IDapperCommand AddSort(string column, CommandOrderDirection direction)
        {
            sorts.Add(CommandSort.Add(column, direction));
            return this;
        }

        public IDapperCommand SetTake(int limit)
        {
            this.limit = limit;
            return this;
        }

        public IDapperCommand InitUpdate(string tableName, Dictionary<string, object?> parameters)
        {
            this.tableName = tableName;
            this.bindings = parameters;
            type = CommandType.UPDATE;
            return this;
        }

        public IDapperCommand AddWhereAnd(string column, CommandMatchType matchType, object? value)
        {
            this.conditions.Add(CommandCondition.Add(column, matchType, value, operation: CommandOperation.And));
            return this;
        }

        public IDapperCommand AddWhereAndGroup(ICollection<(CommandOperation operation, string column, CommandMatchType matchType, object? value)> conditions)
        {
            for (int i = 0; i < conditions.Count; i++)
            {
                var cond = conditions.ElementAt(i);
                if (i == 0)
                    this.conditions.Add(CommandCondition.Add(cond.column, cond.matchType, cond.value, operation: CommandOperation.Or, beginGroup: true));
                else if (i == conditions.Count - 1)
                    this.conditions.Add(CommandCondition.Add(cond.column, cond.matchType, cond.value, operation: cond.operation, endGroup: true));
                else
                    this.conditions.Add(CommandCondition.Add(cond.column, cond.matchType, cond.value, operation: cond.operation));
            }
            return this;
        }

        public IDapperCommand AddWhereOr(string column, CommandMatchType matchType, object? value)
        {
            this.conditions.Add(CommandCondition.Add(column, matchType, value, operation: CommandOperation.Or));
            return this;
        }

        public IDapperCommand AddWhereOrGroup(ICollection<(CommandOperation operation, string column, CommandMatchType matchType, object? value)> conditions)
        {
            for (int i = 0; i < conditions.Count; i++)
            {
                var cond = conditions.ElementAt(i);
                if (i == 0)
                    this.conditions.Add(CommandCondition.Add(cond.column, cond.matchType, cond.value, operation: CommandOperation.Or, beginGroup: true));
                else if (i == conditions.Count - 1)
                    this.conditions.Add(CommandCondition.Add(cond.column, cond.matchType, cond.value, operation: cond.operation, endGroup: true));
                else
                    this.conditions.Add(CommandCondition.Add(cond.column, cond.matchType, cond.value, operation: cond.operation));
            }
            return this;
        }

        public IDapperCommandResult BuildCommand()
        {
            if (string.IsNullOrEmpty(tableName))
                throw new NullReferenceException(nameof(tableName));

            IDapperCommandResult result = CommandResultBuilder.Create();
            Dictionary<string, object?> parameters = new Dictionary<string, object?>();

            switch (type)
            {
                case CommandType.INSERT:
                    {
                        if (bindings.Count == 0)
                            throw new Exception($"{nameof(bindings)} is required for INSERT statement");

                        result.Script = CommandBuilderFunction.GenerateInsertScript(tableName, bindings, ref parameters);
                        result.Parameters = parameters;
                    }
                    break;
                case CommandType.UPDATE:
                    {
                        if (bindings.Count == 0)
                            throw new Exception($"{nameof(bindings)} is required for UPDATE statement");

                        result.Script = CommandBuilderFunction.GenerateUpdateScript(tableName, bindings, conditions, ref parameters);
                        result.Parameters = parameters;
                    }
                    break;
                case CommandType.DELETE:
                    {
                        result.Script = CommandBuilderFunction.GenerateDeleteScript(tableName, conditions, ref parameters);
                        result.Parameters = parameters;
                    }
                    break;
                case CommandType.SELECT:
                default:
                    {
                        if (limit > 0 && adapter == null)
                            throw new NullReferenceException(nameof(adapter));

                        result.Script = CommandBuilderFunction.GenerateSelectScript(adapter ?? CommandAdapter.Mysql, tableName, selections.ToArray(), references, conditions, groupings.ToArray(), sorts, limit, offset, ref parameters);
                        result.Parameters = parameters;
                    }
                    break;
            }

            return result;
        }

        private class CommandBuilderFunction
        {
            public static string GenerateDeleteScript(string tableName, Collection<CommandCondition> conditions, ref Dictionary<string, object?> parameters)
            {
                string command = "DELETE FROM {TableName} WHERE {Conditions}";
                command = command.Replace("{TableName}", tableName);
                command = command.Replace("{Conditions}", ExtractConditions(conditions, ref parameters));

                return command;
            }

            public static string GenerateUpdateScript(string tableName, Dictionary<string, object?> binds, Collection<CommandCondition> conditions, ref Dictionary<string, object?> parameters)
            {
                string command = "UPDATE {TableName} SET {ColumnsValues} WHERE {Conditions}";
                command = command.Replace("{TableName}", tableName);
                parameters = new Dictionary<string, object?>(binds.Select(t => new KeyValuePair<string, object?>($"{Guid.NewGuid().ToString().Replace("-", "")}__{t.Key}", t.Value)).ToList());
                command = command.Replace("{ColumnsValues}", string.Join(", ", parameters.Select(t => $"{t.Key.Split("__", StringSplitOptions.RemoveEmptyEntries)[1]} = @{t.Key}")));
                command = command.Replace("{Conditions}", ExtractConditions(conditions, ref parameters));

                return command;
            }

            public static string GenerateInsertScript(string tableName, Dictionary<string, object?> binds, ref Dictionary<string, object?> parameters)
            {
                string command = "INSERT INTO {TableName}({Columns}) VALUES({Values})";
                command = command.Replace("{TableName}", tableName);
                parameters = new Dictionary<string, object?>(binds.Select(t => new KeyValuePair<string, object?>($"{Guid.NewGuid().ToString().Replace("-", "")}__{t.Key}", t.Value)).ToList());
                command = command.Replace("{Columns}", string.Join(", ", parameters.Select(t => t.Key.Split("__", StringSplitOptions.RemoveEmptyEntries)[1]).ToList()));
                command = command.Replace("{Values}", string.Join(", ", parameters.Select(t => $"@{t.Key}").ToList()));

                return command;
            }

            public static string GenerateSelectScript(CommandAdapter adapter, string tableName, string[] selections, Collection<CommandReference> references, Collection<CommandCondition> conditions, string[] groupings, Collection<CommandSort> sorts, int limit, int offset, ref Dictionary<string, object?> parameters)
            {
                string command = "SELECT {Selections} FROM {TableName} {References} {Conditions} {Groupings} {Sorts} {LimitAndOffset}";
                command = command.Replace("{TableName}", tableName);
                command = command.Replace("{Selections}", selections != null && selections.Length > 0 ? string.Join(", ", selections) : "*");
                command = command.Replace("{References}", ExtractReferences(references));
                command = command.Replace("{Conditions}", conditions.Count > 0 ? "WHERE " + ExtractConditions(conditions, ref parameters) : "");
                command = command.Replace("{Groupings}", groupings != null && groupings.Length > 0 ? "GROUP BY " + string.Join(", ", groupings) : "");
                command = command.Replace("{Sorts}", sorts.Count > 0 ? "ORDER BY " + string.Join(", ", sorts.Select(t => $" {t.Column} {t.Direction}")) : "");
                switch (adapter)
                {
                    case CommandAdapter.Mysql:
                    case CommandAdapter.PgSql:
                        command = command.Replace("{LimitAndOffset}", limit > 0 ? $"LIMIT {limit} OFFSET {offset}" : "");
                        break;
                    case CommandAdapter.Oracle:
                    case CommandAdapter.SqlServer:
                    default:
                        command = command.Replace("{LimitAndOffset}", limit > 0 ? $"OFFSET {offset} ROWS FETCH NEXT {limit} ROWS ONLY" : "");
                        break;
                }

                return command;
            }

            public static string ExtractConditions(Collection<CommandCondition> conditions, ref Dictionary<string, object?> parametes)
            {
                StringBuilder sbConditions = new StringBuilder();
                sbConditions.Append(" 1=1 ");
                foreach (var condition in conditions)
                {
                    string key = $"{Guid.NewGuid().ToString().Replace("-", "")}__{condition.Column.Replace(".", "_")}";
                    sbConditions.Append($" {condition.Operation?.ToString()} {(condition.BeginGroup ? "(" : "")} {condition.Column} {CommandBuilderFunction.MatchType(condition.Match)} {(IsValueBinding(condition.Match) ? "@" + key : "")} {(condition.EndGroup ? ")" : "")} ");
                    parametes.Add(key, condition.Value);
                }

                return sbConditions.ToString();
            }

            public static string ExtractReferenceConditions(Collection<CommandCondition> conditions)
            {
                StringBuilder sbConditions = new StringBuilder();
                foreach (var condition in conditions)
                {
                    sbConditions.Append($" {(sbConditions.Length > 0 ? condition.Operation?.ToString() : "")} {(condition.BeginGroup ? "(" : "")} {condition.Column} {CommandBuilderFunction.MatchType(condition.Match)} {condition.Value} {(condition.EndGroup ? ")" : "")} ");
                }

                return sbConditions.ToString();
            }

            public static string ExtractReferences(Collection<CommandReference> references)
            {
                StringBuilder sbReference = new StringBuilder();
                foreach (var reference in references)
                {
                    sbReference.AppendLine($" {ReferenceType(reference.ReferenceType)} {reference.TableName} ON {ExtractReferenceConditions(reference.OnConditions)} ");
                }
                return sbReference.ToString();
            }

            public static string ReferenceType(CommandReferenceType type)
            {
                switch (type)
                {
                    case CommandReferenceType.InnerJoin: return "INNER JOIN";
                    case CommandReferenceType.LeftJoin: return "LEFT JOIN";
                    case CommandReferenceType.RightJoin: return "RIGHT JOIN";
                    case CommandReferenceType.OuterJoin: return "OUTER JOIN";
                    case CommandReferenceType.Join:
                    default:
                        return "JOIN";
                }
            }

            public static string MatchType(CommandMatchType match)
            {
                switch (match)
                {
                    case CommandMatchType.NotEqual: return "<>";
                    case CommandMatchType.Lesser: return "<";
                    case CommandMatchType.LesserOrEqual: return "<=";
                    case CommandMatchType.GreaterOrEqual: return ">=";
                    case CommandMatchType.Greater: return ">";
                    case CommandMatchType.Contains: return "LIKE";
                    case CommandMatchType.NotContains: return "NOT LIKE";
                    case CommandMatchType.IsIn: return "IN";
                    case CommandMatchType.IsNotIn: return "NOT IN";
                    case CommandMatchType.IsNull: return "IS NULL";
                    case CommandMatchType.IsNotNull: return "IS NOT NULL";
                    case CommandMatchType.Equal:
                    default: return "=";
                }
            }

            public static bool IsValueBinding(CommandMatchType match)
            {
                switch (match)
                {
                    case CommandMatchType.IsNotNull:
                    case CommandMatchType.IsNull: return false;
                    case CommandMatchType.IsIn:
                    case CommandMatchType.IsNotIn:
                    case CommandMatchType.NotEqual:
                    case CommandMatchType.Lesser:
                    case CommandMatchType.LesserOrEqual:
                    case CommandMatchType.GreaterOrEqual:
                    case CommandMatchType.Greater:
                    case CommandMatchType.Equal:
                    case CommandMatchType.NotContains:
                    case CommandMatchType.Contains:
                    default: return true;
                }
            }
        }
    }
}
