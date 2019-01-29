//BB 05/2018
using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Data.SQLite;


namespace getLogsV15
{
    class Program
    {
        public static string GetTempPath()
        {
            string path = System.Environment.GetEnvironmentVariable("localappdata");
            if (!path.EndsWith("\\")) path += "\\";
            return path;
        }

        public static void LogMessageToFile(string msg)

        {
            System.IO.StreamWriter sw = new StreamWriter(GetTempPath() + "cvgetlog\\CVGetLogs.txt", true);

            try
            {
                string logLine = System.String.Format("{0:G}: {1}.", System.DateTime.Now, msg);
                sw.WriteLine(logLine);
            }
            finally
            {
                sw.Close();
            }
        }
        



            static void Main(string[] args)
        {
            ConsoleColor defaultForeground = Console.ForegroundColor;
            Console.SetWindowSize(85, 63); //Resize window
            //Console.SetWindowSize(1, 1);
            Console.SetBufferSize(85, 80);
            //Console.SetWindowSize(40, 20);
            //Console.SetWindowPosition(0, 0);
            Retry:
            string path = System.Environment.GetEnvironmentVariable("localappdata"); //Get folder %localappdata%
            string cvgetlog = path + "\\cvgetlog"; //path.combine("localappdata","cvgetlog")

            if (!Directory.Exists(cvgetlog))
            {
                Directory.CreateDirectory(cvgetlog);
                LogMessageToFile("INFO: CreateDirectory: " + cvgetlog);
            }

            LogMessageToFile("Info: Console.SetWindowSize(85,72)");

            LogMessageToFile("INFO: ################# STARTING CVGETLOGS #################");
            string inputArgs = "cvgetlogs";
            string jobID;

            if (args == null)
            {
                Console.WriteLine("No Customer info found");
                LogMessageToFile("ERROR: No Customer info found.");
            }
            else
            {
                for (int i = 0; i < args.Length; i++)
                {
                    string argument = args[i];
                    inputArgs = args[0];
                    LogMessageToFile("INFO:" + args[0]);
                }
            }


            string[] cmdArgs = inputArgs.Split('/');



            if (cmdArgs[5] == null)
            {
                Console.WriteLine("No job id found");
            }
            else
            {
                jobID = cmdArgs[5];
                //Console.WriteLine("Would you like to continue using job id: " + jobID);
                //jobID = Console.ReadLine();
                //Console.WriteLine("\n");
            }

            var iniPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory.ToString(), ".\\inputfile.ini");





            if (File.Exists(iniPath))
            {
                LogMessageToFile("INFO: File.Exists for: " + iniPath + "is valid: Continue");
            }
            else
            {
                Console.WriteLine("######################################################################\n.\\inputfile.ini does not exist. \nThis file should be in the same directory as getLogs.exe\n\n\nCREATING INPUTFILE.INI with the default values below.\n\nFILE FORMAT:\n7zipPath=c:\\progra~1\\7-Zip\\7z.exe\nceLogPath=\\\\eng\\celogs\\\nlocalStagingDir=C:\\LogFiles\\\nextractDMP=false\n######################################################################\n\n");
                LogMessageToFile("ERROR: File.Exists failed for iniPath " + iniPath);
                LogMessageToFile("ERROR: ################# EXITING CVGETLOGS #################");

                using (FileStream fs = File.Create(iniPath))
                {
                    Byte[] info = new UTF8Encoding(true).GetBytes("7zipPath = c:\\progra~1\\7-Zip\\7z.exe\nceLogPath =\\\\eng\\celogs\\\nlocalStagingDir = C:\\LogFiles\\\nextractDMP = false\nftpUrl =ftp://ccust01:NOTTHISPWD@qnftp01.commvault.com/incoming/");

                    fs.Write(info, 0, info.Length);
                }

            }




