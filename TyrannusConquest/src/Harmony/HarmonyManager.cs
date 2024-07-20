using System;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using HarmonyLib;

namespace Ele.VSModTemplate;

public class HarmonyManager : ModSystem
{
    private ICoreAPI _api;
    private Harmony _harmony;
    

    public override double ExecuteOrder() => 0.03;
    public override void Start(ICoreAPI api)
    {
        _api = api;
        _harmony = new Harmony(ModConstants.harmonyID);
        _harmony?.PatchAll();
    }

    public override void StartServerSide(ICoreServerAPI sapi)
    {
        base.StartServerSide(sapi);
    }

    public override void StartClientSide(ICoreClientAPI capi)
    {
        base.StartClientSide(capi);
    }

    public override void Dispose()
    {
        _harmony?.UnpatchAll(ModConstants.harmonyID);
    }
}