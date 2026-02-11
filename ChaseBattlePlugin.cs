using System.Reflection;
using AssettoServer.Server;
using AssettoServer.Server.Plugin;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace ChaseBattlePlugin;

public class ChaseBattlePlugin : BackgroundService
{
    private readonly ChaseManager _chaseManager;

    public ChaseBattlePlugin(ChaseManager chaseManager, CSPServerScriptProvider scriptProvider)
    {
        _chaseManager = chaseManager;
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ChaseBattlePlugin.lua.chase_battle.lua");
        if (stream != null)
             scriptProvider.AddScript(stream, "chase_battle.lua");
        else
             Log.Error("Could not find embedded resource: ChaseBattlePlugin.lua.chase_battle.lua");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Information("ChaseBattlePlugin Service Started.");
        return Task.CompletedTask;
    }


    public override Task StopAsync(CancellationToken cancellationToken)
    {
        Log.Information("ChaseBattlePlugin Service Stopped.");
        return Task.CompletedTask;
    }
}
