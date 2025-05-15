using Ekr.Core.Entities;
using Ekr.Core.Entities.DataMaster.DataReader;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Business.Contracts.DataMaster
{
    public interface IDataReaderService
    {
        Task<(string msg, bool status)> ExcelBulkInsert(IFormFile file, int PegawaiId);
    }
}
