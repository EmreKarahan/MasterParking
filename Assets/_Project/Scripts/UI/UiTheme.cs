using MasterParking.Utilities;
using UnityEngine;

namespace MasterParking.UI
{
    /// <summary>Shared palette and text sizes for every runtime-built screen.</summary>
    public static class UiTheme
    {
        public static readonly Color BackgroundTop = ColorUtil.Hex("#0b1626");
        public static readonly Color BackgroundBottom = ColorUtil.Hex("#18283f");
        public static readonly Color Panel = new Color(0.045f, 0.075f, 0.13f, 0.94f);
        public static readonly Color PanelLight = new Color(0.08f, 0.13f, 0.21f, 0.95f);
        public static readonly Color PanelSoft = new Color(0.10f, 0.15f, 0.23f, 0.82f);
        public static readonly Color Gold = ColorUtil.Hex("#f6c928");
        public static readonly Color GoldDark = ColorUtil.Hex("#b58a11");
        public static readonly Color Green = ColorUtil.Hex("#34d399");
        public static readonly Color GreenDark = ColorUtil.Hex("#0f766e");
        public static readonly Color Red = ColorUtil.Hex("#ef5350");
        public static readonly Color RedDark = ColorUtil.Hex("#991b1b");
        public static readonly Color Text = ColorUtil.Hex("#f8fafc");
        public static readonly Color Muted = ColorUtil.Hex("#94a3b8");
        public static readonly Color Button = ColorUtil.Hex("#1d3a5f");
        public static readonly Color ButtonHighlight = ColorUtil.Hex("#2b5688");
        public static readonly Color StarEmpty = ColorUtil.Hex("#324155");
        public static readonly Color Overlay = new Color(0.015f, 0.025f, 0.05f, 0.78f);
        public static readonly Color Transparent = new Color(0f, 0f, 0f, 0f);

        public const int TitleSize = 84;
        public const int HeadingSize = 40;
        public const int ButtonSize = 30;
        public const int BodySize = 24;
        public const int SmallSize = 19;
    }
}
