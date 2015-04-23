using PeanutButter.ServiceShell;

namespace AutoHibernater
{
    static class Program
    {
        static int Main(string[] args)
        {
            return Shell.RunMain<ServiceMain>(args);
        }
    }
}
