
using System;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;

namespace accpagibigph3srv
{
    class DAL
    {    
        private DataTable _dt = new DataTable();
        private object _object;
        private string strErrorMessage;
        private IDataReader _readerResult;

        private SqlConnection con;
        private SqlCommand cmd;
        private SqlDataAdapter da;

        public string ErrorMessage
        {
            get { return strErrorMessage; }
        }

        public object ObjectResult
        {
            get { return _object; }
        }   

        public DataTable TableResult
        {
            get { return _dt; }
        }               
        
        public bool IsConnectionOK(string conStr)
        {
            SqlConnection con = new SqlConnection(conStr);            

            try
            {
                con.Open();
                con.Close();

                return true;
            }
            catch 
            {                
                return false;
            }
            finally
            {             
                if (con.State == ConnectionState.Open) { con.Close(); }
            }
        }        


        public bool SelectTxnForTransfer(string conStr, DateTime dtmReportDate, string doneIDs)
        {
            SqlConnection con = new SqlConnection(conStr);
            SqlCommand cmd = null;
            SqlDataAdapter da = null;

            try
            {                
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("SELECT dbo.tbl_Member.ID, dbo.tbl_Member.RefNum, dbo.tbl_Member.PagIBIGID, CAST(dbo.tbl_Member.EntryDate AS date) AS dtm ");
                sb.Append(string.Format("FROM dbo.tbl_Member WHERE (dbo.tbl_Member.EntryDate BETWEEN '{0} 00:00:00' AND '{0} 23:59:59') ", dtmReportDate.ToString("yyyy-MM-dd")));                
                if (doneIDs!="") sb.Append(" AND dbo.tbl_Member.PagIBIGID NOT IN ('" + doneIDs.Replace(",","','") + "')");

                //filter by mid
                //sb.Append(" AND dbo.tbl_Member.PagIBIGID='121178381382'"); 

                cmd = new SqlCommand(sb.ToString(), con);
                cmd.CommandTimeout = 0;
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter(cmd);
                da.Fill(_dt);

                return true;
            }
            catch (Exception ex)
            {
                strErrorMessage = ex.Message;
                return false;
            }
            finally
            {
                if (cmd != null) { cmd.Dispose(); }
                if (da != null) { da.Dispose(); }
                if (con.State == ConnectionState.Open) { con.Close(); }
            }
        }

        public bool SelectPendingKYC(string conStr)
        {
            SqlConnection con = new SqlConnection(conStr);
            SqlCommand cmd = null;
            SqlDataAdapter da = null;

            try
            {
                cmd = new SqlCommand("spSelectPendingKYC", con);
                cmd.CommandTimeout = 0;
                cmd.CommandType = CommandType.Text;
                da = new SqlDataAdapter(cmd);
                da.Fill(_dt);

                return true;
            }
            catch (Exception ex)
            {
                strErrorMessage = ex.Message;
                return false;
            }
            finally
            {
                if (cmd != null) { cmd.Dispose(); }
                if (da != null) { da.Dispose(); }
                if (con.State == ConnectionState.Open) { con.Close(); }
            }
        }

        private void OpenConnection()
        {
            if (con == null) con = new SqlConnection(Utilities.ConStr);
        }

        public void ClearAllPools()
        {
            SqlConnection.ClearAllPools();
        }

        private void CloseConnection()
        {
            if (cmd != null) cmd.Dispose();
            if (da != null) da.Dispose();
            if (_readerResult != null)
            {
                _readerResult.Close();
                _readerResult.Dispose();
            }
            if (con != null)
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
            ClearAllPools();
        }

        private void ExecuteNonQuery(CommandType cmdType)
        {
            cmd.CommandType = cmdType;

            // If con.State = ConnectionState.Open Then con.Close()
            // con.Open()
            if (con.State == ConnectionState.Closed)
                con.Open();
            cmd.ExecuteNonQuery();
            con.Close();
        }

        public bool AddSFTP(string refNum, string pagIBIGID, string GUID, string type)
        {
            try
            {
                OpenConnection();
                cmd = new SqlCommand("prcAddSFTP", con);
                cmd.Parameters.AddWithValue("RefNum", refNum);
                cmd.Parameters.AddWithValue("PagIBIGID", pagIBIGID);
                cmd.Parameters.AddWithValue("GUID", Utilities.EncryptData(GUID));
                cmd.Parameters.AddWithValue("Type", type);

                ExecuteNonQuery(CommandType.StoredProcedure);

                return true;
            }
            catch (Exception ex)
            {
                strErrorMessage = ex.Message;
                return false;
            }
        }

