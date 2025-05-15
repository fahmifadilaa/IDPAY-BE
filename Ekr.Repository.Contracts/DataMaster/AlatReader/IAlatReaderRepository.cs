using Ekr.Core.Entities;
using Ekr.Core.Entities.DataMaster.AlatReader;
using Ekr.Core.Entities.DataMaster.DataReader.Entity;
using Ekr.Core.Entities.DataMaster.Utility.ViewModel;
using Ekr.Core.Entities.Logging;
using System.Threading.Tasks;

namespace Ekr.Repository.Contracts.DataMaster.AlatReader
{
    public interface IAlatReaderRepository
    {
        Task<GridResponse<TblMasterAlatReaderVM>> GridGetAll(MasterAlatReaderFilter req);
        Task<TblMasterAlatReaderVM> GetDataByUID(string uid);
        Task<TblMasterAlatReaderUser> GetAlatMasterReaderUserByUID(string uid);
        Task<GridResponse<Tbl_VersionAgentVM>> GridGetAllVersionApps(AppsVersionRequestFilter req);
        Task<Tbl_VersionAgent> GetVersionById(int Id);
        Task<TblMasterAlatReaderUser> GetAlatMasterReaderUserByUIDPegawaiId(string uid, string npp);
        Task<TblMasterAlatReaderUser> GetAlatMasterReaderUserByUIDPegawaiIdMax(string uid, string npp);
        Task<ReqCreateMasterAlatReader> CreateAlatMasterReader(ReqCreateMasterAlatReader req);
        Task<ReqCreateLogActivity> CreateLogActivity(ReqCreateLogActivity req);
        Task<ReqCreateMasterAlatReaderLogError> CreateLogError(ReqCreateMasterAlatReaderLogError req);
        Task<Tbl_MasterAlatReaderLogActvity> CreateLogActivity2(Tbl_MasterAlatReaderLogActvity req);
        Task<int> InsertLogEnrollThirdParty(Tbl_Enrollment_ThirdParty_Log logActivity);
        Task<ReqMasterAlatReaderGetByUid> CreateAlatMasterReaderUser(ReqMasterAlatReaderGetByUid req);
        Task<ReqCreateLogConnection> CreateAlatMasterReaderLogConnection(ReqCreateLogConnection req);
        Task<bool> UpdateAlatMasterReaderUserByUID(string uid);
        Task<bool> UpdateAlatMasterReaderUserByID(int id);
        //Task<bool> UpdateAlatMasterReaderUserByUID(string uid);
        Task<ReqCreateMasterAlatReader> UpdateAlatMasterReader(ReqCreateMasterAlatReader req);
        Task<UpdateStatusManifestAlatReader> UpdateStatusManifestAlatReader(UpdateStatusManifestAlatReader req, int idPegawai);
        Task<ReqCreateMasterAlatReader> UpdateAlatMasterReaderFromLogActivity(ReqCreateMasterAlatReader req);
        long CreateAlatReaderLogActivity(Tbl_MasterAlatReaderLogActvity log);
        long CreateAlatReaderLogActivityNew(Tbl_MasterAlatReaderLogActvity logActivity);
        Task<Tbl_MasterAlatReaderLog> CreateAlatReaderLog(Tbl_MasterAlatReaderLog log);
        Task<ReqUpdateMasterAlatReader> UpdateStatusAlatMasterReader(ReqUpdateMasterAlatReader req);
        Task<string> DeleteApps(int Id, string updateBy);
    }
}