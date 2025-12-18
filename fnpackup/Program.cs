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

            app.UseCors("AllowAll");

            app.MapControllers();

            app.Run();
        }
    }
}
