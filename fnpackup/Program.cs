using fnpackup.Controllers;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.FileProviders;

namespace fnpackup
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);
            
            builder.Services.AddControllers();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()       // 允许任何源
                          .AllowAnyMethod()       // 允许任何HTTP方法
                          .AllowAnyHeader();       // 允许任何请求头
                });
            });

            var app = builder.Build();
            app.MapControllers();
            app.UseCors("AllowAll");
            
            
            var dir = Path.Combine(builder.Environment.ContentRootPath, "web");
            if(Directory.Exists(dir) == false) Directory.CreateDirectory(dir);
            app.UseStaticFiles(new StaticFileOptions
            {
                
                FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "web")),
                RequestPath = "/web"
            }); ;
            app.UseDefaultFiles();
            

            app.UseRouting();
            app.Run();

        }
    }
}
