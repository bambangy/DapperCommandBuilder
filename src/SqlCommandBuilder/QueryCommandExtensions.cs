using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SqlCommandBuilder
{
    public static class IDapperCommandExtensions
    {
        /// <summary>
        /// Set Adapter for query. This is to separate specially for Limit and Offset query pattern accross database type adapter.
        /// </summary>
        /// <param name="command">IDapperCommand</param>
        /// <param name="adapter">CommandAdapter : Type of supported adapter to translate to query</param>
        /// <returns>IDapperCommand</returns>
        public static IQueryCommand SetAdapter(this IQueryCommand command, CommandAdapter adapter) => command.SetAdapter(adapter);

        /// <summary>
        /// Add additional selection fields to query
        /// </summary>
        /// <param name="command">IDapperCommand</param>
        /// <param name="selections">string[] : Array of fields or columns</param>
        /// <returns>IDapperCommand</returns>
        public static IQueryCommand Select(this IQueryCommand command, string[] selections)
        {
            foreach (var selection in selections)
            {
                command.AddField(selection);
            }

            return command;
        }

        /// <summary>
        /// Initial select query statement with specified table and column to select
        /// </summary>
        /// <param name="command">IDapperCommand</param>
        /// <param name="tableName">string : Table name</param>
        /// <param name="selections">string[] : Array of fields or columns</param>
        /// <returns>IDapperCommand</returns>
        public static IQueryCommand Select(this IQueryCommand command, string tableName, string[] selections) => command.InitSelect(tableName, selections);

        /// <summary>
        /// Initial insert query statement with specific tablename
        /// </summary>
        /// <param name="command">IDapperCommand</param>
        /// <param name="tableName">string : Table name</param>
        /// <param name="parameters">Dictionary<string, object?> : Sets of column name and the value to insert</param>
        /// <returns>IDapperCommand</returns>
        public static IQueryCommand Insert(this IQueryCommand command, string tableName, Dictionary<string, object?> parameters) => command.InitInsert(tableName, parameters);

        /// <summary>
        /// Initial update query statement with specific tablename. !!!Don't forget to add Where() to update specific data!!!
        /// </summary>
        /// <param name="command">IDapperCommand</param>
        /// <param name="tableName">string : Table name</param>
        /// <param name="parameters">Dictionary<string, object?> : Sets of column name and the value to update</param>
        /// <returns>IDapperCommand</returns>
        public static IQueryCommand Update(this IQueryCommand command, string tableName, Dictionary<string, object?> parameters) => command.InitUpdate(tableName, parameters);

        /// <summary>
        /// Initial delete query statement with specific tablename. !!!Don't forget to add Where() to update specific data!!!
        /// </summary>
        /// <param name="command">IDapperCommand</param>
        /// <param name="tableName">string : Table name</param>
        /// <returns>IDapperCommand</returns>
        public static IQueryCommand Delete(this IQueryCommand command, string tableName) => command.InitDelete(tableName);

        public static IQueryCommand WhereAnd(this IQueryCommand command, string column, CommandMatchType matchType, object? value) => command.AddWhereAnd(column, matchType, value);

        public static IQueryCommand WhereOr(this IQueryCommand command, string column, CommandMatchType matchType, object? value) => command.AddWhereOr(column, matchType, value);

        public static IQueryCommand WhereGroupAnd(this IQueryCommand command, ICollection<(CommandOperation operation, string column, CommandMatchType matchType, object? value)> conditions) => command.AddWhereAndGroup(conditions);

        public static IQueryCommand WhereGroupOr(this IQueryCommand command, ICollection<(CommandOperation operation, string column, CommandMatchType matchType, object? value)> conditions) => command.AddWhereOrGroup(conditions);

        public static IQueryCommand Sort(this IQueryCommand command, string column, CommandOrderDirection direction) => command.AddSort(column, direction);

        public static IQueryCommand Join(this IQueryCommand command, CommandReferenceType type, string tableName, ICollection<(CommandOperation operation, string column, CommandMatchType matchType, object? value)> onConditions) => command.AddReference(type, tableName, onConditions);

        public static IQueryCommand Take(this IQueryCommand command, int take, int skip = 0) => command.SetTake(take).SetSkip(skip);

        public static IQueryCommandResult Build(this IQueryCommand command) => command.BuildCommand();
    }
}
