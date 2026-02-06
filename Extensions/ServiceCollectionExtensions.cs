using Microsoft.Extensions.DependencyInjection;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Implementations;
using PRJ_WAREHOUSE_BIVN.Data.Repositories.Interfaces;
using PRJ_WAREHOUSE_BIVN.Services.Service.Implementations;
using PRJ_WAREHOUSE_BIVN.Services.Service.Interfaces;

namespace PRJ_WAREHOUSE_BIVN.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        // Repositories & Services
        services.AddScoped<ITmNccNewService, TmNccNewService>();
        services.AddScoped<ITmNccNewRepository, TmNccNewRepository>();

        services.AddScoped<IBaoGiaService, BaoGiaService>();
        services.AddScoped<IBaoGiaRepository, BaoGiaRepository>();

        services.AddScoped<IMaterialRepository, MaterialRepository>();
        services.AddScoped<IMaterialService, MaterialService>();

        services.AddScoped<IMasterApproverSendMailService, MasterApproverSendMailService>();
        services.AddScoped<IMasterApproverSendMailRepository, MasterApproverSendMailRepository>();

        services.AddScoped<ITmUserRepository, TmUserRepository>();
        services.AddScoped<ITmUserService, TmUserService>();

        services.AddScoped<IBaoGiaStepService, BaoGiaStepService>();
        services.AddScoped<IBaoGiaStepRepository, BaoGiaStepRepository>();

        services.AddScoped<ITmSectionRepository, TmSectionRepository>();
        services.AddScoped<ITmSectionService, TmSectionService>();

        services.AddScoped<IEmployeeWorkingRepository, EmployeeWorkingRepository>();
        services.AddScoped<IEmployeeWorkingService, EmployeeWorkingService>();

        services.AddScoped<INhomViTriRepository, NhomViTriRepository>();
        services.AddScoped<INhomViTriService, NhomViTriService>();

        services.AddScoped<IBaoGiaStatusRepository, BaoGiaStatusRepository>();
        services.AddScoped<IBaoGiaStatusService, BaoGiaStatusService>();

        services.AddScoped<IBaoGiaConfirmNameRepository, BaoGiaConfirmNameRepository>();
        services.AddScoped<IBaoGiaConfirmNameService, BaoGiaConfirmNameService>();

        services.AddScoped<IBaoGiaNCCRepository, BaoGiaNCCRepository>();
        services.AddScoped<IBaoGiaNCCService, BaoGiaNCCService>();

        services.AddScoped<IBaoGiaHistoryRepository, BaoGiaHistoryRepository>();
        services.AddScoped<IBaoGiaHistoryService, BaoGiaHistoryService>();

        services.AddScoped<IHistoryApproverRepository, HistoryApproverRepository>();
        services.AddScoped<IHistoryApproverServive, HistoryApproverServive>();

        services.AddScoped<IBaoGiaDetailRepository, BaoGiaDetailRepository>();
        services.AddScoped<IBaoGiaDetailService, BaoGiaDetailService>();

        services.AddScoped<IBaoGiaNccCategory, BaoGiaNccCategory>();
        services.AddScoped<IBaoGiaNccCategoryService, BaoGiaNccCategoryService>();
        return services;
    }
}
