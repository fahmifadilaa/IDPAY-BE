using Ekr.Auth;
using Ekr.Auth.Contracts;
using Ekr.Business.Contracts.DataKTP;
using Ekr.Business.DataKTP;
using Ekr.Business.BanchlinkCifUpdate;
using Ekr.Business.Contracts.BanchlinkCifUpdate;
using Ekr.Core.Configuration;
using Ekr.Core.Helper;
using Ekr.Core.Services;
using Ekr.Dapper.Connection.Base;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Dapper.Connection.Sql;
using Ekr.Repository;
using Ekr.Repository.Auth;
using Ekr.Repository.Contracts.Auth;
using Ekr.Repository.Contracts.DataKTP;
using Ekr.Repository.Contracts.DataMaster.DataReader;
using Ekr.Repository.Contracts.DataMaster.Lookup;
using Ekr.Repository.Contracts.DataMaster.AgeSegmentation;
using Ekr.Repository.Contracts.DataMaster.Unit;
using Ekr.Repository.Contracts.Recognition;
using Ekr.Repository.Contracts.Token;
using Ekr.Repository.DataKTP;
using Ekr.Repository.DataMaster.DataReader;
using Ekr.Repository.DataMaster.Lookup;
using Ekr.Repository.DataMaster.Segmentation;
using Ekr.Repository.DataMaster.Unit;
using Ekr.Repository.Recognition;
using Ekr.Repository.Token;
using Ekr.Services.Contracts.Recognition;
using Ekr.Services.Recognition;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using Ekr.Services.Contracts.Account;
using Ekr.Services.Account;
using Ekr.Repository.Contracts.Enrollment;
using Ekr.Repository.Enrollment;
using Ekr.Business.Contracts.Enrollment;
using Ekr.Business.Enrollment;
using Ekr.Repository.Contracts.DataMaster.BornGeneration;
using Ekr.Repository.DataMaster.BornGeneration;
using Ekr.Repository.DataMaster.Menu;
using Ekr.Repository.Contracts.DataMaster.Menu;
using Ekr.Repository.Contracts.DataEnrollment;
using Ekr.Repository.DataEnrollment;
using Ekr.Repository.DataMaster.User;
using Ekr.Repository.Contracts.DataMaster.User;
using Ekr.Repository.DataMaster.SystemParameter;
using Ekr.Repository.Contracts.DataMaster.SystemParameters;
using Ekr.Repository.DataMaster.MasterAplikasi;
using Ekr.Repository.Contracts.DataMaster.MasterAplikasi;
using Ekr.Repository.Contracts.DataMaster.UserFinger;
using Ekr.Repository.Contracts.DataMaster.MasterTypeJari;
using Ekr.Repository.DataMaster.UserFinger;
using Ekr.Repository.DataMaster.MasterTypeJari;
using Ekr.Repository.Contracts.DataMaster.Utility;
using Ekr.Repository.DataMaster.Utility;
using Ekr.Repository.Contracts.DataMaster.AlatReader;
using Ekr.Repository.DataMaster.AlatReaders;
using Ekr.Repository.CekAlat;
using Ekr.Business.Contracts.DataMaster;
using Ekr.Business.DataMaster;
using Ekr.Business.Recognition;
using Ekr.Business.Contracts.Recognition;
using Ekr.Repository.Contracts.Logging;
using Ekr.Repository.Logging;
using Ekr.Business.Contracts.DataMigration;
using Ekr.Business.DataMigration;
using Ekr.Repository.Contracts.DataMaster.Pegawai;
using Ekr.Repository.DataMaster.Pegawai;
using Ekr.Business.Contracts.MappingNIKPegawai;
using Ekr.Business.MappingNIKPegawai;
using Ekr.Repository.Contracts.MappingNIK;
using Ekr.Repository.MappingNIKPegawai;
using Ekr.Repository.Contracts.Setting;
using Ekr.Repository.Setting;
using Ekr.Business.Contracts.SettingThreshold;
using Ekr.Business.SettingThreshold;
using Ekr.Repository.Contracts.EnrollmentNoMatching;
using Ekr.Repository.EnrolmentNoMatching;
using Ekr.Business.Contracts.EnrollmentNoMatching;
using Ekr.Business.EnrollmentNoMatching;
using Ekr.Repository.Contracts.DataMaster.MasterApps;
using Ekr.Repository.DataMaster.MasterApps;
using Ekr.Services.Contracts.IKD;
using Ekr.Services.IKD;
using Ekr.Repository.Contracts.DataMaster.MasterTreshold;
using Ekr.Repository.DataMaster.MasterTreshold;
using Ekr.Repository.Contracts.MessageCode;
using Ekr.Repository.MessageCode;
using Ekr.Repository.Contracts.BanchlinkCifUpdate;
using Ekr.Repository.BanchlinkCifUpdate;



