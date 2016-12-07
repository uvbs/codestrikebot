﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.SQLite;
using System.Data.SqlServerCe;
using System.Data;

namespace CodeStrikeBot
{
    public class BotDatabase
    {
        public SqlCeConnection Connection { get; private set; }
        public SQLiteConnection Connection2 { get; private set; }

        public int[] ActiveEmulators;
        public string ScreenshotDir, MapDir;

        public BotDatabase()
        {
            SqlCeEngine en = new SqlCeEngine("DataSource=\"data.sdf\";Password='codestrikebot'");

            if (!File.Exists("data.sdf"))
            {
                en.CreateDatabase();
            }

            if (!File.Exists("data.db"))
            {
                SQLiteConnection.CreateFile("data.db");
            }

            Connection = GetNewConnection();
            Connection.Open();

            /*SqlCeCommand command = new SqlCeCommand("SELECT * FROM Names", Connection);
            SqlCeDataAdapter dataAdapter = new SqlCeDataAdapter(command);
            DataSet ds = new DataSet();
            dataAdapter.Fill(ds);*/

            Connection2 = GetNewConnection2();
            Connection2.Open();

            this.ActiveEmulators = new int[4];

            this.UpdateDatabase();

            this.LoadSettings();
        }

        public static string DateTimeSQLite(DateTime datetime)
        {
            return String.Format("{0}-{1}-{2} {3}:{4}:{5}.{6}", datetime.Year, datetime.Month, datetime.Day, datetime.Hour, datetime.Minute, datetime.Second, datetime.Millisecond);
        }

        private static SQLiteConnection GetNewConnection2()
        {
            return new SQLiteConnection("Data Source=data.db;Version=3;DateTimeFormat=CurrentCulture");
        }

        private static SqlCeConnection GetNewConnection()
        {
            return new SqlCeConnection("DataSource=\"data.sdf\";Password='codestrikebot'");
        }

