
using System;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace ExternalLocalizerJpPack;

internal class UIHoverImage : UIImage
{
    public Func<string>? TooltipTextGenerator = null;
    private string _tooltipText
    {
        get
        {
            if (string.IsNullOrEmpty(field) && this.TooltipTextGenerator != null)
                field = this.TooltipTextGenerator.Invoke();
            return field ?? string.Empty;
        }
        set => field = value;
    }

    public UIHoverImage(Asset<Texture2D> texture) : base(texture) { }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        if (this.IsMouseHovering)
            UICommon.TooltipMouseText(this._tooltipText);
    }

    public override void MouseOut(UIMouseEvent evt)
    {
        base.MouseOut(evt);
        this._tooltipText = string.Empty;
    }
}
