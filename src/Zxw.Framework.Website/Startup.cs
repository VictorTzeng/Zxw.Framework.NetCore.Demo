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
            //��ʼ��log4net
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
        /// IoC��ʼ��
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        private IServiceProvider InitIoC(IServiceCollection services)
        {
            //database connectionstring
            var dbConnectionString = Configuration.GetConnectionString("MsSqlServer");

            #region Redis

            //var redisConnectionString = Configuration.GetConnectionString("Redis");
            ////����Redis
            //services.UseCsRedisClient(redisConnectionString);
            //ȫ������Redis������Чʱ��Ϊ5���ӡ�
            //services.Configure<DistributedCacheEntryOptions>(option =>
            //    option.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5));

            #endregion

            #region MemoryCache

            //����MemoryCache
            //services.AddMemoryCache();

            #endregion

            #region ����DbContextOption

            //����DbContextOption
            services.Configure<DbContextOption>(options =>
            {
                options.ConnectionString = dbConnectionString;
                options.ModelAssemblyName = "Zxw.Framework.Website.Models";
                options.IsOutputSql = true;
            });

            #endregion

            #region ����CodeGenerateOption

            //����CodeGenerateOption
            services.Configure<CodeGenerateOption>(options =>
            {
                options.ModelsNamespace = "Zxw.Framework.Website.Models";
                options.IRepositoriesNamespace = "Zxw.Framework.Website.IRepositories";
                options.RepositoriesNamespace = "Zxw.Framework.Website.Repositories";
                options.ControllersNamespace = "Zxw.Framework.Website.Controllers";
            });

            #endregion

            #region ����ע��

            //services.AddDbContext<SqlServerDbContext>(builder =>
            //{
            //    builder.UseSqlServer(dbConnectionString)
            //        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            //});
            services.AddSingleton(Configuration)//ע��Configuration��ConfigHelperҪ��
                                                //.AddScoped<IDbContextCore, PostgreSQLDbContext>()//ע��EF������
                                                .AddDbContext<IDbContextCore, SqlServerDbContext>()//ע��EF������
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
                .AddScopedAssembly("Zxw.Framework.Website.IRepositories", "Zxw.Framework.Website.Repositories");//ע��ִ�

            #endregion


            return services.AddCoreX(config =>
            {
                config.AddEventBus(r =>
                    {
                        // ʹ�û���������Ϊ�汾��
                        r.Version = env.EnvironmentName;
                        //r.Version = "Test";
                        // ʹ��HealthDbContext��Ϊ�洢��
                        r.UseSqlServer(dbConnectionString);
                        // ����rabbit ��Ϊ��Ϣ����
                        r.UseRabbitMQ(option =>
                        {
                            option.HostName = Configuration.GetValue<string>("Mq:Server");
                            option.UserName = Configuration.GetValue<string>("Mq:UserName");
                            option.Password = Configuration.GetValue<string>("Mq:Password");
                            option.Port = Configuration.GetValue<int>("Mq:Port");

                            // rabbit����������
                            option.ExchangeName = $"cap.demo.router";
                        });

                        if (Configuration.GetValue<bool>("CAP:CapDashboard", false))
                            // ����ʽ�� ʹ��cap ui
                            // ���ÿ��ӻ����
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
            });//����AspectCore.Injector
        }
    }
}
