using System;
using System.Collections.Generic;
using WinSCP;
using System.IO;


namespace accpagibigph3srv
{
    class SFTP
    {
        private delegate void dlgtProcess();      

        private static string configFile = "sftpbank";
        //private static bool IsProcessReady = true;        

        private static string SFTP_HOST = ""; //"172.18.3.214";
        private static int SFTP_PORT = 0; //22;
        private static string SFTP_USER = ""; //"allcarduser";
        private static string SFTP_PASS = ""; //"allcardus3r@2019";
        private static string SFTP_SshHostKeyFingerprint = ""; //"ssh-ed25519 256 9d:f4:bc:3d:bc:7e:07:f9:ca:e1:74:05:22:09:89:c1";
        public string SFTP_LOCALPATH = ""; //@"E:\SFTP UBP Acknowledgement File\For Upload";
        private static string SFTP_REMOTEPATH_ZIP = ""; //"/CPT/IN/ACKNOWLEDGEMENT_FILE";
        private static string SFTP_REMOTEPATH_PAGIBIGMEMU = ""; //"/CPT/IN/ACKNOWLEDGEMENT_FILE";
      
        public SFTP()
        {
            using (StreamReader sr = new StreamReader(configFile))
            {
                while (!sr.EndOfStream)
                {
                    string strLine = sr.ReadLine();
                    if (strLine.Trim() != "")
                    {
                        switch (strLine.Split('=')[0])
                        {
                            case "SFTP_HOST":
                                SFTP_HOST = strLine.Split('=')[1];
                                break;
                            case "SFTP_PORT":
                                SFTP_PORT = Convert.ToInt32(strLine.Split('=')[1]);
                                break;
                            case "SFTP_USER":
                                SFTP_USER = strLine.Split('=')[1];
                                break;
                            case "SFTP_PASS":
                                SFTP_PASS = strLine.Split('=')[1];
                                break;
                            case "SFTP_SshHostKeyFingerprint":
                                SFTP_SshHostKeyFingerprint = strLine.Split('=')[1];
                                break;
                            case "SFTP_LOCALPATH":
                                SFTP_LOCALPATH = strLine.Split('=')[1];
                                break;
                            case "SFTP_SFTPPATH":
                                SFTP_REMOTEPATH_ZIP = strLine.Split('=')[1];
                                break;                         
                        }
                    }

                }
                sr.Dispose();
                sr.Close();
            }
        }
                
        private static SessionOptions sessionOptions()
        {
            return new SessionOptions
            {
                Protocol = Protocol.Sftp,
                HostName = SFTP_HOST,
                UserName = SFTP_USER,
                Password = SFTP_PASS,
                PortNumber = SFTP_PORT,
                SshHostKeyFingerprint = SFTP_SshHostKeyFingerprint
            };
        }         
     
        public bool Upload_SFTP_Files(string path, bool IsZip, ref string errMsg)
        {
            try
            {
                int intFileCount = Directory.GetFiles(SFTP_LOCALPATH).Length;

                if (intFileCount == 0)
                {
                    errMsg = string.Format("[Upload] {0} is empty. No file to push.", SFTP_LOCALPATH);                  
                    return false;
                }             

                using (Session session = new Session())
                {                                 
                    session.DisableVersionCheck = true;

                    session.Open(sessionOptions());

                    // Upload files
                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;
                    //transferOptions.ResumeSupport.State = TransferResumeSupportState.Smart;                  
                    
                    //transferOptions.PreserveTimestamp = false;

                    //Console.Write(AppDomain.CurrentDomain.BaseDirectory);
                    string remotePath = SFTP_REMOTEPATH_ZIP;
                    if (!IsZip) remotePath = SFTP_REMOTEPATH_PAGIBIGMEMU;

                     TransferOperationResult transferResult = null;
                    if (File.Exists(path))
                    {
                        {
                            if (!session.FileExists(remotePath + Path.GetFileName(path)))
                            {
                                transferResult = session.PutFiles(string.Format(@"{0}*", path), remotePath, false, transferOptions);
                            }

                            else
                            {
                                errMsg = string.Format("Upload_SFTP_Files(): Remote file exist " + Path.GetFileName(path));                               
                                return false;
                            }
                        }
                    }
                      else
                    
                        transferResult = session.PutFiles(string.Format(@"{0}\*", SFTP_LOCALPATH), remotePath, false, transferOptions);
                    

                        // Throw on any error
                        transferResult.Check();

                        // Print results
                        foreach (TransferEventArgs transfer in transferResult.Transfers)
                        {
                            //Console.WriteLine(TimeStamp() + Path.GetFileName(transfer.FileName) + " transferred successfully");
                            //string strFilename = Path.GetFileName(transfer.FileName);
                            //File.Delete(transfer.FileName);
                        }                        
                    }

                //Console.WriteLine("Success sftp transfer " + path);
                //System.Threading.Thread.Sleep(100);

                return true;
                
            }                            
            catch (Exception ex)
            {
                errMsg = string.Format("Upload_SFTP_Files(): Runtime error {0}", ex.Message);
                Console.WriteLine(errMsg);
                //Utilities.WriteToRTB(errMsg, ref rtb, ref tssl);
                return false;
            }
        }

        private static string BANK_REPO = "";
        private static System.Text.StringBuilder sbDone = new System.Text.StringBuilder();
        private static int TotalSftpTransfer;
        private static DAL dal;

