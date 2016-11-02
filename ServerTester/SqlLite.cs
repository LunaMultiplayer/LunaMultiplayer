using System;
using System.Data;
using Mono.Data.SqliteClient;

namespace ServerTester
{
    public class SqlLite
    {
        public static void Test()
        {
            string _strDBName = "URI=file:MasterSQLite.db";
            IDbConnection _connection = new SqliteConnection(_strDBName);
            IDbCommand _command = _connection.CreateCommand();
            string sql;

            _connection.Open();

            sql = "CREATE TABLE highscores (name VARCHAR(20), score INT)";
            _command.CommandText = sql;
            _command.ExecuteNonQuery();

            sql = "INSERT INTO highscores (name, score) VALUES ('Me', 3000)";
            _command.CommandText = sql;
            _command.ExecuteNonQuery();

            sql = "insert into highscores (name, score) values ('Myself', 6000)";
            _command.CommandText = sql;
            _command.ExecuteNonQuery();

            sql = "insert into highscores (name, score) values ('And I', 9001)";
            _command.CommandText = sql;
            _command.ExecuteNonQuery();

            sql = "select * from highscores order by score desc";
            _command.CommandText = sql;
            IDataReader _reader = _command.ExecuteReader();
            while (_reader.Read())
                Console.Write("****** Name: " + _reader["name"] + "\tScore: " + _reader["score"]);

            _reader.Close();
            _reader = null;
            _command.Dispose();
            _command = null;
            _connection.Close();
            _connection = null;
        }
    }
}
