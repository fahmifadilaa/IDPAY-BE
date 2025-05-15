using Ekr.Core.Entities.DataEnrollment.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Repository.Contracts.DataEnrollment
{
    public interface IDashboardReaderRepository
    {
        Task<IEnumerable<MonitoringReaderDataVM>> GetMonitoringReaderChart(string UnitIds);
        Task<IEnumerable<PresentaseReaderDigunakanVM>> GetPresentaseAlatDigunakanChart(string UnitIds);
    }
}
