using static Ele.TyrannusConquest.ModConstants;
using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Ele.Configuration;

namespace Ele.TyrannusConquest
{
    [HarmonyPatch]
    public class ModMain : ModSystem
    {
        private ICoreAPI _api = null!;
        private ICoreServerAPI _sapi = null!;
        private ICoreClientAPI _capi = null!;
        public Harmony harmony;
        public static ModConfig LoadedConfig;
        protected const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        public static bool purgeWpGroups = false;


        public override double ExecuteOrder() => 0.01;
        public override void StartPre(ICoreAPI api)
        {
            base.StartPre(api);
            _api = api;

        }

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            LoadedConfig = ConfigHelper.ReadConfig<ModConfig>(api);
            api.RegisterBlockEntityClass("cartography-table-entity", typeof(BlockEntityCartographyTable));
            api.RegisterBlockClass("cartography-table", typeof(BlockCartographyTable));
            /*
             * "tyrconquest.cartography-table",
             * "entityClass": "tyrconquest.cartography-table-entity",
             */
        }
        public override void AssetsFinalize(ICoreAPI api)
        {
            base.AssetsFinalize(api);
        }

        #region Server
        public override void StartServerSide(ICoreServerAPI sapi)
        {
            base.StartServerSide(sapi);
            _sapi = sapi;
            sapi.ChatCommands.Create("purgewpgroups")
            .WithDescription("removes groups from all the waypoints created by other mods on the next cartography table interaction")
            .RequiresPrivilege(Privilege.chat)
            .RequiresPlayer()
            .HandleWith((args) => {
                purgeWpGroups = true;
                return TextCommandResult.Success("Groups set to be purged from all waypoints. Interact with a cartography table to apply.");
            });
        }
        #endregion

        #region Client
        public override void StartClientSide(ICoreClientAPI capi)
        {
            base.StartClientSide(capi);
            _capi = capi;
        }
        #endregion

        public override void Dispose()
        {
            base.Dispose();
            if (_capi != null)
            {
                _capi = null;
            }
            _api = null;
            _sapi = null;
            harmony?.UnpatchAll(Mod.Info.ModID);
        }
    }
}