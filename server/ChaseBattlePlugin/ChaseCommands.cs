using AssettoServer.Commands;
using AssettoServer.Commands.Attributes;
using AssettoServer.Network.Tcp;
using Qmmands;

namespace ChaseBattlePlugin;

public class ChaseCommands : ACModuleBase
{
    private readonly ChaseManager _chaseManager;

    public ChaseCommands(ChaseManager chaseManager)
    {
        _chaseManager = chaseManager;
    }

    [Command("chase"), RequireConnectedPlayer]
    public void ChaseCommand(int targetId)
    {
        // Placeholder implementation
        // var target = ... (logic to find target by ID)
        // _chaseManager.TryStartBattle(Client!.SessionId, targetId);
        Client?.SendChatMessage($"Chase command received for target {targetId}");
    }
}
