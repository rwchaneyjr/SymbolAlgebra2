using System;
using System.Collections.Generic;
using System.Text;
using DragonBoxAlgebra.Core;
using UnityEngine;

namespace DragonBoxAlgebra.UI
{
    /// <summary>
    /// Loads custom creature art from Assets/DragonBoxAlgebra/Resources/CreatureSprites/.
    /// lightFish + darkFish, lightTurtle + darkTurtle, etc. (same animal, not frog/snake).
    /// </summary>
    public static class CardSpriteLoader
    {
        private const int CreatureFolderPriority = 100;
        private const int LegacyFolderPriority = 10;

        private static readonly Sprite[,] ThemedSprites = new Sprite[CreatureArt.ThemeCount, 2];
        private static readonly int[,] ThemedPriorities = new int[CreatureArt.ThemeCount, 2];
        private static Sprite _box;
        private static int _boxPriority;
        private static bool _initialized;

        private static readonly List<string> DebugFilesFound = new();
        private static readonly List<string> DebugRegistered = new();
        private static readonly List<string> DebugUnmatched = new();
        private static readonly Dictionary<string, int> DebugFolderCounts = new();

        public static int UnmatchedFileCount => DebugUnmatched.Count;

        private static readonly string[] CreatureSlugs =
        {
            "fish", "turtle", "clam", "dolphin", "eel", "lobster", "seahorse", "starfish"
        };

        /// <summary>Call each Play session so new/changed images are picked up.</summary>
        public static void Reset()
        {
            for (int row = 0; row < CreatureArt.ThemeCount; row++)
            {
                for (int col = 0; col < 2; col++)
                {
                    ThemedSprites[row, col] = null;
                    ThemedPriorities[row, col] = 0;
                }
            }

            _box = null;
            _boxPriority = 0;
            _initialized = false;
            DebugFilesFound.Clear();
            DebugRegistered.Clear();
            DebugUnmatched.Clear();
            DebugFolderCounts.Clear();
        }

        public static void EnsureLoaded()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;

            // CreatureSprites wins over any legacy Resources folders.
            LoadFolder("CreatureSprites", CreatureFolderPriority);
            LoadFolder("Sprites", LegacyFolderPriority);
            LoadFolder("CardSprites", LegacyFolderPriority);
            LoadFolder("Cards", LegacyFolderPriority);
        }

        public static void FlushLoadDebug()
        {
            // Called after EnsureLoaded so startup logging sees final state.
        }

