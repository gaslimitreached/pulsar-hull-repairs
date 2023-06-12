using PulsarModLoader;

namespace HullRepairs
{
    public class Mod : PulsarMod
    {
        public override string Version => "0.0";
        public override string Author => "System32";
        public override string Name => "HullRepairs";
        public override string HarmonyIdentifier() => $"{Author}.{Name}";
        public override string ShortDescription => "Repair the Hull";
        public override string LongDescription => "Licence: DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE";

    }
}