namespace Ekr.Dependency
{
    public class IoCConfiguration
    {
        private readonly IServiceCollection _container;

        public IoCConfiguration(IServiceCollection container)
        {
            _container = container;
        }
        private readonly IConfiguration _configuration;

        public IoCConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void RegisterRepository()
        {
            var repoAssembly = typeof(BaseRepository).Assembly.GetExportedTypes();

            DynamicRegisterRepository(repoAssembly, typeof(SqlServerConnection));

            _container.AddSingleton<IEKtpReaderBackendDb, EKtpReaderBackendDb>();
            _container.AddSingleton<IEKtpReaderBackendDb2, EKtpReaderBackendDb2>();

            _container.AddSingleton<ILookupRepository, LookupRepository>();
            _container.AddSingleton<IDataReaderRepository, DataReaderRepository>();
            _container.AddSingleton<IUnitRepository, UnitRepository>();
            _container.AddSingleton<IAgeSegmentationRepository, AgeSegmentationRepository>();
            _container.AddSingleton<IPegawaiRepository, PegawaiRepository>();
            _container.AddSingleton<IBornGenerationRepository, BornGenerationRepository>();
            _container.AddSingleton<IMenuRepository, MenuRepository>();
            _container.AddSingleton<IDashboardDataEnrollRepository, DashboardDataEnrollRepository>();
            _container.AddSingleton<IDashboardDataEnrollNoMatchingRepository, DashboardDataEnrollNoMatchingRepository>();
            _container.AddSingleton<IUserRepository, UserRepository>();
            _container.AddSingleton<ISysParameterRepository, SysParameterRepository>();
            _container.AddSingleton<IMasTypeJariRepository, MasTypeJariRepository>();
            _container.AddSingleton<IUserFingerRepository, UserFingerRepository>();
            _container.AddSingleton<IMasterAplikasiRepository, MasterAplikasiRepository>();
            _container.AddSingleton<IUtilityRepository, UtilityRepository>();
            _container.AddSingleton<IUtility1Repository, Utility1Repository>();
            _container.AddSingleton<IDashboardReaderRepository, DashboardReaderRepository>();
            _container.AddSingleton<IAlatReaderRepository, AlatReaderRepository>();
            _container.AddSingleton<IMasterAppsRepository, MasterAppsRepository>();
            _container.AddSingleton<ITresholdRepository, TresholdRepository>();

            _container.AddSingleton<IMappingNIKRepository, MappingNIKRepository>();

            _container.AddSingleton<IFingerRepository, FingerRepository>();
            _container.AddSingleton<IProfileRepository, ProfileRepository>();

            _container.AddSingleton<IRTokenRepository, RTokenRepository>();
            _container.AddSingleton<IAuthRepository, AuthRepository>();

            _container.AddSingleton<IEnrollmentKTPRepository, EnrollmentKTPRepository>();
            _container.AddSingleton<IEnrollTempRepository, EnrollTempRepository>();
            _container.AddSingleton<ICekAlatRepository, CekAlatRepository>();
            _container.AddSingleton<IErrorLogRepository, ErrorLogRepository>();

            _container.AddSingleton<ISettingThresholdRepository, SettingThresholdRepository>();
            _container.AddSingleton<IEnrollmentNoMatchingRepository, EnrollmentNoMatchingRepository>();
            _container.AddSingleton<IMessageCodeRepository, MessageCodeRepository>();
            _container.AddSingleton<IBanchlinkCifUpdateRepository, BanchlinkCifUpdateRepository>();
        }

        public void RegisterBusiness()
        {
            _container.AddSingleton<IProfileService, ProfileService>();
            _container.AddSingleton<IEnrollmentService, EnrollmentService>();
            _container.AddSingleton<IAppVersionService, AppVersionService>();
            _container.AddSingleton<IUtilityService, UtilityService>();
            _container.AddSingleton<IProfileSettingService, ProfileSettingService>();
            _container.AddSingleton<IMatchingFingerService, MatchingFingerService>();
            _container.AddSingleton<IUserDataService, UserDataService>();
            _container.AddSingleton<IDataReaderService, DataReaderService>();
            _container.AddSingleton<IEnrollment, Enrollment>();
            _container.AddSingleton<ISettingThresholdService, SettingThresholService>();
            _container.AddSingleton<IMappingNIKPegawaiService, MappingNIKPegawaiService>();
            _container.AddSingleton<IEnrollmentNoMatchingService, EnrollmentNoMatchingService>();
            _container.AddSingleton<IBanchlinkCifUpdateService, BanchlinkCifUpdateService>();
        }

