#if NET8_0_OR_GREATER
using System.Threading;
using System.Threading.Tasks;

using RiceTea.Core.Text;

namespace RiceTea.Core.IO.Internals;

partial class NullStreamReader : IStreamReader
{
    public ValueTask<int> PeekAsync(CancellationToken token) 
        => ValueTask.FromResult(-1);

    public ValueTask<int> ReadAsync(CancellationToken token) 
        => ValueTask.FromResult(-1);

    public ValueTask<string?> ReadLineAsync(CancellationToken token) 
        => ValueTask.FromResult<string?>(null);

    public ValueTask<StringWrapper?> ReadLineAsStringWrapperAsync(CancellationToken token) 
        => ValueTask.FromResult<StringWrapper?>(null);

    public ValueTask<string> ReadToEndAsync(CancellationToken token)
        => ValueTask.FromResult(string.Empty);

    public ValueTask<StringWrapper> ReadToEndAsStringWrapperAsync(CancellationToken token)
        => ValueTask.FromResult(StringWrapper.Empty);
}
#endif