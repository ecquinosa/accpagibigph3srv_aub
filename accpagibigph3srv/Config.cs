using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace accpagibigph3srv
{
    class Config
    {
        public short BankID { get; set; }
        public string DbaseConStrUbp { get; set; }
        public string DbaseConStrAub { get; set; }
        public string DbaseConStrSys { get; set; }

        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUser { get; set; }
        public string SmtpPassword { get; set; }
        public int SmtpTimeout { get; set; }
        public short IsSendToSftp { get; set; }

        public string EmailRecipientsTo { get; set; }
        public string EmailRecipientsCC { get; set; }

        public string WS_Repo { get; set; }
        public string BankRepo { get; set; }
        public string SftpHost { get; set; }
        public int SftpPort { get; set; }
        public string SftpUser { get; set; }
        public string SftpPass { get; set; }
        public string SftpSshHostKeyFingerprint { get; set; }
        public string SftpLocalPath { get; set; }
        public string SftpRemotePath { get; set; }

        public int ProcessIntervalSeconds { get; set; }

        public short IsSendEmail { get; set; }
        public string LastSuccessEmailSend { get; set; }

        public Config()
        {
            IsSendToSftp = 1;
            IsSendEmail = 1;
            LastSuccessEmailSend = "";
        }
    }
}