            LogMessageToFile("####iniPath#### " + iniPath);
            var dic = File.ReadAllLines(iniPath).Select(l => l.Split(new[] { '=' })).ToDictionary(s => s[0].Trim(), s => s[1].Trim());
            string zPath = dic["7zipPath"]; //OLD init - string zPath = "C:\\Program Files\\7-Zip\\7z.exe"; //7zip path.
            LogMessageToFile("INFO_CFG: 7Zip path: " + zPath);
            string ceLogs = dic["ceLogPath"]; //OLD init - string ceLogs = "\\\\eng\\celogs\\"; 
            LogMessageToFile("INFO_CFG: Customer log path: " + ceLogs);
            string stagingDir = dic["localStagingDir"]; //OLD init - string stagingDir = "F:\\-LogFiles-\\";
            LogMessageToFile("INFO_CFG: Local Staging directory: " + stagingDir);
            string extractDMP = dic["extractDMP"];
            LogMessageToFile("INFO_CFG: ExtractDMP: " + extractDMP);
            string ftpUrl = dic["ftpUrl"];
            string ticketNumber = "\\" + cmdArgs[2]; //Not currenlty used other than to create folder structure.
            LogMessageToFile("INFO: ticketNumber: " + ticketNumber);
            string customerName = cmdArgs[4];
            LogMessageToFile("INFO: customerName: " + customerName);
            string CCID = cmdArgs[3]; //Customer commcell ID.
            LogMessageToFile("INFO: CCID: " + CCID);
            string fullLogPath = ceLogs + CCID; // Combines customer log path and commcell id to make valid path.
            LogMessageToFile("INFO: fullLogPath: " + fullLogPath);
            string extractTo = stagingDir + customerName + ticketNumber;
            //string extractTo = stagingDir + customerName + "\\" + CCID + ticketNumber;
            LogMessageToFile("INFO: extractTo: " + extractTo);
            string engLogs = "\\\\eng\\escalationlogs";
            string stagePath = engLogs + "\\" + CCID + ticketNumber;
            List<string> zipFileList = new List<string>(); //List for file selecti;on
            List<string> zipFileList2 = new List<string>(); //List for .7z selection
            List<string> zipFileList3 = new List<string>(); //List for tar file selection
            List<string> zipFileList4 = new List<string>(); //List for tar file selection
            List<string> zipFileList5 = new List<string>(); //List for .zip file selection

            List<string> dirFolderList4 = new List<string>(); //List for tar file selection
            string cmdPath = "C:\\Windows\\System32\\cmd.exe";
            LogMessageToFile("INFO: cmdPath: " + cmdPath);
            string localStagePath = GetTempPath() + "cvgetlog\\" + CCID;
            string fileList;// = "zipfilename";
            string parentZipFile;
            string parentZipFileExt;
            string parentFilePath;
            string excludeCSDB = "";
            string recheckShare = "y";
            string recheckFolder = "y";
            //Console.WriteLine(stagePath);
            Console.WriteLine("Customer: " + cmdArgs[4] + "\nTicket Number: " + cmdArgs[2] + "\nCCID: " + cmdArgs[3]);

            int numlog = 0; //Used to display the number of logs in the folder.
            int numlog2 = 0; //Used to display the number of logs in the folder.
            int numlog22 = 0; //Used to display the number of logs in the folder.
            int numfile3 = 0; //Used when listing zip file contents to increment counter.
            int numFile4 = 0;
            int numFolder4 = 0;
            int NumberOfRetries = 3;
            int DelayOnRetry = 1000;

            var fileExt = new[] { ".7z", ".gz", ".tar" };

            var time = DateTime.Now;

            Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);

            string sqlLiteDBFile = ".\\sqlLiteDBFile.db";
            string path2 = Directory.GetCurrentDirectory();
            //Console.WriteLine(path2);
            if (!File.Exists(sqlLiteDBFile))
            {
                SQLiteConnection.CreateFile(sqlLiteDBFile);
                SQLiteConnection m_dbConnection;
                m_dbConnection = new SQLiteConnection("Data Source=.\\sqlLiteDBFile.db;Version=3;");
                m_dbConnection.Open();
                string sql = "CREATE TABLE Incident (Name VARCHAR(50), CCID VARCHAR(6), Ticket VARCHAR(12), FolderSelected VARCHAR(255), zFile VARCHAR(255), zFolderSelected VARCHAR(255), DateTime VARCHAR(30), Active VARCHAR(3), Deleted VARCHAR(3))";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                command.ExecuteNonQuery();
                LogMessageToFile("INFO: Database Created: " + path2 + "\\" + sqlLiteDBFile);
                LogMessageToFile("INFO: SQLite Query: " + sql);
                //Console.WriteLine(path2);
            }

