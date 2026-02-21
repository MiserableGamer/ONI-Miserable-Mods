# KIN Logic Terminal – Deep dive (from D:\mcp\oni-serena\indexed\reference\KIN\UndergroundConduit)

## 1. Animator.cs (KIN)

```csharp
public void Sim1000ms(float dt)
{
    bool isActive = this.operational.IsActive;
    if (isActive != this.lastOperational)
    {
        this.animator.Play(isActive ? "on" : "off", 1, 1f, 0f);
        this.lastOperational = isActive;
    }
}
```

- **Only runs Play when operational state changes** (`isActive != lastOperational`). No per-second Play, no visibility logic, no enabling/disabling GameObjects.
- Uses `[MyCmpReq] Operational` and `[MyCmpReq] KAnimControllerBase animator` on the same GameObject.
- The `Play("on"/"off")` call on layer 1 was making the building disappear on unpause (first sim tick after load). We now **skip** this for Logic Terminals via a prefix.

## 2. LogicTerminalConfig.cs (KIN)

- **CreateBuildingDef**: Uses `"uc_logic_terminal_kanim"` as the anim name. We **postfix** this to replace `AnimFiles` with our own kanim (`controlledmods_logic_terminal_kanim`).
- **DoPostConfigureComplete**: `AddOrGet<Animator>(go); AddOrGet<LogicTerminal>(go);` — same GameObject gets Animator then LogicTerminal.

## 3. LogicTerminal.cs (KIN)

- Extends **ChannelSelector** (not Terminal). No Sim1000ms.
- **OnSpawn**: `InitTerminal()` then `base.OnSpawn()`. `InitTerminal` sets `OutputCell = component.GetPowerOutputCell()`.
- **OutputCell** is a public int; we read it for logic network state.
- No segment or visibility logic here.

## 4. Why the building was disappearing

- KIN's `Animator.Sim1000ms` calls `Play("on"/"off", 1, 1f, 0f)` on the first sim tick after load. This was making the building disappear.
- Previously, we were also shipping the kanim with the **same ID** as KIN (`uc_logic_terminal_kanim`), which meant asset load order determined whose kanim the game used — unreliable.
- Additionally, the kanim files inside the folder were misnamed (`zLogicConduit*` instead of matching the folder name), so our kanim likely never loaded at all.

## 5. What we do now

- **CreateBuildingDef postfix** — replaces `AnimFiles` with `Assets.GetAnim("controlledmods_logic_terminal_kanim")`. Our kanim has a unique ID so it always loads alongside KIN's, no conflict.
- **Animator.Sim1000ms prefix** — returns `false` for Logic Terminals to skip KIN's `Play("on"/"off")` call which was causing the building to disappear.
- **Animator.Sim1000ms postfix** — still runs (even though original is skipped for Logic Terminals) to call `UpdateLogicTerminalLight(go)`.
- **DoPostConfigureComplete postfix** — adds our `LogicTerminalLightController`.
- **LogicTerminal OnSpawn postfix** — ensures component exists and applies initial tint.
- Our component uses LogicEvent + ISim1000ms and only does **SetSymbolTint("output", tint)**; we never call Play.

## 6. Kanim requirements

- Asset folder: `anim/assets/controlledmods_logic_terminal/` → game registers as `controlledmods_logic_terminal_kanim`.
- Files: `controlledmods_logic_terminal_0.png`, `controlledmods_logic_terminal_anim.bytes`, `controlledmods_logic_terminal_build.bytes`.
- Must have clips **"off"** and **"on"** (though KIN's Animator is skipped for Logic Terminals, the game/other systems may expect them).
- Must have symbol **"output"** for our tint (unlit / lit / red / green).
