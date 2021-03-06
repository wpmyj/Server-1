﻿using AlarmAndPlan.Model;
using Infrastructure.Model;
using Microsoft.EntityFrameworkCore;
using Resources.Model;
using PAPS.Model;
using Surveillance.Model;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Infrastructure.Utility;

/*************
 * http://blog.csdn.net/starnight_cbj/article/details/6904781
 * 迁移到PostgreSql注意点：
 * 1.使用Guid类型的主键一定要添加"Npgsql.EntityFrameworkCore.PostgreSQL.Design": "1.0.1"的引用，注意1.0.0版本是不支持的。
 * 2.要想使用Guid类型的主键一定要在DbContext的 OnModelCreating 重写方法中启用uuid的扩展 builder.HasPostgresExtension("uuid-ossp");
 * 
 * PostgreSQL实现TCP/IP访问步骤：
 * 1.进入PostgreSql安装目录，打开data\postgresql.conf,修改listen_addresses,如下所示
 * listen_addresses = 'localhost,192.168.18.71,192.168.20.104'		# what IP address(es) to listen on;
 * 2.打开data\pg_hba.conf,找到# IPv4 local connections:  ，在下面行插入
 * host    all             postgres       123.123.0.0/16            password 
 * 以上配置分别表示host->?, all-指所有数据库, postgres->指用户名, 123.123.0.0/16->指允许访问的postgresql数据库的网络，password->指连接数据库时需要密码
 * 
 * 2016-12-20 zhrx 取消设备名字唯一性限制
 **************/

namespace AllInOneContext
{
    public class AllInOneContext : DbContext
    {
        public AllInOneContext()
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //var connection = @"Server = 192.168.18.58; User ID=sa; Password=aebell; Database = AllInOne; MultipleActiveResultSets=true;"; /*MultipleActiveResultSets=true;*/
            //var connection = @"Server = 192.168.18.71; User ID=sa; Password=gz6021; Database = AllInOne; MultipleActiveResultSets=true;";

            // optionsBuilder.UseSqlServer(connection);

            //var connection = "User ID=postgres;Host=192.168.18.58;Password=AEBELL123;Port=5432;Database=AllInOne;Pooling=true;";
            //var connection = "User ID=postgres;Host=127.0.0.1;Password=123;Port=5432;Database=AllInOne;Pooling=true;";
            var connection = GlobalSetting.ConnectionString;
            optionsBuilder.UseNpgsql(connection);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("uuid-ossp"); //针对PostgreSql

            ////Application
            modelBuilder.Entity<Application>().HasIndex(p => p.ApplicationName).IsUnique();

            ////ApplicationSetting
            modelBuilder.Entity<ApplicationSetting>().HasIndex(p => p.SettingKey).IsUnique();

            ////Organization
            modelBuilder.Entity<Organization>().HasIndex(p => p.OrganizationFullName).IsUnique();

            ////Role
            modelBuilder.Entity<Role>().HasIndex(p => p.RoleName).IsUnique();
            //modelBuilder.Entity<Role>().HasOne(t => t.ControlResourcesType).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            //modelBuilder.Entity<Role>().HasOne(t => t.RolePermissions).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.SetNull);

            ////Schedule
            modelBuilder.Entity<Schedule>().HasIndex(p => p.ScheduleName).IsUnique();

            ////Staff
            modelBuilder.Entity<Staff>().HasIndex(p => p.StaffName).IsUnique();

            ////User
            modelBuilder.Entity<User>().HasIndex(p => p.UserName).IsUnique();

            modelBuilder.Entity<SystemOption>().HasIndex(p => p.SystemOptionCode).IsUnique();

            modelBuilder.Entity<MonitorySite>().HasIndex(p => new { p.OrganizationId, p.MonitorySiteName }).IsUnique();

            modelBuilder.Entity<ServerInfo>().HasIndex(p => new { p.OrganizationId, p.ServerName }).IsUnique();

            modelBuilder.Entity<CruiseScanGroup>().HasIndex(p => new { p.CameraId, p.GroupName }).IsUnique();

