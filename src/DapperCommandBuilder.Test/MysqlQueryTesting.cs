using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace DapperCommandBuilder.Test
{
    public class MySqlQueryTesting
    {
        private MySqlConnection _connection;

        [SetUp]
        public void Setup()
        {
            string connectionString = "Server=localhost;Database=sakila;Uid=root;Pwd=Developer11!;";
            _connection = new MySqlConnection(connectionString);
            _connection.Open();
        }

        [Test]
        public void TestComposeScript()
        {
            IDapperCommandResult strCommand = CommandBuilder.Init()
                .Select("film_category a", new string[] { "b.title Film_Title", "b.description Film_Description", "c.name Category" })
                .Join(CommandReferenceType.Join, "film b", new Collection<(CommandOperation operation, string column, CommandMatchType matchType, object? value)>()
                {
                    new (CommandOperation.And, "b.film_id", CommandMatchType.Equal, "a.film_id")
                })
                .Join(CommandReferenceType.Join, "category c", new Collection<(CommandOperation operation, string column, CommandMatchType matchType, object? value)>()
                {
                    new (CommandOperation.And, "c.category_id", CommandMatchType.Equal, "a.category_id")
                })
                .WhereAnd("title", CommandMatchType.Contains, "%G%")
                .Build();
            Console.WriteLine(strCommand.Script.ToString());
            Assert.That(strCommand.Script, Is.Not.Null);
            Assert.That(strCommand.Parameters, Is.Not.Null);
        }

        [Test]
        public void TestQuerySimpleSelect()
        {
            IDapperCommandResult strCommand = CommandBuilder.Init()
                .Select("film", new string[] { })
                .Build();

            IEnumerable<dynamic> films = _connection.Query<dynamic>(strCommand.Script, strCommand.Parameters);
            Console.WriteLine(JsonConvert.SerializeObject(films));
            Assert.That(films, Is.Not.Null);
            Assert.That(films.Any(), Is.True);
        }

        [Test]
        public void TestQueryWhere()
        {
            IDapperCommandResult strCommand = CommandBuilder.Init()
                .Select("film", new string[] { })
                .WhereAnd("rating", CommandMatchType.Equal, "PG")
                .Build();

            IEnumerable<dynamic> films = _connection.Query<dynamic>(strCommand.Script, strCommand.Parameters);
            Console.WriteLine(JsonConvert.SerializeObject(films));
            Assert.That(films, Is.Not.Null);
            Assert.That(films.Any(), Is.True);
        }

        [Test]
        public void TestQueryLimit()
        {
            IDapperCommandResult strCommand = CommandBuilder.Init()
                .SetAdapter(CommandAdapter.Mysql)
                .Select("film", new string[] { })
                .Take(5)
                .Build();

            IEnumerable<dynamic> films = _connection.Query<dynamic>(strCommand.Script, strCommand.Parameters);
            Console.WriteLine(JsonConvert.SerializeObject(films));
            Assert.That(films, Is.Not.Null);
            Assert.That(films.Count(), Is.EqualTo(5));
        }

        [Test]
        public void TestQueryOrder()
        {
            IDapperCommandResult strCommand = CommandBuilder.Init()
                .Select("film", new string[] { })
                .Sort("title", CommandOrderDirection.Desc)
                .Build();

            IEnumerable<dynamic> films = _connection.Query<dynamic>(strCommand.Script, strCommand.Parameters);
            Console.WriteLine(JsonConvert.SerializeObject(films));
            Assert.That(films, Is.Not.Null);
            Assert.That(films.Count(), Is.GreaterThan(0));
            Assert.That(films.FirstOrDefault().title.Substring(0, 1), Is.EqualTo("Z"));
        }

        [Test]
        public void TestInsertUpdateDelete()
        {
            IDapperCommandResult strCommand = CommandBuilder.Init()
                .Insert("category", new Dictionary<string, object?>()
                {
                    { "name", "Fiksi Ilmiah" }
                })
                .Build();
            _connection.Execute(strCommand.Script, strCommand.Parameters);
            strCommand = CommandBuilder.Init()
                .Select("category", new string[] { })
                .WhereAnd("name", CommandMatchType.Equal, "Fiksi Ilmiah")
                .Build();
            dynamic category = _connection.Query<dynamic>(strCommand.Script, strCommand.Parameters).FirstOrDefault();
            Assert.That(category, Is.Not.Null);

            strCommand = CommandBuilder.Init()
                .Update("category", new Dictionary<string, object?>()
                {
                    {"name", "Karya Ilmiah" }
                })
                .WhereAnd("category_id", CommandMatchType.Equal, (int)category.category_id)
                .Build();
            _connection.Execute(strCommand.Script, strCommand.Parameters);

            strCommand = CommandBuilder.Init()
                .Select("category", new string[] { })
                .WhereAnd("name", CommandMatchType.Equal, "Karya Ilmiah")
                .Build();
            category = _connection.Query<dynamic>(strCommand.Script, strCommand.Parameters).FirstOrDefault();
            Assert.That(category, Is.Not.Null);

            strCommand = CommandBuilder.Init()
                .Delete("category").WhereAnd("category_id", CommandMatchType.Equal, (int)category.category_id).Build();
            _connection.Execute(strCommand.Script, strCommand.Parameters);

            strCommand = CommandBuilder.Init()
                .Select("category", new string[] { })
                .WhereAnd("name", CommandMatchType.Equal, "Karya Ilmiah")
                .Build();
            category = _connection.Query<dynamic>(strCommand.Script, strCommand.Parameters).FirstOrDefault();
            Assert.That(category, Is.Null);
        }
    }
}