        public static string GetLoadSummary()
        {
            var sb = new StringBuilder();
            foreach (KeyValuePair<string, int> entry in DebugFolderCounts)
            {
                sb.AppendLine($"  Resources/{entry.Key}: {entry.Value} asset(s)");
            }

            if (DebugFolderCounts.Count == 0)
            {
                sb.AppendLine("  (no Resources folders found — images must be in Resources/CreatureSprites/)");
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetSlotReport()
        {
            var sb = new StringBuilder();
            for (int theme = 0; theme < CreatureArt.ThemeCount; theme++)
            {
                string name = CreatureArt.ThemeNameFor(theme);
                Sprite light = GetThemed(theme, light: true);
                Sprite dark = GetThemed(theme, light: false);
                sb.AppendLine(
                    $"  {name,-10} light={(light != null ? light.name : "MISSING"),-14} dark={(dark != null ? dark.name : "MISSING")}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetFileReport()
        {
            if (DebugFilesFound.Count == 0)
            {
                return "  (none)";
            }

            var sb = new StringBuilder();
            foreach (string file in DebugFilesFound)
            {
                sb.AppendLine($"  {file}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetUnmatchedReport()
        {
            if (DebugUnmatched.Count == 0)
            {
                return "  (none)";
            }

            var sb = new StringBuilder();
            foreach (string file in DebugUnmatched)
            {
                sb.AppendLine($"  {file}  (rename like lightFish / darkFish)");
            }

            return sb.ToString().TrimEnd();
        }

        public static Sprite ForCard(BoardCard card)
        {
            EnsureLoaded();
            int theme = ResolveTheme(card);

            return card.Kind switch
            {
                CardKind.DayCreature => GetThemed(theme, light: true),
                CardKind.NightCreature => GetThemed(theme, light: false),
                CardKind.Box => _box,
                _ => null
            };
        }

        public static bool HasCustomArt(int theme, bool light)
        {
            EnsureLoaded();
            return GetThemed(theme, light) != null;
        }

        private static int ResolveTheme(BoardCard card) =>
            card.VisualTheme >= 0 ? card.VisualTheme : CreatureArt.ThemeIndex;

        private static Sprite GetThemed(int theme, bool light)
        {
            int row = NormalizeTheme(theme);
            int col = light ? 0 : 1;
            return ThemedSprites[row, col];
        }

        private static void LoadFolder(string folder, int priority)
        {
            int folderCount = 0;
            Sprite[] sprites = Resources.LoadAll<Sprite>(folder);
            Array.Sort(sprites, (a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
            foreach (Sprite sprite in sprites)
            {
                folderCount++;
                DebugFilesFound.Add($"{folder}/{sprite.name} (sprite)");
                RegisterByName(sprite, priority, folder);
            }

            Texture2D[] textures = Resources.LoadAll<Texture2D>(folder);
            Array.Sort(textures, (a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
            foreach (Texture2D texture in textures)
            {
                if (texture == null)
                {
                    continue;
                }

                bool alreadyLoadedAsSprite = false;
                foreach (Sprite sprite in sprites)
                {
                    if (sprite != null && sprite.name == texture.name)
                    {
                        alreadyLoadedAsSprite = true;
                        break;
                    }
                }

                if (alreadyLoadedAsSprite)
                {
                    continue;
                }

                folderCount++;
                DebugFilesFound.Add($"{folder}/{texture.name} (texture)");

                Sprite created = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100f);
                created.name = texture.name;
                RegisterByName(created, priority, folder);
            }

            DebugFolderCounts[folder] = folderCount;
        }

        private static void RegisterByName(Sprite sprite, int priority, string folder)
        {
            if (sprite == null)
            {
                return;
            }

            string name = sprite.name.ToLowerInvariant();

            if (IsBox(name))
            {
                if (_box == null || priority > _boxPriority)
                {
                    _box = sprite;
                    _boxPriority = priority;
                    DebugRegistered.Add($"{folder}/{sprite.name} -> box");
                }

                return;
            }

            if (TryParseThemePairName(name, out int theme, out bool light))
            {
                RegisterThemed(theme, light, sprite, priority);
                DebugRegistered.Add($"{folder}/{sprite.name} -> theme{theme}_{(light ? "light" : "dark")}");
                return;
            }

            if (TryParseCreatureSide(name, out string creature, out bool isLight))
            {
                int creatureTheme = ThemeForCreature(creature);
                if (creatureTheme >= 0)
                {
                    RegisterThemed(creatureTheme, isLight, sprite, priority);
                    DebugRegistered.Add(
                        $"{folder}/{sprite.name} -> {CreatureArt.ThemeNameFor(creatureTheme)} {(isLight ? "light" : "dark")}");
                    return;
                }
            }

            DebugUnmatched.Add($"{folder}/{sprite.name}");
        }

        private static bool TryParseThemePairName(string name, out int theme, out bool light)
        {
            theme = -1;
            light = false;

            if (!name.StartsWith("theme", StringComparison.Ordinal))
            {
                return false;
            }

            int underscore = name.IndexOf('_');
            if (underscore < 6)
            {
                return false;
            }

            string themeDigits = name.Substring(5, underscore - 5);
            if (!int.TryParse(themeDigits, out int parsedTheme))
            {
                return false;
            }

            string side = name.Substring(underscore + 1);
            if (side is "light" or "day")
            {
                theme = parsedTheme;
                light = true;
                return true;
            }

            if (side is "dark" or "night")
            {
                theme = parsedTheme;
                light = false;
                return true;
            }

            return false;
        }

        private static bool TryParseCreatureSide(string name, out string creature, out bool light)
        {
            creature = string.Empty;
            light = false;

            if (TryStripPrefix(name, "light_", out creature) || TryStripPrefix(name, "light", out creature))
            {
                light = true;
                creature = NormalizeCreature(creature);
                return creature.Length > 0;
            }

            if (TryStripPrefix(name, "dark_", out creature) || TryStripPrefix(name, "dark", out creature))
            {
                light = false;
                creature = NormalizeCreature(creature);
                return creature.Length > 0;
            }

            if (TryStripPrefix(name, "day_", out creature) || TryStripPrefix(name, "day", out creature))
            {
                light = true;
                creature = NormalizeCreature(creature);
                return creature.Length > 0;
            }

            if (TryStripPrefix(name, "night_", out creature) || TryStripPrefix(name, "night", out creature))
            {
                light = false;
                creature = NormalizeCreature(creature);
                return creature.Length > 0;
            }

            return false;
        }

        private static bool TryStripPrefix(string name, string prefix, out string remainder)
        {
            if (name.StartsWith(prefix, StringComparison.Ordinal))
            {
                remainder = name.Substring(prefix.Length);
                return true;
            }

            remainder = string.Empty;
            return false;
        }

        private static string NormalizeCreature(string creature)
        {
            if (string.IsNullOrEmpty(creature))
            {
                return creature;
            }

            if (creature.EndsWith("png", StringComparison.Ordinal))
            {
                creature = creature.Substring(0, creature.Length - 3);
            }

            if (creature == "eeel")
            {
                return "eel";
            }

            return creature;
        }

        private static int ThemeForCreature(string creature)
        {
            if (string.IsNullOrEmpty(creature))
            {
                return -1;
            }

            for (int i = 0; i < CreatureSlugs.Length; i++)
            {
                if (creature == CreatureSlugs[i])
                {
                    return i;
                }
            }

            int bestTheme = -1;
            int bestLength = 0;
            for (int i = 0; i < CreatureSlugs.Length; i++)
            {
                string slug = CreatureSlugs[i];
                if (creature.Contains(slug, StringComparison.Ordinal) && slug.Length > bestLength)
                {
                    bestTheme = i;
                    bestLength = slug.Length;
                }
            }

            return bestTheme;
        }

        private static void RegisterThemed(int theme, bool light, Sprite sprite, int priority)
        {
            int row = NormalizeTheme(theme);
            int col = light ? 0 : 1;
            if (priority < ThemedPriorities[row, col])
            {
                return;
            }

            ThemedSprites[row, col] = sprite;
            ThemedPriorities[row, col] = priority;
        }

        private static int NormalizeTheme(int theme) =>
            ((theme % CreatureArt.ThemeCount) + CreatureArt.ThemeCount) % CreatureArt.ThemeCount;

        private static bool IsBox(string name) =>
            name.Contains("box") || name.Contains("dragonbox");
    }
}
