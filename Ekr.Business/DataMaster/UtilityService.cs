using Ekr.Business.Contracts.DataMaster;
using Ekr.Core.Entities.DataMaster.Utility.Entity;
using Ekr.Core.Entities.DataMaster.Utility.ViewModel;
using Ekr.Repository.Contracts.DataMaster.Utility;
using System;

namespace Ekr.Business.DataMaster
{
    public class UtilityService : IUtilityService
    {
        private readonly IUtilityRepository _utilityRepository;
        private readonly IUtility1Repository _utility1Repository;

        public UtilityService(IUtilityRepository utilityRepository, IUtility1Repository utility1Repository)
        {
            _utilityRepository = utilityRepository;
            _utility1Repository = utility1Repository;
        }
        public bool UpdateSessionLog(UserSessionLogVM userSessionLogVM)
        {
            bool isSuccess = false;
            TblUserSession ds = _utilityRepository.GetUserSession(userSessionLogVM.UserId);
            if (ds != null)
            {
                ds.SessionId = userSessionLogVM.SessionId;
                ds.LastActive = DateTime.Now;
                if (userSessionLogVM.RoleId != null && userSessionLogVM.RoleId != "")
                {
                    ds.RoleId = int.Parse(userSessionLogVM.RoleId);
                }
                if (userSessionLogVM.UnitId != null && userSessionLogVM.UnitId != "")
                {
                    ds.UnitId = int.Parse(userSessionLogVM.UnitId);
                }
                ds.Info = userSessionLogVM.IpAddress;

                isSuccess = _utilityRepository.UpdateUserSession(ds);
            }
            else
            {
                ds = new TblUserSession();
                ds.UserId = userSessionLogVM.UserId;
                ds.SessionId = userSessionLogVM.SessionId;
                ds.LastActive = DateTime.Now;
                ds.Info = userSessionLogVM.IpAddress;
                if (userSessionLogVM.RoleId != null && userSessionLogVM.RoleId != "")
                {
                    ds.RoleId = int.Parse(userSessionLogVM.RoleId);
                }

                if (userSessionLogVM.UnitId != null && userSessionLogVM.UnitId != "")
                {
                    ds.UnitId = int.Parse(userSessionLogVM.UnitId);

                }

                isSuccess = _utilityRepository.InsertUserSession(ds);
            }

            //Masukkan ke dalam table Log Activity
            TblLogActivity dataLog = new TblLogActivity();
            dataLog.UserId = userSessionLogVM.UserId;
            dataLog.Npp = userSessionLogVM.Npp;
            dataLog.Url = userSessionLogVM.Url;
            dataLog.ActionTime = DateTime.Now;
            dataLog.Browser = userSessionLogVM.Browser;
            dataLog.Os = userSessionLogVM.Os;
            dataLog.Ip = userSessionLogVM.IpAddress;
            dataLog.ClientInfo = userSessionLogVM.ClientInfo;

            isSuccess = _utility1Repository.InsertLogActivity(dataLog);

            return isSuccess;
        }
    }
}