            //Open SQLite DB - Start
            SQLiteConnection m_dbConnection2;
            LogMessageToFile("INFO: Database Opened: " + path2 + "\\" + sqlLiteDBFile);
            m_dbConnection2 = new SQLiteConnection("Data Source=.\\sqlLiteDBFile.db;Version=3;");
            m_dbConnection2.Open();
            //Open SQLite DB - Done


            if (extractDMP == "false")
            {
                excludeCSDB = " -x!*.dmp ";
                LogMessageToFile("INFO: Extract DMP file? " + extractDMP);
            }

            if (!Directory.Exists(extractTo))
            {
                DirectoryInfo di = Directory.CreateDirectory(extractTo);
                LogMessageToFile("INFO: extractTo was created " + extractTo);
            }


            if (!File.Exists(zPath))
            {
                Console.WriteLine("\n\n\n####### 7zip does not exist in in the following path. #######\n \t\t{0}\n\nPlease check the input file and correct the path.\nPress enter to exit.", zPath);
                LogMessageToFile("ERROR: File.Exists failed for zPath" + zPath);
                LogMessageToFile("ERROR: ################# EXITING CVGETLOGS #################");
                Console.Read();
                return;
            }

            if (!Directory.Exists(ceLogs))
            {
                Console.WriteLine("\n\n\n####### Customer log path does not exist in in the following path. #######\n \t\t{0}\n\nPlease check the input file and correct the path.\nPress enter to exit.", ceLogs);
                LogMessageToFile("ERROR: Directory.Exists failed for ceLogs: " + ceLogs);
                LogMessageToFile("ERROR: ################# EXITING CVGETLOGS #################");
                Console.Read();
                return;
            }

            if (!Directory.Exists(stagingDir))
            {
                Console.WriteLine("######################################################################\nCreating the following directory to store your log files\nIf you wish to change this path please update the inputfile.ini file LocalStagingDir\nValue: " + stagingDir + "\n######################################################################\n\n");
                DirectoryInfo di = Directory.CreateDirectory(stagingDir);
                LogMessageToFile("CREATED: Directory.Exists failed for stagingDir: Creating new: " + stagingDir);
                LogMessageToFile("ERROR: ################# EXITING CVGETLOGS #################");
            }

            recheckFullLogPath:

            if (!Directory.Exists(fullLogPath))
            {
                Console.WriteLine("Customer CCID folder {0} does not yet exist.\n\nWould you like to re-check for the folder [Y/N]\n\nIf you want to open qnftp01 enter [f]\n\nIf you would like to open {0} type [c].\n\nIf you would like to open {1} type [e].\n\nIf you would like to open {2} type [d].", fullLogPath,stagePath,extractTo);
                recheckFolder = Console.ReadLine();
                if (recheckFolder == "y")
                {
                    LogMessageToFile("re-check log files selected");
                    goto recheckFullLogPath;
                }
                else if (recheckFolder == "c")
                {
                    //Console.WriteLine("Opening ftp...");
                    //Windows.OpenExplorer("c:\test");
                    Process.Start("explorer", fullLogPath);
                    //Console.ReadLine();
                    return;
                }
                else if (recheckFolder == "d")
                {
                    //Console.WriteLine("Opening ftp...");
                    //Windows.OpenExplorer("c:\test");
                    Process.Start("explorer",extractTo);
                    //Console.ReadLine();
                    return;
                }
                else if (recheckFolder == "e")
                {
                    //Console.WriteLine("Opening ftp...");
                    //Windows.OpenExplorer("c:\test");
                    Process.Start("explorer", stagePath);
                    //Console.ReadLine();
                    return;
                }
                else if (recheckFolder == "f")
                {
                    Console.WriteLine("Opening ftp...");
                    //Windows.OpenExplorer("c:\test");
                    Process.Start("explorer", ftpUrl + CCID);
                    //Console.ReadLine();
                    return;


                }
                LogMessageToFile("ERROR: Directory.Exists failed for fullLogPath: " + fullLogPath);
                LogMessageToFile("ERROR: ################# EXITING CVGETLOGS #################");
                return;
            }


