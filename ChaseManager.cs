using AssettoServer.Network.Tcp;
using AssettoServer.Server;
using AssettoServer.Server.Plugin;
using AssettoServer.Shared.Network.Packets.Shared;
using Serilog;

namespace ChaseBattlePlugin;

public class ChaseManager
{
    private readonly EntryCarManager _entryCarManager;
    
    // Key: Leader SessionID, Value: ChaseBattle
    private readonly Dictionary<int, ChaseBattle> _activeBattles = new();

    private ACTcpClient? _leader;
    private ACTcpClient? _chaser;
    private int _currentState = 0; // 0:Idle, 1:Countdown, 2:Active, 3:Finished

    public ChaseManager(EntryCarManager entryCarManager)
    {
        _entryCarManager = entryCarManager;
        _entryCarManager.ClientDisconnected += OnClientDisconnected;
    }

    private void OnClientDisconnected(ACTcpClient client, EventArgs args)
    {
        // Check if client was in a battle
        var battle = _activeBattles.Values.FirstOrDefault(b => b.Leader == client || b.Chaser == client);
        if (battle == null) return;

        _activeBattles.Remove(battle.Leader.SessionId);
        Log.Information($"Chase Battle Ended: {client.Name} disconnected.");

        // Notify other player
        var otherPlayer = battle.Leader == client ? battle.Chaser : battle.Leader;
        if (otherPlayer.IsConnected)
        {
             // Disconnection Win Logic
             string role = battle.Leader == client ? "Leader" : "Chaser";
             _entryCarManager.BroadcastPacket(new ChatMessage { SessionId = 255, Message = $"Chase Result: {otherPlayer.Name} WON! (Opponent Disconnected)" });
             otherPlayer.SendPacket(new ChatMessage { SessionId = 255, Message = $"CHASE_END: {battle.Leader.SessionId}" }); 
        }
    }

    private void BroadcastProtocol(string opcode, string payload = "")
    {
        var msg = $"CHASE_BATTLE:{opcode}:{payload}";
        _entryCarManager.BroadcastPacket(new ChatMessage { SessionId = 255, Message = msg });
    }

    public void Reset()
    {
        _activeBattles.Clear();
        _leader = null;
        _chaser = null;
        _currentState = 0;
        Log.Information("Chase Manager Reset by Admin.");
        _entryCarManager.BroadcastPacket(new ChatMessage { SessionId = 255, Message = "Chase Battle System has been RESET by Admin." });
        BroadcastProtocol("STATE", "0");
    }

    public void SetContestants(ACTcpClient leader, ACTcpClient chaser)
    {
        _leader = leader;
        _chaser = chaser;
        _currentState = 0;
        BroadcastProtocol("SETUP", $"{leader.SessionId},{chaser.SessionId}");
    }

    public void StartBattle()
    {
        if (_leader == null || _chaser == null) return;
        
        _currentState = 1;
        BroadcastProtocol("START", "");
        
        // Let Lua handle the countdown and Go logic, or we can handle it here.
        // If Lua does it automatically after START, we just wait for RESULT.
        // Wait, the TryStartBattle was adding to _activeBattles for disconnect logic, let's keep that
        _activeBattles.Clear();
        _activeBattles.Add(_leader.SessionId, new ChaseBattle(_leader, _chaser));
    }

    public bool TryStartBattle(ACTcpClient leader, ACTcpClient chaser)
    {
        if (_activeBattles.ContainsKey(leader.SessionId) ||
            _activeBattles.Values.Any(b => b.Chaser == chaser || b.Leader == chaser))
        {
            return false;
        }

        SetContestants(leader, chaser);
        StartBattle();

        return true;
    }

    public void ReportResult(ACTcpClient reporter, string result)
    {
        // Find battle where reporter is involved
        var battle = _activeBattles.Values.FirstOrDefault(b => b.Leader == reporter || b.Chaser == reporter);
        if (battle == null) return;
        
        // Ensure only one result is processed
        _activeBattles.Remove(battle.Leader.SessionId);
        
        if (result == "DRAW")
        {
            Log.Information($"Chase Draw: {battle.Leader.Name} vs {battle.Chaser.Name}");
            _entryCarManager.BroadcastPacket(new ChatMessage { SessionId = 255, Message = "Chase Result: DRAW! Swapping roles..." });
            
            // Auto-Swap and Restart
            bool success = TryStartBattle(battle.Chaser, battle.Leader);
            if (!success)
            {
                 _entryCarManager.BroadcastPacket(new ChatMessage { SessionId = 255, Message = "Could not auto-start swap battle." });
            }
        }
        else if (result == "GIVEUP")
        {
            Log.Information($"Chase Forfeit: {reporter.Name} gave up.");
            var winner = battle.Leader == reporter ? battle.Chaser : battle.Leader;
            string message = $"Chase Result: {winner.Name} WON! ({reporter.Name} gave up)";
            _entryCarManager.BroadcastPacket(new ChatMessage { SessionId = 255, Message = message });
            BroadcastProtocol("RESULT", "GIVEUP");
        }
        else
        {
            string message = result == "WIN" 
                ? $"Chase Result: {battle.Chaser.Name} CAUGHT {battle.Leader.Name}!" 
                : $"Chase Result: {battle.Leader.Name} ESCAPED from {battle.Chaser.Name}!";
                
            Log.Information($"Chase Ended: {message}");
            _entryCarManager.BroadcastPacket(new ChatMessage { SessionId = 255, Message = message });
            BroadcastProtocol("RESULT", result);
        }

        _currentState = 0;
    }
}

public class ChaseBattle
{
    public ACTcpClient Leader { get; }
    public ACTcpClient Chaser { get; }
    public DateTime StartTime { get; }

    public ChaseBattle(ACTcpClient leader, ACTcpClient chaser)
    {
        Leader = leader;
        Chaser = chaser;
        StartTime = DateTime.UtcNow;
    }
}
