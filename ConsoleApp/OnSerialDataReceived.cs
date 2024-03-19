using Terminal.Gui;

namespace ConsoleApp;

internal partial class Program
{
    private static void OnSerialDataReceived(SerialInput serialInput, Window inputWindow)
    {
        serialInput.DataReceived += (sender, e) =>
        {
            Application.MainLoop.Invoke(() =>
            {
                inputWindow.RemoveAll();
                inputWindow.Add(new Label(e.Data));
            });
        };
    }
}