using static Ele.TyrannusConquest.ModConstants;
using Newtonsoft.Json;
using Vintagestory.API.Common;
using Ele.Configuration;


namespace Ele.VSModTemplate
{
    public class ModConfig : IModConfig
    {
        public bool Is_Enabled { get; set; }

        /*----------------
         * 
         * Add config fields here
         * 
         -----------------*/


        public ModConfig(ICoreAPI api, ModConfig previousConfig = null)
        {
            Is_Enabled = previousConfig?.Is_Enabled ?? true;

            //Initialize the rest of the fields here
        }
    }
}