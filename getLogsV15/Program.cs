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


        //private const int NumberOfRetries = 5;
        //private const int DelayOnRetry = 1000;

        public static void LogMessageToFile(string msg)

        {
            //System.IO.StreamWriter sw = System.IO.File.AppendText(GetTempPath() + "cvgetlog\\CVGetLogs.txt");
            System.IO.StreamWriter sw = new StreamWriter(GetTempPath() + "cvgetlog\\CVGetLogs.txt", true);

            //for (int i = 1; i <= NumberOfRetries; ++i)
            //{
            try
            {
                string logLine = System.String.Format("{0:G}: {1}.", System.DateTime.Now, msg);
                sw.WriteLine(logLine);
            }
            finally
            {

                sw.Close();
            }
            //}
        }




        static void Main(string[] args)
        {
            Console.SetWindowSize(85, 72);

            string path = System.Environment.GetEnvironmentVariable("localappdata");
            string cvgetlog = path + "\\cvgetlog";

            if (Directory.Exists(cvgetlog))
            {
                LogMessageToFile("INFO: Directory: " + cvgetlog + " Already Exists");

            }
            else
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

            Console.WriteLine("Customer: " + cmdArgs[4] + "\nTicket Number: " + cmdArgs[2] + "\nCCID: " + cmdArgs[3]);

            if (cmdArgs[5] == null)
            {
                Console.WriteLine("No job id found");
            }
            else
            {
                jobID = cmdArgs[5];
                Console.WriteLine("Would you like to continue using job id: " + jobID);
                //jobID = Console.ReadLine();
                Console.WriteLine("\n");
            }

            //Need to make this to the same directory as the executable.
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
                //Console.Read();
                //return;

                using (FileStream fs = File.Create(iniPath))
                {
                    Byte[] info = new UTF8Encoding(true).GetBytes("7zipPath = c:\\progra~1\\7-Zip\\7z.exe\nceLogPath =\\\\eng\\celogs\\\nlocalStagingDir = C:\\LogFiles\\\nextractDMP = false");


                    fs.Write(info, 0, info.Length);
                }


            }



            LogMessageToFile("####iniPath#### " + iniPath);
            //var dic = File.ReadAllLines("C:\\LogFiles\\inputfile.ini").Select(l => l.Split(new[] { '=' })).ToDictionary(s => s[0].Trim(), s => s[1].Trim());
            var dic = File.ReadAllLines(iniPath).Select(l => l.Split(new[] { '=' })).ToDictionary(s => s[0].Trim(), s => s[1].Trim());
            string zPath = dic["7zipPath"]; //OLD init - string zPath = "C:\\Program Files\\7-Zip\\7z.exe"; //7zip path.
            LogMessageToFile("INFO_CFG: 7Zip path: " + zPath);
            string ceLogs = dic["ceLogPath"]; //OLD init - string ceLogs = "\\\\eng\\celogs\\"; 
            LogMessageToFile("INFO_CFG: Customer log path: " + ceLogs);
            string stagingDir = dic["localStagingDir"]; //OLD init - string stagingDir = "F:\\-LogFiles-\\";
            LogMessageToFile("INFO_CFG: Local Staging directory: " + stagingDir);
            string extractDMP = dic["extractDMP"];
            LogMessageToFile("INFO_CFG: ExtractDMP: " + extractDMP);
            string ticketNumber = "\\" + cmdArgs[2]; //Not currenlty used other than to create folder structure.
            LogMessageToFile("INFO: ticketNumber: " + ticketNumber);
            string customerName = cmdArgs[4];
            LogMessageToFile("INFO: customerName: " + customerName);
            string CCID = cmdArgs[3]; //Customer commcell ID.
            LogMessageToFile("INFO: CCID: " + CCID);
            string fullLogPath = ceLogs + CCID; // Combines customer log path and commcell id to make valid path.
            LogMessageToFile("INFO: fullLogPath: " + fullLogPath);
            string extractTo = stagingDir + CCID + ticketNumber;
            LogMessageToFile("INFO: extractTo: " + extractTo);
            List<string> zipFileList = new List<string>(); //List for file selection
            List<string> zipFileList2 = new List<string>(); //List for file selection
            List<string> zipFileList3 = new List<string>(); //List for tar file selection
            List<string> zipFileList4 = new List<string>(); //List for tar file selection
            //List<string> FolderList = new List<string>(); //List for tar file selection
            string cmdPath = "C:\\Windows\\System32\\cmd.exe";
            LogMessageToFile("INFO: cmdPath: " + cmdPath);
            string localStagePath = GetTempPath() + "cvgetlog\\" + CCID;
            string fileList;// = "zipfilename";

            string parentZipFile;
            string parentZipFileExt;
            string parentFilePath;
            string excludeCSDB = "";
            string recheckShare = "y";

            int numlog = 0; //Used to display the number of logs in the folder.
            int numlog2 = 0; //Used to display the number of logs in the folder.
            int numfile3 = 0; //Used when listing zip file contents to increment counter.


            var fileExt = new[] { ".7z", ".gz", ".tar" };

            if (extractDMP == "false")
            {
                excludeCSDB = " -x!*.dmp ";
                LogMessageToFile("INFO: Extract DMP file? " + extractDMP);
            }


            if (File.Exists(zPath))
            {
                LogMessageToFile("INFO: File.Exists for 7zip: " + zPath + "is valid: Continue");
            }
            else
            {
                Console.WriteLine(File.Exists(zPath) ? "7zip exits...." : "\n\n\n####### 7zip does not exist in in the following path. #######\n \t\t{0}\n\nPlease check the input file and correct the path.\nPress enter to exit.", zPath);
                LogMessageToFile("ERROR: File.Exists failed for zPath" + zPath);
                LogMessageToFile("ERROR: ################# EXITING CVGETLOGS #################");
                Console.Read();
                return;
            }

            if (Directory.Exists(ceLogs))
            {
                LogMessageToFile("INFO: Directory.Exists for celogs: " + ceLogs + " is valid: Continue");
            }
            else
            {
                Console.WriteLine(Directory.Exists(ceLogs) ? "Customer log path exits...." : "\n\n\n####### Customer log path does not exist in in the following path. #######\n \t\t{0}\n\nPlease check the input file and correct the path.\nPress enter to exit.", ceLogs);
                LogMessageToFile("ERROR: Directory.Exists failed for ceLogs: " + ceLogs);
                LogMessageToFile("ERROR: ################# EXITING CVGETLOGS #################");
                Console.Read();
                return;
            }

            if (Directory.Exists(stagingDir))
            {
                LogMessageToFile("INFO: Directory.Exists for local stagingDir: " + stagingDir + "is valid: Continue");
            }
            else
            {
                Console.WriteLine("######################################################################\nCreating the following directory to store your log files\nIf you wish to change this path please update the inputfile.ini file LocalStagingDir\nValue: " + stagingDir + "\n######################################################################\n\n");
                DirectoryInfo di = Directory.CreateDirectory(stagingDir);
                LogMessageToFile("CREATED: Directory.Exists failed for stagingDir: Creating new: " + stagingDir);
                LogMessageToFile("ERROR: ################# EXITING CVGETLOGS #################");
                //Console.Read();
                //return;
            }

            if (Directory.Exists(fullLogPath))
            {
                LogMessageToFile("INFO: Directory.Exists for fullLogPath" + fullLogPath + " is valid: Continue");
            }
            else
            {
                Console.WriteLine(Directory.Exists(fullLogPath) ? "Customer commcell folder exits...." : "\n\n\n####### The customers commcell folder does not exist in in the following path. #######\n \t\t{0}\n\nPlease check the input file and correct the path.\nPress enter to exit.", fullLogPath);
                LogMessageToFile("ERROR: Directory.Exists failed for fullLogPath: " + fullLogPath);
                LogMessageToFile("ERROR: ################# EXITING CVGETLOGS #################");
                Console.Read();
                return;
            }
            recheck:
            if (Directory.GetFileSystemEntries(fullLogPath, "*.7z", SearchOption.AllDirectories).Length == 0)
            {
                Console.WriteLine("\nNo uploads were found for this customer.\n\nWould you like to re-check the share [y/n]...");
                recheckShare = Console.ReadLine();

                if (recheckShare == "y")
                {
                    LogMessageToFile("re-check log files selected");
                    goto recheck;
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

            if (Directory.Exists(localStagePath))
            {
                LogMessageToFile("INFO: localStagePath already exists" + localStagePath);
            }
            else
            {
                DirectoryInfo di = Directory.CreateDirectory(localStagePath);
                LogMessageToFile("INFO: localStagePath was created " + localStagePath);
            }

            if (Directory.Exists(extractTo))
            {
                LogMessageToFile("INFO: extracTo already exists" + extractTo);
            }
            else
            {
                DirectoryInfo di = Directory.CreateDirectory(extractTo);
                LogMessageToFile("INFO: extractTo was created " + extractTo);
            }


            Retry:

            Regex g = new Regex(@"\w+\.7z|.gz|.dmp|.zip"); //search 7z extracted cabs to find files inside.

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
                pro.Arguments = string.Format("/c {3} L \"{0}\" -r >{1}\\{2}.txt\"", dir, localStagePath, fileList + numlog2++, zPath);    // extracts the contents of the 7 zip file.
                LogMessageToFile("INFO: List zip contents: cmd.exe " + pro.Arguments);
                Process x = Process.Start(pro); //Added for extraction of zip file contents to extract server name per top level cab.
                x.WaitForExit();
                Console.WriteLine("File #: [{0}] \tFile Size: {2} MB\tCreated on: {3}\n {1}", numlog++, dir, s1, File.GetCreationTime(dir));


                using (StreamReader r = new StreamReader(localStagePath + "\\" + fileList + numfile3++ + ".txt"))
                {
                    string line;
                    while ((line = r.ReadLine()) != null)
                    {
                        Match m = g.Match(line);
                        if (m.Success)
                        {
                            if (line.Contains("celogs") == true)
                            {
                                //Console.WriteLine("not here");
                            }
                            //else if (line.Contains("Type = zip") == true)
                            //{
                            //numfile3 = numfile3 -1;
                            //Console.WriteLine("\tNo Machine Info found");
                            //}
                            else //need to convert to else if due to .zip files in source upload.
                            {
                                //LogMessageToFile("INFO: " + line);
                                Console.WriteLine("\tFile included: " + line.Remove(0, 53));
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
                Console.WriteLine("Choose File [#] [Enter] = 0, [97] = Recheck, [98] = Local dir, [99] = celogs dir:");
                //Console.WriteLine(numlog);
                numlog = Convert.ToInt32(Console.ReadLine());
                //Console.WriteLine(numlog);
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
                    //return;
                }

                Console.WriteLine("You Selected: " + numlog);
                Console.WriteLine("File Selected: " + zipFileList[numlog]);
                LogMessageToFile("INFO: Log file selected " + zipFileList[numlog]);

                parentZipFileExt = Path.GetFileName(zipFileList[numlog]);
                LogMessageToFile("INFO: Copy file from log share: " + zipFileList[numlog]);
                File.Copy(zipFileList[numlog], Path.Combine(extractTo, parentZipFileExt), true);

            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }

            finally { }

            try
            {
                //if (Directory.Exists(extractTo)) // Determine whether the directory exists.
                //{

                //parentZipFile = Path.GetFileNameWithoutExtension(zipFileList[numlog]);
                parentZipFileExt = Path.GetFileName(zipFileList[numlog]);
                string extractParentZip;
                extractParentZip = extractTo + "\\" + parentZipFileExt;
                Console.WriteLine("\nExtracting...\n" + extractTo + "\\" + parentZipFileExt);
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

                            Console.WriteLine("Still working on extraction for .tar.gz files\n{0}", subfile1);
                            zipFileList3.Add(subfile1);
                            LogMessageToFile("INFO: zipFileList3 item added to list" + zipFileList3);

                        }
                    }

                }
                else
                {
                    Console.WriteLine("Cab files is not done downloading: Returning to log selection menu. Try again!");
                    LogMessageToFile("INFO: Cab files is not done downloading: Returning to log selection menu. Try again!");
                    goto Retry;
                }


                Parallel.ForEach(zipFileList2, (currentFile) =>
                {
                    Console.WriteLine("\nExtracting...\n" + currentFile);
                    pro.Arguments = string.Format("x \"{0}\" -y -o\"{1}\"", currentFile, parentFilePath + "\\*");    // extracts the 7z file found in the foreach above.
                    Process z = Process.Start(pro);
                    //LogMessageToFile("INFO: Extracting parent zips: 7z.exe " + pro.Arguments);
                    z.WaitForExit();
                    //LogMessageToFile("DELETE: Filename to delete: " + currentFile);
                    File.Delete(currentFile);
                });

                pro.FileName = cmdPath;

                Parallel.ForEach(zipFileList3, (currentFile) =>
                {

                    string result;
                    result = Path.GetFileNameWithoutExtension(currentFile);
                    Console.WriteLine("\nExtracting...\n" + currentFile);
                    string sub7zipArg;
                    sub7zipArg = "x -si -ttar -o";
                    string top7zipArg;
                    top7zipArg = "/c c:\\progra~1\\7-Zip\\7z.exe x -so ";
                    pro.Arguments = string.Format("{4}\"{0}\" | \"{2}\" {3}\"{1}\"", currentFile, parentFilePath + "\\" + result + "\\", zPath, sub7zipArg, top7zipArg);
                    Process z = Process.Start(pro);
                    z.WaitForExit();
                    LogMessageToFile("INFO: cmd.exe " + pro.Arguments);

                    //7z x -so nc2plcvma01_logs.tar.gz | 7z x -si -ttar -onc2plcvma01
                });


                pro.FileName = zPath;

                string[] dirs = Directory.GetDirectories(parentFilePath, "*", SearchOption.TopDirectoryOnly);


                foreach (string dir in dirs)
                {
                    //LogMessageToFile("INFO: Working in TopLevelDir: " + dir);
                    foreach (string file in Directory.GetFileSystemEntries(dir, "*.7z"))
                    {

                        //LogMessageToFile("INFO: SubServerZipFile: " + file);
                        pro.Arguments = string.Format("x \"{0}\" -y -o\"{1}\"", file, dir + "\\*");    // extracts the 7z file found in the foreach above.
                        Process z = Process.Start(pro);
                        LogMessageToFile("INFO: Extracting child zips: 7z.exe " + pro.Arguments);
                        z.WaitForExit();
                        LogMessageToFile("DELETE: filename to delete: " + file);
                        File.Delete(file);

                    }



                }

                LogMessageToFile("INFO: Opening folder " + parentFilePath);
                Process.Start("explorer", parentFilePath);
                LogMessageToFile("INFO: ################# EXITING CVGETLOGS #################");

                return;

                //}
            }

            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());


            }
            finally { }

            //Console.WriteLine("Last");
            LogMessageToFile("INFO: Done working with" + customerName + ". Goodbye!");
            Console.Read();

        }
    }
}