            modelBuilder.Entity<PresetSite>().HasIndex(p => new { p.CameraId, p.PresetSizeName }).IsUnique();

            //modelBuilder.Entity<IPDeviceInfo>().HasIndex(p => new { p.OrganizationId, p.IPDeviceName, p.DeviceTypeId,p.StatusId }).IsUnique();

            modelBuilder.Entity<TemplateLayout>().HasIndex(p => new { p.TemplateLayoutName, p.TemplateTypeId }).IsUnique();

            modelBuilder.Entity<DeviceGroup>().HasIndex(p => new { p.OrganizationId, p.DeviceGroupName }).IsUnique();

            modelBuilder.Entity<VideoRoundScene>().HasIndex(p => p.VideoRoundSceneName).IsUnique();

            modelBuilder.Entity<Camera>().HasIndex(p => new { p.EncoderId, p.EncoderChannel });

            modelBuilder.Entity<ApplicationResource>().HasOne(t => t.ParentResource).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<UserRole>().HasOne(t => t.User).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);

            //User & UserSetting relation: n->n
            modelBuilder.Entity<UserSettingMapping>().HasKey(t => new { t.UserId, t.UserSettingId });
            modelBuilder.Entity<UserSettingMapping>().HasOne(t => t.User).WithMany(t => t.UserSettings).HasForeignKey(t => t.UserId);

            // Device Group relation:many to many
            modelBuilder.Entity<DeviceGroupIPDevice>().HasKey(t => new { t.DeviceGroupId, t.IPDeviceInfoId });
            // modelBuilder.Entity<DeviceGroupIPDevice>().HasOne(t => t.DeviceGroup).WithMany(t => t.DeviceGroupIPDevices).HasForeignKey(p => p.DeviceGroupId);
            //modelBuilder.Entity<DeviceGroupIPDevice>().HasOne(t => t.IPDeviceInfo).WithMany(t => t.DeviceGroupIPDevices).HasForeignKey(p => p.IPDeviceInfoId);

            //CuriseScan & PresetSite 
            modelBuilder.Entity<CruiseScanGroupPresetSite>().HasKey(t => new { t.CruiseScanGroupId, t.PresetSiteID });
            modelBuilder.Entity<CruiseScanGroupPresetSite>().HasOne(t => t.CruiseScanGroup).WithMany(t => t.PresetSites).HasForeignKey(p => p.CruiseScanGroupId);
            //modelBuilder.Entity<CruiseScanGroupPresetSite>().HasOne(t => t.PresetSite).WithMany(t => t.Groups).HasForeignKey(t => t.PresetSiteID);

            //    modelBuilder.Entity<MonitorySite>().Property(t => t.CameraType).HasColumnName("CameraTypes");
            modelBuilder.Entity<ApplicationSystemOption>().HasKey(t => new { t.ApplicationId, t.SystemOptionId });
            modelBuilder.Entity<ApplicationSystemOption>().HasOne(t => t.SystemOption).WithMany(t => t.ApplicationSystemOptions).HasForeignKey(t => t.SystemOptionId);
            modelBuilder.Entity<ApplicationSystemOption>().HasOne(t => t.Application).WithMany(t => t.ApplicationSystemOptions).HasForeignKey(t => t.ApplicationId);

