using AssettoServer.Commands;
using AssettoServer.Commands.Attributes;
using AssettoServer.Network.Tcp;
using AssettoServer.Server;
using AssettoServer.Shared.Network.Packets.Shared;
using Qmmands;
using Serilog;

namespace ChaseBattlePlugin;

public class ChaseCommands : ACModuleBase
{
    private readonly ChaseManager _chaseManager;
    private readonly EntryCarManager _entryCarManager;

    public ChaseCommands(ChaseManager chaseManager, EntryCarManager entryCarManager)
    {
        _chaseManager = chaseManager;
        _entryCarManager = entryCarManager;
    }

    [Command("chase"), RequireConnectedPlayer]
    public void ChaseCommand(int targetId)
    {
        if (targetId < 0 || targetId >= _entryCarManager.EntryCars.Length)
        {
            Client?.SendPacket(new ChatMessage { SessionId = 255, Message = "Invalid target ID." });
            return;
        }

        var targetCar = _entryCarManager.EntryCars[targetId];
        
        // Debug info
        Log.Information($"Chase Request: {Client?.Name} ({Client?.SessionId}) -> TargetID {targetId}");
        Log.Information($"Target Slot Status: Model={targetCar.Model}, AiControlled={targetCar.AiControlled}, HasClient={targetCar.Client != null}, ClientConnected={targetCar.Client?.IsConnected}");

        if (targetCar.AiControlled)
        {
             Client?.SendPacket(new ChatMessage { SessionId = 255, Message = "Cannot chase AI cars." });
             return;
        }

        var targetClient = targetCar.Client;

        if (targetClient == null || !targetClient.IsConnected)
        {
            Client?.SendPacket(new ChatMessage { SessionId = 255, Message = $"Target is not connected (ID: {targetId})." });
            return;
        }


        if (targetClient == Client)
        {
            Client?.SendPacket(new ChatMessage { SessionId = 255, Message = "You cannot chase yourself." });
            return;
        }

        if (_chaseManager.TryStartBattle(targetClient, Client!))
        {
            // Success message handled by ChaseManager broadcast
        }
        else
        {
            Client?.SendPacket(new ChatMessage { SessionId = 255, Message = "Could not start chase (players busy?)." });
        }
    }

    [Command("chasereport"), RequireConnectedPlayer]
    public void ChaseReportCommand(string result)
    {
        // Hidden command called by Lua
        _chaseManager.ReportResult(Client!, result);
    }

    [Command("chase_reset"), RequireAdmin]
    public void ChaseResetCommand()
    {
        _chaseManager.Reset();
        Client?.SendPacket(new ChatMessage { SessionId = 255, Message = "Chase manager reset." });
    }
}
