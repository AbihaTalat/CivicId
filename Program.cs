
namespace CivicID
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            CivicID.Data.DatabaseService.Initialize();

            Application.Run(new Form1());
        }
    }
}
