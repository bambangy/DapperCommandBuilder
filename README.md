# DapperCommandBuilder
Command Builder (not only) for Dapper. This will make querying raw script more comfortable (at least for me). And reduce the mistaken in writing a sql command.

## Prequisite
None to be prepared

## How to use
### Select statement
```
DapperCommandResult cmd = DapperCommand.Init()
                .Select("Employee a", new string[] { "a.Name", "a.BirthDate" })
                .Reference(CommandReference.Add(CommandReferenceType.Join, "Department b", new System.Collections.ObjectModel.Collection<CommandCondition>()
                {
                    CommandCondition.Add("b.ID", CommandMatchType.Equal, "a.DepartmentID"),
                    CommandCondition.Add("b.RowStatus", CommandMatchType.Equal, 0, CommandOperation.And)
                }))
                .Where(CommandCondition.Add("a.BirthDate", CommandMatchType.GreaterOrEqual, new DateTime(1992, 01, 01)))
                .Where(CommandCondition.Add("b.Name", CommandMatchType.Equal, "HR", operation: CommandOperation.And, beginGroup: true))
                .Where(CommandCondition.Add("b.Name", CommandMatchType.Equal, "IT", operation: CommandOperation.Or, endGroup: true))
                .Build();
```
Then access the script from `cmd.Script`
```
SELECT a.Name, a.BirthDate FROM Employee a  JOIN Department b ON    b.ID = a.DepartmentID   And  b.RowStatus = 0  WHERE    a.BirthDate >= @e8e3274ebcdc40579dbf57ca21f4af2a__a_BirthDate   And ( b.Name = @052f981356164179b3d9ec3befe46b04__b_Name   Or  b.Name = @f7d67afdd86d43f7b13a19f75f3c069a__b_Name )
```
And access the binding parameters in `cmd.Parameters` as `Dictionary<string, object?>` types.
Be awae of using `alias` after table name, it must be specified to all properties of it table inside builder.
