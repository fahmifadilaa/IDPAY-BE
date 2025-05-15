using Ekr.Core.Constant;
using System;

namespace Ekr.Core.Helper
{
    public static class GetEnvironment
    {
        public static EnvironmentSys Hosting()
        {
            return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") switch
            {
                "Development" => EnvironmentSys.DEVELOPMENT,
                "Staging" => EnvironmentSys.STAGING,
                "Production" => EnvironmentSys.PRODUCTION,
                _ => throw new NotImplementedException()
            };
        }
    }
}
