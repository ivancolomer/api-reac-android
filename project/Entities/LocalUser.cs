﻿using REAC_AndroidAPI.Utils.Output;
using REAC_AndroidAPI.Utils.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace REAC_AndroidAPI.Entities
{
    public class LocalUser : User
    {
        private const string URL_TO_IMAGE = "/api/image/";
        private const string URL_TO_PROFILE_IMAGE_BEFORE = "/api/user/";
        private const string URL_TO_PROFILE_IMAGE_AFTER = "/profile/image";

        //Stuff for the API in-memory User
        public string SessionID { get; set; }
        public string IPAddress { get; set; }
        public long TimeCreated { get; set; }

        public static LocalUser GetAdministratorFromDB(string userName, string password)
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

            //uint profilePhoto = UInt32.TryParse(userRow["profile_photo"].ToString(), out profilePhoto) ? profilePhoto : 0;

            LocalUser newUser = new LocalUser();
            newUser.UserID = userId;
            newUser.IsOwner = true;
            newUser.Role = userRow["role"].ToString();
            newUser.Name = userRow["name"].ToString();
            newUser.ProfilePhoto = URL_TO_PROFILE_IMAGE_BEFORE + userId.ToString() + URL_TO_PROFILE_IMAGE_AFTER;// + userRow["profile_photo_path"];
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

        public static int GetImagesByUserFromDB(uint userId, out List<String> images)
        {
            images = new List<String>();

            DataTable table = null;
            try
            {
                using (SqlDatabaseClient client = SqlDatabaseManager.GetClient())
                {
                    client.SetParameter("@user_id", userId);
                    table = client.ExecuteQueryTable("SELECT p.id " +
                        "FROM Photo AS p " +
                        "INNER JOIN Member AS m ON m.id = p.member_id " +
                        "WHERE m.id = @user_id;");

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
                images.Add(URL_TO_IMAGE + row["id"].ToString());
            }

            return 0;
        }

        public static int GetUserFromDB(uint userId, out User user)
        {
            DataRow row = null;
            user = null;

            try
            {
                using (SqlDatabaseClient client = SqlDatabaseManager.GetClient())
                {
                    client.SetParameter("@user_id", userId);
                    row = client.ExecuteQueryRow("SELECT m.id, m.name, m.role, a.id AS admin_id " +
                        "FROM Member AS m " +
                        "LEFT JOIN Administrator AS a ON m.id = a.member_id " +
                        "WHERE m.id = @user_id;");
                }
            }
            catch (Exception e)
            {
                Logger.WriteLine(e.ToString(), Logger.LOG_LEVEL.WARN);
            }

            if (row == null)
                return -1;

            //uint profilePhoto = UInt32.TryParse(row["profile_photo"].ToString(), out profilePhoto) ? profilePhoto : 0;
            uint adminID = UInt32.TryParse(row["admin_id"].ToString(), out adminID) ? adminID : 0;

            User newUser = new User();
            user.UserID = userId;
            user.Role = row["role"].ToString();
            user.IsOwner = adminID > 0;
            user.Name = row["name"].ToString();
            user.ProfilePhoto = URL_TO_PROFILE_IMAGE_BEFORE + userId.ToString() + URL_TO_PROFILE_IMAGE_AFTER;

            return 0;
        }

        public static int GetUsersFromDB(out List<User> users)
        {
            users = new List<User>();

            DataTable table = null;
            try
            {
                using (SqlDatabaseClient client = SqlDatabaseManager.GetClient())
                {
                    table = client.ExecuteQueryTable("SELECT m.id, m.name, m.role, a.id AS admin_id " +
                        "FROM Member AS m " +
                        "LEFT JOIN Administrator AS a ON m.id = a.member_id;");
                }
            }
            catch (Exception e)
            {
                Logger.WriteLine(e.ToString(), Logger.LOG_LEVEL.WARN);
            }

            if (table == null)
                return -1;

            foreach(DataRow row in table.Rows)
            {
                uint userId;
                if (!UInt32.TryParse(row["id"].ToString(), out userId))
                    continue;

                //uint profilePhoto = UInt32.TryParse(row["profile_photo"].ToString(), out profilePhoto) ? profilePhoto : 0;
                uint adminID = UInt32.TryParse(row["admin_id"].ToString(), out adminID) ? adminID : 0;

                User newUser = new User();
                newUser.UserID = userId;
                newUser.Role = row["role"].ToString();
                newUser.IsOwner = adminID > 0;
                newUser.Name = row["name"].ToString();
                newUser.ProfilePhoto = URL_TO_PROFILE_IMAGE_BEFORE + userId.ToString() + URL_TO_PROFILE_IMAGE_AFTER;

                users.Add(newUser);
            }

            return 0;
        }

        public static long InsertNewMemberToDB(User user, SqlDatabaseClient client = null)
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

                long lastInsertedId = 0;
                if (count > 0)
                    lastInsertedId = client.LastInsertedId();

                if (clientIsNull)
                    client.Dispose();

                return lastInsertedId;
            }
            catch (DbException e)
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

        public static long InsertNewAdministratorToDB(LocalUser user, byte[] password)
        {
            long lastInsertedMember = 0;
            try
            {
                using (SqlDatabaseClient client = SqlDatabaseManager.GetClient())
                {
                    string sql = "INSERT INTO Administrator (member_id, password_hash) VALUES(@member_id, @password_hash);";

                    lastInsertedMember = InsertNewMemberToDB(user, client);
                    if (lastInsertedMember > 0)
                    {
                        client.SetParameter("@member_id", lastInsertedMember);
                        client.SetParameter("@password_hash", password);

                        if (client.ExecuteNonQuery(sql) == 0)
                            return 0;
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

            return lastInsertedMember;
        }

        public static int InsertNewAdministratorToDBFromExistingUser(LocalUser user, byte[] password)
        {
            int count = 0;
            try
            {
                using (SqlDatabaseClient client = SqlDatabaseManager.GetClient())
                {
                    string sql = "INSERT INTO Administrator (member_id, password_hash) VALUES(@member_id, @password_hash);";
                    client.SetParameter("@member_id", user.UserID);
                    client.SetParameter("@password_hash", password);
                    count = client.ExecuteNonQuery(sql);
                }
            }
            catch (DbException e)
            {
                if (e.Message.Contains("Administrator_FKIndex1"))
                    return -4;
                else if (e.Message.Contains("unique_member_administrator_ck"))
                    return -3;
                else
                    Logger.WriteLine(e.ToString(), Logger.LOG_LEVEL.WARN);
            }

            return count;
        }
    }
}
