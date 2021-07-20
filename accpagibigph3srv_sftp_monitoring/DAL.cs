
using System;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;

namespace accpagibigph3srv_sftp_monitoring
{
    class DAL : IDisposable

    {

        private string ConStr = Utilities.DbaseConStr;
        private DataTable dtResult;
        //private DataSet dsResult;
        private object objResult;
        private IDataReader _readerResult;
        private string strErrorMessage;

        private SqlConnection con;
        private SqlCommand cmd;
        private SqlDataAdapter da;

        public string ErrorMessage
        {
            get { return strErrorMessage; }
        }

        public DataTable TableResult
        {
            get
            {
                return dtResult;
            }
        }

        public object ObjectResult
        {
            get
            {
                return objResult;
            }
        }


        public void ClearAllPools()
        {
            SqlConnection.ClearAllPools();
        }

        private void OpenConnection()
        {
            if (con == null) con = new SqlConnection(ConStr);
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

        private void _ExecuteScalar(CommandType cmdType)
        {
            cmd.CommandType = cmdType;

            // If con.State = ConnectionState.Open Then con.Close()
            // con.Open()
            if (con.State == ConnectionState.Closed) con.Open();
            object _obj;
            _obj = cmd.ExecuteScalar();
            con.Close();

            objResult = _obj;
        }

        private void _ExecuteReader(CommandType cmdType)
        {
            cmd.CommandType = cmdType;

            // If con.State = ConnectionState.Open Then con.Close()
            // con.Open()
            if (con.State == ConnectionState.Closed)
                con.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            _readerResult = reader;
        }

        private void FillDataAdapter(CommandType cmdType)
        {
            cmd.CommandTimeout = 0;
            cmd.CommandType = cmdType;
            da = new SqlDataAdapter(cmd);
            DataTable _dt = new DataTable();
            da.Fill(_dt);
            dtResult = _dt;
        }

        public bool SelectQuery(string strQuery)
        {
            try
            {
                OpenConnection();
                cmd = new SqlCommand(strQuery, con);

                FillDataAdapter(CommandType.Text);

                return true;
            }
            catch (Exception ex)
            {
                strErrorMessage = ex.Message;
                return false;
            }
        }

        public bool SelectCapturedDataSummary(string dtmStart, string dtmEnd)
        {
            try
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("SELECT CAST(EntryDate as date) As CapturedDate, COUNT(*) As Qty from tbl_Member ");
                sb.Append(string.Format("WHERE EntryDate BETWEEN '{0} 00:00:00' AND '{1} 23:59:59' ",dtmStart,dtmEnd));
                sb.Append("GROUP BY CAST(EntryDate as date) ");
                sb.Append("ORDER BY CAST(EntryDate as date) ");

                OpenConnection();
                cmd = new SqlCommand(sb.ToString(), con);
                cmd.CommandTimeout = 0;

                FillDataAdapter(CommandType.Text);

                return true;
            }
            catch (Exception ex)
            {
                strErrorMessage = ex.Message;
                return false;
            }
        }

        public bool SelectSFTPDetails(string dtmStart, string dtmEnd)
        {
            try
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("SELECT * from tbl_SFTP ");
                sb.Append(string.Format("WHERE DatePosted BETWEEN '{0}' AND '{1}' ", dtmStart, dtmEnd));                
                sb.Append("ORDER BY DatePosted ");

                OpenConnection();
                cmd = new SqlCommand(sb.ToString(), con);
                cmd.CommandTimeout = 0;

                FillDataAdapter(CommandType.Text);

                return true;
            }
            catch (Exception ex)
            {
                strErrorMessage = ex.Message;
                return false;
            }
        }

