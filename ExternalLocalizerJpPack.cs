using Terraria.ModLoader;

namespace ExternalLocalizerJpPack;

public class ExternalLocalizerJpPack : Mod
{
    internal static ExternalLocalizerJpPack Instance { get; private set; } = null!;

    public ExternalLocalizerJpPack()
    {
        Instance = this;
    }
}
