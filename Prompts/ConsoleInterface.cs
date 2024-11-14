namespace filemeta.Prompts;

internal static class ConsoleInterface
{
    public static void Prompt()
    {
        var p = new DigitPrompt(Console.CursorLeft, value: 3);

        Console.CursorLeft += p.Length;

        ConsoleKeyInfo input;
        do
        {
            input = Console.ReadKey(intercept: true);
            p.HandleInput(input);
        } while (input.Key != ConsoleKey.Enter);
        Console.WriteLine();

        Console.WriteLine($"Result: {p.Value}");
    }
}