            //RolePermission
            modelBuilder.Entity<RolePermission>().HasKey(t => new { t.RoleId, t.PermissionId });
            //modelBuilder.Entity<RolePermission>().HasKey(t => new { t.PermissionId, t.RoleId });
            //modelBuilder.Entity<RolePermission>().HasOne(t => t.Permission).WithMany(t => t.RolePermissions).HasForeignKey(t => t.PermissionId);
            modelBuilder.Entity<RolePermission>().HasOne(t => t.Role).WithMany(t => t.RolePermissions).HasForeignKey(t => t.RoleId);
            modelBuilder.Entity<RolePermission>().HasOne(t => t.Role).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);

            modelBuilder.Entity<UserRole>().HasKey(t => new { t.UserId, t.RoleId });
            modelBuilder.Entity<UserRole>().HasOne(t => t.User).WithMany(t => t.UserManyToRole).HasForeignKey(t => t.UserId);
            modelBuilder.Entity<UserRole>().HasOne(t => t.Role).WithMany(t => t.UserManyToRole).HasForeignKey(t => t.RoleId);


            modelBuilder.Entity<AlarmPeripheral>().HasOne(t => t.AlarmType).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<DeviceGroup>().HasOne(t => t.DeviceGroupType).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<IPDeviceInfo>().HasOne(t => t.DeviceType).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<Materiel>().HasOne(t => t.Unit).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<SentinelVideo>().HasOne(t => t.VideoType).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<Materiel>().HasOne(t => t.Unit).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<ServiceInfo>().HasOne(t => t.ServiceType).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<TemplateLayout>().HasOne(t => t.LayoutType).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<TemplateLayout>().HasOne(t => t.TemplateType).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<VideoRoundScene>().HasOne(t => t.VideoRoundSceneFlag).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<MonitorySite>().HasOne(t => t.Organization).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<Camera>().HasOne(t => t.VideoForward).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<ServiceInfo>().HasOne(t => t.ServerInfo).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<ServiceInfo>().HasOne(t => t.ModifiedBy).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<ServerInfo>().HasOne(t => t.ModifiedBy).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<ServerInfo>().HasOne(t => t.Organization).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);

            modelBuilder.Entity<AlarmLog>().HasOne(t => t.AlarmSource).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<AlarmLog>().HasOne(t => t.AlarmLevel).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<AlarmLog>().HasOne(t => t.AlarmType).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<DeviceAlarmMapping>().HasOne(t => t.AlarmType).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<DeviceAlarmMapping>().HasOne(t => t.DeviceType).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<AlarmSetting>().HasOne(t => t.EmergencyPlan).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<AlarmSetting>().HasOne(t => t.BeforePlan).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<AlarmSetting>().HasOne(t => t.AlarmType).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<PredefinedAction>().HasOne(t => t.Action).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);

            modelBuilder.Entity<EventLog>().HasOne(t => t.EventLevel).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<EventLog>().HasOne(t => t.EventSource).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<EventLog>().HasOne(t => t.EventLogType).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);

            modelBuilder.Entity<Schedule>().HasOne(t => t.ScheduleType).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<Staff>().HasOne(t => t.PositionType).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<Staff>().HasOne(t => t.RankType).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<UserTerminal>().HasOne(t => t.UserTerminalType).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);

            #region PAPS
            // EmergencyTeam
            modelBuilder.Entity<EmergencyTeam>().HasKey(t => new { t.DutyGroupScheduleId, t.StaffId });
            modelBuilder.Entity<EmergencyTeam>().HasOne(t => t.DutyGroupSchedule).WithMany(t => t.EmergencyTeam).HasForeignKey(t => t.DutyGroupScheduleId);
            modelBuilder.Entity<EmergencyTeam>().HasOne(t => t.Staff).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);

            // Reservegroup
            modelBuilder.Entity<Reservegroup>().HasKey(t => new { t.DutyGroupScheduleId, t.StaffId });
            modelBuilder.Entity<Reservegroup>().HasOne(t => t.DutyGroupSchedule).WithMany(t => t.Reservegroup).HasForeignKey(t => t.DutyGroupScheduleId);
            modelBuilder.Entity<Reservegroup>().HasOne(t => t.Staff).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);

            // DutyCheckPackageLog
            modelBuilder.Entity<DutyCheckPackageLog>().HasKey(t => new { t.DutyCheckLogId, t.DutyCheckPackageId });
            //modelBuilder.Entity<DutyCheckPackageLog>().HasOne(t => t).WithMany(t => t.).HasForeignKey(p => p.DeviceGroupId);
            //Fault
            modelBuilder.Entity<Fault>().HasOne(t => t.CheckDutySite).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<Fault>().HasOne(t => t.CheckMan).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<Fault>().HasOne(t => t.DutyCheckOperation).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<Fault>().HasOne(t => t.DutyOrganization).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<Fault>().HasOne(t => t.FaultType).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            //Circular
            modelBuilder.Entity<Circular>().HasOne(t => t.CircularStaff).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            //modelBuilder.Entity<Circular>().HasOne(t => t.Appraise).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<Circular>().HasOne(t => t.DutyCheckLog).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            //DailyOnDuty
            modelBuilder.Entity<DailyOnDuty>().HasOne(t => t.DutyOfficerToday).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<DailyOnDuty>().HasOne(t => t.Organization).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            //modelBuilder.Entity<DailyOnDuty>().HasOne(t => t.Status).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<DailyOnDuty>().HasOne(t => t.TomorrowAttendant).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            //DutyCheckAppraise
            modelBuilder.Entity<DutyCheckAppraise>().HasOne(t => t.AppraiseICO).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<DutyCheckAppraise>().HasOne(t => t.AppraiseType).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<DutyCheckAppraise>().HasOne(t => t.Organization).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            //DutyCheckLog
            modelBuilder.Entity<DutyCheckLog>().HasOne(t => t.DutyCheckOperation).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<DutyCheckLog>().HasOne(t => t.DutyCheckSiteSchedule).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<DutyCheckLog>().HasOne(t => t.DutyCheckStaff).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<DutyCheckLog>().HasOne(t => t.Organization).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<DutyCheckLog>().HasOne(t => t.RecordType).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            //modelBuilder.Entity<DutyCheckLog>().HasOne(t => t.Schedule).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<DutyCheckLog>().HasOne(t => t.Status).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            //modelBuilder.Entity<DutyCheckLog>().HasOne(t => t.TimePeriod).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            //DutyCheckMatter
            modelBuilder.Entity<DutyCheckMatter>().HasOne(t => t.MatterICO).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<DutyCheckMatter>().HasOne(t => t.Organization).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<DutyCheckMatter>().HasOne(t => t.VoiceFile).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            //DutyCheckOperationAttachment
            modelBuilder.Entity<DutyCheckOperationAttachment>().HasOne(t => t.AttachmentType).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            //DutyCheckPackage
            //modelBuilder.Entity<DutyCheckPackage>().HasOne(t => t.DutyCheckSchedule).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<DutyCheckPackage>().HasOne(t => t.Organization).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            //DutyCheckPackageTimePlan
            modelBuilder.Entity<DutyCheckPackageTimePlan>().HasOne(t => t.Schedule).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<DutyCheckPackageTimePlan>().HasOne(t => t.Organization).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            //DutyCheckSchedule
            modelBuilder.Entity<DutyCheckSchedule>().HasOne(t => t.Leader).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<DutyCheckSchedule>().HasOne(t => t.Deputy).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<DutyCheckSchedule>().HasOne(t => t.CheckTimePeriod).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);

            //DutyCheckSiteSchedule
            //modelBuilder.Entity<DutyCheckSiteSchedule>().HasKey(t => new { t.CheckDutySiteId, t.CheckManId });
            modelBuilder.Entity<DutyCheckSiteSchedule>().HasOne(t => t.CheckDutySite).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<DutyCheckSiteSchedule>().HasOne(t => t.CheckMan).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            //DutyGroupScheduleDetail


            //DutyGroupSchedule
            modelBuilder.Entity<DutyGroupSchedule>().HasOne(t => t.Organization).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<DutyGroupSchedule>().HasOne(t => t.Lister).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            //modelBuilder.Entity<DutyGroupSchedule>().HasOne(t => t.DutyGroupScheduleDetails).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            //modelBuilder.Entity<DutyGroupSchedule>().HasOne(t => t.EmergencyTeam).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            //modelBuilder.Entity<DutyGroupSchedule>().HasOne(t => t.Reservegroup).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            //DutySchedule
            modelBuilder.Entity<DutySchedule>().HasOne(t => t.Organization).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<DutySchedule>().HasOne(t => t.Lister).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            //modelBuilder.Entity<DutySchedule>().HasOne(t => t.NetWatcherSchedule).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            //DutyScheduleDetail
            modelBuilder.Entity<DutyScheduleDetail>().HasOne(t => t.CadreSchedule).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<DutyScheduleDetail>().HasOne(t => t.OfficerSchedule).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);

            //Feedback
            modelBuilder.Entity<Feedback>().HasOne(t => t.Circular).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<Feedback>().HasOne(t => t.FeedbackStaff).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            //InstitutionsDutyCheckSchedule
            modelBuilder.Entity<InstitutionsDutyCheckSchedule>().HasOne(t => t.InspectedOrganization).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<InstitutionsDutyCheckSchedule>().HasOne(t => t.Lead).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            //ShiftHandoverLog
            modelBuilder.Entity<ShiftHandoverLog>().HasOne(t => t.Organization).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<ShiftHandoverLog>().HasOne(t => t.OffGoing).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<ShiftHandoverLog>().HasOne(t => t.Status).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            //TemporaryDuty
            modelBuilder.Entity<TemporaryDuty>().HasOne(t => t.Organization).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            //modelBuilder.Entity<TemporaryDuty>().HasOne(t => t.Equipments).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<TemporaryDuty>().HasOne(t => t.Commander).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<TemporaryDuty>().HasOne(t => t.DutyType).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<TemporaryDuty>().HasOne(t => t.VehicleType).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);

            #endregion

            //IPDeviceInfo
            modelBuilder.Entity<IPDeviceInfo>().HasOne(t => t.Organization).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            //modelBuilder.Entity<DefenseDevice>().HasOne(t => t.Sentinel).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);

            modelBuilder.Entity<DeviceGroupIPDevice>().HasOne(t => t.IPDeviceInfo).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<CruiseScanGroupPresetSite>().HasOne(t => t.PresetSite).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);

            //
            modelBuilder.Entity<MonitorySite>().HasOne(t => t.Camera).WithOne().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Cascade);
            modelBuilder.Entity<Camera>().HasOne(t => t.IPDevice).WithOne().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<AlarmPeripheral>().HasOne(t => t.AlarmDevice).WithOne().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            //modelBuilder.Entity<AlarmPeripheral>().HasOne(t => t.IPDeviceInfo).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);

            //modelBuilder.Entity<PunchLog>().HasOne(t => t.Staff).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);

            modelBuilder.Entity<AlarmSetting>().HasIndex(t => new { t.AlarmSourceId, t.AlarmTypeId }).IsUnique();

            //人员指纹
            //modelBuilder.Entity<Fingerprint>().HasOne(t => t.Staff).WithMany(t => t.Fingerprints).OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<DeviceChannelSetting>().HasOne(t => t.ChannelType).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<DefenseDevice>().HasOne(t => t.DeviceInfo).WithOne().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Cascade);

            //分队--查勤安排表
            modelBuilder.Entity<UnitDutyCheckSchedule>().HasOne(t => t.Organization).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<UnitDutyCheckSchedule>().HasOne(t => t.Lister).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            modelBuilder.Entity<UnitDutyCheckSchedule>().HasOne(t => t.Schedule).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
            //
            modelBuilder.Entity<UnitDutyCheckScheduleDetail>().HasOne(t => t.CheckMan).WithMany().OnDelete(Microsoft.EntityFrameworkCore.Metadata.DeleteBehavior.Restrict);
        }

        #region Infranstructure set
        /// <summary>
        /// 应用集合
        /// </summary>
        public DbSet<Application> Application
        {
            get; set;
        }

        /// <summary>
        /// 应用中心集合
        /// </summary>
        public DbSet<ApplicationCenter> ApplicationCenter
        {
            get; set;
        }

        /// <summary>
        /// 应用设置集合
        /// </summary>
        public DbSet<ApplicationSetting> ApplicationSetting
        {
            get; set;
        }

        /// <summary>
        /// 附件集合
        /// </summary>
        public DbSet<Attachment> Attachment
        {
            get; set;
        }

        /// <summary>
        /// 事件日志集合
        /// </summary>
        public DbSet<EventLog> EventLog
        {
            get; set;
        }

        /// <summary>
        /// 指纹集合
        /// </summary>
        public DbSet<Fingerprint> Fingerprint
        {
            get; set;
        }

        /// <summary>
        /// 组织机构集合
        /// </summary>
        public DbSet<Organization> Organization
        {
            get; set;
        }

        /// <summary>
        /// 角色集合
        /// </summary>
        public DbSet<Role> Role
        {
            get; set;
        }

        /// <summary>
        /// 人员集合
        /// </summary>
        public DbSet<Staff> Staff
        {
            get; set;
        }

        /// <summary>
        /// 人员组集合
        /// </summary>
        public DbSet<StaffGroup> StaffGroup
        {
            get; set;
        }

        /// <summary>
        /// 系统选项集合
        /// </summary>
        public DbSet<SystemOption> SystemOption
        {
            get; set;
        }

        /// <summary>
        /// 用户集合
        /// </summary>
        public DbSet<User> User
        {
            get; set;
        }

        /// <summary>
        /// 在线用户集合
        /// </summary>
        public DbSet<OnlineUser> OnlineUser
        {
            get; set;
        }

        /// <summary>
        /// 权限范围
        /// </summary>
        public DbSet<Permission> Permission
        {
            get; set;
        }

        /// <summary>
        /// 操作权限
        /// </summary>
        public DbSet<ResourcesAction> ResourcesAction
        {
            get; set;
        }

        /// <summary>
        /// 任务排程
        /// </summary>
        public DbSet<Schedule> Schedule
        {
            get; set;
        }

        //public DbSet<ScheduleCycleMonth> MonthSchedule
        //{
        //    get;set;
        //}

        //public DbSet<ScheduleCycleWeek> WeekSchedule
        //{
        //    get;set;
        //}

        /// <summary>
        /// 系统资源
        /// </summary>
        public DbSet<ApplicationResource> ApplicationResource
        {
            get; set;
        }

        /// <summary>
        /// 角色和权限多对多
        /// </summary>
        public DbSet<RolePermission> RolePermission
        {
            get; set;
        }

        /// <summary>
        /// 用户和角色多对多
        /// </summary>
        public DbSet<UserRole> UserRole
        {
            get; set;
        }

        /// <summary>
        /// 用户和管控资源多对多
        /// </summary>
        public DbSet<ControlResources> ControlResources
        {
            get; set;
        }


        //引发Cannot use table 'UserSetting' in schema '' for entity 'UserSettingMapping' since it is being used for another entity. ，去掉
        //需要加载时可用DbContext.Set<UserSettingMapping>()  20160919 zhrx
        ///// <summary>
        ///// 用户和用户设置一对多
        ///// </summary>
        //public DbSet<UserSettingMapping> UserSetting
        //{
        //    get; set;
        //}

        /// <summary>
        ///系统应用和系统选项多对多
        /// </summary>
        public DbSet<ApplicationSystemOption> ApplicationSystemOption
        {
            get; set;
        }

        #endregion

        #region Resource Set
        public DbSet<ServiceInfo> ServiceInfo
        {
            get; set;
        }

        /// <summary>
        /// 监控点集合
        /// </summary>
        public DbSet<MonitorySite> MonitorySite
        {
            get; set;
        }

        /// <summary>
        /// 巡航组集合
        /// </summary>
        public DbSet<CruiseScanGroup> CruiseScanGroup
        {
            get; set;
        }

        /// <summary>
        /// 预置点集合
        /// </summary>
        public DbSet<PresetSite> PresetSite
        {
            get; set;
        }

        /// <summary>
        /// 设备集合
        /// </summary>
        public DbSet<IPDeviceInfo> IPDeviceInfo
        {
            get; set;
        }

        /// <summary>
        /// 设备组集合
        /// </summary>
        public DbSet<DeviceGroup> DeviceGroup
        {
            get; set;
        }

        /// <summary>
        /// 编码器集合
        /// </summary>
        public DbSet<Encoder> Encoder
        {
            get; set;
        }

        /// <summary>
        /// 服务集合
        /// </summary>
        public DbSet<ServerInfo> ServerInfo
        {
            get; set;
        }

        /// <summary>
        /// 哨位台集合
        /// </summary>
        public DbSet<Sentinel> Sentinel
        {
            get; set;
        }

        public DbSet<DefenseDevice> DefenseDevice
        {
            get; set;
        }

        /// <summary>
        /// 模板集合
        /// </summary>
        public DbSet<TemplateLayout> TemplateLayout
        {
            get; set;
        }

        /// <summary>
        /// 场景轮巡集合
        /// </summary>
        public DbSet<VideoRoundScene> VideoRoundScene
        {
            get; set;
        }

        public DbSet<AlarmPeripheral> AlarmPeripheral
        {
            get; set;
        }

        public DbSet<SentinelLayout> SentinelLayout { get; set; }

        public DbSet<DeviceChannelTypeMapping> DeviceChannelTypeMapping { get; set; }

        public DbSet<AlarmMainframe> AlarmMainframe { get; set; }
        #endregion

        #region Alarm&Plan set
        public DbSet<AlarmSetting> AlarmSetting
        {
            get; set;
        }

        public DbSet<AlarmProcessed> AlarmProcessed
        {
            get; set;
        }

        public DbSet<AlarmLog> AlarmLog
        {
            get; set;
        }

        public DbSet<DeviceAlarmMapping> DeviceAlarmMapping
        {
            get; set;
        }

        public DbSet<Plan> Plan { get; set; }

        public DbSet<TimerTask> TimerTask { get; set; }

        public DbSet<ServiceEventLog> ServiceEventLog { get; set; }

        public DbSet<SentinelFingerPrintMapping> SentinelFingerPrintMapping { get; set; }
        #endregion

        #region Paps set
        public DbSet<Circular> Circular { get; set; }

        public DbSet<DailyOnDuty> DailyOnDuty { get; set; }

        public DbSet<DutyCheckAppraise> DutyCheckAppraise { get; set; }

        public DbSet<DutyCheckGroup> DutyCheckGroup { get; set; }

        public DbSet<DutyCheckLog> DutyCheckLog { get; set; }

        public DbSet<DutyCheckMatter> DutyCheckMatter { get; set; }

        public DbSet<DutyCheckOperation> DutyCheckOperation { get; set; }

        public DbSet<DutyCheckPackage> DutyCheckPackage { get; set; }

        public DbSet<DutyCheckPackageTimePlan> DutyCheckPackageTimePlan { get; set; }

        public DbSet<DutyCheckSchedule> DutyCheckSchedule { get; set; }

        public DbSet<DutyGroupSchedule> DutyGroupSchedule { get; set; }

        public DbSet<DutySchedule> DutySchedule { get; set; }
        public DbSet<Feedback> Feedback { get; set; }

        public DbSet<TemporaryDuty> TemporaryDuty { get; set; }

        /// <summary>
        /// 机关--查勤检查安排
        /// </summary>
        public DbSet<InstitutionsDutyCheckSchedule> InstitutionsDutyCheckSchedule { get; set; }

        /// <summary>
        /// 值班交接记录
        /// </summary>
        public DbSet<ShiftHandoverLog> ShiftHandoverLog { get; set; }

        /// <summary>
        /// 子弹箱开启记录
        /// </summary>
        public DbSet<BulletboxLog> BulletboxLog { get; set; }

        /// <summary>
        /// 打卡记录
        /// </summary>
        public DbSet<PunchLog> PunchLog { get; set; }

        /// <summary>
        /// 分队--查勤安排表
        /// </summary>
        public DbSet<UnitDutyCheckSchedule> UnitDutyCheckSchedule { get; set; }
        #endregion

        #region Sureillance set
        public DbSet<DeviceStatusHistory> DeviceStatusHistory
        {
            get; set;
        }

        public DbSet<FaceRecognition> FaceRecognition
        {
            get; set;
        }

        public DbSet<LicensePlateRecognition> LicensePlateRecognition
        {
            get; set;
        }
        #endregion
    }
}
