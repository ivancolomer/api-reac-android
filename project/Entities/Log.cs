using REAC_AndroidAPI.Utils.Output;
using REAC_AndroidAPI.Utils.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace REAC_AndroidAPI.Entities
{
    public class Log
    {
        public uint LogID { get; set; }

        public uint UserID { get; set; }
        public string Name { get; set; }
        public string ProfilePhoto { get; set; }

        public string Date { get; set; } //TimeZone.CurrentTimeZone.ToUniversalTime()
        public string Info { get; set; }


        public static int GetLogs(DateTime beginDate, DateTime endDate, out List<Log> logs)
        {
            logs = new List<Log>();

            DataTable table = null;
            try
            {
                using (SqlDatabaseClient client = SqlDatabaseManager.GetClient())
                {
                    client.SetParameter("@beginDate", beginDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    client.SetParameter("@endDate", endDate.ToString("yyyy-MM-dd HH:mm:ss"));

                    table = client.ExecuteQueryTable("SELECT l.id, l.member_id, l.date_added, l.info, m.name " +
                        "FROM Entry AS l " +
                        "INNER JOIN Member AS m ON m.id = l.member_id " +
                        "WHERE l.date_added >= @beginDate AND l.date_added <= @endDate;");
                }
            }
            catch (Exception e)
            {
                Logger.WriteLine(e.ToString(), Logger.LOG_LEVEL.WARN);
            }

            if (table == null)
                return -1;

            foreach (DataRow row in table.Rows)
            {
                uint logId, userId;
                if (!UInt32.TryParse(row["id"].ToString(), out logId) || !UInt32.TryParse(row["member_id"].ToString(), out userId))
                    continue;

                Log newLog = new Log();
                newLog.LogID = logId;

                newLog.UserID = userId;
                newLog.Name = row["name"].ToString();
                newLog.ProfilePhoto = LocalUser.URL_USER_IMAGE + userId.ToString() + LocalUser.URL_USER_PROFILE_IMAGE;
                
                newLog.Info = row["info"].ToString();
                newLog.Date = DateTime.Parse(row["date_added"].ToString()).ToUniversalTime().ToString("dd/MM/yyyy-HH:mm:ss"); //DateTime.ParseExact(row["date_added"].ToString(), "yyyy-MM-dd HH:mm:ss", null, DateTimeStyles.AssumeLocal).ToUniversalTime().ToString("dd/MM/yyyy-HH:mm:ss");

                logs.Add(newLog);
            }

            return 0;
        }

        public static int GetNotifications(LocalUser user, out List<Log> logs)
        {
            logs = new List<Log>();

            DataTable table = null;
            try
            {
                using (SqlDatabaseClient client = SqlDatabaseManager.GetClient())
                {
                    client.SetParameter("@userid", user.UserID);
                    client.SetParameter("@date", user.TimeRegisteredLocal.ToString("yyyy-MM-dd HH:mm:ss"));

                    table = client.ExecuteQueryTable("SELECT l.id, l.member_id, l.date_added, l.info, m.name " +
                        "FROM Entry AS l " +
                        "LEFT JOIN EntryRead AS e ON e.member_id = @userid AND e.entry_id = l.id " +
                        "INNER JOIN Member AS m ON m.id = l.member_id " +
                        "WHERE e.entry_id IS NULL AND l.date_added >= @date;");
                }
            }
            catch (Exception e)
            {
                Logger.WriteLine(e.ToString(), Logger.LOG_LEVEL.WARN);
            }

            if (table == null)
                return -1;

            foreach (DataRow row in table.Rows)
            {
                uint logId, userId;
                if (!UInt32.TryParse(row["id"].ToString(), out logId) || !UInt32.TryParse(row["member_id"].ToString(), out userId))
                    continue;

                Log newLog = new Log();
                newLog.LogID = logId;

                newLog.UserID = userId;
                newLog.Name = row["name"].ToString();
                newLog.ProfilePhoto = LocalUser.URL_USER_IMAGE + userId.ToString() + LocalUser.URL_USER_PROFILE_IMAGE;

                newLog.Info = row["info"].ToString();
                newLog.Date = DateTime.Parse(row["date_added"].ToString()).ToUniversalTime().ToString("dd/MM/yyyy-HH:mm:ss"); // DateTime.ParseExact(row["date_added"].ToString(), "yyyy-MM-dd HH:mm:ss", null, DateTimeStyles.AssumeLocal).ToUniversalTime().ToString("dd/MM/yyyy-HH:mm:ss");

                logs.Add(newLog);
            }

            return 0;
        }

        public static bool MarkNotificationAsRead(LocalUser user, uint notificationId)
        {
            try
            {
                using (SqlDatabaseClient client = SqlDatabaseManager.GetClient())
                {
                    string sql = "INSERT INTO EntryRead (member_id, entry_id) VALUES(@member_id, @entry_id);";
                    client.SetParameter("@member_id", user.UserID);
                    client.SetParameter("@entry_id", notificationId);

                    return client.ExecuteNonQuery(sql) == 1;
                }
            }
            catch (DbException e)
            {
                if (e.Message.Contains("EntryRead_Member_FKIndex1") || e.Message.Contains("EntryRead_Entry_FKIndex1"))
                    return false;
                /*else
                    Logger.WriteLine(e.ToString(), Logger.LOG_LEVEL.WARN);*/
            }

            return false;
        }

        public static int InsertNewLog(uint userId, string info)
        {
            int count = 0;
            try
            {
                using (SqlDatabaseClient client = SqlDatabaseManager.GetClient())
                {
                    string sql = "INSERT INTO Entry (member_id, info) VALUES(@member_id, @info);";
                    client.SetParameter("@member_id", userId);
                    client.SetParameter("@info", info);
                    count = client.ExecuteNonQuery(sql);
                }
            }
            catch (DbException e)
            {
                if (e.Message.Contains("Entry_FKIndex1"))
                    return -1;
                else
                    Logger.WriteLine(e.ToString(), Logger.LOG_LEVEL.WARN);
            }

            return count;
        }
    }
}
