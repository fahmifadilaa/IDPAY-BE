using ServiceStack.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ekr.Core.Entities.Token
{
    public class Tbl_Jwt_Repository
    {
        [AutoIncrement]
        [Column(TypeName = "bigint")]
        public long Id { get; set; }
        [Column(TypeName = "varchar(150)")]
        public string UserCode { get; set; }
        [Column(TypeName = "varchar(150)")]
        public string ClientId { get; set; }
        [Column(TypeName = "varchar(150)")]
        public string ClientIp { get; set; }
        [Column(TypeName = "varchar(250)")]
        public string RefreshToken { get; set; }
        public bool IsExpired { get; set; }
        [Column(TypeName = "varchar(250)")]
        public string TokenId { get; set; }
        [Column(TypeName = "varchar(250)")]
        public string Token { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public DateTime? UpdatedTime { get; set; }
        public int ChannelTokenId { get; set; }
    }
}
