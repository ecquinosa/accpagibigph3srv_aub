
using System;
using System.Data;
using System.Security.Cryptography;
using System.IO;

namespace accpagibigph3srv
{
    class Program
    {

        #region constructors

        private static string APP_NAME = "accpagibigph3srv";
        private static string WS_REPO = "";
        private static string BANK_REPO = "";
        private static int PROCESS_INTERVAL_SECONDS = 120;
        private static short SEND_TO_SFTP;

        private static string DB_SERVER = "";
        private static string DB_NAME = "";
        private static string DB_USER = "";
        private static string DB_PASS = "";

        private delegate void dlgtProcess(DateTime dtmReportDate);

        private static System.Threading.Thread _ubpthread;

        private static string configFile = AppDomain.CurrentDomain.BaseDirectory + "config";
        private static string refDateFile = AppDomain.CurrentDomain.BaseDirectory + "refDates";

        private static bool IsBankProcessReady = true;

        private static DataTable dtBankFiles;

        #endregion

        static void Main()
        {
            sbEmail.AppendLine(Utilities.TimeStamp() + "Application started");

            if (IsProgramRunning(APP_NAME) > 1) return;

            short intRetry = 1;

            while (!Init())
            {
                if (intRetry == 5)
                {
                    System.Threading.Thread.Sleep(PauseTime());
                    Environment.Exit(0);
                }
                System.Threading.Thread.Sleep(5000);
                intRetry += 1;
            }

            ////tempo 06/20

            //SFTP sftp2 = new SFTP();
            ////System.Text.StringBuilder sbDone = new System.Text.StringBuilder();
            //int _TotalSftpTransfer = 0;
            //string errMsg2 = "";

            ////send zip files                    
            //LogToSystemLog("Sending files to sftp...");
            //LogToSystemLog("SynchronizeDirectories started...");

            //if (!sftp2.SynchronizeDirectories(BANK_REPO, ref errMsg2, ref _TotalSftpTransfer))
            //{
            //    LogToErrorLog(Utilities.TimeStamp() + "RunBankProcess(): SynchronizeDirectories failed.Error " + errMsg2);
            //}

            //sftp2 = null;
            //LogToSystemLog("Total zipped file(s) uploaded: " + _TotalSftpTransfer.ToString("N0"));

            //return;
            ////temp 06/20

            if (IsRefDateExist())
            {
                string refDates = File.ReadAllText(refDateFile);
                if (refDates.Contains("-"))
                {
                    try
                    {
                        Utilities.ReportStartDate = Convert.ToDateTime(refDates.Split('-')[0]).Date;
                        Utilities.ReportEndDate = Convert.ToDateTime(refDates.Split('-')[1]).Date;
                        Utilities.SystemDate = Utilities.ReportStartDate;
                    }
                    catch (Exception ex)
                    {
                        LogToErrorLog("Ref dates are " + refDates + ". Runtime error catched " + ex.Message + ". Application will close in " + PauseTime().ToString() + " seconds.");
                        File.Delete(refDateFile);
                        System.Threading.Thread.Sleep(PauseTime());
                        Environment.Exit(0);
                    }
                }
                else
                {
                    LogToErrorLog("Ref dates are invalid " + refDates + ". Enter valid start date and end date. Application will close in " + PauseTime().ToString() + " seconds.");
                    File.Delete(refDateFile);
                    System.Threading.Thread.Sleep(PauseTime());
                    Environment.Exit(0);
                }
            }
            else Utilities.SystemDate = DateTime.Now.Date;

            StartThread();
        }

