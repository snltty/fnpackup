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
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();
            app.MapControllers();
            app.UseCors("AllowAll");
            
            
            var dir = Path.Combine(builder.Environment.ContentRootPath, "web");
            if(Directory.Exists(dir) == false) Directory.CreateDirectory(dir);
            app.UseStaticFiles(new StaticFileOptions
            {
                
                FileProvider = new PhysicalFileProvider(dir),
                RequestPath = "/web"
            }); ;
            app.UseDefaultFiles();

            app.UseRouting();
            app.Run();

        }
    }
}
