using System;

namespace Ekr.Core.Entities.Token
{
    public class Principal
    {
        public string NamaPegawai { get; set; }
        public string NIK { get; set; }
        public string PegawaiId { get; set; }
        public string UnitId { get; set; }
        public string NamaUnit { get; set; }
        public string UserId { get; set; }
        public string RoleId { get; set; }
        public string RoleUnitId { get; set; }
        public string RoleNamaUnit { get; set; }
        public string NamaRole { get; set; }
        public string ImagesUser { get; set; }
        public string StatusRole { get; set; }
        public string UserRoleId { get; set; }
        public string ApplicationId { get; set; }
        public string KodeUnit { get; set; }

    }

    public class PrincipalAgent
    {
        public string Name { get; set; }
    }

    public class PrincipalThirdParty { 
        public string Name { get; set;}
        public DateTimeOffset IssuedTime { get; set; }
        public DateTimeOffset ExpTime { get; set;}
    }
}
