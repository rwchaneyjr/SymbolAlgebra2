using System.Collections.Generic;
using System.Text;
using DragonBoxAlgebra.Core;
using DragonBoxAlgebra.Gameplay;
using UnityEngine;

namespace DragonBoxAlgebra.UI
{
    /// <summary>
    /// Console diagnostics for custom creature sprite loading.
    /// Filter Console by "DragonBox" to see only these messages.
    /// </summary>
    public static class CreatureSpriteDebug
    {
        public static bool Enabled = true;

        public static int LoadedSpriteCount => CountLoadedSprites();

        public static bool HasOldGameCode =>
            LevelLibrary.Count > 0 && IsOldCodeLevelTitle(LevelLibrary.GetLevel(0).Title);

        public static bool HasProblems =>
            HasOldGameCode || LoadedSpriteCount < 16;

        /// <summary>Short message shown on screen when images won't load.</summary>
        public static string GetOnScreenMessage()
        {
            int loaded = LoadedSpriteCount;
            if (HasOldGameCode)
            {
                return $"IMAGE DEBUG: Old code (Butterfly/Bat). Run: bash scripts/update.sh — sprites {loaded}/16";
            }

            if (loaded == 0)
            {
                return "IMAGE DEBUG: 0/16 sprites loaded. Put PNGs in Resources/CreatureSprites, set Sprite (2D and UI).";
            }

            if (loaded < 16)
            {
                return $"IMAGE DEBUG: Only {loaded}/16 sprites loaded. Check Console (filter: DragonBox) for missing names.";
            }

            return string.Empty;
        }

        public static void LogStartup()
        {
            if (!Enabled)
            {
                return;
            }

            CardSpriteLoader.EnsureLoaded();
            CardSpriteLoader.FlushLoadDebug();

            var lines = new StringBuilder();
            lines.AppendLine("========== DragonBox Sprite Debug ==========");

            int loaded = CountLoadedSprites();
            lines.AppendLine($"Custom sprites loaded: {loaded}/16");
            lines.AppendLine($"Folder: Assets/DragonBoxAlgebra/Resources/CreatureSprites/");
            lines.AppendLine(CardSpriteLoader.GetLoadSummary());

            if (LevelLibrary.Count > 0)
            {
                LevelDefinition level1 = LevelLibrary.GetLevel(0);
                lines.AppendLine($"Level 1 title: '{level1.Title}'");
                lines.AppendLine($"Level 1 theme: {CreatureArt.ThemeNameFor(level1.CreatureTheme)} (index {level1.CreatureTheme})");

                if (IsOldCodeLevelTitle(level1.Title))
                {
                    lines.AppendLine("WARNING: Old game code detected (Butterfly/Bat/Ch1).");
                    lines.AppendLine("Fix: run bash scripts/update.sh in Git Bash, then restart Unity.");
                }
                else if (loaded == 0)
                {
                    lines.AppendLine("WARNING: No sprites loaded. Check folder + Sprite (2D and UI) import.");
                }
                else if (loaded < 16)
                {
                    lines.AppendLine("WARNING: Some sprites missing — see slot list below.");
                }
                else
                {
                    lines.AppendLine("OK: New curriculum build with full sprite set.");
                }
            }

            lines.AppendLine("--- Registered slots ---");
            lines.AppendLine(CardSpriteLoader.GetSlotReport());
            lines.AppendLine("--- Files in Resources ---");
            lines.AppendLine(CardSpriteLoader.GetFileReport());

            if (CardSpriteLoader.UnmatchedFileCount > 0)
            {
                lines.AppendLine("--- Unmatched file names (fix these) ---");
                lines.AppendLine(CardSpriteLoader.GetUnmatchedReport());
            }

            lines.AppendLine("===========================================");

            string report = lines.ToString();
            if (HasProblems)
            {
                Debug.LogWarning(report);
            }
            else
            {
                Debug.Log(report);
            }
        }

        public static void LogLevel(AlgebraBoard board, IReadOnlyList<BoardCard> hand, LevelDefinition level)
        {
            if (!Enabled)
            {
                return;
            }

            var lines = new StringBuilder();
            lines.AppendLine($"[DragonBox] Level {level.Title} — card sprites:");

            LogSide(lines, "Left", board.Left.Cards);
            LogSide(lines, "Right", board.Right.Cards);
            LogSide(lines, "Hand", hand);

            Debug.Log(lines.ToString());
        }

        private static void LogSide(StringBuilder lines, string sideName, IReadOnlyList<BoardCard> cards)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                BoardCard card = cards[i];
                if (card.Kind is not (CardKind.DayCreature or CardKind.NightCreature or CardKind.Box))
                {
                    continue;
                }

                Sprite custom = CardSpriteLoader.ForCard(card);
                string themeName = CreatureArt.ThemeNameFor(
                    card.VisualTheme >= 0 ? card.VisualTheme : CreatureArt.ThemeIndex);
                string side = CardFlipRules.IsLight(card) ? "light" : "dark";

                if (custom != null)
                {
                    lines.AppendLine(
                        $"  {sideName}[{i}] {card.Kind} theme={card.VisualTheme}({themeName}) {side} -> CUSTOM '{custom.name}'");
                }
                else
                {
                    lines.AppendLine(
                        $"  {sideName}[{i}] {card.Kind} theme={card.VisualTheme}({themeName}) {side} -> FALLBACK (no PNG loaded)");
                }
            }
        }

        private static int CountLoadedSprites()
        {
            int loaded = 0;
            for (int theme = 0; theme < CreatureArt.ThemeCount; theme++)
            {
                if (CardSpriteLoader.HasCustomArt(theme, light: true))
                {
                    loaded++;
                }

                if (CardSpriteLoader.HasCustomArt(theme, light: false))
                {
                    loaded++;
                }
            }

            return loaded;
        }

        private static bool IsOldCodeLevelTitle(string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                return false;
            }

            return title.Contains("Ch1")
                   || title.Contains("Butterfly")
                   || title.Contains("Bat")
                   || title.Contains("Matching Pairs");
        }
    }
}