        private void UpdateDatabase()
        {
            SqlCeCommand command = new SqlCeCommand("SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'settings'", Connection);
            SqlCeDataReader reader = command.ExecuteReader();

            if (!reader.Read()) //database is not built
            {
                command = new SqlCeCommand("CREATE TABLE settings (version INT NOT NULL, emulatorId1 INT, emulatorId2 INT, emulatorId3 INT, emulatorId4 INT, screenshotDir NVARCHAR(255), mapDir NVARCHAR(255))", Connection);
                command.ExecuteNonQuery();
                command = new SqlCeCommand("INSERT INTO settings (version) VALUES (0)", Connection);
                command.ExecuteNonQuery();
            }

            int version = 0;

            while (version != 1)
            {
                command = new SqlCeCommand("SELECT version FROM settings", Connection);
                reader = command.ExecuteReader();

                reader.Read();
                {
                    switch ((int)reader["version"])
                    {
                        case 0:
                            command = new SqlCeCommand("CREATE TABLE accounts (id INTEGER PRIMARY KEY IDENTITY, name NVARCHAR(20) NOT NULL, username NVARCHAR(30) NOT NULL, email NVARCHAR(60) NOT NULL, password NVARCHAR(30) NOT NULL, priority INT DEFAULT 0, foodNegativeAmt INT DEFAULT 0, lastLogin DATETIME DEFAULT 0, lastLogout DATETIME DEFAULT 0)", Connection);
                            command.ExecuteNonQuery();
                            command = new SqlCeCommand("CREATE TABLE log (id INTEGER PRIMARY KEY IDENTITY, type INT NOT NULL, description NVARCHAR(100), detail NVARCHAR(255), data IMAGE, timestamp DATETIME DEFAULT GETDATE() NOT NULL)", Connection);
                            command.ExecuteNonQuery();
                            command = new SqlCeCommand("CREATE TABLE schedules (id INTEGER PRIMARY KEY IDENTITY, accountId INT NOT NULL, type INT NOT NULL, interval INT NOT NULL, amount INT NOT NULL, count INT DEFAULT 1, x INT, y INT, lastAction DATETIME DEFAULT 0)", Connection);
                            command.ExecuteNonQuery();
                            command = new SqlCeCommand("CREATE UNIQUE INDEX NameIdx ON accounts (name)", Connection);
                            command.ExecuteNonQuery();
                            command = new SqlCeCommand("UPDATE settings SET version = 1, emulatorId1 = 0, emulatorId2 = 0, emulatorId3 = 0, emulatorId4 = 0, screenshotDir = 'output\\ss', mapDir = 'output\\map'", Connection);
                            command.ExecuteNonQuery();
                            command = new SqlCeCommand("CREATE TABLE emulators (id INTEGER PRIMARY KEY IDENTITY, type INT NOT NULL, command NVARCHAR(100), windowName NVARCHAR(60), lastKnownAccountId INT NOT NULL)", Connection);
                            command.ExecuteNonQuery();
                            break;
                    }
                }

                command.Dispose();
                reader.Dispose();

                version++;
            }

            SQLiteCommand command2 = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name='settings'", Connection2);
            //SQLiteCommand command = new SQLiteCommand("SELECT version FROM settings", Connection);
            SQLiteDataReader reader2 = command2.ExecuteReader();

            if (!reader2.Read()) //database is not built
            {
                command2 = new SQLiteCommand("CREATE TABLE settings (version INT NOT NULL, emulatorId1 INT, emulatorId2 INT, emulatorId3 INT, emulatorId4 INT, screenshotDir VARCHAR(255), mapDir VARCHAR(255))", Connection2);
                command2.ExecuteNonQuery();
                command2 = new SQLiteCommand("INSERT INTO settings (version) VALUES (0)", Connection2);
                command2.ExecuteNonQuery();
            }

            version = 0;

            while (version != 2)
            {
                command2 = new SQLiteCommand("SELECT version FROM settings", Connection2);
                reader2 = command2.ExecuteReader();

                reader2.Read();
                {
                    switch ((int)reader2["version"])
                    {
                        case 0:
                            command2 = new SQLiteCommand("CREATE TABLE accounts (id INTEGER PRIMARY KEY AUTOINCREMENT, name VARCHAR(20) NOT NULL, username VARCHAR(30) NOT NULL, email VARCHAR(60) NOT NULL, password VARCHAR(30) NOT NULL, priority INT DEFAULT 0, foodNegativeAmt INT DEFAULT 0, lastLogin DATETIME DEFAULT 0, lastLogout DATETIME DEFAULT 0)", Connection2);
                            command2.ExecuteNonQuery();
                            command2 = new SQLiteCommand("CREATE TABLE log (id INTEGER PRIMARY KEY AUTOINCREMENT, type INT NOT NULL, desc VARCHAR(100), detail VARCHAR(255), data BLOB, timestamp DATETIME DEFAULT CURRENT_TIMESTAMP NOT NULL)", Connection2);
                            command2.ExecuteNonQuery();
                            command2 = new SQLiteCommand("CREATE TABLE schedules (id INTEGER PRIMARY KEY AUTOINCREMENT, accountId INT NOT NULL, type INT NOT NULL, interval INT NOT NULL, amount INT NOT NULL, count INT DEFAULT 1, x INT, y INT, lastAction DATETIME)", Connection2);
                            command2.ExecuteNonQuery();
                            command2 = new SQLiteCommand("CREATE UNIQUE INDEX NameIdx ON accounts (name)", Connection2);
                            command2.ExecuteNonQuery();
                            command2 = new SQLiteCommand("UPDATE settings SET version = 1, emulatorId1 = 0, emulatorId2 = 0, emulatorId3 = 0, emulatorId4 = 0, screenshotDir = 'output\\ss', mapDir = 'output\\map'", Connection2);
                            command2.ExecuteNonQuery();
                            break;
                        case 1:
                            command2 = new SQLiteCommand("CREATE TABLE emulators (id INTEGER PRIMARY KEY AUTOINCREMENT, type INT NOT NULL, command VARCHAR(100), windowName VARCHAR(60), lastKnownAccountId INT NOT NULL)", Connection2);
                            command2.ExecuteNonQuery();
                            command2 = new SQLiteCommand("UPDATE settings SET version = 2", Connection2);
                            command2.ExecuteNonQuery();
                            break;
                    }
                }

                command2.Dispose();
                reader2.Dispose();

                version++;
            }
        }

