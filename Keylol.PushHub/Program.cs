using Keylol.ServiceBase;
using SimpleInjector;

namespace Keylol.PushHub
{
    public static class Program
    {
        public static Container Container { get; } = new Container();

        public static void Main(string[] args)
        {
            KeylolService.Run<PushHub>(args, Container);
        }
    }
}