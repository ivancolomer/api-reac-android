using REAC_AndroidAPI.Utils.Output;
using REAC_AndroidAPI.Utils.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace REAC_AndroidAPI.Entities
{
    public class User
    {
        public bool IsOwner { get; set; }
        public uint UserID { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }

        //Stuff for the API in-memory User
        public string SessionID { get; set; }
        public string IPAddress { get; set; }
        public long TimeCreated { get; set; }

        public static User GetAdministratorFromDB(string userName, string password)
        {
            DataRow userRow = null;
            try
            {
                using (SqlDatabaseClient client = SqlDatabaseManager.GetClient())
                {
                    string sql = "SELECT m.id, m.name, m.role " +
                        "FROM Administrator AS a " +
                        "INNER JOIN Member AS m ON m.id = a.member_id " +
                        "WHERE m.name = @user_name AND a.password_hash = @password_hash;";
                    client.SetParameter("@user_name", userName);
                    client.SetParameterByteArray("@password_hash", Convert.FromBase64String(password));

                    userRow = client.ExecuteQueryRow(sql);
                }
            }
            catch (Exception e)
            {
                Logger.WriteLine(e.ToString(), Logger.LOG_LEVEL.WARN);
            }

            if (userRow == null)
            {
                return null;
            }

            uint userId;
            if (!UInt32.TryParse(userRow["id"].ToString(), out userId))
                return null;


            User newUser = new User();
            newUser.UserID = userId;
            newUser.IsOwner = true;
            newUser.Name = userRow["name"].ToString();

            return newUser;
        }

        public static int GetAdministratorsCountFromDB()
        {
            DataRow row = null;
            try
            {
                using (SqlDatabaseClient client = SqlDatabaseManager.GetClient())
                {
                    row = client.ExecuteQueryRow("SELECT COUNT(*) FROM Administrator;");
                }
            }
            catch (Exception e)
            {
                Logger.WriteLine(e.ToString(), Logger.LOG_LEVEL.WARN);
            }

            if (row == null)
                return -1;

            int count;
            if (!Int32.TryParse(row[0].ToString(), out count))
                return -1;

            return count;
        }

        public static int InsertNewMemberToDB(User user, SqlDatabaseClient client = null)
        {
            bool clientIsNull = client == null;
            try
            {
                if (clientIsNull)
                    client = SqlDatabaseManager.GetClient();

                string sql = "INSERT INTO Member (name, role) VALUES(@name, @role);";
                client.SetParameter("@name", user.Name);
                client.SetParameter("@role", user.Role);

                int count = client.ExecuteNonQuery(sql);

                if (clientIsNull)
                    client.Dispose();

                return count;
            }
            catch(DbException e)
            {
                if (!clientIsNull)
                    throw;

                client?.Dispose();

                if (e.Message.Contains("unique_member_name_ck"))
                    return -2;
                else
                    Logger.WriteLine(e.ToString(), Logger.LOG_LEVEL.WARN);
            }

            return 0;
        }

        public static int InsertNewAdministratorToDB(User user, byte[] password)
        {
            int count = 0;
            try
            {
                using (SqlDatabaseClient client = SqlDatabaseManager.GetClient())
                {
                    string sql = "INSERT INTO Administrator (member_id, password_hash) VALUES(@member_id, @password_hash);";

                    count = InsertNewMemberToDB(user, client);
                    if(count > 0)
                    {
                        client.SetParameter("@member_id", client.LastInsertedId());
                        client.SetParameter("@password_hash", password);

                        count = client.ExecuteNonQuery(sql);
                    }
                }
            }
            catch (DbException e)
            {
                if (e.Message.Contains("unique_member_name_ck"))
                    return -2;
                else if (e.Message.Contains("unique_member_administrator_ck"))
                    return -3;
                else
                    Logger.WriteLine(e.ToString(), Logger.LOG_LEVEL.WARN);
            }

            return count;
        }
    }
}