        private void LoadSettings()
        {
            SQLiteCommand command = new SQLiteCommand("SELECT emulatorId1, emulatorId2, emulatorId3, emulatorId4, screenshotDir, mapDir FROM settings", Connection2);
            SQLiteDataReader reader = command.ExecuteReader();
            reader.Read();
            this.ActiveEmulators[0] = reader.GetInt32(0);
            this.ActiveEmulators[1] = reader.GetInt32(1);
            this.ActiveEmulators[2] = reader.GetInt32(2);
            this.ActiveEmulators[3] = reader.GetInt32(3);
            this.ScreenshotDir = reader.GetString(4);
            this.MapDir = reader.GetString(5);
            command.Dispose();
            reader.Dispose();
        }

        public void UpdateSettings(EmulatorInstance[] emulators, string ssPath, string mapPath)
        {
            if (emulators[0] != null)
            {
                this.ActiveEmulators[0] = emulators[0].Id;
            }
            else
            {
                this.ActiveEmulators[0] = 0;
            }
            if (emulators[1] != null)
            {
                this.ActiveEmulators[1] = emulators[1].Id;
            }
            else
            {
                this.ActiveEmulators[1] = 0;
            }
            if (emulators[2] != null)
            {
                this.ActiveEmulators[2] = emulators[2].Id;
            }
            else
            {
                this.ActiveEmulators[2] = 0;
            }
            if (emulators[3] != null)
            {
                this.ActiveEmulators[3] = emulators[3].Id;
            }
            else
            {
                this.ActiveEmulators[3] = 0;
            }
            this.ScreenshotDir = ssPath;
            this.MapDir = mapPath;

            SQLiteCommand command = new SQLiteCommand(String.Format("UPDATE settings SET emulatorId1 = {0}, emulatorId2 = {1}, emulatorId3 = {2}, emulatorId4 = {3}, screenshotDir = '{4}', mapDir = '{5}'", ActiveEmulators[0], ActiveEmulators[1], ActiveEmulators[2], ActiveEmulators[3], ScreenshotDir, MapDir), Connection2);
            command.ExecuteNonQuery();
            command.Dispose();
        }

