using ServiceStack.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ekr.Core.Entities.Token
{
    public class TblJwtRepoitory
    {
        [AutoIncrement]
        public int Id { get; set; }
        [Column(TypeName = "varchar(150)")]
        public string UserCode { get; set; }
        [Column(TypeName = "varchar(150)")]
        public string ClientId { get; set; }
        [Column(TypeName = "varchar(150)")]
        public string ClientIp { get; set; }
        [Column(TypeName = "varchar(250)")]
        public string RefreshToken { get; set; }
        public bool IsExpired { get; set; }
        [Column(TypeName = "varchar(MAX)")]
        public string TokenId { get; set; }
    }
}
