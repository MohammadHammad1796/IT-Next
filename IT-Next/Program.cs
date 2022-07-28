using IT_Next.Core.Repositories;
using IT_Next.Core.Services;
using IT_Next.Custom.FilterAttributes;
using IT_Next.Infrastructure.Data;
using IT_Next.Infrastructure.Repositories;
using IT_Next.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace IT_Next;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ConfigureServices(builder.Services, builder.Configuration, builder.Environment);

        var app = builder.Build();
        ConfigureApplication(app);

        app.Run();

    }

    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Default")));

        services.AddControllersWithViews(options =>
        {
            options.Filters.Add<ValidateModelFilter>();
            options.Filters.Add<TrimStringsFilter>();
        })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

        services.AddAutoMapper(typeof(Program));

        if (environment.IsDevelopment())
        {
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ISubCategoryRepository, SubCategoryRepository>();
        services.AddScoped<ISubCategoryService, SubCategoryService>();
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<IBrandService, BrandService>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductService, ProductService>();

        services.AddScoped<IFileRepository, FileRepository>();
        services.AddScoped<IPhotoRepository, PhotoRepository>();
    }

    public static void ConfigureApplication(WebApplication application)
    {
        if (application.Environment.IsDevelopment())
        {
            application.UseDeveloperExceptionPage();

            application.UseSwagger(options =>
            {
                options.SerializeAsV2 = true;
            });
            application.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            });
        }

        application.UseStaticFiles();
        application.UseRouting();
        application.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller}/{action}/{id?}",
                defaults: new { controller = "Home", action = "Index" });
        });
    }
}