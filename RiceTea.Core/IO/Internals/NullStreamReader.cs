using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using RiceTea.Core.Text;

namespace RiceTea.Core.IO.Internals;

internal sealed partial class NullStreamReader : IStreamReader
{
    public Stream BaseStream => Stream.Null;

    public Encoding CurrentEncoding => Encoding.Default;

    public bool EndOfStream => true;

    public NullStreamReader() => GC.SuppressFinalize(this);

    public int Peek()
        => -1;

    public Task<int> PeekAsync()
        => Task.FromResult(-1);

    public int Read()
        => -1;

    public Task<int> ReadAsync()
        => Task.FromResult(-1);

    public string? ReadLine()
        => null;

    public StringWrapper? ReadLineAsStringWrapper()
        => null;

    public Task<string?> ReadLineAsync()
        => Task.FromResult<string?>(null);

    public Task<StringWrapper?> ReadLineAsStringWrapperAsync()
        => Task.FromResult<StringWrapper?>(null);

    public string ReadToEnd()
        => string.Empty;

    public StringWrapper ReadToEndAsStringWrapper()
        => StringWrapper.Empty;

    public Task<string> ReadToEndAsync() 
        => Task.FromResult(string.Empty);

    public Task<StringWrapper> ReadToEndAsStringWrapperAsync()
        => Task.FromResult(StringWrapper.Empty);

    public void Dispose() { }
}
