using System;
using System.Text;
using AspectCore.Configuration;
using log4net;
using log4net.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zxw.Framework.NetCore.Attributes;
using Zxw.Framework.NetCore.DbContextCore;
using Zxw.Framework.NetCore.EventBus;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.Filters;
using Zxw.Framework.NetCore.Helpers;
using Zxw.Framework.NetCore.IDbContext;
using Zxw.Framework.NetCore.IoC;
using Zxw.Framework.NetCore.Options;
using Zxw.Framework.NetCore.Web;

namespace Zxw.Framework.Website
{
    public class Startup
    {
        public static ILoggerRepository Repository { get; set; }
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            env = environment;
            //初始化log4net
            Repository = LogManager.CreateRepository("NETCoreRepository");
            Log4NetHelper.SetConfig(Repository, "log4net.config");
        }
        IWebHostEnvironment env { get; }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            InitIoC(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            if (env.EnvironmentName == "Development")
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCookiePolicy(new CookiePolicyOptions()
            {
                CheckConsentNeeded = context => true,
                MinimumSameSitePolicy = SameSiteMode.None
            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
        /// <summary>
        /// IoC初始化
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        private IServiceProvider InitIoC(IServiceCollection services)
        {
            //database connectionstring
            var dbConnectionString = Configuration.GetConnectionString("MsSqlServer");

            #region Redis

            //var redisConnectionString = Configuration.GetConnectionString("Redis");
            ////启用Redis
            //services.UseCsRedisClient(redisConnectionString);
            //全局设置Redis缓存有效时间为5分钟。
            //services.Configure<DistributedCacheEntryOptions>(option =>
            //    option.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5));

            #endregion

            #region MemoryCache

            //启用MemoryCache
            //services.AddMemoryCache();

            #endregion

            #region 配置DbContextOption

            //配置DbContextOption
            services.Configure<DbContextOption>(options =>
            {
                options.ConnectionString = dbConnectionString;
                options.ModelAssemblyName = "Zxw.Framework.Website.Models";
                options.IsOutputSql = true;
            });

            #endregion

            #region 配置CodeGenerateOption

            //配置CodeGenerateOption
            services.Configure<CodeGenerateOption>(options =>
            {
                options.ModelsNamespace = "Zxw.Framework.Website.Models";
                options.IRepositoriesNamespace = "Zxw.Framework.Website.IRepositories";
                options.RepositoriesNamespace = "Zxw.Framework.Website.Repositories";
                options.ControllersNamespace = "Zxw.Framework.Website.Controllers";
            });

            #endregion

            #region 各种注入

            //services.AddDbContext<SqlServerDbContext>(builder =>
            //{
            //    builder.UseSqlServer(dbConnectionString)
            //        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            //});
            services.AddSingleton(Configuration)//注入Configuration，ConfigHelper要用
                                                //.AddScoped<IDbContextCore, PostgreSQLDbContext>()//注入EF上下文
                                                .AddDbContext<IDbContextCore, SqlServerDbContext>()//注入EF上下文
            //.AddDbContextFactory(factory =>
            //    {
            //        factory.AddDbContext<IDbContextCore, SqlServerDbContext>(
            //            new DbContextOption()
            //            {
            //                TagName = "db2",
            //                ConnectionString = dbConnectionString,
            //                ModelAssemblyName = "Zxw.Framework.Website.Models",
            //                IsOutputSql = true
            //            });
            //    })
                .AddScopedAssembly("Zxw.Framework.Website.IRepositories", "Zxw.Framework.Website.Repositories");//注入仓储

            #endregion


            return services.AddCoreX(config =>
            {
                config.AddEventBus(r =>
                    {
                        // 使用环境变量作为版本号
                        r.Version = env.EnvironmentName;
                        //r.Version = "Test";
                        // 使用HealthDbContext作为存储库
                        r.UseSqlServer(dbConnectionString);
                        // 配置rabbit 作为消息队列
                        r.UseRabbitMQ(option =>
                        {
                            option.HostName = Configuration.GetValue<string>("Mq:Server");
                            option.UserName = Configuration.GetValue<string>("Mq:UserName");
                            option.Password = Configuration.GetValue<string>("Mq:Password");
                            option.Port = Configuration.GetValue<int>("Mq:Port");

                            // rabbit交换机名称
                            option.ExchangeName = $"cap.demo.router";
                        });

                        if (Configuration.GetValue<bool>("CAP:CapDashboard", false))
                            // 非正式服 使用cap ui
                            // 配置可视化面板
                            r.UseDashboard();
                    });

                config.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(options =>
                    {
                        options.LoginPath = "/Account/Index";
                        options.LogoutPath = "/Account/Logout";
                    });
                config.AddOptions();
                config.AddControllersWithViews();
                config.AddMvc(option =>
                    {
                        option.Filters.Add<GlobalExceptionFilter>(0);
                    })
                    .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                    .AddControllersAsServices();

            }, aspectConfig =>
            {
                aspectConfig.Interceptors.AddTyped<FromDbContextFactoryInterceptor>();
            });//接入AspectCore.Injector
        }
    }
}
