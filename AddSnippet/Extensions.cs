using System.Threading;

namespace AddSnippet;

public static class Extensions
{
    public static void RunSta(this Thread thread)
    {
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }
}
