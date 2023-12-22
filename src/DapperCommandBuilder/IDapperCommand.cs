using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperCommandBuilder
{
    public interface IDapperCommand
    {
        /// <summary>
        /// Set query adapter. Mysql, SqlServer, Postgre, Orcale
        /// </summary>
        /// <param name="adapter">CommandAdapter</param>
        /// <returns>IDapperCommand</returns>
        IDapperCommand SetAdapter(CommandAdapter adapter);
        /// <summary>
        /// Add additional column for select statement
        /// </summary>
        /// <param name="columns">string: column name to select</param>
        /// <returns></returns>
        IDapperCommand AddField(string column);
        /// <summary>
        /// Set Select statement for query clause with initial columns selection
        /// </summary>
        /// <param name="tableName">string: Table name</param>
        /// <param name="columns">string[]: Array of column name. Don't forget source alias if table name contains alias</param>
        /// <returns>IDapperCommand</returns>
        IDapperCommand InitSelect(string tableName, string[] columns);
        /// <summary>
        /// Set Insert statement for query clause with specific columns and values to insert
        /// </summary>
        /// <param name="tableName">string: Table name</param>
        /// <param name="parameters">Dictionary<string, object?>: Sets of column and value to insert</param>
        /// <returns>IDapperCommand</returns>
        IDapperCommand InitInsert(string tableName, Dictionary<string, object?> parameters);
        /// <summary>
        /// Set Update statement for query clause with specific column and values to update
        /// </summary>
        /// <param name="tableName">string: Table name</param>
        /// <param name="parameters">Dictionary<string, object?>: Sets of column and value to update</param>
        /// <returns>IDapperCommand</returns>
        IDapperCommand InitUpdate(string tableName, Dictionary<string, object?> parameters);
        /// <summary>
        /// Set Delete statement for query clause. !!!Please add where condition to prevent delete all data!!!
        /// </summary>
        /// <param name="tableName">string: Table name</param>
        /// <returns>IDapperCommand</returns>
        IDapperCommand InitDelete(string tableName);
        /// <summary>
        /// Add condition with AND operation to other condition
        /// </summary>
        /// <param name="column">string: Column name</param>
        /// <param name="matchType">CommandMatchType: Match type like equal, lesser, greater etc.</param>
        /// <param name="value">object?: Value of condition. It also accept wildcard characters for Contains, ContainsStart and ContainsEnd condition</param>
        /// <returns>IDapperCommand</returns>
        IDapperCommand AddWhereAnd(string column, CommandMatchType matchType, object? value);
        /// <summary>
        /// Add condition with OR operation to other condition
        /// </summary>
        /// <param name="column">string: Column name</param>
        /// <param name="matchType">CommandMatchType: Match type like equal, lesser, greater etc.</param>
        /// <param name="value">object?: Value of condition. It also accept wildcard characters for Contains, ContainsStart and ContainsEnd condition</param>
        /// <returns>IDapperCommand</returns>
        IDapperCommand AddWhereOr(string column, CommandMatchType matchType, object? value);
        /// <summary>
        /// Add Sets of condition that grouped by ( and ) with AND operation to other conditions.
        /// </summary>
        /// <param name="conditions">ICollection<(CommandOperation operation, string column, CommandMatchType matchType, object? value)></param>
        /// <returns>IDapperCommand</returns>
        IDapperCommand AddWhereAndGroup(ICollection<(CommandOperation operation, string column, CommandMatchType matchType, object? value)> conditions);
        /// <summary>
        /// Add Sets of condition that grouped by ( and ) with OR operation to other conditions.
        /// </summary>
        /// <param name="conditions">ICollection<(CommandOperation operation, string column, CommandMatchType matchType, object? value)></param>
        /// <returns>IDapperCommand</returns>
        IDapperCommand AddWhereOrGroup(ICollection<(CommandOperation operation, string column, CommandMatchType matchType, object? value)> conditions);
        /// <summary>
        /// Add Reference (JOIN) of other source to the initial source.
        /// </summary>
        /// <param name="type">CommandReferenceType</param>
        /// <param name="tableName">string: Table name</param>
        /// <param name="onConditions">ICollection<(CommandOperation operation, string column, CommandMatchType matchType, object? value)></param>
        /// <returns>IDapperCommand</returns>
        IDapperCommand AddReference(CommandReferenceType type, string tableName, ICollection<(CommandOperation operation, string column, CommandMatchType matchType, object? value)> onConditions);
        /// <summary>
        /// Add sort clause to statement
        /// </summary>
        /// <param name="column">string: Column name to sort</param>
        /// <param name="direction">CommandOrderDirection</param>
        /// <returns>IDapperCommand</returns>
        IDapperCommand AddSort(string column, CommandOrderDirection direction);
        /// <summary>
        /// Add column to grouped
        /// </summary>
        /// <param name="groupName">string column name</param>
        /// <returns>IDapperCommand</returns>
        IDapperCommand AddGroupBy(string column);
        /// <summary>
        /// Add limit clause to statement
        /// </summary>
        /// <param name="limit">int</param>
        /// <returns>IDapperCommand</returns>
        IDapperCommand SetTake(int limit);
        /// <summary>
        /// Add offset clause to statement
        /// </summary>
        /// <param name="offset">int</param>
        /// <returns>IDapperCommand</returns>
        IDapperCommand SetSkip(int offset);
        /// <summary>
        /// Build command then produce raw sql query and parameters to bind
        /// </summary>
        /// <returns>IDapperCommandResult</returns>
        IDapperCommandResult BuildCommand();
    }
}
