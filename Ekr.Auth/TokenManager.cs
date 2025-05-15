using Ekr.Core.Entities.Token;
using System;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace Ekr.Auth
{
    public static class TokenManager
    {
        public static PrincipalAgent GetPrincipalAgent(string token)
        {
            //string authorization = _context.HttpContext.Request.Headers["Authorization"];
            if (token == null) return null;

            var tokenHandler = new JwtSecurityTokenHandler();

            var parsedToken = tokenHandler.ReadJwtToken(token);

            var Name = parsedToken.Claims.FirstOrDefault(c => c.Type == "Name").Value;

            return new PrincipalAgent
            {
                Name = Name
            };
        }

        public static Principal GetPrincipal(string token)
        {
            //string authorization = _context.HttpContext.Request.Headers["Authorization"];
            if (token == null) return null;

            var tokenHandler = new JwtSecurityTokenHandler();

            var parsedToken = tokenHandler.ReadJwtToken(token);

            var NamaPegawai = parsedToken.Claims.FirstOrDefault(c => c.Type == "NamaPegawai").Value;
            var NIK = parsedToken.Claims.FirstOrDefault(c => c.Type == "NIK").Value;
            var PegawaiId = parsedToken.Claims.FirstOrDefault(c => c.Type == "PegawaiId").Value;
            var UnitId = parsedToken.Claims.FirstOrDefault(c => c.Type == "UnitId").Value;
            var NamaUnit = parsedToken.Claims.FirstOrDefault(c => c.Type == "NamaUnit").Value;
            var UserId = parsedToken.Claims.FirstOrDefault(c => c.Type == "UserId").Value;
            var RoleId = parsedToken.Claims.FirstOrDefault(c => c.Type == "RoleId").Value;
            var RoleUnitId = parsedToken.Claims.FirstOrDefault(c => c.Type == "RoleUnitId").Value;
            var RoleNamaUnit = parsedToken.Claims.FirstOrDefault(c => c.Type == "RoleNamaUnit").Value;
            var NamaRole = parsedToken.Claims.FirstOrDefault(c => c.Type == "NamaRole").Value;
            var ImagesUser = parsedToken.Claims.FirstOrDefault(c => c.Type == "ImagesUser").Value;
            var StatusRole = parsedToken.Claims.FirstOrDefault(c => c.Type == "StatusRole").Value;
            var UserRoleId = parsedToken.Claims.FirstOrDefault(c => c.Type == "UserRoleId").Value;
            var ApplicationId = parsedToken.Claims.FirstOrDefault(c => c.Type == "ApplicationId").Value;
            var KodeUnit = parsedToken.Claims.FirstOrDefault(c => c.Type == "KodeUnit").Value;

            return new Principal
            {
                ApplicationId = ApplicationId,
                ImagesUser = NamaPegawai,
                NamaPegawai = NamaPegawai,
                NamaRole = NamaRole,
                NamaUnit = NamaUnit,
                NIK = NIK,
                PegawaiId = PegawaiId,
                RoleId = RoleId,
                RoleNamaUnit = RoleNamaUnit,
                RoleUnitId = RoleUnitId,
                StatusRole = StatusRole,
                UnitId = UnitId,
                UserId = UserId,
                UserRoleId = UserRoleId,
                KodeUnit = KodeUnit
            };

        }

        public static PrincipalThirdParty GetPrincipalThirdParty(string token)
        {
            //string authorization = _context.HttpContext.Request.Headers["Authorization"];
            if (token == null) return null;

            var tokenHandler = new JwtSecurityTokenHandler();

            var parsedToken = tokenHandler.ReadJwtToken(token);

            var NamaChannel = parsedToken.Claims.FirstOrDefault(c => c.Type == "Name").Value;
            var TimeIssued = parsedToken.Claims.FirstOrDefault(c => c.Type == "iat").Value;
            var TimeExpired = parsedToken.Claims.FirstOrDefault(c => c.Type == "exp").Value;

            DateTimeOffset dateTimeIssued = DateTimeOffset.FromUnixTimeSeconds(Int64.Parse (TimeIssued));
            DateTimeOffset dateTimeExpired = DateTimeOffset.FromUnixTimeSeconds(Int64.Parse(TimeExpired));

            var issueTime = dateTimeIssued.ToLocalTime();
            var expTime = dateTimeExpired.ToLocalTime();
            
            return new PrincipalThirdParty
            {
                Name = NamaChannel,
                IssuedTime = issueTime,
                ExpTime = expTime
            };

        }
    }
}
