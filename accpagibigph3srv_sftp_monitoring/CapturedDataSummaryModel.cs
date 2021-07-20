using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace accpagibigph3srv_sftp_monitoring
{
    class CapturedDataSummaryModel
    {
        public DateTime CapturedDate { get; set; }
        public int DbaseQty { get; set; }
        public int FolderDoneQty { get; set; }
        public int SFTPTransferredTxt { get; set; }
        public int SFTPTransferredZip { get; set; }

        public CapturedDataSummaryModel()
        {
            DbaseQty = 0;
            DbaseQty = 0;
            SFTPTransferredTxt = 0;
            SFTPTransferredZip = 0;
        }
    }
}
