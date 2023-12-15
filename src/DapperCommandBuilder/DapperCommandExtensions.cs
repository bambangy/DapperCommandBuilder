using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DapperCommandBuilder
{
    public static class DapperCommandExtensions
    {
        public static DapperCommand SetAdapter(this DapperCommand command, CommandAdapter adapter)
        {
            command.Adapter = adapter;
            return command;
        }

        public static DapperCommand Select(this DapperCommand command, string tableName, string[] selections)
        {
            command.Type = CommandType.SELECT;
            command.TableName = tableName;
            command.Selections = selections;
            return command;
        }

        public static DapperCommand Insert(this DapperCommand command, string tableName)
        {
            command.TableName = tableName;
            command.Type = CommandType.INSERT;
            return command;
        }

        public static DapperCommand Update(this DapperCommand command, string tableName)
        {
            command.TableName = tableName;
            command.Type = CommandType.UPDATE;
            return command;
        }

        public static DapperCommand Delete(this DapperCommand command, string tableName)
        {
            command.TableName = tableName;
            command.Type = CommandType.DELETE;
            return command;
        }

        public static DapperCommand Where(this DapperCommand command, CommandCondition condition)
        {
            if (command.Conditions == null) command.Conditions = new();
            command.Conditions.Add(condition);
            return command;
        }

        public static DapperCommand AddBinds(this DapperCommand command, Dictionary<string, object?> binds)
        {
            if (command.Bindings == null) command.Bindings = new Dictionary<string, object?>();
            foreach (var bind in binds)
            {
                command.Bindings.Add(bind.Key, bind.Value);
            }
            return command;
        }

        public static DapperCommand Reference(this DapperCommand command, CommandReference reference)
        {
            if (command.References == null) command.References = new System.Collections.ObjectModel.Collection<CommandReference>();
            command.References.Add(reference);
            return command;
        }

        public static DapperCommand Sort(this DapperCommand command, CommandSort sort)
        {
            if (command.Sorts == null) command.Sorts = new System.Collections.ObjectModel.Collection<CommandSort>();
            command.Sorts.Add(sort);
            return command;
        }

        public static DapperCommand Limit(this DapperCommand command, int limit)
        {
            command.Limit = limit;
            return command;
        }

        public static DapperCommand Offset(this DapperCommand command, int offset)
        {
            command.Offset = offset;
            return command;
        }

        public static DapperCommand Grouping(this DapperCommand command, string[] groupings)
        {
            command.Groupings = groupings;
            return command;
        }

        public static DapperCommandResult Build(this DapperCommand command)
        {
            DapperCommandResult result = new DapperCommandResult();
            Dictionary<string, object?> parameters = new Dictionary<string, object?>();

            switch (command.Type)
            {
                case CommandType.INSERT:
                    {
                        result.Script = Function.GenerateInsertScript(command.TableName, command.Bindings, ref parameters);
                        result.Parameters = parameters;
                    }
                    break;
                case CommandType.UPDATE:
                    {
                        result.Script = Function.GenerateUpdateScript(command.TableName, command.Bindings, command.Conditions, ref parameters);
                        result.Parameters = parameters;
                    }
                    break;
                case CommandType.DELETE:
                    {
                        result.Script = Function.GenerateDeleteScript(command.TableName, command.Conditions, ref parameters);
                        result.Parameters = parameters;
                    }
                    break;
                case CommandType.SELECT:
                default:
                    {
                        result.Script = Function.GenerateSelectScript(command.Adapter, command.TableName, command.Selections, command.References, command.Conditions, command.Groupings, command.Sorts, command.Limit, command.Offset, ref parameters);
                        result.Parameters = parameters;
                    }
                    break;
            }

            return result;
        }

        private class Function
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
                command = command.Replace("{Columnsvalues}", string.Join(", ", parameters.Select(t=>$"{t.Key.Split("__", StringSplitOptions.RemoveEmptyEntries)[1]} = @{t.Key}")));
                command = command.Replace("{Conditions}", ExtractConditions(conditions, ref parameters));

                return command;
            }

            public static string GenerateInsertScript(string tableName, Dictionary<string, object?> binds, ref Dictionary<string, object?> parameters)
            {
                string command = "INSERT INTO {TableName}({Columns}) VALUES({Values})";
                command = command.Replace("{TableName}", tableName);
                parameters = new Dictionary<string, object?>(binds.Select(t => new KeyValuePair<string, object?>($"{Guid.NewGuid().ToString().Replace("-", "")}__{t.Key}", t.Value)).ToList());
                command = command.Replace("{Columns}", string.Join(", ", parameters.Select(t=>t.Key.Split("__", StringSplitOptions.RemoveEmptyEntries)[1]).ToList()));
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
                foreach (var condition in conditions)
                {
                    string key = $"{Guid.NewGuid().ToString().Replace("-", "")}__{condition.Column.Replace(".","_")}";
                    sbConditions.Append($" {condition.Operation?.ToString()} {(condition.BeginGroup ? "(" : "")} {condition.Column} {Function.MatchType(condition.Match)} {(IsValueBinding(condition.Match) ? "@" + key : "")} {(condition.EndGroup ? ")" : "")} ");
                    parametes.Add(key, condition.Value);
                }

                return sbConditions.ToString();
            }

            public static string ExtractReferenceConditions(Collection<CommandCondition> conditions)
            {
                StringBuilder sbConditions = new StringBuilder();
                foreach (var condition in conditions)
                {
                    sbConditions.Append($" {condition.Operation?.ToString()} {(condition.BeginGroup ? "(" : "")} {condition.Column} {Function.MatchType(condition.Match)} {condition.Value} {(condition.EndGroup ? ")" : "")} ");
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
                    case CommandMatchType.ContainsStart:
                    case CommandMatchType.ContainsEnd:
                    case CommandMatchType.Contains: return "LIKE";
                    case CommandMatchType.NotContainsStart:
                    case CommandMatchType.NotContainsEnd:
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
                    case CommandMatchType.NotContainsStart:
                    case CommandMatchType.ContainsStart:
                    case CommandMatchType.NotContainsEnd:
                    case CommandMatchType.ContainsEnd:
                    case CommandMatchType.NotContains:
                    case CommandMatchType.Contains:
                    default: return true;
                }
            }
        }
    }
}
