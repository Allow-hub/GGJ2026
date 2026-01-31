using GGJ2026.InGame;

namespace GGJ2026.Events
{
    /// <summary>
    /// 強化イベント
    /// </summary>
    public readonly struct ImproveEvents
    {
        public readonly PlayerParam playerParam;
        public readonly int level;

        public ImproveEvents(PlayerParam playerParam, int level)
        {
            this.playerParam = playerParam;
            this.level = level;
        }
    }
}
