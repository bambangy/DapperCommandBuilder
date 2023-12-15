namespace DapperCommandBuilder.Test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
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
            Console.WriteLine(cmd.Script.ToString());
            Assert.IsNotNull(cmd.Script);
            Assert.IsNotNull(cmd.Parameters);
        }
    }
}