using System;

namespace Ele.VSModTemplate
{
    public static class ModConstants
    {
        public const string modName = "VSModTemplate"; //<--Cannot contain spaces
        public const string modDomain = "vsmodtemplate"; //<--Cannot contain spaces
        public const string mainChannel = $"{modDomain}:main";
        public const string harmonyID = $"com.elephantstudios.{modDomain}";

        public const string langCodeEmpty = "Empty";

        public class EventIDs
        {
            public const string configReloaded = $"{modDomain}:configreloaded";
        }
    }
}