        public static DataObject SaveObject(DataObject obj)
        {
            SQLiteConnection con = GetNewConnection2();
            con.Open();

            SQLiteCommand command = null;

            try
            {
                if (obj is Account)
                {
                    Account account = (Account)obj;

                    if (account.Id == 0)
                    {
                        command = new SQLiteCommand("INSERT INTO accounts (name, username, email, password, priority, foodNegativeAmt, lastLogin, lastLogout) VALUES (@name, @username, @email, @password, @priority, @foodNegAmt, @lastLogin, @lastLogout)", con);
                        command.Parameters.AddWithValue("@lastLogin", BotDatabase.DateTimeSQLite(new DateTime()));
                        command.Parameters.AddWithValue("@lastLogout", BotDatabase.DateTimeSQLite(new DateTime()));
                    }
                    else
                    {
                        command = new SQLiteCommand("UPDATE accounts SET name = @name, username = @username, email = @email, password = @password, priority = @priority, foodNegativeAmt = @foodNegAmt, lastLogin = @lastLogin, lastLogout = @lastLogout WHERE id = @id", con);
                        command.Parameters.AddWithValue("@lastLogin", BotDatabase.DateTimeSQLite(account.LastLogin));
                        command.Parameters.AddWithValue("@lastLogout", BotDatabase.DateTimeSQLite(account.LastLogout));
                        command.Parameters.AddWithValue("@id", account.Id);
                    }

                    command.Parameters.AddWithValue("@name", account.Name);
                    command.Parameters.AddWithValue("@username", account.UserName);
                    command.Parameters.AddWithValue("@email", account.Email);
                    command.Parameters.AddWithValue("@password", account.Password);
                    command.Parameters.AddWithValue("@priority", (int)account.Priority);
                    command.Parameters.AddWithValue("@foodNegAmt", account.FoodNegativeAmount);
                    command.ExecuteNonQuery();

                    if (account.Id == 0)
                    {
                        command = new SQLiteCommand("SELECT last_insert_rowid()", con);
                        account.Id = Convert.ToInt32(command.ExecuteScalar());
                    }

                    obj = account;
                }
                else if (obj is ScheduleTask)
                {
                    ScheduleTask task = (ScheduleTask)obj;

                    if (task.Id == 0)
                    {
                        command = new SQLiteCommand("INSERT INTO schedules (accountId, type, interval, amount, count, x, y, lastAction) VALUES (@accountId, @type, @interval, @amt, @count, @x, @y, @lastAction)", con);
                        command.Parameters.AddWithValue("@lastAction", BotDatabase.DateTimeSQLite(new DateTime()));
                    }
                    else
                    {
                        command = new SQLiteCommand("UPDATE schedules SET accountId = @accountId, type = @type, interval = @interval, amount = @amt, count = @count, x = @x, y = @y, lastAction = @lastAction WHERE id = @id", con);
                        command.Parameters.AddWithValue("@lastAction", BotDatabase.DateTimeSQLite(task.LastAction));
                        command.Parameters.AddWithValue("@id", task.Id);
                    }

                    command.Parameters.AddWithValue("@accountId", task.Account.Id);
                    command.Parameters.AddWithValue("@type", (int)task.Type);
                    command.Parameters.AddWithValue("@interval", task.Interval);
                    command.Parameters.AddWithValue("@amt", task.Amount);
                    command.Parameters.AddWithValue("@count", task.Count);
                    command.Parameters.AddWithValue("@x", task.X);
                    command.Parameters.AddWithValue("@y", task.Y);
                    command.ExecuteNonQuery();

                    if (task.Id == 0)
                    {
                        command = new SQLiteCommand("SELECT last_insert_rowid()", con);
                        task.Id = Convert.ToInt32(command.ExecuteScalar());
                    }

                    obj = task;
                }
                else if (obj is EmulatorInstance)
                {
                    EmulatorInstance emulator = (EmulatorInstance)obj;

                    if (emulator == null)
                    {
                        emulator = emulator;
                    }
                    
                    if (emulator.Id == 0)
                    {
                        command = new SQLiteCommand("INSERT INTO emulators (type, windowName, command, lastKnownAccountId) VALUES (@type, @windowName, @command, @accountId)", con);
                    }
                    else
                    {
                        command = new SQLiteCommand("UPDATE emulators SET type = @type, windowName = @windowName, command = @command, lastKnownAccountId = @accountId WHERE id = @id", con);
                        command.Parameters.AddWithValue("@id", emulator.Id);
                    }

                    command.Parameters.AddWithValue("@type", (int)emulator.Type);
                    command.Parameters.AddWithValue("@windowName", emulator.WindowName);
                    command.Parameters.AddWithValue("@command", emulator.Command);
                    command.Parameters.AddWithValue("@accountId", emulator.LastKnownAccount == null ? 0 : emulator.LastKnownAccount.Id);
                    command.ExecuteNonQuery();

                    if (emulator.Id == 0)
                    {
                        command = new SQLiteCommand("SELECT last_insert_rowid()", con);
                        emulator.Id = Convert.ToInt32(command.ExecuteScalar());
                    }

                    obj = emulator;
                }
            }
            catch (SQLiteException e)
            {
                if ((SQLiteErrorCode)e.ErrorCode == SQLiteErrorCode.Constraint_Check)
                {
                    //name already in use
                }
            }
            finally
            {
                command.Dispose();
            }

            con.Close();

            return obj;
        }

        public static void DeleteObject(DataObject obj)
        {
            if (obj.Id > 0)
            {
                SQLiteConnection con = GetNewConnection2();
                con.Open();

                SQLiteCommand command = null;

                try
                {
                    if (obj is Account)
                    {
                        Account account = (Account)obj;

                        command = new SQLiteCommand("DELETE FROM accounts WHERE id = @id", con);
                        command.Parameters.AddWithValue("@id", account.Id);
                        command.ExecuteNonQuery();
                    }
                    else if (obj is ScheduleTask)
                    {
                        ScheduleTask task = (ScheduleTask)obj;

                        command = new SQLiteCommand("DELETE FROM schedules WHERE id = @id", con);
                        command.Parameters.AddWithValue("@id", task.Id);
                        command.ExecuteNonQuery();
                    }
                    else if (obj is EmulatorInstance)
                    {
                        EmulatorInstance emulator = (EmulatorInstance)obj;

                        command = new SQLiteCommand("DELETE FROM emulators WHERE id = @id", con);
                        command.Parameters.AddWithValue("@id", emulator.Id);
                        command.ExecuteNonQuery();
                    }
                }
                catch (SQLiteException e)
                {

                }
                
                command.Dispose();
                con.Close();
            }
        }