            recheck:
            if (Directory.GetFileSystemEntries(fullLogPath, "*.7z", SearchOption.AllDirectories).Length == 0)
            {
                Console.WriteLine("\nNo uploads were found for this customer.\n\nWould you like to re-check the share [y/n]\n\nIf you want to open qnftp01 enter [f]\n\nIf you would like to open {0} type [c]\n\nIf you would like to open {1} type [e]\n\nIf you would like to open {2} type [d]", fullLogPath, stagePath, extractTo);
                recheckShare = Console.ReadLine();
                LogMessageToFile("INFO: recheckShare " + recheckShare);
                if (recheckShare == "y")
                {
                    LogMessageToFile("re-check log files selected");
                    goto recheck;
                }
                else if (recheckShare == "f")
                {
                    Console.WriteLine("Opening ftp...");
                    //Windows.OpenExplorer("c:\test");
                    Process.Start("explorer", ftpUrl + CCID);
                    //Console.ReadLine();
                    return;
                }
                else if (recheckShare == "c")
                {
                    Process.Start("explorer", fullLogPath);
                    return;
                }

                else if (recheckShare == "d")
                {
                    Console.WriteLine("Opening ..." + extractTo);
                    Process.Start("explorer", extractTo);
                    return;
                }


                else if (recheckShare == "e")
                {
                    if (!Directory.Exists(stagePath))
                    {
                        Directory.CreateDirectory(stagePath);
                        LogMessageToFile("INFO: CreateDirectory: " + stagePath);
                        Process.Start("explorer", stagePath);
                        return;
                    }
                    else
                    {
                        Process.Start("explorer", stagePath);
                        return;
                    }

                }
                else
                {
                    LogMessageToFile("INFO: No uploads found in" + fullLogPath);
                    LogMessageToFile("ERROR: ################# EXITING CVGETLOGS #################");
                    //Console.Read();
                    return;
                }

            }
            else
            {
                LogMessageToFile("INFO: Uploads found in - " + fullLogPath + " Continuing");
            }

            if (!Directory.Exists(localStagePath))
            {
                DirectoryInfo di = Directory.CreateDirectory(localStagePath);
                LogMessageToFile("INFO: localStagePath was created " + localStagePath);
            }

            //if (!Directory.Exists(extractTo))
            //{
            //    DirectoryInfo di = Directory.CreateDirectory(extractTo);
            //    LogMessageToFile("INFO: extractTo was created " + extractTo);
            //}


            Regex g = new Regex(@"\w+\.7z|.gz|.dmp|.zip|.tar"); //search 7z extracted cabs to find files inside.

            ProcessStartInfo pro = new ProcessStartInfo();
            pro.WindowStyle = ProcessWindowStyle.Hidden;
            pro.FileName = cmdPath; //Added for extraction of zip file contents to extract server name per top level cab.


            foreach (string dir in Directory.GetFileSystemEntries(fullLogPath, "*.7z", SearchOption.AllDirectories).OrderByDescending(File.GetCreationTime))
            {
                LogMessageToFile("INFO: List zip contents for File: " + dir);
                FileInfo f = new FileInfo(dir);
                Decimal s1 = f.Length / 1024 / 1024;
                zipFileList.Add(dir); // Add each 7zip file to the list zipFileList
                fileList = Path.GetFileNameWithoutExtension(dir);
                string stageFile7z = localStagePath + "\\" + fileList + numlog22++ + ".txt";

                if (File.Exists(stageFile7z))
                {
                    LogMessageToFile("INFO: stageFile7z exists " + stageFile7z);
                    Console.ForegroundColor = ConsoleColor.Green;

                    Console.Write("File:[{0}]", numlog++);
                    //Console.ForegroundColor = defaultForeground;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("\tSize: [{0} MB]", s1);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("\tCreated on: {0}", File.GetCreationTime(dir));
                    Console.ForegroundColor = defaultForeground;
                    Console.WriteLine("\n {0}", dir);
                    //Console.WriteLine("File #: [{0}] \tFile Size: {2} MB\tCreated on: {3}\n {1}", numlog++, dir, s1, File.GetCreationTime(dir));

                }
                else
                {
                    LogMessageToFile("INFO: stageFile7z does not exist " + stageFile7z);
                    pro.Arguments = string.Format("/c {3} L \"{0}\" -r >{1}\\{2}.txt\"", dir, localStagePath, fileList + numlog2++, zPath);    // extracts the contents of the 7 zip file.
                    LogMessageToFile("INFO: List zip contents: cmd.exe " + pro.Arguments);
                    Process x = Process.Start(pro); //Added for extraction of zip file contents to extract server name per top level cab.
                    x.WaitForExit();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("File #: [{0}] \tFile Size: {2} MB\tCreated on: {3}\n {1}", numlog++, dir, s1, File.GetCreationTime(dir));
                    Console.ForegroundColor = defaultForeground;
                }

                using (StreamReader r = new StreamReader(localStagePath + "\\" + fileList + numfile3++ + ".txt"))
                {
                    string line;
                    while ((line = r.ReadLine()) != null)
                    {
                        Match m = g.Match(line);
                        if (m.Success)
                        {
                            if (line.Contains("celogs") == true) //This is used to skip regex search for files that contain celogs in the path.
                            {
                                //Console.WriteLine("not here");
                            }
                            //else if (line.Contains("Type = zip") == true) // Logic to collect .zip files also. Currently broken.
                            //{
                            //numfile3 = numfile3 -1;
                            //Console.WriteLine("\tNo Machine Info found");
                            //}
                            else //need to convert to else if due to .zip files in source upload.
                            {
                                //LogMessageToFile("INFO: " + line);
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine("\tFile: " + line.Remove(0, 53));
                                Console.ForegroundColor = defaultForeground;
                            }
                            
                        }

                        //else
                        //{
                        //    //Console.WriteLine(line);
                        //}
                    }
                }

                Console.WriteLine(" ");

            }

