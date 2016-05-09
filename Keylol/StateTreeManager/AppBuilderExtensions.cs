using System;
using System.Threading.Tasks;
using Owin;

namespace Keylol.StateTreeManager
{
    /// <summary>
    /// OWIN App Builder 扩展方法
    /// </summary>
    public static class AppBuilderExtensions
    {
        /// <summary>
        /// 启用状态树管理器
        /// </summary>
        /// <param name="app">OWIN <see cref="IAppBuilder"/></param>
        /// <returns><see cref="IAppBuilder"/></returns>
        /// <exception cref="ArgumentNullException">参数 app 为 null</exception>
        public static IAppBuilder UseStateTreeManager(this IAppBuilder app)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            return app.Use(typeof(StateTreeManagerMiddleware));
        }
    }
}