        public bool AddSFTPv2(string refNum, string pagIBIGID, string GUID, string type, string remark, DateTime dtm)
        {
            try
            {
                OpenConnection();
                cmd = new SqlCommand("prcAddSFTPv2", con);
                cmd.Parameters.AddWithValue("RefNum", refNum);
                cmd.Parameters.AddWithValue("PagIBIGID", pagIBIGID);
                cmd.Parameters.AddWithValue("GUID", Utilities.EncryptData(GUID));
                cmd.Parameters.AddWithValue("Type", type);
                cmd.Parameters.AddWithValue("PagIbigMemConsoDate", dtm);
                cmd.Parameters.AddWithValue("Remark", remark);
                cmd.Parameters.AddWithValue("SFTPTransferDate", dtm);

                ExecuteNonQuery(CommandType.StoredProcedure);

                return true;
            }
            catch (Exception ex)
            {
                strErrorMessage = ex.Message;
                return false;
            }
        }

        public bool InsertSFTP(string mid, string remark, DateTime sftpTransferDate)
        {
            try
            {
                OpenConnection();

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("INSERT INTO tbl_SFTP (PagIBIGID, Type, Remark, SFTPTransferDate, DatePosted, TimePosted) ");
                sb.Append("VALUES (@PagIBIGID, 'ZIP', @Remark, @SFTPTransferDate, GETDATE(), GETDATE()) ");

                cmd = new SqlCommand(sb.ToString(), con);
                cmd.Parameters.AddWithValue("PagIBIGID", mid);
                cmd.Parameters.AddWithValue("Remark", remark);
                cmd.Parameters.AddWithValue("SFTPTransferDate", sftpTransferDate);

                ExecuteNonQuery(CommandType.Text);

                return true;
            }
            catch (Exception ex)
            {
                strErrorMessage = ex.Message;
                return false;
            }
        }

        public bool UpdateSFTPZipProcessDate(string refNum, string pagIBIGID, string GUID)
        {
            try
            {
                OpenConnection();
                cmd = new SqlCommand("prcUpdateSFTPZipProcessDate", con);
                //cmd.Parameters.AddWithValue("RefNum", refNum);
                //cmd.Parameters.AddWithValue("PagIBIGID", pagIBIGID);
                cmd.Parameters.AddWithValue("GUID", Utilities.EncryptData(GUID));

                ExecuteNonQuery(CommandType.StoredProcedure);

                return true;
            }
            catch (Exception ex)
            {
                strErrorMessage = ex.Message;
                return false;
            }
        }

        public bool UpdateSFTP(string refNum, string pagIBIGID, string GUID, string type)
        {
            try
            {
                OpenConnection();
                cmd = new SqlCommand("prcAddSFTP", con);
                cmd.Parameters.AddWithValue("RefNum", refNum);
                cmd.Parameters.AddWithValue("PagIBIGID", pagIBIGID);
                cmd.Parameters.AddWithValue("GUID", Utilities.EncryptData(GUID));
                cmd.Parameters.AddWithValue("Type", type);

                ExecuteNonQuery(CommandType.StoredProcedure);

                return true;
            }
            catch (Exception ex)
            {
                strErrorMessage = ex.Message;
                return false;
            }
        }

        public bool UpdatePagIBIGMemConso(string pagibiMemFileName, string pagIBIGID, string GUID)
        {
            try
            {
                OpenConnection();
                cmd = new SqlCommand("UPDATE tbl_SFTP SET PagIbigMemConsoDate=GETDATE(),Remark=@Remark WHERE PagIBIGID=@PagIBIGID AND GUID=@GUID AND PagIbigMemConsoDate IS NULL AND Type='TXT'", con);
                cmd.Parameters.AddWithValue("Remark", pagibiMemFileName);
                cmd.Parameters.AddWithValue("PagIBIGID", pagIBIGID);
                cmd.Parameters.AddWithValue("GUID", Utilities.EncryptData(GUID));

                ExecuteNonQuery(CommandType.Text);

                return true;
            }
            catch (Exception ex)
            {
                strErrorMessage = ex.Message;
                return false;
            }
        }

