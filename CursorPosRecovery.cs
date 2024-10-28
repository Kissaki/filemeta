namespace filemeta;

public sealed class CursorPosRecovery : IDisposable
{
    private readonly int _pos;

    public CursorPosRecovery() => _pos = Console.CursorLeft;
    public void Dispose() => Console.CursorLeft = _pos;
}
