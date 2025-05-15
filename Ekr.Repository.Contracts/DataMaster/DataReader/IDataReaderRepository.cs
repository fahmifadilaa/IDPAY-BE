using Ekr.Core.Entities;
using Ekr.Core.Entities.DataMaster.AlatReader;
using Ekr.Core.Entities.DataMaster.DataReader.Entity;
using Ekr.Core.Entities.DataMaster.DataReader.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ekr.Repository.Contracts.DataMaster.DataReader
{
    public interface IDataReaderRepository
    {
        Task<GridResponse<MasterAlatReaderVM>> LoadData(DataReaderFilterVM req, int unitId, int roleId);
        Task<GridResponse<MasterAlatReaderVM>> LoadDataWithCondition(DataReaderConditionFilterVM req, int unitId, int roleId);
        Task<TblMasterAlatReaderVM> GetDataReader(DataReaderViewFilterVM req);
        Task<DashboardDetailReaderVM> GetDetailDataReader(DataReaderDetailFilter req);
        Task<MonitoringReaderExcelVM> GetDetailAlatDataReader(DataReaderDetailAlatFilter req);
        Task<DashboardDetailReaderVM> GetDetailDataReaderByUID(string UID);
        Task<Tbl_MasterAlatReader> InsertDataReader(Tbl_MasterAlatReader req);
        Task<Tbl_MasterAlatReader> UpdateDataReader(Tbl_MasterAlatReader req);
        Task DeleteDataReader(DataReaderViewFilterVM req, int PegawaiId);
        Task<GridResponse<MasterAlatReaderLogActivityVM>> LoadDataLogActivity(LogActivityDataReaderFilterVM req);
        Task<GridResponse<MasterAlatReaderLogConnectionVM>> LoadDataLogConnection(LogConnectionDataReaderFilterVM req);
        Task<GridResponse<MasterAlatReaderLogUserVM2>> LoadDataLogUser(LogUserDataReaderFilterVM req);
        Task<Tbl_MasterAlatReader> GetDatareaderUid(string uid);
        Task<Tbl_MasterAlatReader> GetDatareaderBySN(string sn);
        bool ExcelBulkInsert(List<Tbl_MasterAlatReader> req);
        Task<int> GetCountJumlahDataReader(string UnitIds);
    }
}