        public void RegisterService()
        {
            _container.AddSingleton<IImageRecognitionService, ImageRecognitionService>();
            _container.AddSingleton<ICIFService, CIFService>();
            _container.AddSingleton<IIKDServices, IKDService>();

        }

        public void RegisterCore()
        {
            _container.AddSingleton<IHttpRequestService, HttpRequestService>();
            _container.AddSingleton<ILdapService, LdapService>();
            _container.AddSingleton<LdapConnectionPool>();
            //_container.Configure<LDAPConfig>(_configuration.GetSection("LDAPConfig"));      
        }

        public void RegisterAuth()
        {
            _container.AddSingleton<IRefreshToken, RefreshToken>();
            _container.AddSingleton<IUserManager, UserManager>();
        }

        public void RegisterForWebApi()
        {
            RegisterCore();
            RegisterRepository();
            RegisterBusiness();
            RegisterService();
        }

        public void RegisterForWebIdentity()
        {
            RegisterRepository();
            RegisterAuth();
            RegisterService();
            RegisterCore();
        }

        public void LoadConfiguration(IConfiguration configuration)
        {
            var constringConfig = configuration.GetSection("ConnectionStrings");
            var credConfig = configuration.GetSection("AppSettings");
            var globalSettingsConfig = configuration.GetSection("GlobalSettings");
            var ldapConfig = configuration.GetSection("LDAPConfig");
            var sftpConfig = configuration.GetSection("SFTPSettings");
            var errorMessageConfig = configuration.GetSection("ErrorMessageSettings");
            var logConfig = configuration.GetSection("LogConfig");
            var successMessageConfig = configuration.GetSection("SuksesMessageSettings");
            var opsiTipeJariKananConfig = configuration.GetSection("OpsiTipeJariKanan");
            var opsiTipeJariKiriConfig = configuration.GetSection("OpsiTipeJariKiri");
            var opsiAgamaConfig = configuration.GetSection("OpsiAgama");
            var opsiStatusPerkawinanConfig = configuration.GetSection("OpsiStatusPerkawinan");
            var opsiGenderConfig = configuration.GetSection("OpsiGender");
            var opsiTipeFileMarker = configuration.GetSection("OpsiTipeFileMarker");
            var opsiGolDarah = configuration.GetSection("OpsiGolDarah");
            var opsiKewarganegaraan = configuration.GetSection("OpsiKewarganegaraan");

            _container.Configure<ConnectionStringConfig>(constringConfig);
            _container.Configure<CredConfig>(credConfig);
            _container.Configure<GlobalSettingConfig>(globalSettingsConfig);
            _container.Configure<LDAPConfig>(ldapConfig);
            _container.Configure<SftpConfig>(sftpConfig);
            _container.Configure<ErrorMessageConfig>(errorMessageConfig);
            _container.Configure<successMessageConfig>(successMessageConfig);
            _container.Configure<LogConfig>(logConfig);
            _container.Configure<OpsiTipeJariKananConfig>(opsiTipeJariKananConfig);
            _container.Configure<OpsiTipeJariKiriConfig>(opsiTipeJariKiriConfig);
            _container.Configure<OpsiAgamaConfig>(opsiAgamaConfig);
            _container.Configure<OpsiStatusPerkawinanConfig>(opsiStatusPerkawinanConfig);
            _container.Configure<OpsiGenderConfig>(opsiGenderConfig);
            _container.Configure<OpsiTipeFileMarkerConfig>(opsiTipeFileMarker);
            _container.Configure<OpsiGolDarahConfig>(opsiGolDarah);
            _container.Configure<OpsiKewarganegaraanConfig>(opsiKewarganegaraan);
        }

        private void DynamicRegisterRepository(IEnumerable<Type> repoAssembly, Type baseType)
        {
            var theRepo =
                from type in repoAssembly
                where type.IsSubclassOf(baseType)
                where type.GetInterfaces().Any(c => c.Name == "I" + type.Name)
                select new
                {
                    Service = type.GetInterfaces().LastOrDefault(),
                    Implementation = type
                };

            foreach (var reg in theRepo.Where(reg => reg.Service != null))
            {
                _container.AddSingleton(reg.Service, reg.Implementation);
            }
        }
    }
}
