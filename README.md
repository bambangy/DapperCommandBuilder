# SqlCommandBuilder
A library for building raw SQL queries more maintainably and with fewer mistakes.

## Prequisite
None to be prepared

## How to use
### Introduction
 - You can start querying by call `CommandBuilder.Init()`. Then write your query as your needs.
 - You can use some method to construct your query and at the end call `.Build()`
 - `.Build()` have return of `IQueryCommandResult`, which have two properties `Script` and `Parameters`
 - `Script` is raw sql query
 - `Parameters` is sets of name and value bind parameters to query.
### SELECT
 - `Select(string tableName, string[] columns)` used to construct initial statement as select statement with initial columns to select
 - `Select(string[] columns)` used to add additional field to select within statement
 
Example
```
CommandBuilder.Init().Select("category", new string[] { "name", "category_id" }).Build()
```
This will produce raw query
```
SELECT name, category_id FROM category 
```
### INSERT
You can construct insert statement and specify sets of column and value to insert to specific table
```
CommandBuilder.Init().Insert("category", new Dictionary<string, object?>()
{
{ "name", "Karya Ilmiah" }
}).Build();
```
This will produce raw query
```
INSERT INTO category(name) VALUES(@name)
```
And the binding parameters will available in `IQueryCommandResult.Parameters` that contains key value pair of `@name` dan `Karya Ilmiah`