        private static bool Init()
        {
            try
            {
                LogToSystemLog("Checking config...");
                if (!File.Exists(configFile))
                {
                    LogToErrorLog("Init(): Config file is missing");
                    sbEmail.AppendLine(Utilities.TimeStamp() + "Init(): Config file is missing");
                    return false;
                }

                try
                {
                    using (StreamReader sr = new StreamReader(configFile))
                    {
                        while (!sr.EndOfStream)
                        {
                            string strLine = sr.ReadLine();
                            switch (strLine.Trim().Split('=')[0].ToUpper())
                            {
                                case "WS_REPO":
                                    WS_REPO = strLine.Trim().Split('=')[1];
                                    break;
                                case "BANK_REPO":
                                    BANK_REPO = strLine.Trim().Split('=')[1];
                                    break;
                                case "DB_SERVER":
                                    DB_SERVER = strLine.Trim().Split('=')[1];
                                    break;
                                case "DB_NAME":
                                    DB_NAME = strLine.Trim().Split('=')[1];
                                    break;
                                case "DB_USER":
                                    DB_USER = strLine.Trim().Split('=')[1];
                                    break;
                                case "DB_PASS":
                                    DB_PASS = strLine.Trim().Split('=')[1];
                                    break;
                                case "PROCESS_INTERVAL_SECONDS":
                                    PROCESS_INTERVAL_SECONDS = Convert.ToInt32(strLine.Trim().Split('=')[1]);
                                    break;
                                case "SEND_TO_SFTP":
                                    SEND_TO_SFTP = Convert.ToInt16(strLine.Trim().Split('=')[1]);
                                    break;
                            }
                        }

                        sr.Dispose();
                        sr.Close();
                    }

                    Utilities.ConStr = "Server=" + DB_SERVER + "; Database=" + DB_NAME + "; User=" + DB_USER + "; Password=" + DB_PASS + ";";
                }
                catch (Exception ex)
                {
                    LogToErrorLog("Init(): Error reading config file. Runtime catched error " + ex.Message);
                    sbEmail.AppendLine(Utilities.TimeStamp() + "Init(): Error reading config file. Runtime catched error " + ex.Message);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                LogToErrorLog("Init(): Runtime catched error " + ex.Message);
                return false;
            }
        }

        private static int IsProgramRunning(string Program)
        {
            System.Diagnostics.Process[] p;
            p = System.Diagnostics.Process.GetProcessesByName(Program.Replace(".exe", "").Replace(".EXE", ""));

            return p.Length;
        }

        private static void StartThread()
        {
            System.Threading.Thread objNewThread = new System.Threading.Thread(BankThread);
            objNewThread.Start();
            _ubpthread = objNewThread;
        }

        private static short intDateProcess = 1;

        private static void BankThread()
        {
            try
            {
                while (true)
                {
                    if (IsBankProcessReady)
                    {
                        IsBankProcessReady = false;
                        dlgtProcess _delegate = new dlgtProcess(RunBankProcess);


                        if (!IsRefDateExist())
                        {
                            DateTime prevDate = Utilities.SystemDate.AddDays(-1);
                            Console.WriteLine(Utilities.TimeStamp() + "Processing previous date " + prevDate.ToShortDateString() + "...");
                            _delegate.Invoke(prevDate);
                            System.Threading.Thread.Sleep(10000);
                        }

                        Console.WriteLine(Utilities.TimeStamp() + "Processing " + Utilities.SystemDate.ToShortDateString() + "...");
                        _delegate.Invoke(Utilities.SystemDate);
                        _delegate = null;

                        if (intDateProcess > 1)
                        {
                            if (IsRefDateExist())
                            {
                                intDateProcess = 1;
                                Utilities.SystemDate = Utilities.SystemDate.AddDays(1);
                            }
                            else
                            {
                                Console.WriteLine(Utilities.TimeStamp() + "Application will close in " + PauseTime().ToString() + " seconds.");
                                System.Threading.Thread.Sleep(PauseTime());
                                Environment.Exit(0);
                            }
                        }

                        if (IsRefDateExist())
                        {
                            if (Utilities.SystemDate.Date == Utilities.ReportEndDate.AddDays(1)) //Utilities.ReportEndDate.Date)
                            {
                                File.Delete(refDateFile);
                                Console.WriteLine(Utilities.TimeStamp() + "Application will close in " + PauseTime().ToString() + " seconds.");
                                System.Threading.Thread.Sleep(PauseTime());
                                Environment.Exit(0);
                            }
                        }

                        System.Threading.Thread.Sleep(PROCESS_INTERVAL_SECONDS * 1000); // 60000 = 1minute                        
                    }
                }
            }
            catch (Exception ex)
            {
                LogToErrorLog("ProgramThread(): Runtime catched error " + ex.Message);
            }
        }

        private static System.Text.StringBuilder sbEmail = new System.Text.StringBuilder();

        private static void RunBankProcess(DateTime dtmReportDate)
        {
            string startTime = Utilities.TimeStamp();
            LogToSystemLog("RunBankProcess started...");

            sbEmail.AppendLine(startTime + "Process started");

            DateTime dtmLast = dtmReportDate;
            string strLastProcessTime = dtmLast.ToString();
            string strNextProcessTime = dtmLast.AddSeconds(PROCESS_INTERVAL_SECONDS).ToString();
            int intCntr = 0;

            System.Text.StringBuilder sbForDeletion = new System.Text.StringBuilder();

            string doneFolder = string.Format("{0}\\DONE", BANK_REPO);
            string transferFolder = string.Format("{0}\\FOR_TRANSFER", BANK_REPO);

            //string doneTodayFolder = string.Format("{0}\\{1}", doneFolder, dtmLast.ToString("yyyy-MM-dd")) + "_2";
            //string transferTodayFolder = string.Format("{0}\\{1}", transferFolder, dtmLast.ToString("yyyy-MM-dd")) + "_2";
            string doneTodayFolder = string.Format("{0}\\{1}", doneFolder, dtmLast.ToString("yyyy-MM-dd"));
            string transferTodayFolder = string.Format("{0}\\{1}", transferFolder, dtmLast.ToString("yyyy-MM-dd"));

            if (!Directory.Exists(doneFolder)) Directory.CreateDirectory(doneFolder);
            if (!Directory.Exists(transferFolder)) Directory.CreateDirectory(transferFolder);
            if (!Directory.Exists(doneTodayFolder)) Directory.CreateDirectory(doneTodayFolder);
            if (!Directory.Exists(transferTodayFolder)) Directory.CreateDirectory(transferTodayFolder);

            LogToSystemLog("Extracting data from webservice...");
            sbEmail.AppendLine(startTime + "Data extraction from database with report date " + dtmLast.Date.ToShortDateString());
            if (GetDailyTxnAndPackData(dtmLast))
            {
                LogToSystemLog("Compressing folder/s...");
                foreach (string strFolder in Directory.GetDirectories(BANK_REPO))
                {
                    string acctNo = strFolder.Substring(strFolder.LastIndexOf("\\") + 1);

                    if (acctNo == "DONE") { }
                    else if (acctNo == "FOR_TRANSFER") { }
                    else if (acctNo == "DONE2") { }
                    //else if (acctNo == "FTP") { }
                    else if (acctNo.Contains("-")) { }
                    else
                    {
                        if (File.Exists(string.Format("{0}\\{1}.zip", BANK_REPO, acctNo)))
                        {
                            sbForDeletion.AppendLine(strFolder);
                        }
                        else
                        {
                            string zipFile = "";
                            if (Directory.GetFiles(strFolder).Length > 0)
                            {
                                if (!FileCompression.Compress(strFolder, string.Format("{0}\\{1}", transferTodayFolder, acctNo), ref zipFile)) LogToErrorLog("Failed compressing " + acctNo);
                                else
                                {
                                    LogToSystemLog(acctNo + " compressed successfully");
                                    intCntr += 1;
                                    sbForDeletion.AppendLine(strFolder);
                                }
                            }
                            else LogToSystemLog(string.Format("Folder {0} is empty", acctNo));
                        }
                    }
                }

                LogToSystemLog("Total compressed folder(s): " + intCntr.ToString("N0"));
                intCntr = 0;
                LogToSystemLog("Deleting folder/s already compressed...");
                foreach (string strFolder in sbForDeletion.ToString().Split('\r'))
                {
                    if (Directory.Exists(strFolder.Replace("\n", "")))
                    {
                        Directory.Delete(strFolder.Replace("\n", ""), true);
                        LogToSystemLog(strFolder.Replace("\n", "") + " deleted successfully");
                    }
                }
            }

            System.Text.StringBuilder sbReport = new System.Text.StringBuilder();

            string errMsg = "";
            SFTP sftp = null;

            if (SEND_TO_SFTP == 1)
            {
                sftp = new SFTP();
                //System.Text.StringBuilder sbDone = new System.Text.StringBuilder();
                int _TotalSftpTransfer = 0;

                //send zip files                    
                LogToSystemLog("Sending files to sftp...");
                LogToSystemLog("SynchronizeDirectories started...");

                if (!sftp.SynchronizeDirectories(BANK_REPO, ref errMsg, ref _TotalSftpTransfer))
                {
                    LogToErrorLog(Utilities.TimeStamp() + "RunBankProcess(): SynchronizeDirectories failed.Error " + errMsg);
                }

                sftp = null;
                LogToSystemLog("Total zipped file(s) uploaded: " + _TotalSftpTransfer.ToString("N0"));
            }

            foreach (string dir in Directory.GetDirectories(transferFolder)) if (Directory.GetFiles(dir).Length == 0) Directory.Delete(dir);

            string endTime = Utilities.TimeStamp();

            LogToSystemLog("Start: " + startTime);
            LogToSystemLog("End: " + endTime);
            LogToSystemLog("End of process");

            //disable if process is thru scheduled task
            //Console.WriteLine(Utilities.TimeStamp() + "Last process: " + strLastProcessTime);
            //Console.WriteLine(Utilities.TimeStamp() + "Waiting for next process on {0}...", strNextProcessTime);

            intDateProcess += 1;

            IsBankProcessReady = true;
        }

        private static bool GetDailyTxnAndPackData(DateTime dtmReportDate)
        {
            DAL dal = new DAL();
            try
            {
                string conStr = "Server=" + DB_SERVER + "; Database=" + DB_NAME + "; User=" + DB_USER + "; Password=" + DB_PASS + ";";

                string doneIDs = "";
                string doneIDsFile = Utilities.DoneIDsFile(dtmReportDate);
                if (System.IO.File.Exists(doneIDsFile)) doneIDs = System.IO.File.ReadAllText(doneIDsFile);

                if (dal.SelectTxnForTransfer(conStr, dtmReportDate, doneIDs))
                //if (dal.SelectPendingKYC(conStr))
                {
                    pagibig_aub_ws.ACC_MS_WEBSERVICE ws = new pagibig_aub_ws.ACC_MS_WEBSERVICE();
                    dtBankFiles = dal.TableResult;
                    if (dtBankFiles.DefaultView.Count == 0) LogToSystemLog("Table txnfortransfer is empty");
                    else
                    {
                        LogToSystemLog("Extracted data: " + dtBankFiles.DefaultView.Count.ToString("N0"));
                        sbEmail.AppendLine("Extracted data: " + dtBankFiles.DefaultView.Count.ToString("N0"));
                        int intRecord = 1;
                        foreach (DataRow rw in dtBankFiles.Rows)
                        {
                            if (File.Exists(string.Format("{0}\\{1}.zip", BANK_REPO, rw["PagIBIGID"].ToString()))) { }
                            else if (Directory.Exists(string.Format("{0}\\{1}", BANK_REPO, rw["PagIBIGID"].ToString()))) { }
                            else
                            {
                                try
                                {
                                    var response = ws.ManualPackUpData(rw["RefNum"].ToString(), "");
                                    if (response.IsSuccess) LogToSystemLog("MPD extracted " + rw["RefNum"].ToString() + " " + intRecord.ToString("N0") + " of " + dtBankFiles.DefaultView.Count.ToString("N0"));
                                    else LogToErrorLog("MPD failed to extract " + rw["RefNum"].ToString() + ". " + response.ErrorMessage);
                                }
                                catch (Exception ex)
                                {
                                    LogToErrorLog("ws.ManualPackUpData(): Failed in " + rw["RefNum"].ToString() + ". Runtime error " + ex.Message);
                                }
                            }

                            System.Threading.Thread.Sleep(100);
                            intRecord += 1;
                        }
                    }
                }
                else
                {
                    LogToErrorLog("SelectTxnForTransfer failed . " + dal.ErrorMessage);
                    sbEmail.AppendLine(Utilities.TimeStamp() + "SelectTxnForTransfer failed . " + dal.ErrorMessage);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                LogToErrorLog("GetDailyTxnAndPackData(): " + ex.Message);
                sbEmail.AppendLine(Utilities.TimeStamp() + "GetDailyTxnAndPackData(): " + ex.Message);
                return false;
            }
            finally { dal = null; }
        }

        private static bool DeleteFile(string strFile)
        {
            try
            {
                File.Delete(strFile);

                return true;
            }
            catch (Exception ex)
            {
                LogToErrorLog("DeleteFile(): Runtime catched error " + ex.Message);
                return false;
            }
        }
        //
        public static string Encrypt(string clearText)
        {
            const string EncryptionKey = "ragMANOK2kx2014";
            byte[] clearBytes = System.Text.Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6E, 0x20, 0x4D, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        public static string Decrypt(string cipherText)
        {
            const string EncryptionKey = "ragMANOK2kx2014";
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes decryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6E, 0x20, 0x4D, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                decryptor.Key = pdb.GetBytes(32);
                decryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = System.Text.Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }

        private static void LogToSystemLog(string logDesc)
        {
            Console.WriteLine(Utilities.TimeStamp() + logDesc);
            Utilities.SaveToSystemLog(Utilities.TimeStamp() + logDesc);
        }

        private static void LogToErrorLog(string logDesc)
        {
            Console.WriteLine(Utilities.TimeStamp() + logDesc);
            Utilities.SaveToErrorLog(Utilities.TimeStamp() + logDesc);
        }

        private static bool IsRefDateExist()
        {
            return File.Exists(refDateFile);
        }

        private static int PauseTime()
        {
            return 10000;
        }

    }

}
