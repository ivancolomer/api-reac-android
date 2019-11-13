using REAC_AndroidAPI.Utils.Output;
using REAC_AndroidAPI.Utils.Storage;
using System;
using System.Collections.Generic;
using System.Data;
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

        public static User GetAdministratorFromDB(uint user_id, string password)
        {
            DataRow userRow = null;
            try
            {
                using (SqlDatabaseClient client = SqlDatabaseManager.GetClient())
                {
                    string sql = "SELECT m.id, m.name, m.role " +
                        "FROM Administrator AS a " +
                        "INNER JOIN Member AS m ON m.id = a.member_id " +
                        "WHERE a.member_id = @member_id AND a.password_hash = @password_hash";
                    client.SetParameter("@member_id", user_id);
                    client.SetParameter("@password_hash", password);

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
    }
}
