using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using Umpo.Common.IniFileLib;

namespace ConsoleApplication1
{
    class Program
    {
        static Queue q = Queue.Synchronized(new Queue());
        static Queue q2 = Queue.Synchronized(new Queue());
        static bool endFindFlag = false;
        static bool endHashFlag = false;
        

        class GetFilesThread
        {
            Thread thread;

            public GetFilesThread(string name, string StartDir) //constructor
            {

                thread = new Thread(this.func);
                thread.Name = name;
                thread.Start(StartDir);
            }

            void func(object StartDir)//worker
            {
                DirectoryInfo dir = new DirectoryInfo((string)StartDir);

                //files in start dir
                foreach (var fitem in dir.GetFiles())
                {
                    q.Enqueue(fitem.FullName);
                }

                //files in subdirs
                foreach (var item in dir.GetDirectories("*.*", System.IO.SearchOption.AllDirectories))
                {
                    foreach (var fitem in item.GetFiles())
                    {
                        q.Enqueue(fitem.FullName);
                    }
                }
                endFindFlag = true;
            }
        }

        /*
        class PrintFilesThread
        {
            Thread thread;

            public PrintFilesThread(string name) //constructor
            {

                thread = new Thread(this.func);
                thread.Name = name;
                thread.Start();
            }

            void func()//worker
            {
                string FileHash;
                do
                {
                    if (q2.Count == 0) { Thread.Sleep(1); }
                    else
                    {
                        FileHash = q2.Dequeue().ToString();
                        Console.WriteLine(FileHash);
                    }
                }
                while (!(endHashFlag == true && q2.Count == 0));
            }
        }
        */

        class HashFilesThread
        {
            Thread thread;

            public HashFilesThread(string name) //constructor
            {

                thread = new Thread(this.func);
                thread.Name = name;
                thread.Start();
            }

            void func()//worker
            {
                string FileName;
                do
                {
                    if (q.Count == 0) { Thread.Sleep(1); }
                    else
                    {
                        FileItem HashFile = new FileItem();
                        FileName = q.Dequeue().ToString();
                        HashFile.Hash = CalculateMD5(FileName);
                        HashFile.Name = FileName;
                        q2.Enqueue(HashFile);
                    }
                }
                while (!(endFindFlag == true && q.Count == 0));
                endHashFlag = true;
            }


