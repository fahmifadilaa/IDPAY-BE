using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.Utility.ViewModel
{
    public class UserSessionLogVM
    {
        public int UserId { get; set; }
        public string Npp { get; set; }
        public string UnitId { get; set; }
        public string RoleId { get; set; }
        public string IpAddress { get; set; }
        public string SessionId { get; set; }
        public string Url { get; set; }
        public string Browser { get; set; }
        public string Os { get; set; }
        public string ClientInfo { get; set; }
    }
    public class UserSessionLogFilterVM
    {
        public string IpAddress { get; set; }
        public string SessionId { get; set; }
        public string Url { get; set; }
        public string Browser { get; set; }
        public string Os { get; set; }
        public string ClientInfo { get; set; }
    }
}
