using Ekr.Business.Contracts.DataMaster;
using Ekr.Core.Entities.DataMaster.User;
using Ekr.Repository.Contracts.DataMaster.MasterAplikasi;
using System.Collections.Generic;
using System.Linq;

namespace Ekr.Business.DataMaster
{
    public class UserDataService : IUserDataService
    {
        private readonly IMasterAplikasiRepository _masterAplikasiRepository;

        public UserDataService(IMasterAplikasiRepository masterAplikasiRepository)
        {
            _masterAplikasiRepository = masterAplikasiRepository;
        }

        public List<RoleUserVM> GetModelRole(string ListDataRoles)
        {
            int no = 0;
            List<RoleUserVM> AllData = new List<RoleUserVM>();
            List<string> SplitAllData = ListDataRoles.Split('~').ToList();
            if (SplitAllData.Count > 0 && ListDataRoles != "")
            {
                foreach (var item in SplitAllData)
                {
                    no++;
                    string[] SplitDataRole = item.Split('|');
                    var SplitApp = SplitDataRole[6].Split(',');
                    foreach (var i in _masterAplikasiRepository.GetMasterAplikasiByIds(SplitApp.ToList()))
                    {
                        RoleUserVM DataRole = new();
                        DataRole.Id = no;
                        DataRole.Role_Id = int.Parse(SplitDataRole[0]);
                        DataRole.Role_Name = SplitDataRole[1];
                        DataRole.RoleDivisiId = int.Parse(SplitDataRole[2]);
                        DataRole.Unit_Name = SplitDataRole[3];
                        DataRole.Tanggal = SplitDataRole[4];
                        DataRole.Status_Role_Name = SplitDataRole[5] == "1" ? "PJB" : "PGS";
                        DataRole.AppName = i;
                        AllData.Add(DataRole);
                    }
                }
            }

            return AllData;

        }
    }
}
