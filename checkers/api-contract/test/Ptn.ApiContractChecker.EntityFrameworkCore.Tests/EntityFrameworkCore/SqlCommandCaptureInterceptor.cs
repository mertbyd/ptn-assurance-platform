using System.Collections.Concurrent;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Ptn.ApiContractChecker.EntityFrameworkCore;

// islevi: EF integration testlerinde calisan SQL komut metinlerini thread-safe bellekte yakalar.
// sistemdeki gorevi: Hafif okuma yollarinin agir kolonlari (run findings JSON'u, ham spec govdesi) secmedigini davranissal olarak kanitlar.
public class SqlCommandCaptureInterceptor : DbCommandInterceptor
{
    private readonly ConcurrentQueue<string> _commands = new();

    public IReadOnlyCollection<string> Commands => _commands.ToArray();

    // Yeni senaryo oncesi onceki SQL kanitlarini temizler.
    public void Clear()
    {
        _commands.Clear();
    }

    // Senkron reader sorgusunu calismadan once kaydeder.
    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        Capture(command);
        return result;
    }

    // Asenkron reader sorgusunu calismadan once kaydeder.
    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        Capture(command);
        return ValueTask.FromResult(result);
    }

    // Senkron scalar sorgusunu calismadan once kaydeder.
    public override InterceptionResult<object> ScalarExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result)
    {
        Capture(command);
        return result;
    }

    // Asenkron scalar sorgusunu calismadan once kaydeder.
    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        Capture(command);
        return ValueTask.FromResult(result);
    }

    // Tek SQL komut metnini yakalama kuyruguna ekler.
    private void Capture(DbCommand command)
    {
        _commands.Enqueue(command.CommandText);
    }
}
