using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace accpagibigph3srv
{
    class Utilities
    {

        private static string encryptionKey = "@cCP@g1bIgPH3*";
        //public static DateTime SystemDate  = DateTime.Now;
        //public static DateTime SystemDate = Convert.ToDateTime("10/10/2019");
        public static DateTime SystemDate;
        public static DateTime ReportStartDate;
        public static DateTime ReportEndDate;
        public static string ConStr;

        //dcs aub start date
        //public static DateTime SystemDate = Convert.ToDateTime("07/11/2019");

        public static string EncryptData(string data)
        {
            AllcardEncryptDecrypt.EncryptDecrypt enc = new AllcardEncryptDecrypt.EncryptDecrypt(encryptionKey);
            string encryptedData = enc.TripleDesEncryptText(data);
            enc = null;            
            return encryptedData;
        }

        public static string DecryptData(string data)
        {
            AllcardEncryptDecrypt.EncryptDecrypt dec = new AllcardEncryptDecrypt.EncryptDecrypt(encryptionKey);
            string decryptedData = dec.TripleDesEncryptText(data);
            dec = null;
            return decryptedData;
        }

        public static string TimeStamp()
        {
            //return SystemDate.ToString("MM/dd/yyyy hh:mm:ss tt") + " ";
            return Convert.ToDateTime(SystemDate.ToShortDateString() + " " + DateTime.Now.TimeOfDay.ToString()).ToString("MM/dd/yyyy hh:mm:ss tt") + " ";            
        }        

        public static void InitLogFolder()
        {
            if (!System.IO.Directory.Exists("Logs"))
                System.IO.Directory.CreateDirectory("Logs");
            if (!System.IO.Directory.Exists(@"Logs\" + SystemDate.ToString("MMddyyyy")))
                System.IO.Directory.CreateDirectory(@"Logs\" + SystemDate.ToString("MMddyyyy"));
        }

        public static void SaveToErrorLog(string strData)
        {
            InitLogFolder();
            try
            {
                System.IO.StreamWriter sw = new System.IO.StreamWriter(@"Logs\" + SystemDate.ToString("MMddyyyy") + @"\Error.txt", true);
                sw.WriteLine(strData);
                sw.Dispose();
                sw.Close();
            }
            catch (Exception ex)
            {
            }
        }

        public static void SaveToSystemLog(string strData)
        {
            InitLogFolder();
            try
            {
                System.IO.StreamWriter sw = new System.IO.StreamWriter(@"Logs\" + SystemDate.ToString("MMddyyyy") + @"\System.txt", true);
                sw.WriteLine(strData);
                sw.Dispose();
                sw.Close();
            }
            catch (Exception ex)
            {
            }
        }

        public static void SaveToDoneIDs(string strData, DateTime dtmReportDate)
        {
            InitLogFolder();
            try
            {
                System.IO.StreamWriter sw = new System.IO.StreamWriter(DoneIDsFile(dtmReportDate), true);
                sw.Write(strData);
                sw.Dispose();
                sw.Close();
            }
            catch (Exception ex)
            {
            }
        }

        public static string DoneIDsFile(DateTime dtmReportDate)
        {
            //return @"Logs\" + SystemDate.ToString("MMddyyyy") + @"\doneIDs.txt";
            return @"Logs\" + dtmReportDate.ToString("MMddyyyy") + @"\doneIDs.txt";
        }

        public static string PreviousDoneIDsFile()
        {
            DateTime prevDate = SystemDate.AddDays(-1);
            return @"Logs\" + prevDate.ToString("MMddyyyy") + @"\doneIDs.txt";
        }

        public static string PendingIDsFile()
        {
            return @"Logs\" + SystemDate.ToString("MMddyyyy") + @"\pendingIDs.txt";
        }


    }
}
