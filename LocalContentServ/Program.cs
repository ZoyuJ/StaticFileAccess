namespace LocalContentServ {
  using Microsoft.AspNetCore;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.AspNetCore.Hosting.WindowsServices;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.Hosting;
  using Microsoft.Extensions.Logging;

  using System;
  using System.Diagnostics;
  using System.IO;
  using System.Linq;
  using System.Text;

  public class Program {
    public static void Main(string[] args) {
      TraceUnhandleException();
      var IsService = !(Debugger.IsAttached || args.Contains("--console"));
      var ListenPort = args.Where(E => E.StartsWith("ListenAt=")).Select(E => E.Substring(9, E.Length - 9)).FirstOrDefault() ?? "1024";
      string ExeDirectory = null;
      if (IsService) {
        var ExePath = Process.GetCurrentProcess().MainModule.FileName;
        ExeDirectory = Path.GetDirectoryName(ExePath);
        Directory.SetCurrentDirectory(ExeDirectory);
      }
      if (IsService) {
        var Hoster = CreateWebHostBuilder(args.Where(E => E != "--console").ToArray(), ExeDirectory, ListenPort).Build();
        Hoster.RunAsService();
      }
      else {
        var Hoster = CreateWebHostBuilder(args, ListenPort).Build();
        Hoster.Run();
      }
    }

    static void TraceUnhandleException() {
      var DomainUnhandleExceptionPath = Path.Combine(Environment.CurrentDirectory, "DomainUnhandle.expstl");
      if (!File.Exists(DomainUnhandleExceptionPath))
        File.Create(DomainUnhandleExceptionPath).Close();
      File.Create(DomainUnhandleExceptionPath);
      AppDomain.CurrentDomain.UnhandledException += (Dunsender, Dunargs) => {
        using (var sw = new StreamWriter(DomainUnhandleExceptionPath, true, new UTF8Encoding(false))) {
          sw.WriteLine($"ExcepTraceStart~~~~~~~~~~~~~~~~~~~~~~{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff")}~~~~~~~~~~~~~~~~~~~~~~");
          sw.WriteLine(((Exception)Dunargs.ExceptionObject).ToString());
          sw.WriteLine($"ExcepTraceE n d~~~~~~~~~~~~~~~~~~~~~~{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff")}~~~~~~~~~~~~~~~~~~~~~~");
        }
      };
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args, string ContentRoot, string Port) {
      return WebHost.CreateDefaultBuilder(args)
        .UseKestrel()
        .UseContentRoot(ContentRoot)
        .ConfigureLogging((hostingContext, logging) => {
          logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
          logging.AddDebug();
          logging.AddEventLog();
        })
        .UseStartup<Startup>()
        .UseUrls($"http://*:{Port}");
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args, string Port) {
      return WebHost.CreateDefaultBuilder(args)
        .UseKestrel()
        .ConfigureLogging((hostingContext, logging) => {
          logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
          logging.AddDebug();
          logging.AddEventLog();
        })
        .UseStartup<Startup>()
        .UseUrls($"http://*:{Port}");
    }
  }
}
