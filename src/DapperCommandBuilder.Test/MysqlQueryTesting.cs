using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace DapperCommandBuilder.Test
{
    public class MySqlQueryTesting
    {
        private MySqlConnection _connection;
        private DapperCommandResult _strCommand;

        [SetUp]
        public void Setup()
        {
            string connectionString = "Server=localhost;Database=sakila;Uid=root;Pwd=Developer11!;";
            _connection = new MySqlConnection(connectionString);
            _strCommand = DapperCommand.Init()
                .Select("film_category a", new string[] { "b.title Film_Title", "b.description Film_Description", "c.name Category" })
                .Reference(CommandReference.Add(CommandReferenceType.Join, "film b", new System.Collections.ObjectModel.Collection<CommandCondition>()
                {
                    CommandCondition.Add("b.film_id", CommandMatchType.Equal, "a.film_id")
                }))
                .Reference(CommandReference.Add(CommandReferenceType.Join, "category c", new System.Collections.ObjectModel.Collection<CommandCondition>()
                {
                    CommandCondition.Add("c.category_id", CommandMatchType.Equal, "a.category_id")
                }))
                .Where(CommandCondition.Add("b.rating", CommandMatchType.Contains, "%G%"))
                .Build();
        }

        [Test]
        public void TestComposeScript()
        {
            Console.WriteLine(_strCommand.Script.ToString());
            Assert.IsNotNull(_strCommand.Script);
            Assert.IsNotNull(_strCommand.Parameters);
        }

        [Test]
        public void TestQuerySelect()
        {
            _connection.Open();
            IEnumerable<dynamic> films = _connection.Query<dynamic>(_strCommand.Script, _strCommand.Parameters);
            _connection.Close();
            Console.WriteLine(JsonConvert.SerializeObject(films));
            Assert.IsNotNull(films);
            Assert.IsTrue(films.Any());
        }
    }
}