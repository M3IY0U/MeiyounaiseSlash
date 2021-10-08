using MeiyounaiseSlash.Core;

namespace MeiyounaiseSlash
{
    class Program
    {
        static void Main(string[] args)
        {
            var b = new Bot();
            b.RunAsync().Wait();
        }
    }
}