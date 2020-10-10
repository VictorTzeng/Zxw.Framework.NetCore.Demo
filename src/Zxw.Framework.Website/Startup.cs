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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zxw.Framework.NetCore.Attributes;
using Zxw.Framework.NetCore.DbContextCore;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.Filters;
using Zxw.Framework.NetCore.Helpers;
using Zxw.Framework.NetCore.IDbContext;
using Zxw.Framework.NetCore.Options;
using Zxw.Framework.NetCore.Web;

namespace Zxw.Framework.Website
{
    public class Startup
    {
        public static ILoggerRepository Repository { get; set; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            //��ʼ��log4net
            Repository = LogManager.CreateRepository("NETCoreRepository");
            Log4NetHelper.SetConfig(Repository, "log4net.config");
        }

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

            services.AddSingleton(Configuration)//ע��Configuration��ConfigHelperҪ��
                                                .AddScoped<IDbContextCore, SqlServerDbContext>()//ע��EF������
                                                                                                 //.AddDbContext<IDbContextCore, SqlServerDbContext>()//ע��EF������
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

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Index";
                    options.LogoutPath = "/Account/Logout";
                });
            services.AddOptions();
            //services.AddHttpContextAccessor();
            services.AddControllersWithViews();
            services.AddMvc(option =>
            {
                option.Filters.Add(new GlobalExceptionFilter());
            })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddControllersAsServices();

            return services.AddCoreX(config =>
            {
                config.AddSingleton<IWebContext, DemoWebContext>();
            }, aspectConfig =>
            {
                aspectConfig.Interceptors.AddTyped<FromDbContextFactoryInterceptor>();
            });//����AspectCore.Injector
        }
    }
}