        public static void InsertLog(int type, string desc, string detail, byte[] data)
        {
            SQLiteConnection con = GetNewConnection2();
            con.Open();

            using (SQLiteCommand command = new SQLiteCommand("INSERT INTO log (type, desc, detail, data) VALUES (@type, @desc, @detail, @data)", con))
            {
                try
                {
                    command.Parameters.AddWithValue("@type", type);
                    command.Parameters.AddWithValue("@desc", desc);
                    command.Parameters.AddWithValue("@detail", detail);
                    command.Parameters.AddWithValue("@data", data);
                    command.ExecuteNonQuery();
                }
                catch (SQLiteException e)
                {
                    e = e;
                }
            }

            con.Close();
        }

        public static List<DataObject> GetObjects<T>()
        {
            List<DataObject> list = new List<DataObject>();

            SQLiteConnection con = GetNewConnection2();
            con.Open();

            SQLiteCommand command = null;
            SQLiteDataReader reader = null;

            try
            {
                if (typeof(T) == typeof(ScheduleTask))
                {
                    command = new SQLiteCommand("SELECT id, accountId, type, interval, amount, count, x, y, lastAction FROM schedules", con);
                    reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        ScheduleTask task = new ScheduleTask(reader.GetInt32(0), new Account(reader.GetInt32(1)), (ScheduleType)reader.GetInt32(2), reader.GetInt32(3), reader.GetInt32(4), reader.GetInt32(5), reader.GetInt32(6), reader.GetInt32(7), reader.GetDateTime(8));
                        list.Add(task);
                    }
                }
                else if (typeof(T) == typeof(Account))
                {
                    command = new SQLiteCommand("SELECT id, name, username, email, password, priority, foodNegativeAmt, lastLogin, lastLogout FROM accounts", con);
                    reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Account account = new Account(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), (AccountPriority)reader.GetInt32(5), reader.GetInt32(6), reader.GetDateTime(7), reader.GetDateTime(8));
                        list.Add(account);
                    }
                }
                else if (typeof(T) == typeof(EmulatorInstance))
                {
                    command = new SQLiteCommand("SELECT id, type, windowName, command, lastKnownAccountId FROM emulators", con);
                    reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        EmulatorInstance emulator = new EmulatorInstance(reader.GetInt32(0), (EmulatorType)reader.GetInt32(1), reader.GetString(2), reader.GetString(3), new Account(reader.GetInt32(4)));
                        list.Add(emulator);
                    }
                }
            }
            catch (SQLiteException e)
            {
                if ((SQLiteErrorCode)e.ErrorCode == SQLiteErrorCode.Constraint_Check)
                {
                    //name already in use
                }
            }
            catch (Exception e)
            {
                e = e;
            }
            finally
            {
                command.Dispose();

                if (reader != null)
                {
                    reader.Dispose();
                }
            }

            con.Close();

            return list;
        }

        /*public AccountInfo LoadAccounts(int id)
        {
            AccountInfo account;

            SQLiteCommand command = new SQLiteCommand(String.Format("SELECT id, name, username, email, password, priority, foodNegativeAmt, lastLogin, lastLogout FROM accounts WHERE id = {0}", id), Connection);
            SQLiteDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                account = new AccountInfo(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), (ushort)reader.GetInt32(5), reader.GetInt32(6), reader.GetDateTime(7), reader.GetDateTime(8)));
            }

            return accounts;
        }*/

        public List<Account> GetAccounts()
        {
            List<Account> accounts = new List<Account>();

            SQLiteCommand command = new SQLiteCommand("SELECT id, name, username, email, password, priority, foodNegativeAmt, lastLogin, lastLogout FROM accounts", Connection2);
            SQLiteDataReader reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                accounts.Add(new Account(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), (AccountPriority)reader.GetInt32(5), reader.GetInt32(6), reader.GetDateTime(7), reader.GetDateTime(8)));
            }

            return accounts;
        }

        public void InsertLog(int logType, byte[] data)
        {
            SQLiteCommand command = new SQLiteCommand("INSERT INTO log (type, data) VALUES (?, ?)", Connection2);
            command.Parameters.Add(logType);
            command.Parameters.Add(String.Format("X'{0}'", BitConverter.ToString(data).Replace("-", "")));
            command.ExecuteNonQuery();

            command.Dispose();
        }
    }
}