        public bool SelectSFTPDetailsPending()
        {
            try
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("SELECT dbo.tbl_SFTP.* ");
                //sb.Append("CASE WHEN Type = 'TXT' THEN ");
                //sb.Append("CASE WHEN PagIbigMemConsoDate IS NULL THEN 'For consolidation' ELSE '' END ");
                //sb.Append("ELSE ");
                //sb.Append("CASE WHEN ZipProcessDate IS NULL THEN ''For compression' ELSE '' END ");
                //sb.Append("END + ', Not SFTP transferred' ");
                sb.Append("FROM dbo.tbl_SFTP ");
                sb.Append("WHERE SFTPTransferDate IS NULL ");
                sb.Append(" AND DatePosted <> '01/03/2020'");
                sb.Append("ORDER BY ID ");
                //sb.Append("WHERE SFTPTransferDate IS NULL-- AND DatePosted<> '01/20/2020' ");

                OpenConnection();
                cmd = new SqlCommand(sb.ToString(), con);
                cmd.CommandTimeout = 0;

                FillDataAdapter(CommandType.Text);

                return true;
            }
            catch (Exception ex)
            {
                strErrorMessage = ex.Message;
                return false;
            }
        }

        public bool GetSFTPDetailsSFTPTransferDateDateCount(string dtmStart, string dtmEnd, string type)
        {
            try
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("SELECT Count(SFTPTransferDate) from tbl_SFTP ");
                sb.Append(string.Format("WHERE DatePosted BETWEEN '{0}' AND '{1}' ", dtmStart, dtmEnd));
                sb.Append(string.Format("AND SFTPTransferDate IS NOT NULL AND Type='{0}'",type));

                OpenConnection();
                cmd = new SqlCommand(sb.ToString(), con);

                _ExecuteScalar(CommandType.Text);

                return true;
            }
            catch (Exception ex)
            {
                strErrorMessage = ex.Message;
                return false;
            }
        }

        public bool IsConnectionOK(string strConString = "")
        {
            try
            {
                if (strConString != "")
                    ConStr = strConString;
                OpenConnection();

                con.Open();
                con.Close();

                return true;
            }
            catch (Exception ex)
            {
                strErrorMessage = ex.Message;
                return false;
            }
        }

        

        public bool ExecuteQuery(string strQuery)
        {
            try
            {
                OpenConnection();
                cmd = new SqlCommand(strQuery, con);

                ExecuteNonQuery(CommandType.Text);

                return true;
            }
            catch (Exception ex)
            {
                strErrorMessage = ex.Message;
                return false;
            }
        }

        //public bool AddSFTP(string refNum, string pagIBIGID, string GUID, string type)
        //{
        //    try
        //    {
        //        OpenConnection();
        //        cmd = new SqlCommand("prcAddSFTP", con);
        //        cmd.Parameters.AddWithValue("RefNum", refNum);
        //        cmd.Parameters.AddWithValue("PagIBIGID", pagIBIGID);
        //        cmd.Parameters.AddWithValue("GUID", Utilities.EncryptData(GUID));
        //        cmd.Parameters.AddWithValue("Type", type);

        //        ExecuteNonQuery(CommandType.StoredProcedure);

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        strErrorMessage = ex.Message;
        //        return false;
        //    }
        //}

        //public bool UpdateSFTPZipProcessDate(string refNum, string pagIBIGID, string GUID)
        //{
        //    try
        //    {
        //        OpenConnection();
        //        cmd = new SqlCommand("prcUpdateSFTPZipProcessDate", con);
        //        //cmd.Parameters.AddWithValue("RefNum", refNum);
        //        //cmd.Parameters.AddWithValue("PagIBIGID", pagIBIGID);
        //        cmd.Parameters.AddWithValue("GUID", Utilities.EncryptData(GUID));
                
        //        ExecuteNonQuery(CommandType.StoredProcedure);

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        strErrorMessage = ex.Message;
        //        return false;
        //    }
        //}

        //public bool UpdateSFTP(string refNum, string pagIBIGID, string GUID, string type)
        //{
        //    try
        //    {
        //        OpenConnection();
        //        cmd = new SqlCommand("prcAddSFTP", con);
        //        cmd.Parameters.AddWithValue("RefNum", refNum);
        //        cmd.Parameters.AddWithValue("PagIBIGID", pagIBIGID);
        //        cmd.Parameters.AddWithValue("GUID", Utilities.EncryptData(GUID));
        //        cmd.Parameters.AddWithValue("Type", type);

        //        ExecuteNonQuery(CommandType.StoredProcedure);

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        strErrorMessage = ex.Message;
        //        return false;
        //    }
        //}

        //public bool UpdatePagIBIGMemConso(string pagibiMemFileName, string pagIBIGID, string GUID)
        //{
        //    try
        //    {
        //        OpenConnection();
        //        cmd = new SqlCommand("UPDATE tbl_SFTP SET PagIbigMemConsoDate=GETDATE(),Remark=@Remark WHERE PagIBIGID=@PagIBIGID AND GUID=@GUID AND PagIbigMemConsoDate IS NULL AND Type='TXT'", con);
        //        cmd.Parameters.AddWithValue("Remark", pagibiMemFileName);
        //        cmd.Parameters.AddWithValue("PagIBIGID", pagIBIGID);
        //        cmd.Parameters.AddWithValue("GUID", Utilities.EncryptData(GUID));
                
        //        ExecuteNonQuery(CommandType.Text);

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        strErrorMessage = ex.Message;
        //        return false;
        //    }
        //}

        //public bool UpdateSFTPTransferDate(string GUID, string type)
        //{
        //    try
        //    {
        //        OpenConnection();
        //        if(type=="TXT")
        //            cmd = new SqlCommand("UPDATE tbl_SFTP SET SFTPTransferDate=GETDATE() WHERE GUID=@GUID AND Type=@Type AND PagIbigMemConsoDate IS NOT NULL AND SFTPTransferDate IS NULL", con);
        //        else
        //            cmd = new SqlCommand("UPDATE tbl_SFTP SET SFTPTransferDate=GETDATE() WHERE GUID=@GUID AND Type=@Type AND ZipProcessDate IS NOT NULL AND SFTPTransferDate IS NULL", con);

        //        cmd.Parameters.AddWithValue("GUID", Utilities.EncryptData(GUID));
        //        cmd.Parameters.AddWithValue("Type", type);

        //        ExecuteNonQuery(CommandType.Text);

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        strErrorMessage = ex.Message;
        //        return false;
        //    }
        //}

        //public bool UpdateSFTPTransferDateByPagIBIGMemFileName(string pagibigMemFileName)
        //{
        //    try
        //    {
        //        OpenConnection();
        //        cmd = new SqlCommand("UPDATE tbl_SFTP SET SFTPTransferDate=GETDATE() WHERE Remark=@Remark AND Type='TXT' AND PagIbigMemConsoDate IS NOT NULL AND SFTPTransferDate IS NULL", con);
                
        //        cmd.Parameters.AddWithValue("Remark", pagibigMemFileName);

        //        ExecuteNonQuery(CommandType.Text);

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        strErrorMessage = ex.Message;
        //        return false;
        //    }
        //}

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    CloseConnection();
                }



                // Note disposing has been done.
                disposed = true;

            }
        }

    }
}
