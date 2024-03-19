using Terminal.Gui;

namespace ConsoleApp;

internal partial class Program
{
    private static (
        Window inputWindot,
        Window gpggaWindow,
        Window gpgsaWindow,
        Window gpgllWindow,
        Window gpgsvWindow
        )
        CreateWindows()
    {
        var top = Application.Top;

        var inputWindow = new Window("Input")
            { X = 0, Y = 1, Width = Dim.Fill(), Height = 10 };
        var gpggaWindow = new Window("GPGGA Data")
            { X = 0, Y = Pos.Bottom(inputWindow), Width = Dim.Fill(), Height = 13 };
        var gpgsaWindow = new Window("GPGSA Data")
            { X = 0, Y = Pos.Bottom(gpggaWindow), Width = Dim.Fill(), Height = 10 };
        var gpgllWindow = new Window("GPGLL Data")
            { X = 0, Y = Pos.Bottom(gpgsaWindow), Width = Dim.Fill(), Height = 9 };
        var gpgsvWindow = new Window("GPGSV Data")
            { X = 0, Y = Pos.Bottom(gpgllWindow), Width = Dim.Fill(), Height = Dim.Fill() };
        var menu = new MenuBar(new MenuBarItem[]
        {
            new MenuBarItem("_File", new MenuItem[]
            {
                new MenuItem("_Quit", "", () => { Application.RequestStop(); })
            }),
        });

        top.Add(menu, inputWindow, gpggaWindow, gpgsaWindow, gpgllWindow, gpgsvWindow);
        return (inputWindow, gpggaWindow, gpgsaWindow, gpgllWindow, gpgsvWindow);
    }
}