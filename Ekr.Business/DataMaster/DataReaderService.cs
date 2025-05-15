using Ekr.Business.Contracts.DataMaster;
using Ekr.Core.Entities.DataMaster.DataReader.Entity;
using Ekr.Repository.Contracts.DataMaster.DataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Business.DataMaster
{
    public class DataReaderService : IDataReaderService
    {
        private readonly IDataReaderRepository _dataReaderRepository;
        private readonly ILogger<DataReaderService> _logger;

        public DataReaderService(IDataReaderRepository dataReaderRepository, ILogger<DataReaderService> logger)
        {
            _dataReaderRepository = dataReaderRepository;
            _logger = logger;
        }


        public async Task<(string msg, bool status)> ExcelBulkInsert(IFormFile file, int PegawaiId)
        {
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploadDataReader");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            ISheet sheet;

            var uniqueFileName = Guid.NewGuid().ToString().Substring(0, 4) + "_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + "_" + Path.GetFileName(file.FileName);
            var filePath = Path.Combine(folderPath, uniqueFileName);
            string FileExt = Path.GetExtension(file.FileName).ToLower();

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
                stream.Position = 0;
                if (FileExt == ".xls")
                {
                    HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
                    sheet = hssfwb.GetSheetAt(0);
                }
                else if (FileExt == ".xlsx")
                {
                    XSSFWorkbook hssfwb = new XSSFWorkbook(stream);
                    sheet = hssfwb.GetSheetAt(0);
                }
                else
                {
                    return ("format invalid", false);
                }
                if (sheet.LastRowNum <= 1)
                {
                    return ("excel blank", false);
                }
            }

            var listdata = new List<Tbl_MasterAlatReader>();

            for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
            {
                try
                {
                    IRow row = sheet.GetRow(i);
                    if (row == null) continue;
                    if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;

                    var data = new Tbl_MasterAlatReader();

                    if (row.GetCell(0) != null)
                    {
                        data.UID = row.GetCell(0).ToString();
                    }
                    if (row.GetCell(1) != null)
                    {
                        data.SN_Unit = row.GetCell(1).ToString();
                    }
                    if (row.GetCell(2) != null)
                    {
                        data.No_Kartu = row.GetCell(2).ToString();
                    }
                    if (row.GetCell(3) != null)
                    {
                        data.No_Perso_SAM = row.GetCell(3).ToString();
                    }
                    if (row.GetCell(4) != null)
                    {
                        data.PCID = row.GetCell(4).ToString();
                    }
                    if (row.GetCell(5) != null)
                    {
                        data.Confiq = row.GetCell(5).ToString();
                    }

                    data.IsDeleted = false;
                    data.CreatedBy_Id = PegawaiId;
                    data.CreatedTime = DateTime.Now;
                    data.IsActive = true;

                    listdata.Add(data);
                }
                catch (Exception ex)
                {
                    _logger.LogError("[ErrorDataReaderService] : " + ex.Message, ex);
                }
                
            }

            var res = _dataReaderRepository.ExcelBulkInsert(listdata);
            return (
                res == true ? "SUKSES":"GAGAL",
                res
                );
        }
    }
}