            static string CalculateMD5(string filename)
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(filename))
                    {
                        var hash = md5.ComputeHash(stream);
                        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    }
                }
            }
        }

        class DatabaseHashesThread
        {
            Thread thread;

            public DatabaseHashesThread(string name, string ConnectionString) //constructor
            {
                thread = new Thread(this.func);
                thread.Name = name;
                thread.Start(ConnectionString);
            }

            void func(object ConnectionString)//work function
            {
                FileItem HashFile;
                string cmdText;

                SqlConnection sqlConnection = new SqlConnection();
                sqlConnection = new SqlConnection((string)ConnectionString);
                try
                {
                    sqlConnection.Open();
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return;
                }

                do
                {
                    if (q2.Count == 0) { Thread.Sleep(1); }
                    else
                    {
                        HashFile = (FileItem)q2.Dequeue();
                        Console.WriteLine("{0} - {1}", HashFile.Name, HashFile.Hash);
                        cmdText = "INSERT INTO [dbo].[files] ([filename],[hash]) VALUES(@Name, @Hash)";
                        var cmd = new SqlCommand(cmdText, sqlConnection);
                        var pName = new SqlParameter("@Name", HashFile.Name);
                        var pHash = new SqlParameter("@Hash", HashFile.Hash);

                        cmd.Parameters.AddRange(new[] {pName, pHash});

                        cmd.ExecuteNonQuery();
                    }
                }
                while (!(endHashFlag == true && q2.Count == 0));

                sqlConnection.Close();
                sqlConnection.Dispose();
                Console.WriteLine("Press Enter to exit");
            }
        }

        static void Main(string[] args)
        {
            string host = "";
            string instance = "";
            string database = "";
            string startDir = "";

            //load settings
            IniFiles f = new IniFiles("test.ini");
            f.Load();

            var sec = f.GetSection("database");
            if (sec.Keys != null)
            {
                foreach (var key in sec.Keys)
                {
                    if (key.Name == "host")
                    {
                        host = key.Value;
                        Console.WriteLine("host: {0}", host);
                    }
                    if (key.Name == "instance")
                    {
                        instance = key.Value;
                        Console.WriteLine("instance: {0}", instance);
                    }
                    if (key.Name == "database")
                    {
                        database = key.Value;
                        Console.WriteLine("database: {0}", database);
                    }
                }
            }

            sec = f.GetSection("directory");
            if (sec.Keys != null)
            {
                foreach (var key in sec.Keys)
                {
                    if (key.Name == "startdir")
                    {
                        startDir = key.Value;
                        Console.WriteLine("start directory: {0}", startDir);
                    }
                }
            }



            String arg0;

            if (args.Length != 0){
                arg0 = args[0];
                Console.WriteLine("arg0: {0}", arg0);

                if (arg0 == "-c") {

                    //create database
                    String str;
                    SqlConnection sqlConnection = new SqlConnection("Data Source = " + host + "\\" + instance + "; Integrated Security = SSPI; Initial Catalog = master; ");


                    str = "CREATE DATABASE " + database + " ON PRIMARY " +
                        "(NAME = " + database + "_Data, " +
                        "FILENAME = 'C:\\Temp\\" + database + "Data.mdf', " +
                        "SIZE = 5MB, MAXSIZE = 100MB, FILEGROWTH = 10%) " +
                        "LOG ON (NAME = " + database + "_Log, " +
                        "FILENAME = 'C:\\Temp\\" + database + "Log.ldf', " +
                        "SIZE = 2MB, " +
                        "MAXSIZE = 5MB, " +
                        "FILEGROWTH = 10%)";

                    SqlCommand myCommand = new SqlCommand(str, sqlConnection);
                    try
                    {
                        sqlConnection.Open();
                        myCommand.ExecuteNonQuery();
                        Console.WriteLine("DataBase is Created Successfully: {0}", database);
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    finally
                    {
                        if (sqlConnection.State == ConnectionState.Open)
                        {
                            sqlConnection.Close();
                        }
                    }


                    //create table
                    SqlConnection sqlConnection2 = new SqlConnection("Data Source = " + host + "\\" + instance + "; Integrated Security = SSPI; Initial Catalog = " + database + "; ");
                    str = @"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[files]') AND type in (N'U'))
                            BEGIN
                                CREATE TABLE files (
                                          Id integer IDENTITY(1,1) PRIMARY KEY NOT NULL,
                                          filename varchar(512) NOT NULL,
                                          hash varchar(200) NOT NULL,
                                );
                            END";

                    myCommand = new SqlCommand(str, sqlConnection2);
                    try
                    {
                        sqlConnection2.Open();
                        myCommand.ExecuteNonQuery();
                        Console.WriteLine("Tabe is Created Successfully: {0}", "files");
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    finally
                    {
                        if (sqlConnection2.State == ConnectionState.Open)
                        {
                            sqlConnection2.Close();
                        }
                    }
                    return;
                }
            }


            GetFilesThread t1 = new GetFilesThread("GetFilesThread", startDir);  //thread 1 collect filenames
            HashFilesThread t2 = new HashFilesThread("HashFilesThread");             //thread 2 hash files
            // PrintFilesThread t3 = new PrintFilesThread("PrintFilesThread");
            DatabaseHashesThread t3 = new DatabaseHashesThread("DatabaseHashesThread", "Data Source=" + host + "\\" + instance + "; Integrated Security=SSPI; Initial Catalog=" + database + ";"); //thread 3 write to base

            Console.Read();
        }

    }
}