            pro.FileName = zPath;

            try
            {
                numlog = 0;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Choose:[#] | [96]=\\\\Eng | [97]=Recheck | [98]=Local dir | [99]=CELOGS | [100]=QNFTP01");
                Console.ForegroundColor = defaultForeground;
                numlog = Convert.ToInt32(Console.ReadLine());


            }
            catch (Exception e)
            {
                numlog = 0;
            }

            finally { }


            if (numlog == 99)
            {
                Console.WriteLine("Opening ..." + fullLogPath);
                Process.Start("explorer", fullLogPath);
                return;
            }
            else if (numlog == 98)
            {
                Console.WriteLine("Opening ..." + extractTo);
                Process.Start("explorer", extractTo);
                return;
            }
            else if (numlog == 97)
            {
                numlog = 0;
                goto Retry;
            }
            else if (numlog == 100)
            {
                Process.Start("explorer", ftpUrl + CCID);
                Console.WriteLine("Opening ..." + ftpUrl + CCID);
                return;
            }
            else if (numlog == 96)
            {

                if (!Directory.Exists(stagePath))
                {
                    Directory.CreateDirectory(stagePath);
                    LogMessageToFile("INFO: CreateDirectory: " + stagePath);
                    Process.Start("explorer", stagePath);
                    return;
                }
                else
                {
                    Process.Start("explorer", stagePath);
                    return;
                }

            }
            Console.WriteLine("You Selected: " + numlog);
            Console.WriteLine("File Selected: " + zipFileList[numlog]);
            var stopWatch = Stopwatch.StartNew();
            LogMessageToFile("INFO: Log file selected " + zipFileList[numlog]);

            parentZipFileExt = Path.GetFileName(zipFileList[numlog]);
            LogMessageToFile("INFO: Copy file from log share: " + zipFileList[numlog]);
            File.Copy(zipFileList[numlog], Path.Combine(extractTo, parentZipFileExt), true);

            try
            {
                parentZipFileExt = Path.GetFileName(zipFileList[numlog]);
                string extractParentZip;
                extractParentZip = extractTo + "\\" + parentZipFileExt;
                string extractPathWithoutExt;
                extractPathWithoutExt = extractTo + "\\" + Path.GetFileNameWithoutExtension(extractParentZip);
                Console.WriteLine("\nExtracting...\n" + extractTo + "\\" + parentZipFileExt);
                string statusActive = "YES";
                string statusDeleted = "NO";
                // Insert Into SQLite - Start
                //string sqlQuery = "insert into Incident (Name, CCID, Ticket, FolderSelected, DateTime, Active, Deleted) values('" + customerName + "'" + "," + "'" + CCID + "'" + "," + "'" + ticketNumber.TrimStart('\\') + "'" + "," + "'" + extractTo + "'" + "," + "'" + time + "'" + "," + "'" + statusActive + "'" + "," + "'" + statusDeleted + "')";
                string sqlQuery = "insert into Incident (Name, CCID, Ticket, FolderSelected, zFile, zFolderSelected, DateTime, Active, Deleted) values('" + customerName + "'" + "," + "'" + CCID + "'" + "," + "'" + ticketNumber.TrimStart('\\') + "'" + "," + "'" + extractTo + "'" + "," + "'" + extractParentZip + "'" + "," + "'" + extractPathWithoutExt + "'" + "," + "'" + time + "'" + "," + "'" + statusActive + "'" + "," + "'" + statusDeleted + "')";
                LogMessageToFile("INFO: SQLite Query: " + sqlQuery);
                //Console.Read();
                SQLiteCommand command2 = new SQLiteCommand(sqlQuery, m_dbConnection2);
                command2.ExecuteNonQuery();
                // Insert Into SQLite - End

                LogMessageToFile("INFO: extractTo" + extractTo);

                pro.Arguments = string.Format("x \"{0}\" -y -r {2}-o\"{1}\"", extractParentZip, extractTo + "\\*", excludeCSDB);    // extracts the 7z file found in the foreach above.

                parentZipFile = Path.GetFileNameWithoutExtension(zipFileList[numlog]);
                parentFilePath = extractTo + "\\" + parentZipFile;
                Process x = Process.Start(pro);
                LogMessageToFile("INFO: Extracting root zip: 7z.exe " + pro.Arguments);
                x.WaitForExit();

                if (Directory.Exists(parentFilePath))
                {
                    LogMessageToFile("INFO: File is complete continuing with subfile extraction:");

                    foreach (string subfile1 in Directory.GetFiles(parentFilePath).Where(file => fileExt.Any(file.ToLower().EndsWith)).ToList())
                    {

                        if (subfile1.Contains("SQL_ERROR_LOGS"))
                        {
                        }
                        else if (subfile1.Contains(".7z"))
                        {
                            zipFileList2.Add(subfile1); // Add each 7zip file to the list zipFileList
                            LogMessageToFile("INFO: zipFileList2 item added to list" + zipFileList2);
                        }
                        else if (subfile1.Contains(".gz"))
                        {

                            //Console.WriteLine("Still working on extraction for .tar.gz files\n{0}", subfile1);
                            zipFileList3.Add(subfile1);
                            LogMessageToFile("INFO: zipFileList3 item added to list" + zipFileList3);

                        }
                        else if (subfile1.Contains(".tar"))
                        {
                            zipFileList2.Add(subfile1);
                        }
                        //else if (subfile1.Contains(".zip"))
                        //{
                        //    zipFileList5.Add(subfile1);
                        //}

                    }

                }
                else
                {
                    Console.WriteLine("Cab files is not done downloading: Returning to log selection menu. Try again!");
                    LogMessageToFile("INFO: Cab files is not done downloading: Returning to log selection menu. Try again!");
                    goto Retry;
                }


                int zfilecount2 = zipFileList2.Count;
                Console.WriteLine("\nWorking on {0} parent .7z files", zfilecount2);


                Parallel.ForEach(zipFileList2, (currentFile) =>
                {
                    //Console.WriteLine("\nExtracting...\n" + currentFile);
                    pro.Arguments = string.Format("x \"{0}\" -y -o\"{1}\"", currentFile, parentFilePath + "\\*");    // extracts the 7z file found in the foreach above.
                    Process z = Process.Start(pro);
                    //LogMessageToFile("INFO: Extracting parent zips: 7z.exe " + pro.Arguments);
                    z.WaitForExit();
                    //LogMessageToFile("DELETE: Filename to delete: " + currentFile);

                    for (int i = 1; i <= NumberOfRetries; ++i)
                    {
                        try
                        {
                            File.Delete(currentFile);
                            break;
                        }
                        catch (IOException e) when (i <= NumberOfRetries)
                        {
                            Thread.Sleep(DelayOnRetry);
                        }
                    }
                });







                pro.FileName = cmdPath;
                int zfilecount3 = zipFileList3.Count;
                Console.WriteLine("Working on {0} parent .gz/tar files", zfilecount3);
                Parallel.ForEach(zipFileList3, (currentFile) =>
                {

                    string result;
                    result = Path.GetFileNameWithoutExtension(currentFile);
                    //Console.WriteLine("\nExtracting...\n" + currentFile);
                    string sub7zipArg;
                    sub7zipArg = "x -si -ttar -o";
                    string top7zipArg;
                    top7zipArg = "/c c:\\progra~1\\7-Zip\\7z.exe x -so ";
                    pro.Arguments = string.Format("{4}\"{0}\" | \"{2}\" {3}\"{1}\"", currentFile, parentFilePath + "\\" + result + "\\", zPath, sub7zipArg, top7zipArg);
                    Process z = Process.Start(pro);
                    //Console.WriteLine(pro.Arguments);
                    //Console.Read();
                    z.WaitForExit();

                    for (int i = 1; i <= NumberOfRetries; ++i)
                    {
                        try
                        {
                            File.Delete(currentFile);
                            break;
                        }
                        catch (IOException e) when (i <= NumberOfRetries)
                        {
                            Thread.Sleep(DelayOnRetry);
                        }
                    }

                    LogMessageToFile("DELETE " + currentFile);
                    LogMessageToFile("INFO: cmd.exe " + pro.Arguments);

                    //7z x -so nc2plcvma01_logs.tar.gz | 7z x -si -ttar -onc2plcvma01
                });


                pro.FileName = zPath;

                string[] dirs = Directory.GetDirectories(parentFilePath, "*", SearchOption.TopDirectoryOnly);


                foreach (string dir in dirs)
                {

                    //foreach (string file in Directory.GetFileSystemEntries(dir, "*.7z"))
                    foreach (string file in Directory.GetFileSystemEntries(dir))
                        {
                        if (file.Contains(".7z"))
                        {
                            zipFileList4.Add(file);
                            dirFolderList4.Add(dir);
                            LogMessageToFile("dirFolderList4 " + dirFolderList4[numFolder4++]);
                            LogMessageToFile("zipFileList4 " + zipFileList4[numFile4++]);
                        }
                        else if(file.Contains(".zip"))
                        {
                            zipFileList5.Add(file);
                        }


                    }
                }
                int zfilecount4 = zipFileList4.Count;
                Console.WriteLine("Working on {0} sub directory .7z files", zfilecount4);
                Parallel.ForEach(zipFileList4, (currentFile) =>
                {

                string outputFolder;
                outputFolder = Path.GetDirectoryName(currentFile);
                pro.Arguments = string.Format("x \"{0}\" -y -o\"{1}\"", currentFile, outputFolder + "\\*");    // extracts the 7z file found in the foreach above.
                Process z = Process.Start(pro);
                //Console.WriteLine("Extracting: " + currentFile);
                z.WaitForExit();

                    for (int i=1; i <= NumberOfRetries; ++i) 
                    {
                        try
                        {
                                File.Delete(currentFile);
                                break;
                        }
                        catch (IOException e) when(i <= NumberOfRetries)
                        {
                                Thread.Sleep(DelayOnRetry);
                        }
                    }


                });

                int zfilecount5 = zipFileList5.Count;
                Console.WriteLine("Working on {0} sub directory .zip files", zfilecount5);

                Parallel.ForEach(zipFileList5, (currentFile) =>
                {

                    string outputFolder;
                    outputFolder = Path.GetDirectoryName(currentFile);
                    pro.Arguments = string.Format("x \"{0}\" -y -o\"{1}\"", currentFile, outputFolder);    // extracts the 7z file found in the foreach above.
                    Process z = Process.Start(pro);
                    //Console.WriteLine(pro.Arguments);
                    //Console.WriteLine("Extracting: " + currentFile);
                    z.WaitForExit();

                    for (int i = 1; i <= NumberOfRetries; ++i)
                    {
                        try
                        {
                            File.Delete(currentFile);
                            break;
                        }
                        catch (IOException e) when (i <= NumberOfRetries)
                        {
                            Thread.Sleep(DelayOnRetry);
                        }
                    }


                });


                LogMessageToFile("INFO: Opening folder " + parentFilePath);
                Process.Start("explorer", parentFilePath);
                LogMessageToFile("Total run time: " + stopWatch.Elapsed.TotalSeconds);
                LogMessageToFile("INFO: ################# EXITING CVGETLOGS #################");
                //Console.Read();

                return;

            }

            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());


            }
            finally { }

            LogMessageToFile("Total run time: " + stopWatch.Elapsed.TotalSeconds);
            LogMessageToFile("INFO: Done working with" + customerName + ". Goodbye!");
            //Console.Read();

        }
    }
}



//##### Project list:
//Add support for .zip files.
//FIX multiple access to log file.