        public bool SynchronizeDirectories(string bank_repo, ref string errMsg, ref int _TotalSftpTransfer)
        {
            try
            {
                if (dal == null) dal = new DAL();

                BANK_REPO = bank_repo;                

                string forTransferFolder = SFTP_LOCALPATH + "\\FOR_TRANSFER";                

                int intFileCount = Directory.GetFiles(forTransferFolder).Length;

                //if (intFileCount == 0)
                //{
                //    errMsg = string.Format("[Upload] {0} is empty. No file to push.", forTransferFolder);                    
                //    return false;
                //}                

                using (Session session = new Session())
                {
                    session.DisableVersionCheck = true;

                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;
                    transferOptions.FilePermissions = null;
                    transferOptions.PreserveTimestamp = false;
                    

                    // Will continuously report progress of synchronization
                    session.FileTransferred += FileTransferred;                    

                    // Connect
                    session.Open(sessionOptions());                    

                    // Synchronize files
                    SynchronizationResult synchronizationResult;                 
                    synchronizationResult = session.SynchronizeDirectories(SynchronizationMode.Remote, @forTransferFolder, SFTP_REMOTEPATH_ZIP, false, false, SynchronizationCriteria.None, transferOptions);                    

                    // Throw on any error
                    synchronizationResult.Check();
                }

                _TotalSftpTransfer = TotalSftpTransfer;

                if (dal != null) dal = null;

                return true;
            }
            catch (Exception ex)
            {
                errMsg = string.Format("SynchronizeDirectories(): Runtime error {0}", ex.Message);
                Console.WriteLine(errMsg);                
                return false;
            }
        }                

        private static void FileTransferred(object sender, TransferEventArgs e)
        {
            string msg = "";
            if (e.Error == null)
            {
                msg = string.Format("{0}Upload of {1} succeeded", Utilities.TimeStamp(), Path.GetFileName(e.FileName));
                Console.WriteLine(msg);
                Utilities.SaveToSystemLog(msg);

                string[] arr = e.FileName.Split('\\');
                string sourceFolder = "";
                try
                {
                    sourceFolder = arr[arr.Length - 2];
                }
                catch { }

                //sbDone.AppendLine(Path.GetFileName(e.FileName));
                string destiFile = e.FileName.Replace("FOR_TRANSFER", "DONE");
                if (File.Exists(destiFile))
                {
                    string destiFileExisting = string.Format("{0}_{1}.zip", Path.GetFileNameWithoutExtension(destiFile), new FileInfo(destiFile).CreationTime.ToString("yyyyMMdd_hhmmss"));
                    if(!File.Exists(destiFileExisting))File.Move(destiFile, destiFileExisting);
                }

                try
                {
                    File.Move(e.FileName, destiFile);
                }
                catch { }

                dal.InsertSFTP(Path.GetFileName(e.FileName).Replace(".zip", ""), sourceFolder, DateTime.Now);

                //string doneIDs = "";
                //if (System.IO.File.Exists(Utilities.DoneIDsFile())) doneIDs = System.IO.File.ReadAllText(Utilities.DoneIDsFile());
                //if (doneIDs == "") Utilities.SaveToDoneIDs(Path.GetFileNameWithoutExtension(e.FileName));
                //else Utilities.SaveToDoneIDs("," + Path.GetFileNameWithoutExtension(e.FileName));

                TotalSftpTransfer += 1;
                System.Threading.Thread.Sleep(100);
            }
            else
            {
                msg = string.Format("{0}Upload of {1} failed: {2}", Utilities.TimeStamp(), Path.GetFileName(e.FileName), e.Error);
                Console.WriteLine(msg);
                Utilities.SaveToErrorLog(msg);                
            }

            if (e.Chmod != null)
            {
                if (e.Chmod.Error == null)
                {
                    msg = string.Format("{0}Permissions of {1} set to {2}", Utilities.TimeStamp(), Path.GetFileName(e.Chmod.FileName), e.Chmod.FilePermissions);
                    Console.WriteLine(msg);
                    Utilities.SaveToSystemLog(msg);
                }
                else
                {                    
                    msg = string.Format("{0}Setting permissions of {1} failed: {2}", Utilities.TimeStamp(), Path.GetFileName(e.Chmod.FileName), e.Chmod.Error);
                    Console.WriteLine(msg);
                    Utilities.SaveToErrorLog(msg);
                }
            }
            else
            {
                //Console.WriteLine("{0}Permissions of {1} kept with their defaults", TimeStamp(), e.Destination);
            }

            if (e.Touch != null)
            {
                if (e.Touch.Error == null)
                {                    
                    msg = string.Format("{0}Timestamp of {1} set to {2}", Utilities.TimeStamp(), Path.GetFileName(e.Touch.FileName), e.Touch.LastWriteTime);
                    Console.WriteLine(msg);
                    Utilities.SaveToSystemLog(msg);
                }
                else
                {                    
                    msg = string.Format("{0}Setting timestamp of {1} failed: {2}", Utilities.TimeStamp(), Path.GetFileName(e.Touch.FileName), e.Touch.Error);
                    Console.WriteLine(msg);
                    Utilities.SaveToErrorLog(msg);
                }
            }
            else
            {
                // This should never happen during "local to remote" synchronization                
                msg = string.Format("{0}Timestamp of {1} kept with its default (current time)", Utilities.TimeStamp(), e.Destination);
                Console.WriteLine(msg);
                Utilities.SaveToErrorLog(msg);
            }
        }


    }
}
