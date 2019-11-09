﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace REAC_AndroidAPI.Utils.Storage
{
    public class SqlDatabaseClient : IDisposable
    {
        private MySqlConnection mConnection;
        private MySqlCommand mCommand;


        public SqlDatabaseClient(MySqlConnection Connection)
        {
            mConnection = Connection;
            mCommand = new MySqlCommand();
            mCommand.Connection = mConnection;
        }

        /// <summary>
        /// Called when released from using() scope - does not actually dispose, just marks as available
        /// </summary>
        public void Dispose()
        {
            if (mConnection != null)
            {
                mConnection.Close();
                mConnection = null;
            }
            if (mCommand != null)
            {
                mCommand.Dispose();
                mCommand = null;
            }
        }

        public void SetParameter(string Key, object Value)
        {
            mCommand.Parameters.Add(new MySqlParameter(Key, Value));
        }

        public void SetParameterByteArray(string Key, byte[] Value)
        {
            mCommand.Parameters.Add(new MySqlParameter(Key, MySqlDbType.LongBlob, Value.Length)).Value = Value;
        }

        public int ExecuteNonQuery(string CommandText)
        {
            mCommand.CommandText = CommandText;

            int Affected = mCommand.ExecuteNonQuery();

            return Affected;
        }

        public DataSet ExecuteQuerySet(string CommandText)
        {
            DataSet DataSet = new DataSet();

            mCommand.CommandText = CommandText;

            using (MySqlDataAdapter Adapter = new MySqlDataAdapter(mCommand))
            {
                Adapter.Fill(DataSet);
            }

            return DataSet;
        }

        public DataTable ExecuteQueryTable(string CommandText)
        {
            DataSet DataSet = ExecuteQuerySet(CommandText);
            return DataSet.Tables.Count > 0 ? DataSet.Tables[0] : null;
        }

        public DataRow ExecuteQueryRow(string CommandText)
        {
            DataTable DataTable = ExecuteQueryTable(CommandText);
            return DataTable.Rows.Count > 0 ? DataTable.Rows[0] : null;
        }
    }

}
