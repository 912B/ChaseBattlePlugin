# Implementation Plan - Initial D Chase Battle System

## Goal
Implement a Touge Chase Battle system where the server manages matchups and clients handle physics/distance calculations and visual effects.

## User Review Required
> [!IMPORTANT]
> **Architecture Decision**: We will use a **Client-Authoritative** model.
> *   **Clients** (Lua) have access to high-frequency physics data (positions, splines) of all cars. They will calculate the "Gap" and "Win Conditions" locally.
> *   **Server** (C#) acts as a **Matchmaker** (pairing players) and **Announcer** (broadcasting results).
> *   **Reasoning**: Sending 60Hz physics data to the server via chat/packets is impractical. Assetto Corsa clients already know where other cars are.

## Proposed Changes

### 1. Server-Side Plugin (C#)
**Path**: `/home/f13/workspace/initial-d-chase-battle/server/ChaseBattlePlugin/`

#### [NEW] `ChaseBattlePlugin.csproj`
*   Standard AssettoServer plugin project.

#### [NEW] `ChaseManager.cs`
*   **State**: `Dictionary<int, int>` (DriverA -> DriverB) to track active battles.
*   **Logic**:
    *   `StartBattle(driverA, driverB)`: Broadcasts start message.
    *   `StopBattle(driverA, winner, reason)`: Broadcasts result.
    *   **Game Flow**:
        *   Track current Round (1, 2, SuddenDeath).
        *   If `Draw` -> Prompt "Swap Positions" -> Restart Battle with swapped roles.

#### [NEW] `ChaseCommands.cs`
*   `/chase start <target>`: Initiates a challenge.
*   `/chase report <winner> <reason>`: (Hidden) Clients send this when they detect a win.
*   `/admin chase reset`: Force reset.

### 2. Client-Side Script (Lua)
**Path**: `/home/f13/workspace/initial-d-chase-battle/client/lua/chase_battle/`

#### [NEW] `manifest.ini`
*   Metadata for the app.

#### [NEW] `chase_battle.lua`
*   **HUD**: "Street Fighter" style distance bar, Role Icon (Lead/Chase).
*   **Logic**:
    *   Listen for `CHASE_START` chat message.
    *   In `script.update`: Calculate Spline Distance to opponent.
    *   **Adjudication**:
        *   **Automatic (System Advice)**: If `Distance > 150m` (Pull Away) or `Overtake` -> Show "Claim Win" button to user (or auto-report if configured).
        *   **Manual**: User clicks "Claim Win" -> Send `/chase claim <reason>`.
        *   **Opponent**: Sees "Opponent Claims Win", clicks "Confirm" or "Protest".
    *   **Admin Panel**: `ui.window` with player list and start buttons.

## Verification Plan

### Automated Tests
*   N/A (Requires running game instance).

### Manual Verification
1.  **Deployment**:
    *   User copies Server DLL to server.
    *   User copies Client Lua to game folder (or configures CSP remote load).
2.  **Game Test**:
    *   **Admin Panel**: Open Sidebar App "Chase Admin", select two AI/Players, click Start.
    *   **HUD Check**: Verify "Distance Bar" appears on both clients.
    *   **Win Check**:
        *   Drive Leader away > 150m -> Verify System Broadcast "Leader Wins".
        *   Chaser overtakes -> Verify System Broadcast "Chaser Wins".
