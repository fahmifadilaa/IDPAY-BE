using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.Logging
{
    public class Tbl_LogError_FaceRecognition
    {
        public int id { get; set; }
        public string payload { get; set; }
        public string response { get; set; }
        public DateTime createdTime { get; set; }

    }
}
