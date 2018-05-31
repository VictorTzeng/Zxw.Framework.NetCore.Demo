using log4net;
using log4net.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Zxw.Framework.NetCore.DbContextCore;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.Filters;
using Zxw.Framework.NetCore.Helpers;
using Zxw.Framework.NetCore.IoC;
using Zxw.Framework.NetCore.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Zxw.Framework.Website
{
    public class Startup
    {
        public static ILoggerRepository Repository { get; set; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            //初始化log4net
            Repository = LogManager.CreateRepository("NETCoreRepository");
            Log4NetHelper.SetConfig(Repository, "log4net.config");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            return InitIoC(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection()
                .UseStaticFiles()
                .UseCookiePolicy()
                .UseAuthentication()
                .UseMvc(routes =>
                {
                    routes.MapRoute(
                        name: "default",
                        template: "{controller=Home}/{action=Index}/{id?}");
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

            var redisConnectionString = Configuration.GetConnectionString("Redis");
            //启用Redis
            services.AddDistributedRedisCache(option =>
            {
                option.Configuration = redisConnectionString;//redis连接字符串
                option.InstanceName = "sample";//Redis实例名称
            });
            //全局设置Redis缓存有效时间为5分钟。
            //services.Configure<DistributedCacheEntryOptions>(option =>
            //    option.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5));

            #endregion

            #region MemoryCache

            //启用MemoryCache
            services.AddMemoryCache();

            #endregion

            #region 配置DbContextOption

            //配置DbContextOption
            services.Configure<DbContextOption>(options =>
            {
                options.ConnectionString = dbConnectionString;
                options.ModelAssemblyName = "Zxw.Framework.Website.Models";
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

            services.AddSingleton(Configuration)//注入Configuration，ConfigHelper要用
                .AddScoped<IDbContextCore, SqlServerDbContext>()//注入EF上下文
                .AddTransientAssembly("Zxw.Framework.Website.IRepositories", "Zxw.Framework.Website.Repositories");//注入仓储
            
            #endregion

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Index";
                    options.LogoutPath = "/Account/Logout";
                });
            services.AddOptions();
            services.AddMvc(option =>
                {
                    option.Filters.Add(new GlobalExceptionFilter());
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddControllersAsServices();

            return AspectCoreContainer.BuildServiceProvider(services);//接入AspectCore.Injector
        }
    }
}
