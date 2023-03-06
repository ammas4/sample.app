namespace AppSample.CoreTools.Logging;

internal struct CharsWalker
{
    readonly char[] _chars;
    int _curLine;
    int _curLinePosition;

    public CharsWalker(char[] chars)
    {
        _chars = chars;
        _curLine = 1;
        _curLinePosition = 0;
        Position = 0;
    }

    public int Position { get; internal set; }

    internal void GoTo(int line, int linePosition)
    {
        if (_curLine < line)
        {
            GoToLine(line);
            _curLinePosition = linePosition;
            Position += linePosition;
        }
        else
        {
            Position += linePosition - _curLinePosition;
            _curLinePosition = linePosition;
        }
    }

    internal void FindSymbol(char symbol)
    {
        if (_chars[Position] == symbol)
            return;
        while (true)
            if (_chars[++Position] == symbol)
                return;
    }

    internal void FindSymbolBackWard(char symbol)
    {
        if (_chars[Position] == symbol)
            return;
        while (true)
            if (_chars[--Position] == symbol)
                return;
    }

    void GoToLine(int line)
    {
        while (_curLine < line)
        {
            var ch = _chars[Position];
            if (ch == '\r' || ch == '\n')
            {
                if (ch == '\r' && _chars[Position + 1] == '\n')
                    Position++;
                _curLine++;
            }
            Position++;
        }
    }
}