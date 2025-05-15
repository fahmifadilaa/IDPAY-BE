using Ekr.Core.Entities.DataMaster.User;
using System.Collections.Generic;

namespace Ekr.Business.Contracts.DataMaster
{
    public interface IUserDataService
    {
        List<RoleUserVM> GetModelRole(string ListDataRoles);
    }
}
