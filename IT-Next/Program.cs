using IT_Next.Core.Entities;
using IT_Next.Core.Repositories;
using IT_Next.Core.Services;
using IT_Next.Custom.FilterAttributes;
using IT_Next.Custom.Middlewares;
using IT_Next.Infrastructure.Data;
using IT_Next.Infrastructure.Repositories;
using IT_Next.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System.Net;

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

        services.AddHttpsRedirection(options =>
        {
            options.RedirectStatusCode = (int) HttpStatusCode.PermanentRedirect;
            options.HttpsPort = int.Parse(configuration["httpsPort"]);
        });
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IQueryCustomization<Category>, QueryCustomization<Category>>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IQueryCustomization<SubCategory>, QueryCustomization<SubCategory>>();
        services.AddScoped<ISubCategoryRepository, SubCategoryRepository>();
        services.AddScoped<ISubCategoryService, SubCategoryService>();
        services.AddScoped<IQueryCustomization<Brand>, QueryCustomization<Brand>>();
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<IBrandService, BrandService>();
        services.AddScoped<IQueryCustomization<Product>, QueryCustomization<Product>>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IQueryCustomization<ContactMessage>, QueryCustomization<ContactMessage>>();
        services.AddScoped<IContactMessageRepository, ContactMessageRepository>();

        services.AddScoped<IFileRepository, FileRepository>();
        services.AddScoped<IPhotoRepository, PhotoRepository>();

        services.AddScoped<ICertificateService, CertificateService>();
        services.AddScoped<IStorageService, StorageService>();
    }

    public static void ConfigureApplication(WebApplication application)
    {
        if (application.Environment.IsDevelopment())
        {
            application.UseDeveloperExceptionPage();

            application.UseSwagger(options =>
            {
                options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;
            });
            application.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            });
        }
        else
        {
            application.UseHsts();
        }

        application.UseMiddleware<DisableHttpOnApiMiddleware>();
        application.UseHttpsRedirection();
        application.UseStaticFiles();
        application.UseMiddleware<NotFoundPageMiddleware>();
        application.UseRouting();
        application.MapControllerRoute("Default", "{controller}/{action}/{id?}", new { controller = "Main", action = "Home" });
    }
}