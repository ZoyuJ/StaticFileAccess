namespace StaticFileAccess {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
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
            services.Configure<StaticFileServCfgs>(Configuration.GetSection(nameof(StaticFileServCfgs)));
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<StaticFileServCfgs>>().Value);
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
            var StaticFileServCfg = app.ApplicationServices.GetRequiredService<IOptions<StaticFileServCfgs>>().Value.Pathes.Distinct().ToArray();
            var FileExtenProvider = new FileExtensionContentTypeProvider();

            Array.ForEach(StaticFileServCfg, E => {
                app.UseStaticFiles(new StaticFileOptions() {
                    FileProvider = new PhysicalFileProvider(E.RootPath),
                    RequestPath = E.RootUrl,
                    ServeUnknownFileTypes = true,
                    ContentTypeProvider = FileExtenProvider
                });
                if (E.EnableBrowsing) {
                    app.UseDirectoryBrowser(new DirectoryBrowserOptions {
                        FileProvider = new PhysicalFileProvider(E.RootPath),
                        RequestPath = E.RootUrl
                    });
                }
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
    public class StaticFileServCfgs {
        public string UploadTo { get; set; }
        public StaticFileServCfg[] Pathes { get; set; }
    }
    public class StaticFileServCfg : IComparable<StaticFileServCfg>, IComparer<StaticFileServCfg> {
        public string RootPath { get; set; }
        public bool EnableBrowsing { get; set; } = true;
        public string RootUrl { get; set; }

        public int CompareTo([AllowNull] StaticFileServCfg other) => RootUrl.CompareTo(other.RootUrl);
        public int Compare([AllowNull] StaticFileServCfg x, [AllowNull] StaticFileServCfg y) => x.CompareTo(y);

        public override string ToString() => $"{RootUrl} At {RootPath} | {(EnableBrowsing ? "Browsing" : "Guessing")}";
    }
}
