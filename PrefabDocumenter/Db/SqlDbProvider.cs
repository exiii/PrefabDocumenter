﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Data.SQLite;

namespace PrefabDocumenter.Db
{
    public class SqlDbProvider<T> : IDisposable where T : IModel
    {
        private string dbFilePath;
        private SQLiteConnectionStringBuilder sqlConnSB;
        private SQLiteConnection dbConnecter;

        public static string DropTableCommand;
        public static string CreateTableCommand;

        public SqlDbProvider(string DbFilePath)
        {
            dbFilePath = DbFilePath;
            sqlConnSB = new SQLiteConnectionStringBuilder { DataSource = dbFilePath };

            dbConnecter = new SQLiteConnection(sqlConnSB.ToString());
            dbConnecter.Open();
        }

        public void InitTable()
        {
            using (var cmd = new SQLiteCommand(dbConnecter))
            {
                cmd.CommandText = DropTableCommand;
                cmd.ExecuteNonQuery();

                cmd.CommandText = CreateTableCommand;
                cmd.ExecuteNonQuery();
            }
        }

        public void Insert(IModel model)
        {
            using (var cmd = new SQLiteCommand(dbConnecter))
            {
                cmd.CommandText = model.InsertCommand;

                cmd.ExecuteNonQuery();
            }
        }

        public void Inserts(IEnumerable<IModel> models)
        {
            using (var cmd = new SQLiteCommand(dbConnecter))
            {
                foreach (var model in models)
                {
                    cmd.CommandText = model.InsertCommand;

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Dispose()
        {
            dbConnecter.Dispose();
        }
    }
}
