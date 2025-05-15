
using ConsoleWriteLog.Tools;
using ConsoleWriteLog.ViewModels;
using Dapper;
using Ekr.Core.Constant;
using Ekr.Core.Helper;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ConsoleWriteLog
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static void Main(string[] args)
        {
            try
            {
                var version = Config.AppSetting["Version"];

                log.Info(String.Format("Version {0} Started", version));

                var Conn = Config.AppSetting["ConnectionStrings:dbConnectionLog"];

                var _last = new LogTemp();

                using (DbConnection connection = new SqlConnection(Conn))
                {
                    connection.Open();

                    var rawquery = "SELECT TOP 1 ISNULL([IdLastLogActivity], 0) as IdLastLogActivity, " +
                                    "ISNULL([IdLastLogError], 0) as IdLastLogError " +
                                    "FROM [dbo].[Tbl_IdLog_Temp] " +
                                    "ORDER BY ID DESC;";

                    _last = connection.Query<LogTemp>(rawquery).FirstOrDefault();

                    connection.Close();
                    connection.Dispose();
                }

                var listDataActivity = new List<TblLogActivity>();

                var listDataError = new List<Tbl_LogError>();

                using (DbConnection connection = new SqlConnection(Conn))
                {
                    connection.Open();

                    var rawquery = String.Format("EXEC [dbo].[ProcConvertLogActivity] @IdLast = {0}", _last == null ? "0" : _last.IdLastLogActivity.ToString());
                    listDataActivity = connection.Query<TblLogActivity>(rawquery).ToList();

                    connection.Close();
                    connection.Dispose();
                }

                using (DbConnection connection = new SqlConnection(Conn))
                {
                    connection.Open();

                    var rawquery = String.Format("EXEC [dbo].[ProcConvertLogError] @IdLast = {0}", _last == null ? "0" : _last.IdLastLogError.ToString());
                    listDataError = connection.Query<Tbl_LogError>(rawquery).ToList();

                    connection.Close();
                    connection.Dispose();
                }

                if (listDataActivity.Count == 0 && listDataError.Count == 0)
                {
                    log.Info(String.Format("{0} data found, ending conversion proses", listDataActivity.Count));
                }
                else
                {
                    var _lastId = listDataActivity[^1];
                    var _lastIdError = listDataError[^1];

                    using (DbConnection connection = new SqlConnection(Conn))
                    {
                        connection.Open();

                        var rawquery = String.Format("EXEC [dbo].[ProcIdLogTemp] @IdLogActivity = {0}, @IdLogError = {1}", _lastId.Id, _lastIdError.Id);
                        var _ = connection.Query<TblLogActivity>(rawquery).FirstOrDefault();

                        connection.Close();
                        connection.Dispose();
                    }

                    var logConfigIsActive = Config.AppSetting["LogConfig:IsLogActive"];
                    var PathLogActivity = Config.AppSetting["LogConfig:PathLogActivity"];
                    var FileNameActivity = Config.AppSetting["LogConfig:FileNameActivity"];
                    var PathLogError = Config.AppSetting["LogConfig:PathLogError"];
                    var FileNameError = Config.AppSetting["LogConfig:FileNameError"];

                    if (bool.Parse(logConfigIsActive))
                    {
                        if(listDataActivity.Count > 0) {

                            foreach (var item in listDataActivity)
                            {
                                var sb = new StringBuilder(DateTime.Now.ToString("G") + " [RequestInformation] :");
                                sb.AppendLine(JsonConvert.SerializeObject(item));

                                FileHelper.WriteOrReplaceFileContentOrCreateNewFile(sb.ToString(), PathLogActivity, FileNameActivity, "logs", TimingInterval.DAILY);
                            }
                        }
                        if(listDataError.Count> 0) {

                            foreach (var item in listDataError)
                            {
                                var sb = new StringBuilder(DateTime.Now.ToString("G") + " [ErrorInformation] :");
                                sb.AppendLine(JsonConvert.SerializeObject(item));

                                FileHelper.WriteOrReplaceFileContentOrCreateNewFile(sb.ToString(), PathLogError, FileNameError, "logs", TimingInterval.DAILY);
                            }
                        }
                    }

                    log.Info(String.Format("{0} Activity, {1} Error data found, ending conversion proses", listDataActivity.Count, listDataError.Count));
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }
    }
}
