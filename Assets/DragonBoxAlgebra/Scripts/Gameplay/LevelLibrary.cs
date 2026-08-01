using System.Collections.Generic;

namespace DragonBoxAlgebra.Gameplay
{
    public static class LevelLibrary
    {
        private static readonly LevelDefinition[] Templates = BuildTemplates();

        public static int Count => Templates.Length;

        public static LevelDefinition GetLevel(int index)
        {
            if (Templates.Length == 0)
            {
                return new LevelDefinition();
            }

            int clamped = index < 0 ? 0 : index >= Templates.Length ? Templates.Length - 1 : index;
            return Templates[clamped].Clone();
        }

        private static LevelDefinition[] BuildTemplates()
        {
            var levels = new List<LevelDefinition>(LevelGenerator.GenerateAll());
            return levels.ToArray();
        }
    }
}