//##### Change Log: 
//04/18/2018 - Added file size and date created for inital list of parentFilePath.
//04/19/2018 - Added folder open after logs are done extracting.
//04/19/2018 - Store values in txt/ini file.
//04/20/2018 - Additional folder/file checks using directory.exists and file.exists. (zPath,ceLogs,stagingDir,fullLogPath)
//04/20/2018 - Added logging to %localappdata%\CVGetLogs.txt
//04/20/2018 - Added sort of fullLogPath using .OrderByDescending(File.GetCreationTime)
//04/29/2018 - Logic to check if parentFilePath exists before continuing to list the folder contents.
//04/30/2018 - Added check for cab files using localStagePath
//04/30/2018 - created local staging folder to be used with list of cab contents per CCID
//05/01/2018 - Added list of 7z contents to console output using 7z.exe l to create a  txt list of items.
//05/01/2018 - Added logic to create local folder %localappdata%l\cvgetlog if it does not exist.
//05/02/2018 - Modified regex to include .dmp and .gz files "\w+\.7z|.gz|.dmp". Also modified filter path to filter lines with celogs instead of _.
//05/08/2018 - Corrected pro.args from "/c {3} L \"{0}\" -r >{1}\\{2}.txt\"" to  "/c \"{3}\" L \"{0}\" -r >{1}\\{2}.txt\"" to correct 7zip input file path.
//05/08/2018 - added exclude *.dmp from extraction.
//05/14/2018 - Added logic to create the extractTo folder bfore the sub zip file extraction, and removed the second routine that created the folder if it did not exist.
//05/14/2018 - Added Parallel.ForEach to extract each second level zip file.
//05/18/2018 - Added goto Retry; for # 97 when getting a list of initial log files for download.
//05/18/2018 - Added extraction of sub zip files such as systeminfo.7z.
//05/21/2018 - Added parent / child .7z cleanup. Example servername.7z, and systeminfo.7z
//05/21/2018 - Excluded SQL_ERROR_LOGS from being extracted due to error. Needs further review.
//05/22/2018 - Added inputfile.ini check to confirm it exists. Display location/file format when it does not.
//05/22/2018 - Added creation of inputfile.ini with default values.
//05/22/2018 - Added the creation of local staging path if it does not exist.
//05/31/2018 - Added re-check to log share using goto recheck;
//06/05/2018 - Added tar.gz support.


//##### Project list:
//FIX multiple access to log file.
//Add feature to populate inputfile.ini values.





//Problems


//1. Cab file is incomplete so it fails to find the path to extract to

//Extracting...
//F:\LogFiles\F96C9\180521-375\sendLogFiles_F96C9_2018_05_21_11_57_41_14510253.7z
//The process failed: System.IO.DirectoryNotFoundException: Could not find a part of the path 'F:\LogFiles\F96C9\180521-375\sendLogFiles_F96C9_2018_05_21_11_57_41_14510253'.
//   at System.IO.__Error.WinIOError(Int32 errorCode, String maybeFullPath)
//   at System.IO.FileSystemEnumerableIterator`1.CommonInit()
//   at System.IO.FileSystemEnumerableIterator`1..ctor(String path, String originalUserPath, String searchPattern, SearchOption searchOption, SearchResultHandler`1 resultHandler, Boolean checkHost)
//   at System.IO.Directory.GetFiles(String path)
//   at getLogsV14.Program.Main(String[] args) in F:\C#\getLogsV14\getLogsV14\getLogsV14\Program.cs:line 352



