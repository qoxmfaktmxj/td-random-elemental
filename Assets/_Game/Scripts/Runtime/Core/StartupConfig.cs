using UnityEngine;

namespace TdRandomElemental.Core
{
    public static class StartupConfig
    {
        public const string BootstrapSceneName = "Bootstrap";
        public const string BootstrapScenePath = "Assets/_Game/Scenes/Bootstrap/Bootstrap.unity";

        public const string MvpArenaSceneName = "MVP_Arena";
        public const string MvpArenaScenePath = "Assets/_Game/Scenes/MVP_Arena/MVP_Arena.unity";

        public static readonly string[] RequiredFolders =
        {
            "Assets/_Game/Art/Characters",
            "Assets/_Game/Art/Enemies",
            "Assets/_Game/Art/Environment",
            "Assets/_Game/Art/Towers",
            "Assets/_Game/Art/VFX",
            "Assets/_Game/Art/Materials",
            "Assets/_Game/Audio/BGM",
            "Assets/_Game/Audio/SFX",
            "Assets/_Game/Data/Balance",
            "Assets/_Game/Data/Elements",
            "Assets/_Game/Data/Towers",
            "Assets/_Game/Data/Enemies",
            "Assets/_Game/Data/Waves",
            "Assets/_Game/Prefabs/Characters",
            "Assets/_Game/Prefabs/Enemies",
            "Assets/_Game/Prefabs/Towers",
            "Assets/_Game/Prefabs/World",
            "Assets/_Game/Prefabs/UI",
            "Assets/_Game/Scenes/Bootstrap",
            "Assets/_Game/Scenes/MVP_Arena",
            "Assets/_Game/Scripts/Runtime/Core",
            "Assets/_Game/Scripts/Runtime/Character",
            "Assets/_Game/Scripts/Runtime/Board",
            "Assets/_Game/Scripts/Runtime/Economy",
            "Assets/_Game/Scripts/Runtime/Summoning",
            "Assets/_Game/Scripts/Runtime/Towers",
            "Assets/_Game/Scripts/Runtime/Elements",
            "Assets/_Game/Scripts/Runtime/Enemies",
            "Assets/_Game/Scripts/Runtime/Waves",
            "Assets/_Game/Scripts/Runtime/UI",
            "Assets/_Game/Scripts/Runtime/Presentation",
            "Assets/_Game/Scripts/Runtime/Debug",
            "Assets/_Game/Scripts/Editor",
            "Assets/_Game/UI/Fonts",
            "Assets/_Game/UI/Sprites",
            "Assets/_Game/UI/Layouts"
        };

        public static readonly Vector3 ArenaCameraPosition = new Vector3(14f, 16f, -14f);
        public static readonly Vector3 ArenaCameraEulerAngles = new Vector3(42f, 315f, 0f);
        public static readonly Vector3 ArenaGroundPosition = new Vector3(0f, -0.5f, 0f);
        public static readonly Vector3 ArenaGroundScale = new Vector3(24f, 1f, 24f);
        public static readonly Color CameraBackgroundColor = new Color(0.08f, 0.1f, 0.14f, 1f);
    }
}
