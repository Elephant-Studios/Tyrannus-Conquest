using System;

namespace Ele.TyrannusConquest
{
    public static class ModConstants
    {
        public const string modName = "TyrannusConquest"; //<--Cannot contain spaces
        public const string modDomain = "tyrconquest"; //<--Cannot contain spaces
        public const string mainChannel = $"{modDomain}:main";
        public const string harmonyID = $"com.elephantstudios.{modDomain}";

        public const string langCodeEmpty = "Empty";

        public class EventIDs
        {
            public const string configReloaded = $"{modDomain}:configreloaded";
        }
    }
}
