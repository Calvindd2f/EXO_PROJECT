using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;

public class PowerShellExecutor : IDisposable
{
    private readonly RunspacePool _runspacePool;
    private readonly int _maxRunspaces;

    public PowerShellExecutor(int maxRunspaces = 5)
    {
        _maxRunspaces = maxRunspaces;
        _runspacePool = RunspaceFactory.CreateRunspacePool(1, _maxRunspaces);
        _runspacePool.Open();
    }

    public async Task<IEnumerable<PSObject>> ExecuteCommandAsync(string script, Dictionary<string, object> parameters = null)
    {
        using (var powerShell = PowerShell.Create())
        {
            powerShell.RunspacePool = _runspacePool;
            powerShell.AddScript(script);

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    powerShell.AddParameter(param.Key, param.Value);
                }
            }

            return await powerShell.InvokeAsync();
        }
    }

    public async Task<IEnumerable<PSObject>> ExecuteCommandsInParallelAsync(IEnumerable<string> scripts)
    {
        var tasks = new List<Task<IEnumerable<PSObject>>>();
        foreach (var script in scripts)
        {
            tasks.Add(ExecuteCommandAsync(script));
        }

        await Task.WhenAll(tasks);

        var results = new ConcurrentBag<PSObject>();
        foreach (var task in tasks)
        {
            foreach (var result in await task)
            {
                results.Add(result);
            }
        }

        return results;
    }

    public void Dispose()
    {
        _runspacePool?.Dispose();
    }
}