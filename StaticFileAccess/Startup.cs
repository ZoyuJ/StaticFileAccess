namespace StaticFileAccess {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;

  using Microsoft.AspNetCore.Builder;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.AspNetCore.StaticFiles;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.FileProviders;
  using Microsoft.Extensions.Hosting;
  using Microsoft.Extensions.Options;

  public class Startup {
    public Startup(IConfiguration configuration) {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services) {
      services.Configure<StaticFileServCfg>(Configuration.GetSection(nameof(StaticFileServCfg)));
      services.AddSingleton(sp => sp.GetRequiredService<IOptions<StaticFileServCfg>>().Value);
      services.AddControllersWithViews();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
      if (env.IsDevelopment()) {
        app.UseDeveloperExceptionPage();
      }
      else {
        app.UseExceptionHandler("/Home/Error");
      }
      app.UseStaticFiles();
      var StaticFileServCfg = app.ApplicationServices.GetRequiredService<StaticFileServCfg>();
      var FileExtenProvider = new FileExtensionContentTypeProvider();

      app.UseStaticFiles(new StaticFileOptions() {
        FileProvider = new PhysicalFileProvider(StaticFileServCfg.RootPath),
        RequestPath = StaticFileServCfg.RootUrl,
        ServeUnknownFileTypes = true,
        ContentTypeProvider = FileExtenProvider
      });
      if (StaticFileServCfg.EnableBrowsing) {
        app.UseDirectoryBrowser(new DirectoryBrowserOptions {
          FileProvider = new PhysicalFileProvider(StaticFileServCfg.RootPath),
          RequestPath = StaticFileServCfg.RootUrl
        });
      }
      app.UseRouting();

      app.UseAuthorization();

      app.UseEndpoints(endpoints => {
        endpoints.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
      });
    }
  }

  public class StaticFileServCfg {
    public string RootPath { get; set; } = "D:\\Resources";
    public bool EnableBrowsing { get; set; } = true;
    public string RootUrl { get; set; } = "/Storage";
  }
}
