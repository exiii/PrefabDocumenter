using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Data.SQLite;
using System.Reflection;

namespace PrefabDocumenter.DB
{
    public class SqlDbProvider<T> where T : IModel
    {
        private string dbFilePath;
        private SQLiteConnectionStringBuilder sqlConnSB;
        private SQLiteConnection dbConnecter;

        private const string CREATE_TABLE_COMMAND = "CreateTableCommand";

        public SqlDbProvider(string dbFilePath)
        {
            this.dbFilePath = dbFilePath;
            sqlConnSB = new SQLiteConnectionStringBuilder { DataSource = this.dbFilePath };

            dbConnecter = new SQLiteConnection(sqlConnSB.ToString());
            dbConnecter.Open();
        }

        public void CreateTable()
        {
            var subTypes = Assembly.GetAssembly(typeof(T))
                                   .GetTypes()
                                   .Where(t => {
                                       return typeof(T).IsAssignableFrom(t) && !t.IsAbstract;
                                   });

            using (var cmd = new SQLiteCommand(dbConnecter))
            {
                foreach (var type in subTypes)
                {
                    Console.WriteLine(type);
                    Console.WriteLine(type.GetProperties(BindingFlags.Static).Where(x => x.PropertyType == typeof(string)).First().GetValue(type, null) as string);
                    cmd.CommandText = type.GetProperty(CREATE_TABLE_COMMAND, BindingFlags.Static).GetValue(type) as string;
                    //[TODO]

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Insert(IModel model)
        {
            using (var cmd = new SQLiteCommand(dbConnecter))
            {
                cmd.CommandText = model.InsertCommand;
            }
        }

        public void Inserts(IEnumerable<IModel> models)
        {
            using (var cmd = new SQLiteCommand(dbConnecter))
            {
                foreach (var model in models)
                {
                    cmd.CommandText = model.InsertCommand;
                    
                }
            }
        }

        ~SqlDbProvider()
        {
            dbConnecter.Dispose();
        }
    }
}
