# Initial D Chase Battle System - Implementation Guide

## Overview
This document provides a comprehensive guide for an AI (or human developer) to implement an "Initial D" style chase battle system for Assetto Corsa. The system consists of a server-side plugin for match management and a client-side Lua script for visual effects and local game logic.

## Goal
Enable two players to engage in a "Cat and Mouse" touge battle where the chaser must stay within a certain distance of the leader to win, or overtake them.

## Architecture

```mermaid
graph TD
    ClientA[Player A (Client)] <-->|Lua Script Visuals| ClientA_Lua
    ClientB[Player B (Client)] <-->|Lua Script Visuals| ClientB_Lua
    Server[AssettoServer] <-->|Plugin Logic| ChasePlugin
    ChasePlugin -- "Commands / State Updates" --> ClientA
    ChasePlugin -- "Commands / State Updates" --> ClientB
```

## 1. Server-Side Plugin (C#)

The server plugin manages the game state, handles commands, and broadcasts start/stop events.

### Prerequisites
- .NET 6.0 (or matching AssettoServer version)
- AssettoServer SDK references

### File Structure
```
ChaseBattlePlugin/
├── ChaseBattlePlugin.csproj
├── ChaseBattle.cs
├── ChaseCommands.cs
└── ChaseConfiguration.cs
```

### Reference Implementation

#### ChaseBattlePlugin.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="AssettoServer.Shared" />
    <Reference Include="AssettoServer" />
    <!-- Add other necessary references -->
  </ItemGroup>
</Project>
```

#### ChaseCommands.cs
```csharp
using AssettoServer.Commands;
using AssettoServer.Server;
using Qmmands;

public class ChaseCommands : ACModuleBase
{
    private readonly ChaseBattle _service;

    public ChaseCommands(ChaseBattle service)
    {
        _service = service;
    }

    [Command("chase")]
    public void StartChase(ACClient target)
    {
        // ACClient represents the player triggering the command
        // Logic to initiate a challenge to 'target'
        _service.InitiateChallenge(Context.Client, target);
    }
}
```

#### ChaseBattle.cs
```csharp
using AssettoServer.Server;
using AssettoServer.Server.Plugin;
using Microsoft.Extensions.Hosting;

public class ChaseBattle : BackgroundService
{
    private readonly ACServer _server;
    // State: Waiting, Countdown, Active, Finished
    
    public ChaseBattle(ACServer server)
    {
        _server = server;
    }

    public void InitiateChallenge(ACClient challenger, ACClient target)
    {
        // 1. Validate players (distance, car type)
        // 2. Send request to target (via chat)
        // 3. Set state to 'Pending'
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Main Game Loop (e.g., 10Hz)
            // 1. Check distance between active participants
            // 2. Broadcast distance/gap to clients via chat or custom packet
            // 3. Check win conditions (Overtake, Gap > Max, Gap < Min)
            
            await Task.Delay(100, stoppingToken);
        }
    }
}
```

## 2. Client-Side Script (Lua)

The client script renders the UI (bars, text) and can handle some local logic (like detecting overtake if server is slow, but server is authoritative).

### File Structure
```
apps/lua/chase_battle/
├── manifest.ini
└── chase_battle.lua
```

#### manifest.ini
```ini
[ABOUT]
NAME = Chase Battle
AUTHOR = AI Generator
VERSION = 1.0
DESCRIPTION = Initial D Style Chase Battle System
```

#### chase_battle.lua
```lua
local chaseMode = false
local opponentCarIndex = -1
local role = "none" -- "leader" or "chaser"

-- Listen for server messages (simplified via chat for now)
-- In a real implementation, use a custom protocol if available, or parse chat
function onChatMessage(message, sender)
    if message:find("CHASE_START") then
        local targetName = message:match("Target: (.+)")
        -- Find car index by name
        for i, c in ac.iterateCars() do 
            if c.driverName == targetName then
                opponentCarIndex = c.index
                chaseMode = true
            end
        end
    end
end

function script.update(dt)
    if not chaseMode or opponentCarIndex == -1 then return end

    local myCar = ac.getCar(0)
    local oppCar = ac.getCar(opponentCarIndex)

    local distance = (myCar.position - oppCar.position):length()
    
    -- Draw UI
    ui.beginTransparentWindow("chase_ui", vec2(100, 100), vec2(400, 200))
    ui.text("DISTANCE: " .. math.floor(distance) .. "m")
    
    -- TODO: Add "Initial D" style tachometer and tension bars
    ui.endWindow()
end
```

## 3. Communication Protocol

To ensure reliability, use the following chat command structure (hidden commands if possible):

1.  **Challenge**: `Server -> Target`: "Player A challenges you! Type /accept to start."
2.  **Start**: `Server -> All`: "CHASE_START: [DriverA] vs [DriverB]"
3.  **Update**: `Server -> Clients`: (Optional) "GAP: 5.2s" (broadcast every few seconds)
4.  **Finish**: `Server -> All`: "WINNER: [DriverA] (Reason: Overtake)"

## 4. Installation

1.  **Server**: Compile the C# plugin and place the DLL in `AssettoServer/plugins/`.
2.  **Client**: Place the `chase_battle` folder in `ticket/apps/lua/`.
3.  **Config**: Enable the plugin in `server_cfg.ini` (if required by AssettoServer).

## 5. Next Steps for AI

1.  Refine `ChaseBattle.cs` to handle the state machine (Pending -> Countdown -> Racing).
2.  Enhance `chase_battle.lua` with `ac.getCarState` for more telemetry (RPM, Boost).
3.  Implement "Sudden Death" mechanics (if gap stays small for X seconds).
