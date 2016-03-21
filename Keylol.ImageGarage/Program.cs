using Keylol.ServiceBase;
using SimpleInjector;

namespace Keylol.ImageGarage
{
    public static class Program
    {
        public static readonly Container Container = new Container();

        public static void Main(string[] args)
        {
            // 服务特定依赖注册点
            // Container.Register(...)
            //
            KeylolService.Run<ImageGarage>(args, Container);
        }
    }
}