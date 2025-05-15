using Dapper;
using Ekr.Core.Entities.Logging;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Repository.Contracts.DataMaster.Utility;
using System;

namespace Ekr.Repository.DataMaster.Utility
{
    public class Utility1Repository : BaseRepository, IUtility1Repository
    {
        public Utility1Repository(IEKtpReaderBackendDb2 con) : base(con) { }

        public bool InsertLogActivity(Core.Entities.DataMaster.Utility.Entity.TblLogActivity logActivity)
        {
            const string query = "Insert Into [Tbl_LogActivity] (" +
                    "UserId, " +
                    "Unit_Id, " +
                    "Npp, " +
                    "Url, " +
                    "DataLama, " +
                    "DataBaru, " +
                    "ActionTime, " +
                    "Browser, " +
                    "IP, " +
                    "OS, " +
                    "ClientInfo, " +
                    "Keterangan) " +
                "values(" +
                    "@UserId, " +
                    "@UnitId, " +
                    "@Npp, " +
                    "@Url, " +
                    "@DataLama, " +
                    "@DataBaru, " +
                    "@ActionTime, " +
                    "@Browser, " +
                    "@Ip, " +
                    "@Os, " +
                    "@ClientInfo, " +
                    "@Keterangan) ";

            var insert = Db.WithConnection(c => c.ExecuteScalar<int>(query, new
            {
                logActivity.UserId,
                logActivity.UnitId,
                Npp = new DbString
                {
                    Value = logActivity.Npp,
                    Length = 150
                },
                logActivity.Url,
                logActivity.DataLama,
                logActivity.DataBaru,
                logActivity.ActionTime,
                Browser = new DbString
                {
                    Value = logActivity.Browser,
                    Length = 250
                },
                Ip = new DbString
                {
                    Value = logActivity.Ip,
                    Length = 150
                },
                Os = new DbString
                {
                    Value = logActivity.Os,
                    Length = 150
                },
                logActivity.ClientInfo,
                logActivity.Keterangan
            }));

            return (insert == 0);
        }

        public bool InsertLogEnrollThirdParty(Tbl_Enrollment_ThirdParty_Log logActivity)
        {
            try {
                const string query = "Insert Into [Tbl_Enrollment_ThirdParty_Log] (" +
                    "NIK, " +
                    "AppsChannel, " +
                    "SubmitDate) " +
                "values(" +
                    "@NIK, " +
                    "@AppsChannel, " +
                    "@SubmitDate) ";

                var insert = Db.WithConnection(c => c.ExecuteScalar<int>(query, new
                {
                    logActivity.NIK,
                    logActivity.AppsChannel,
                    logActivity.SubmitDate
                }));
                return (insert == 0);


            }
            catch (Exception Ex) {
                return false;
            }


        }
    }
}
