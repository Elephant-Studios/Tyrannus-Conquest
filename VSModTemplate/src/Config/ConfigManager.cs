using System;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Datastructures;
using Vintagestory.ServerMods;
using static Ele.VSModTemplate.ModConstants;
using Ele.Configuration;


namespace Ele.VSModTemplate;

//Courtesy of https://github.com/jayugg/
public class ConfigManager : ModSystem
{
    private ICoreAPI _api;
    private static IClientNetworkChannel _clientChannel;
    private static IServerNetworkChannel _serverChannel;

    public static ModConfig LoadedConfig;
    /*public static ConfigServer ConfigServer { get; set; }
    public static ConfigClient ConfigClient { get; set; }
    public static SyncedConfig SyncedConfig { get; set; } = new();
    public static SyncedConfig SyncedConfigData => ConfigServer ?? SyncedConfig;*/


    public override double ExecuteOrder() => 0.02;
    public override void StartPre(ICoreAPI api)
    {
        LoadedConfig = ConfigHelper.ReadConfig<ModConfig>(api);

        if (api.ModLoader.IsModEnabled("configlib") && LoadedConfig.Is_Enabled)
        {
            _ = new ConfigLibCompat(api);
        }
    }

    public override void Start(ICoreAPI api)
    {
        _api = api;
    }

    #region Client
    public static void StartClientSide(ICoreClientAPI capi)
    {
        _clientChannel = capi.Network.RegisterChannel(mainChannel)
            .RegisterMessageType<SyncedConfig>()
            .SetMessageHandler<SyncedConfig>(ReloadSyncedConfig);
        capi.Event.RegisterEventBusListener(AdminSendSyncedConfig, filterByEventName: EventIDs.AdminSetConfig);
    }

    private static void AdminSendSyncedConfig(string eventname, ref EnumHandling handling, IAttribute data)
    {
        _clientChannel?.SendPacket(ConfigHelper.ReadConfig<SyncedConfig>(_api, ConfigHelper.GetConfigPath(_api, "ClientConfig")));
    }

    private static void ReloadSyncedConfig(SyncedConfig packet)
    {
        ModMain.Logger.Warning("Reloading synced config");
        ConfigHelper.WriteConfig(_api, BtConstants.SyncedConfigName, packet);
        SyncedConfig = packet.Clone();
        _api?.Event.PushEvent(EventIds.ConfigReloaded);
    }
    #endregion

    #region Server
    public static void StartServerSide(ICoreServerAPI sapi)
    {
        _serverChannel = sapi.Network.RegisterChannel(_channelName)
            .RegisterMessageType<SyncedConfig>()
        .SetMessageHandler<SyncedConfig>(ForceConfigFromAdmin);

        sapi.Event.PlayerJoin += SendSyncedConfig;
        sapi.Event.RegisterEventBusListener(SendSyncedConfig, filterByEventName: EventIds.ConfigReloaded);
    }

    private static void ForceConfigFromAdmin(IServerPlayer fromplayer, SyncedConfig packet)
    {
        if (fromplayer.HasPrivilege("controlserver"))
        {
            ModMain.Logger.Warning("Forcing config from admin");
            ConfigHelper.WriteConfig(_api, BtConstants.SyncedConfigName, packet.Clone());
            SyncedConfig = packet;
            _api?.Event.PushEvent(EventIds.ConfigReloaded);
        }
    }

    private static void SendSyncedConfig(string eventname, ref EnumHandling handling, IAttribute data)
    {
        ModMain.Logger.Warning("Config reloaded, sending to all players");
        if (_api?.World == null) return;
        foreach (var player in _api.World.AllPlayers)
        {
            if (player is not IServerPlayer serverPlayer) continue;
            SendSyncedConfig(serverPlayer);
        }
    }

    private static void SendSyncedConfig(IServerPlayer byplayer)
    {
        ModMain.Logger.Warning("Sending config to player: {0}", byplayer.PlayerName);
        _serverChannel?.SendPacket(ConfigHelper.ReadConfig<SyncedConfig>(_api, BtConstants.SyncedConfigName), byplayer);
    }
    #endregion
}