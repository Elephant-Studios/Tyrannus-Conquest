using static Ele.VSModTemplate.ModConstants;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Ele.VSModTemplate
{
    public class ModMain : ModSystem
    {
        private ICoreAPI _api = null!;
        private ICoreServerAPI _sapi = null!;
        private ICoreClientAPI _capi = null!;


        public override double ExecuteOrder() => 0.01;
        public override void StartPre(ICoreAPI api)
        {
            base.StartPre(api);
            _api = api;

        }

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
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
        }
    }
}
