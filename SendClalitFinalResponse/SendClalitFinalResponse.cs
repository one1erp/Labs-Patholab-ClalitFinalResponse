using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using System.Diagnostics;//for debugger :)

using LSEXT;
using LSSERVICEPROVIDERLib;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;


using ADODB;


using Patholab_Common;
using Patholab_DAL_V1;

namespace SendClalitFinalResponse
{

    [ComVisible(true)]
    [ProgId("SendClalitFinalResponse.SendClalitFinalResponseCls")]
    public class SendClalitFinalResponseCls : IWorkflowExtension
    {
        INautilusServiceProvider sp;
        private DataLayer dal;
        private const string Type = "1";
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern SafeFileHandle CreateFile(string lpFileName, FileAccess dwDesiredAccess,
        uint dwShareMode, IntPtr lpSecurityAttributes, FileMode dwCreationDisposition,
        uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        public void Execute(ref LSExtensionParameters Parameters)
        {
            try
            {

                #region params

                string tableName = Parameters["TABLE_NAME"];

                sp = Parameters["SERVICE_PROVIDER"];
                var rs = Parameters["RECORDS"];



                //   Debugger.Launch();
                //Recordset rs = Parameters["RECORDS"];
                string firstSDG = rs["SDG_ID"].Value.ToString();
                rs.MoveLast();
                string tableID = rs.Fields["SDG_ID"].Value.ToString();
                string workstationId = Parameters["WORKSTATION_ID"].ToString();




                #endregion
                ////////////יוצר קונקשן//////////////////////////
                var ntlCon = Utils.GetNtlsCon(sp);
                Utils.CreateConstring(ntlCon);
                /////////////////////////////           
                dal = new DataLayer();
                dal.Connect(ntlCon);
                var sampleID = "";
                var sampleName = "";
                var sampleDscription = "";
                long sdgId = long.Parse(tableID);

                if (tableName == "SDG")
                {
                    SDG sdg = dal.FindBy<SDG>(d => d.SDG_ID == sdgId).SingleOrDefault();
                    string pdfFileName = sdg.SDG_USER.U_PDF_PATH;

                    string XmlDir;
                    PHRASE_HEADER SystemParams = dal.GetPhraseByName("System Parameters");
                    SystemParams.PhraseEntriesDictonary.TryGetValue("XML Directory", out XmlDir);
                    string outputXmlName = Path.GetDirectoryName(XmlDir)+@"\MDR_404_18_"+sdg.SDG_ID+"_"+dal.GetSysdate().ToString("yyyyMMddHHmmss")+"_"+".XML";

                    var xmlReport = new FinalResponse(dal, sdg, pdfFileName, outputXmlName);
                    xmlReport.GenerateFile();

                }
                else
                {
                    MessageBox.Show("זה לא סדג");

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("נכשלה שליחת התוצאה");
                Logger.WriteLogFile(ex);
            }
        }


    }



}
