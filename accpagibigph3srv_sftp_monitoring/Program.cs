using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace accpagibigph3srv_sftp_monitoring
{
    public static class Program
    {

        //capturedData_today.json  --task scheduler daily/ every 20mins
        //capturedData_weekly.json --task scheduler daily/ every  1am
        //capturedData_allprevious.json --task scheduler daily/ every  1130pm (merging only of previous and today),task scheduler monthly every first of month/ every  1am (merging only of previous and today)
        //sftpDtl_today.json --task scheduler daily/ every 20mins
        //sftpDtl_allprevious.json --task scheduler daily/ every  1130pm (merging only of previous and today)
        //sftpDtl_pending.json --task scheduler daily/ every 20mins

        #region Constructors

        private static string APP_NAME = "";
        private static string configFile = AppDomain.CurrentDomain.BaseDirectory + "config";
        private static Config config;
        private static DAL dal;

        #endregion


        #region enums

        enum Process : short
        {
            CapturedData = 1,
            SFTPDetails
        }

        enum SubProcess : short
        {
            Today = 1,
            Weekly,
            AllPrevious,
            Merge,
            Pending
        }

        #endregion

        static void Main(string[] args)
        //static void Main()
        {
            string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            APP_NAME = Path.GetFileName(codeBase);

            WriteToLog(0, string.Format("{0}Application started [{1}]", Utilities.TimeStamp(), APP_NAME));

            //string[] args = { "2", "5" };
            
            //validate arguments
            if (args == null)
            {
                WriteToLog(1, string.Format("{0}Args is null", Utilities.TimeStamp()));
                Environment.Exit(0);
            }
            else
            {
                if (args.Length != 2)
                {
                    WriteToLog(1, string.Format("{0}Args is invalid", Utilities.TimeStamp()));
                    Environment.Exit(0);                
                }                
            }            

            if (IsProgramRunning(APP_NAME) > 1) return;                       

            while (!Init()) System.Threading.Thread.Sleep(5000);

            System.Threading.Thread.Sleep(5000);            

            List<CapturedDataSummaryModel> cdsmList = null;
            List<SFTPDtlModel> sdmList = null;

            string dtmStart = DateTime.Now.ToString("MM/dd/yyyy");
            string dtmEnd = dtmStart;

            if (Convert.ToInt16(args[0]) == (short)Process.CapturedData)
            {
                switch (Convert.ToInt16(args[1]))
                {
                    case (short)SubProcess.Merge:
                        WriteToLog(0, string.Format("{0}Processsing merging of summary of captured...", Utilities.TimeStamp()));
                        MergeCapturedDataFiles(CastTo<Process>(Convert.ToInt16(args[0])), CastTo<SubProcess>(Convert.ToInt16(args[1])));
                        break;
                    default:
                        cdsmList = new List<CapturedDataSummaryModel>();

                        if (Convert.ToInt16(args[1]) == (short)SubProcess.Weekly)
                        {
                            WriteToLog(0, string.Format("{0}Processsing summary of captured weekly...", Utilities.TimeStamp()));
                            var monday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
                            dtmStart = monday.ToString("MM/dd/yyyy");  //get monday date
                            dtmEnd = DateTime.Today.AddDays(-1).ToString("MM/dd/yyyy");
                        }

                        else if (Convert.ToInt16(args[1]) == (short)SubProcess.AllPrevious)
                        {
                            WriteToLog(0, string.Format("{0}Processsing summary of captured previous...", Utilities.TimeStamp()));
                            dtmStart = "06/01/2019";
                            dtmEnd = DateTime.Today.AddDays(-1).ToString("MM/dd/yyyy");
                        }

                        else
                        {
                            WriteToLog(0, string.Format("{0}Processsing summary of captured for today...", Utilities.TimeStamp()));
                        }

                        if (dal.SelectCapturedDataSummary(dtmStart, dtmEnd))
                        {
                            foreach (System.Data.DataRow rw in dal.TableResult.Rows)
                            {
                                CapturedDataSummaryModel cdsm = new CapturedDataSummaryModel();
                                cdsm.CapturedDate = Convert.ToDateTime(rw[0].ToString());
                                cdsm.DbaseQty = Convert.ToInt32(rw[1].ToString());
                                if(Directory.Exists(Path.Combine(config.BankDoneFolder, cdsm.CapturedDate.ToString("yyyy-MM-dd"))))
                                    cdsm.FolderDoneQty = Directory.GetDirectories(Path.Combine(config.BankDoneFolder, cdsm.CapturedDate.ToString("yyyy-MM-dd"))).Length;
                                int SFTPTransferredTxt = 0;
                                int SFTPTransferredZip = 0;
                                if (dal.GetSFTPDetailsSFTPTransferDateDateCount(cdsm.CapturedDate.ToString("MM/dd/yyyy"), cdsm.CapturedDate.ToString("MM/dd/yyyy"), "TXT")) SFTPTransferredTxt = (int)dal.ObjectResult;
                                if (dal.GetSFTPDetailsSFTPTransferDateDateCount(cdsm.CapturedDate.ToString("MM/dd/yyyy"), cdsm.CapturedDate.ToString("MM/dd/yyyy"), "ZIP")) SFTPTransferredZip = (int)dal.ObjectResult;
                                cdsm.SFTPTransferredTxt = SFTPTransferredTxt;
                                cdsm.SFTPTransferredZip = SFTPTransferredZip;
                                cdsmList.Add(cdsm);
                            }
                        }
                        break;
                }                 
            }

            if (Convert.ToInt16(args[0]) == (short)Process.SFTPDetails)
            {
                switch (Convert.ToInt16(args[1]))
                {
                    case (short)SubProcess.Merge:
                        WriteToLog(0, string.Format("{0}Processsing merging of sftp detailed...", Utilities.TimeStamp()));
                        MergeSFTPDetailsFiles(CastTo<Process>(Convert.ToInt16(args[0])), CastTo<SubProcess>(Convert.ToInt16(args[1])));
                        break;
                    default:
                        sdmList = new List<SFTPDtlModel>();
                        System.Data.DataTable dtData = null;

                        if (Convert.ToInt16(args[1]) == (short)SubProcess.AllPrevious)
                        {
                            WriteToLog(0, string.Format("{0}Processsing of sftp detailed previous...", Utilities.TimeStamp()));
                            dtmStart = "01/01/2020";
                            dtmEnd = DateTime.Today.AddDays(-1).ToString("MM/dd/yyyy");
                        }
                        else
                        {
                            WriteToLog(0, string.Format("{0}Processsing of sftp detailed for today...", Utilities.TimeStamp()));
                        }

                        if (Convert.ToInt16(args[1]) == (short)SubProcess.Pending)
                        {
                            WriteToLog(0, string.Format("{0}Processsing of sftp detailed pending...", Utilities.TimeStamp()));
                            if (dal.SelectSFTPDetailsPending()) dtData = dal.TableResult;
                            else WriteToLog(1, string.Format("{0}Failed to extract SFTPDetailsPending. Error ", Utilities.TimeStamp(), dal.ErrorMessage));
                        }
                        else
                        {                            
                            if (dal.SelectSFTPDetails(dtmStart, dtmEnd)) dtData = dal.TableResult;
                            else WriteToLog(1, string.Format("{0}Failed to extract SFTPDetails. Error ", Utilities.TimeStamp(), dal.ErrorMessage));
                        }


                        if (dtData != null)
                        {
                            System.Text.StringBuilder sbRemark = new System.Text.StringBuilder();
                            foreach (System.Data.DataRow rw in dtData.Rows)
                            {
                                sbRemark.Clear();
                                SFTPDtlModel sdm = new SFTPDtlModel();
                                sdm.RefNUm = rw["RefNum"].ToString().Trim();
                                sdm.PagIBIGID = rw["PagIBIGID"].ToString().Trim();
                                sdm.GUID = rw["GUID"].ToString().Trim();
                                sdm.Type = rw["Type"].ToString().Trim();
                                //sdm.Remark = rw["Remark"].ToString();
                                if (rw["PagIbigMemConsoDate"] != DBNull.Value) sdm.PagIbigMemConsoDate = Convert.ToDateTime(rw["PagIbigMemConsoDate"].ToString());                                
                                if (rw["ZipProcessDate"] != DBNull.Value) sdm.ZipProcessDate = Convert.ToDateTime(rw["ZipProcessDate"].ToString());
                                if (rw["SFTPTransferDate"] != DBNull.Value) sdm.SFTPTransferDate = Convert.ToDateTime(rw["SFTPTransferDate"].ToString());
                                sdm.DatePosted = Convert.ToDateTime(rw["DatePosted"].ToString());
                                sdm.TimePosted = TimeSpan.Parse(rw["TimePosted"].ToString());
                                if (rw["PagIbigMemConsoDate"] == DBNull.Value && sdm.Type == "TXT") sbRemark.Append("For consolidation"); else sbRemark.Append(rw["Remark"].ToString().Trim());
                                if (rw["ZipProcessDate"] == DBNull.Value && sdm.Type == "ZIP") sbRemark.Append(((sbRemark.ToString() != "") ? ". " : "") + "For compression");
                                if (rw["SFTPTransferDate"] == DBNull.Value) sbRemark.Append(((sbRemark.ToString() != "") ? ". " : "") + "For sftp transfer");
                                sdm.Remark = sbRemark.ToString();
                                sdmList.Add(sdm);
                            }
                        }                        
                        break;
                }
                 
            }
            
            dal.Dispose();
            dal = null;

            string jsonData="";
            if(cdsmList!=null)jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(cdsmList);
            if (sdmList != null) jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(sdmList);
            
            if(jsonData!="")System.IO.File.WriteAllText(Path.Combine(config.JsonRepo,GetFileName(CastTo<Process>(Convert.ToInt16(args[0])), CastTo<SubProcess>(Convert.ToInt16(args[1])))), jsonData);
        }

        private static bool Init()
        {
            try
            {
                WriteToLog(0, string.Format("{0}{1}", Utilities.TimeStamp(), "Checking config..."));

                //check if file exists
                if (!File.Exists(configFile))
                {
                    WriteToLog(1, string.Format("{0}{1}", Utilities.TimeStamp(), "Config file is missing"));
                    return false;
                }

                try
                {
                    //bind config
                    config = new Config();
                    var configData = JsonConvert.DeserializeObject<List<Config>>(File.ReadAllText(configFile));
                    config = configData[0];
                    Utilities.DbaseConStr = config.DbaseConStr;
                    //WriteToLog(0, string.Format("{0}{1}", Utilities.TimeStamp(), config.DbaseConStr + " , " + Utilities.DbaseConStr));
                    dal = new DAL();
                }
                catch (Exception ex)
                {
                    WriteToLog(1, string.Format("{0}{1}", Utilities.TimeStamp(), "Init(): Error reading config file. Runtime catched error " + ex.Message));
                    return false;
                }

                //check dbase connection
                if (dal.IsConnectionOK())
                {
                    WriteToLog(0, string.Format("{0}{1}", Utilities.TimeStamp(), "Init(): Connection to database is success"));
                    return true;
                }
                else
                {
                    WriteToLog(1, string.Format("{0}{1}", Utilities.TimeStamp(), "Init(): Connection to database failed. " + dal.ErrorMessage));
                    return false;
                }
                
            }
            catch (Exception ex)
            {
                WriteToLog(1, string.Format("{0}{1}", Utilities.TimeStamp(), "Init(): Runtime catched error " + ex.Message));
                return false;
            }

            return true;
        }

        private static int IsProgramRunning(string Program)
        {
            System.Diagnostics.Process[] p;
            p = System.Diagnostics.Process.GetProcessesByName(Program.Replace(".exe", "").Replace(".EXE", ""));

            return p.Length;
        }

        static T CastTo<T>(this object obj) { return (T)obj; }

        static string GetFileName(Process process, SubProcess subProcess)
        {
            switch(process)
            {
                case Process.SFTPDetails:
                    switch (subProcess)
                    {
                        case SubProcess.AllPrevious:
                        case SubProcess.Merge:
                            return "sftpDtl_allprevious.json";
                        case SubProcess.Pending:
                            return "sftpDtl_pending.json";                                
                        default:
                            return "sftpDtl_today.json";                           
                    }                  
                default:
                    switch (subProcess)
                    {
                        case SubProcess.Weekly:
                            return "capturedData_weekly.json";                            
                        case SubProcess.AllPrevious:
                        case SubProcess.Merge:
                            return "capturedData_allprevious.json";                            
                        default:
                            return "capturedData_today.json";                            
                    }                   
            }
        }

        private static void MergeCapturedDataFiles(Process process, SubProcess subProcess)
        {
            List<CapturedDataSummaryModel> capturedDatasummary1 = JsonConvert.DeserializeObject<List<CapturedDataSummaryModel>>(File.ReadAllText(Path.Combine(config.JsonRepo, "capturedData_allprevious.json")));
            List<CapturedDataSummaryModel> capturedDatasummary2 = JsonConvert.DeserializeObject<List<CapturedDataSummaryModel>>(File.ReadAllText(Path.Combine(config.JsonRepo, "capturedData_today.json")));

            foreach (CapturedDataSummaryModel cdsm in capturedDatasummary2)
            {
                CapturedDataSummaryModel _cdsm = new CapturedDataSummaryModel();
                _cdsm.CapturedDate = cdsm.CapturedDate;
                _cdsm.DbaseQty = cdsm.DbaseQty;
                _cdsm.FolderDoneQty = cdsm.FolderDoneQty;
                _cdsm.SFTPTransferredTxt = cdsm.SFTPTransferredTxt;
                _cdsm.SFTPTransferredZip = cdsm.SFTPTransferredZip;
                capturedDatasummary1.Add(_cdsm);
            }

            string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(capturedDatasummary1);            
           
            System.IO.File.WriteAllText(Path.Combine(config.JsonRepo, GetFileName(process, subProcess)), jsonData);
        }

        private static void MergeSFTPDetailsFiles(Process process, SubProcess subProcess)
        {
            List<SFTPDtlModel> sftpDetail1 = JsonConvert.DeserializeObject<List<SFTPDtlModel>>(File.ReadAllText(Path.Combine(config.JsonRepo, "sftpDtl_allprevious.json")));
            List<SFTPDtlModel> sftpDetail2 = JsonConvert.DeserializeObject<List<SFTPDtlModel>>(File.ReadAllText(Path.Combine(config.JsonRepo, "sftpDtl_today.json")));

            foreach (SFTPDtlModel sftpDtl in sftpDetail1)
            {
                SFTPDtlModel _sftpDtl = new SFTPDtlModel();
                _sftpDtl.RefNUm = sftpDtl.RefNUm;
                _sftpDtl.PagIBIGID = sftpDtl.PagIBIGID;
                _sftpDtl.GUID = sftpDtl.GUID;
                _sftpDtl.Remark = sftpDtl.Remark;
                _sftpDtl.PagIbigMemConsoDate = sftpDtl.PagIbigMemConsoDate;
                _sftpDtl.ZipProcessDate = sftpDtl.ZipProcessDate;
                _sftpDtl.SFTPTransferDate = sftpDtl.SFTPTransferDate;
                _sftpDtl.DatePosted = sftpDtl.DatePosted;
                _sftpDtl.TimePosted = sftpDtl.TimePosted;
                sftpDetail1.Add(_sftpDtl);
            }

            string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(sftpDetail1);
          
            System.IO.File.WriteAllText(Path.Combine(config.JsonRepo, GetFileName(process, subProcess)), jsonData);
        }

        private static void WriteToLog(short type, string desc)
        {
            Console.WriteLine(desc);
            if (type == 0) Utilities.SaveToSystemLog(string.Format("[{0}] {1}", APP_NAME, desc));
            if (type == 1) Utilities.SaveToErrorLog(string.Format("[{0}] {1}", APP_NAME, desc));
        }

    }
}
