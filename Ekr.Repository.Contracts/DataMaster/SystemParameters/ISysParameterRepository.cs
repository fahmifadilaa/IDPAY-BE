using Ekr.Core.Entities.DataMaster.SystemParameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Repository.Contracts.DataMaster.SystemParameters
{
    public interface ISysParameterRepository
    {
        Task<TblSystemParameter> GetPathFolder(string KataKunci);
    }
}
