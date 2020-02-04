using System.IO;

namespace getLogsV15.Methods
{
    class LogInfoToFile
    {

        public static string GetTempPath()
        {
            string path = System.Environment.GetEnvironmentVariable("localappdata");
            if (!path.EndsWith("\\")) path += "\\";
            return path;
        }

        //Method to write to a log file "LogMessageToFile("TEXT" + variable);"
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
    }
}
