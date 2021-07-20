using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace accpagibigph3srv_sftp_monitoring
{
    class SFTPDtlModel
    {
        public string RefNUm { get; set; }
        public string PagIBIGID { get; set; }
        public string GUID { get; set; }
        public string Type { get; set; }
        public string Remark { get; set; }
        public DateTime? PagIbigMemConsoDate { get; set; }
        public DateTime? ZipProcessDate { get; set; }
        public DateTime? SFTPTransferDate { get; set; }
        public DateTime DatePosted { get; set; }
        public TimeSpan TimePosted { get; set; }
    }
}
