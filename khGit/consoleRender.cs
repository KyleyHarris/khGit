using System;

public class ConsoleRender
{
    public ConsoleRender()
    {
    }
    public int MarginX = 0;

    public void CR()
    {
        Console.WriteLine();
        Console.CursorLeft = MarginX;
    }

    public void Clear() => Console.Clear();

    public void Write(string text, ConsoleColor color = ConsoleColor.White, ConsoleColor back = ConsoleColor.Black)
    {
        var oldColor = Console.ForegroundColor;
        var oldBack = Console.BackgroundColor;
        Console.ForegroundColor = color;
        Console.BackgroundColor = back;
        Console.Write(text);
    }
    public void WriteLn(string text="", ConsoleColor color = ConsoleColor.White, ConsoleColor back = ConsoleColor.Black)
    {
        this.Write(text, color, back);
        this.CR();
    }

    public void MoveXY(int x,int y)
    {
        Console.CursorLeft = x;
        Console.CursorTop = y;
    }

}