        public bool UpdatePagIBIGMemConsov2(string pagibiMemFileName, string pagIBIGID, string GUID, DateTime dtm)
        {
            try
            {
                OpenConnection();
                cmd = new SqlCommand("UPDATE tbl_SFTP SET PagIbigMemConsoDate=@PagIbigMemConsoDate,Remark=@Remark WHERE PagIBIGID=@PagIBIGID AND GUID=@GUID AND PagIbigMemConsoDate IS NULL AND Type='TXT'", con);
                cmd.Parameters.AddWithValue("PagIbigMemConsoDate", dtm);
                cmd.Parameters.AddWithValue("Remark", pagibiMemFileName);
                cmd.Parameters.AddWithValue("PagIBIGID", pagIBIGID);
                cmd.Parameters.AddWithValue("GUID", Utilities.EncryptData(GUID));

                ExecuteNonQuery(CommandType.Text);

                return true;
            }
            catch (Exception ex)
            {
                strErrorMessage = ex.Message;
                return false;
            }
        }

        public bool UpdateSFTPTransferDate(string GUID, string type)
        {
            try
            {
                OpenConnection();
                if (type == "TXT")
                    cmd = new SqlCommand("UPDATE tbl_SFTP SET SFTPTransferDate=GETDATE() WHERE GUID=@GUID AND Type=@Type AND PagIbigMemConsoDate IS NOT NULL AND SFTPTransferDate IS NULL", con);
                else
                    cmd = new SqlCommand("UPDATE tbl_SFTP SET SFTPTransferDate=GETDATE() WHERE GUID=@GUID AND Type=@Type AND ZipProcessDate IS NOT NULL AND SFTPTransferDate IS NULL", con);

                cmd.Parameters.AddWithValue("GUID", Utilities.EncryptData(GUID));
                cmd.Parameters.AddWithValue("Type", type);

                ExecuteNonQuery(CommandType.Text);

                return true;
            }
            catch (Exception ex)
            {
                strErrorMessage = ex.Message;
                return false;
            }
        }

        public bool UpdateSFTPTransferDatev2(string GUID, string type, DateTime dtm)
        {
            try
            {
                OpenConnection();
                if (type == "TXT")
                    cmd = new SqlCommand("UPDATE tbl_SFTP SET SFTPTransferDate=@SFTPTransferDate WHERE GUID=@GUID AND Type=@Type AND PagIbigMemConsoDate IS NOT NULL AND SFTPTransferDate IS NULL", con);
                else
                    cmd = new SqlCommand("UPDATE tbl_SFTP SET SFTPTransferDate=@SFTPTransferDate WHERE GUID=@GUID AND Type=@Type AND ZipProcessDate IS NOT NULL AND SFTPTransferDate IS NULL", con);

                cmd.Parameters.AddWithValue("SFTPTransferDate", dtm);
                cmd.Parameters.AddWithValue("GUID", Utilities.EncryptData(GUID));
                cmd.Parameters.AddWithValue("Type", type);

                ExecuteNonQuery(CommandType.Text);

                return true;
            }
            catch (Exception ex)
            {
                strErrorMessage = ex.Message;
                return false;
            }
        }

        public bool UpdateSFTPTransferDateByPagIBIGMemFileName(string pagibigMemFileName)
        {
            try
            {
                OpenConnection();
                cmd = new SqlCommand("UPDATE tbl_SFTP SET SFTPTransferDate=GETDATE() WHERE Remark=@Remark AND Type='TXT' AND PagIbigMemConsoDate IS NOT NULL AND SFTPTransferDate IS NULL", con);

                cmd.Parameters.AddWithValue("Remark", pagibigMemFileName);

                ExecuteNonQuery(CommandType.Text);

                return true;
            }
            catch (Exception ex)
            {
                strErrorMessage = ex.Message;
                return false;
            }
        }

        public bool UpdateSFTPTransferDateByPagIBIGMemFileNamev2(string pagibigMemFileName, DateTime dtm)
        {
            try
            {
                OpenConnection();
                cmd = new SqlCommand("UPDATE tbl_SFTP SET SFTPTransferDate=@SFTPTransferDate WHERE Remark=@Remark AND Type='TXT' AND PagIbigMemConsoDate IS NOT NULL AND SFTPTransferDate IS NULL", con);

                cmd.Parameters.AddWithValue("SFTPTransferDate", dtm);
                cmd.Parameters.AddWithValue("Remark", pagibigMemFileName);

                ExecuteNonQuery(CommandType.Text);

                return true;
            }
            catch (Exception ex)
            {
                strErrorMessage = ex.Message;
                return false;
            }
        }





    }
}
