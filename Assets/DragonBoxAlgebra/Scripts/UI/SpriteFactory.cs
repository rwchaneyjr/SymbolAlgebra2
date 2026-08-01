using UnityEngine;

namespace DragonBoxAlgebra.UI
{
    public static class SpriteFactory
    {
        private static Sprite _roundedCard;
        private static Sprite _roundedButton;
        private static Sprite _fishCreature;
        private static Sprite _turtleCreature;
        private static Sprite _diceSprite;
        private static Sprite _positiveDiceSprite;
        private static Sprite _negativeDiceSprite;
        private static Sprite _smileySprite;
        private static Sprite _boxSprite;
        private static Texture2D _boardTexture;

        public static Sprite FishCreature => _fishCreature ??= CreateFishSprite();
        public static Sprite TurtleCreature => _turtleCreature ??= CreateTurtleSprite();

        public static Sprite LightCreature(int theme, int value = 1) =>
            GetThemedSprite(theme, true, value);

        public static Sprite DarkCreature(int theme, int value = 1) =>
            GetThemedSprite(theme, false, value);

        private static readonly Sprite[,] _themedSprites = new Sprite[CreatureArt.ThemeCount, 2];

        private static Sprite GetThemedSprite(int theme, bool light, int value)
        {
            int row = ((theme % CreatureArt.ThemeCount) + CreatureArt.ThemeCount) % CreatureArt.ThemeCount;
            int col = light ? 0 : 1;
            if (_themedSprites[row, col] != null)
            {
                return _themedSprites[row, col];
            }

            _themedSprites[row, col] = CreateSeaCreatureSprite(row, light);

            return _themedSprites[row, col];
        }

        private static Sprite CreateSeaCreatureSprite(int theme, bool light)
        {
            return theme switch
            {
                0 => light ? CreateFishSprite() : CreateFishSpriteDark(),
                1 => light ? CreateTurtleSprite() : CreateTurtleSpriteDark(),
                2 => CreateCreatureSprite(
                    light ? new Color(0.95f, 0.82f, 0.78f) : new Color(0.55f, 0.38f, 0.42f),
                    light ? new Color(0.9f, 0.5f, 0.45f) : new Color(0.35f, 0.2f, 0.25f), light),
                3 => CreateCreatureSprite(
                    light ? new Color(0.45f, 0.75f, 0.95f) : new Color(0.15f, 0.35f, 0.65f),
                    light ? new Color(0.9f, 0.95f, 1f) : new Color(0.5f, 0.65f, 0.85f), true),
                4 => CreateCreatureSprite(
                    light ? new Color(0.55f, 0.85f, 0.45f) : new Color(0.2f, 0.45f, 0.22f),
                    light ? new Color(0.95f, 0.8f, 0.2f) : new Color(0.65f, 0.5f, 0.1f), true),
                5 => CreateCreatureSprite(
                    light ? new Color(0.95f, 0.45f, 0.35f) : new Color(0.65f, 0.18f, 0.12f),
                    light ? new Color(0.85f, 0.2f, 0.15f) : new Color(0.45f, 0.1f, 0.08f), false),
                6 => CreateCreatureSprite(
                    light ? new Color(0.55f, 0.85f, 0.95f) : new Color(0.2f, 0.45f, 0.62f),
                    light ? new Color(0.95f, 0.75f, 0.35f) : new Color(0.75f, 0.55f, 0.2f), true),
                _ => CreateCreatureSprite(
                    light ? new Color(0.95f, 0.55f, 0.25f) : new Color(0.55f, 0.28f, 0.12f),
                    light ? new Color(1f, 0.85f, 0.35f) : new Color(0.75f, 0.55f, 0.2f), true)
            };
        }

        private static Sprite CreateFishSpriteDark()
        {
            return CreateCreatureSprite(new Color(0.12f, 0.35f, 0.72f), new Color(0.75f, 0.35f, 0.15f), true);
        }

        private static Sprite CreateTurtleSpriteDark()
        {
            return CreateCreatureSprite(new Color(0.15f, 0.42f, 0.28f), new Color(0.65f, 0.55f, 0.15f), false);
        }
        public static Sprite DiceSprite => _diceSprite ??= CreateDiceSprite();
        public static Sprite PositiveDiceSprite => _positiveDiceSprite ??= CreatePositiveDiceSprite();
        public static Sprite NegativeDiceSprite => _negativeDiceSprite ??= CreateNegativeDiceSprite();
        public static Sprite SmileySprite => _smileySprite ??= CreateSmileySprite();
        public static Sprite BoxSprite => _boxSprite ??= CreateBoxSprite();

        public static Sprite RoundedCard => _roundedCard ??= CreateRoundedSprite(128, 128, 18, Color.white);
        public static Sprite RoundedButton => _roundedButton ??= CreateRoundedSprite(128, 64, 14, Color.white);

        public static Texture2D BoardTexture
        {
            get
            {
                if (_boardTexture != null)
                {
                    return _boardTexture;
                }

                _boardTexture = new Texture2D(256, 256, TextureFormat.RGBA32, false);
                _boardTexture.filterMode = FilterMode.Bilinear;
                var baseColor = new Color(0.45f, 0.72f, 0.78f);
                for (int y = 0; y < 256; y++)
                {
                    for (int x = 0; x < 256; x++)
                    {
                        float noise = Mathf.PerlinNoise(x * 0.08f, y * 0.08f) * 0.08f;
                        _boardTexture.SetPixel(x, y, baseColor + new Color(noise, noise, noise));
                    }
                }

                _boardTexture.Apply();
                return _boardTexture;
            }
        }

        public static Sprite CreateRoundedSprite(int width, int height, int radius, Color color)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool inside = IsInsideRoundedRect(x, y, width, height, radius);
                    texture.SetPixel(x, y, inside ? color : Color.clear);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        public static Sprite CreateFishSprite()
        {
            return CreateCreatureSprite(new Color(0.2f, 0.55f, 0.95f), new Color(0.95f, 0.45f, 0.2f), true);
        }

        public static Sprite CreateTurtleSprite()
        {
            return CreateCreatureSprite(new Color(0.25f, 0.65f, 0.35f), new Color(0.85f, 0.75f, 0.2f), false);
        }

        public static Sprite CreateDiceSprite()
        {
            const int size = 96;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            var white = new Color(0.98f, 0.98f, 0.95f);
            var dot = new Color(0.15f, 0.15f, 0.18f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }

            FillRoundedRect(texture, 12, 12, 72, 72, 12, white);
            FillEllipse(texture, 48, 48, 6, 6, dot);
            FillEllipse(texture, 30, 30, 5, 5, dot);
            FillEllipse(texture, 66, 66, 5, 5, dot);

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        public static Sprite CreatePositiveDiceSprite()
        {
            const int size = 96;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            var face = new Color(0.82f, 0.96f, 0.62f);
            var edge = new Color(0.35f, 0.62f, 0.22f);
            var dot = new Color(0.12f, 0.28f, 0.08f);
            ClearTexture(texture);
            FillRoundedRect(texture, 10, 10, 76, 76, 14, face);
            FillRoundedRect(texture, 12, 12, 72, 72, 12, edge);
            FillRoundedRect(texture, 14, 14, 68, 68, 10, face);
            FillEllipse(texture, 48, 48, 7, 7, dot);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        public static Sprite CreateNegativeDiceSprite()
        {
            const int size = 96;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            var face = new Color(0.72f, 0.74f, 0.92f);
            var edge = new Color(0.32f, 0.34f, 0.58f);
            var dot = new Color(0.15f, 0.12f, 0.28f);
            ClearTexture(texture);
            FillRoundedRect(texture, 10, 10, 76, 76, 14, face);
            FillRoundedRect(texture, 12, 12, 72, 72, 12, edge);
            FillRoundedRect(texture, 14, 14, 68, 68, 10, face);
            FillEllipse(texture, 34, 34, 5, 5, dot);
            FillEllipse(texture, 62, 62, 5, 5, dot);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        public static Sprite CreateSmileySprite()
        {
            const int size = 96;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            var yellow = new Color(1f, 0.88f, 0.2f);
            var dark = new Color(0.2f, 0.15f, 0.05f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }

            FillEllipse(texture, 48, 48, 36, 36, yellow);
            FillEllipse(texture, 34, 56, 8, 5, dark);
            FillEllipse(texture, 62, 56, 8, 5, dark);
            FillEllipse(texture, 36, 40, 5, 5, dark);
            FillEllipse(texture, 60, 40, 5, 5, dark);

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        public static Sprite CreateBoxSprite()
        {
            const int size = 96;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            var brown = new Color(0.72f, 0.38f, 0.22f);
            var dark = new Color(0.35f, 0.18f, 0.1f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }

            FillRoundedRect(texture, 18, 22, 60, 52, 8, brown);
            FillRoundedRect(texture, 18, 22, 60, 14, 6, dark);
            texture.SetPixel(48, 52, Color.white);

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static void FillRoundedRect(Texture2D texture, int left, int bottom, int width, int height, int radius,
            Color color)
        {
            for (int y = bottom; y < bottom + height; y++)
            {
                for (int x = left; x < left + width; x++)
                {
                    if (x < 0 || y < 0 || x >= texture.width || y >= texture.height)
                    {
                        continue;
                    }

                    bool inside = x >= left + radius && x < left + width - radius
                        || y >= bottom + radius && y < bottom + height - radius;

                    if (!inside)
                    {
                        float cx = x < left + radius ? left + radius : left + width - radius - 1;
                        float cy = y < bottom + radius ? bottom + radius : bottom + height - radius - 1;
                        float dx = x - cx;
                        float dy = y - cy;
                        inside = dx * dx + dy * dy <= radius * radius;
                    }

                    if (inside)
                    {
                        texture.SetPixel(x, y, color);
                    }
                }
            }
        }

        private static Sprite CreateCreatureSprite(Color bodyColor, Color accentColor, bool isFish)
        {
            const int size = 96;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }

            if (isFish)
            {
                FillEllipse(texture, 48, 50, 34, 18, bodyColor);
                FillEllipse(texture, 72, 50, 10, 8, accentColor);
                FillEllipse(texture, 28, 50, 8, 8, Color.white);
                texture.SetPixel(26, 52, Color.black);
            }
            else
            {
                FillEllipse(texture, 48, 52, 30, 22, bodyColor);
                FillEllipse(texture, 48, 68, 24, 10, accentColor);
                FillEllipse(texture, 34, 44, 6, 6, Color.white);
                FillEllipse(texture, 62, 44, 6, 6, Color.white);
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateBirdSprite(bool light)
        {
            var body = light ? new Color(0.35f, 0.72f, 0.98f) : new Color(0.35f, 0.28f, 0.55f);
            var accent = light ? new Color(1f, 0.75f, 0.15f) : new Color(0.85f, 0.72f, 0.35f);
            return CreateWingedCreature(body, accent, light);
        }

        private static Sprite CreateCrabSprite(bool light)
        {
            var body = light ? new Color(0.95f, 0.35f, 0.28f) : new Color(0.55f, 0.25f, 0.75f);
            var accent = light ? new Color(1f, 0.6f, 0.45f) : new Color(0.75f, 0.55f, 0.95f);
            return CreateClawCreature(body, accent);
        }

        private static Sprite CreateWingSprite(bool light)
        {
            var body = light ? new Color(0.95f, 0.55f, 0.85f) : new Color(0.35f, 0.22f, 0.35f);
            var accent = light ? new Color(0.55f, 0.25f, 0.75f) : new Color(0.65f, 0.65f, 0.72f);
            return CreateWingedCreature(body, accent, light);
        }

        private static Sprite CreateStarSprite(bool light)
        {
            var body = light ? new Color(1f, 0.9f, 0.25f) : new Color(0.75f, 0.78f, 0.95f);
            var accent = light ? new Color(1f, 0.65f, 0.1f) : new Color(0.45f, 0.48f, 0.75f);
            return CreateRoundCreature(body, accent, light ? 0.9f : 0.55f);
        }

        private static Sprite CreateHopperSprite(bool light)
        {
            var body = light ? new Color(0.92f, 0.88f, 0.82f) : new Color(0.82f, 0.42f, 0.18f);
            var accent = light ? new Color(0.95f, 0.65f, 0.72f) : new Color(0.35f, 0.22f, 0.12f);
            return CreateRoundCreature(body, accent, 1.1f);
        }

        private static Sprite CreateFrogSprite()
        {
            const int size = 96;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            ClearTexture(texture);

            var body = new Color(0.28f, 0.82f, 0.38f);
            var belly = new Color(0.55f, 0.92f, 0.48f);
            var eyeWhite = Color.white;
            var pupil = new Color(0.12f, 0.12f, 0.15f);

            FillEllipse(texture, 48, 56, 28, 22, body);
            FillEllipse(texture, 48, 62, 18, 12, belly);
            FillEllipse(texture, 34, 44, 10, 10, body);
            FillEllipse(texture, 62, 44, 10, 10, body);
            FillEllipse(texture, 34, 44, 7, 7, eyeWhite);
            FillEllipse(texture, 62, 44, 7, 7, eyeWhite);
            FillEllipse(texture, 36, 44, 4, 4, pupil);
            FillEllipse(texture, 64, 44, 4, 4, pupil);
            FillEllipse(texture, 48, 66, 8, 4, new Color(0.15f, 0.45f, 0.2f));

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateSnakeSprite()
        {
            const int size = 96;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            ClearTexture(texture);

            var body = new Color(0.22f, 0.58f, 0.28f);
            var band = new Color(0.85f, 0.22f, 0.18f);
            var eye = Color.white;

            FillEllipse(texture, 30, 50, 14, 12, body);
            FillEllipse(texture, 48, 54, 16, 14, body);
            FillEllipse(texture, 66, 50, 14, 12, body);
            FillEllipse(texture, 24, 52, 8, 8, body);
            FillEllipse(texture, 40, 56, 3, 10, band);
            FillEllipse(texture, 56, 56, 3, 10, band);
            FillEllipse(texture, 22, 54, 4, 4, eye);
            FillEllipse(texture, 21, 54, 2, 2, Color.black);
            FillEllipse(texture, 72, 48, 5, 4, new Color(0.9f, 0.25f, 0.2f));

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateWeatherSprite(bool light) =>
            light ? CreateSunSprite() : CreateStormSprite();

        private static Sprite CreateSunSprite()
        {
            const int size = 96;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            ClearTexture(texture);

            var ray = new Color(1f, 0.78f, 0.12f);
            var core = new Color(1f, 0.9f, 0.22f);
            var center = new Color(1f, 0.62f, 0.05f);

            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI / 4f;
                int tipX = 48 + (int)(Mathf.Cos(angle) * 34f);
                int tipY = 52 + (int)(Mathf.Sin(angle) * 34f);
                int baseX = 48 + (int)(Mathf.Cos(angle) * 18f);
                int baseY = 52 + (int)(Mathf.Sin(angle) * 18f);
                DrawLine(texture, baseX, baseY, tipX, tipY, 4, ray);
            }

            FillEllipse(texture, 48, 52, 20, 20, core);
            FillEllipse(texture, 48, 52, 12, 12, center);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateStormSprite()
        {
            const int size = 96;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            ClearTexture(texture);

            var cloud = new Color(0.78f, 0.84f, 0.94f);
            var cloudShadow = new Color(0.42f, 0.5f, 0.68f);
            var rain = new Color(0.28f, 0.52f, 0.92f);

            FillEllipse(texture, 48, 58, 30, 16, cloud);
            FillEllipse(texture, 28, 56, 14, 12, cloud);
            FillEllipse(texture, 68, 56, 14, 12, cloud);
            FillEllipse(texture, 48, 50, 20, 14, cloudShadow);

            for (int i = 0; i < 5; i++)
            {
                int x = 30 + i * 9;
                DrawLine(texture, x, 42, x - 2, 24, 2, rain);
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static void DrawLine(Texture2D texture, int x0, int y0, int x1, int y1, int thickness, Color color)
        {
            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int steps = Mathf.Max(dx, dy);
            if (steps == 0)
            {
                FillEllipse(texture, x0, y0, thickness, thickness, color);
                return;
            }

            for (int step = 0; step <= steps; step++)
            {
                float t = step / (float)steps;
                int x = Mathf.RoundToInt(Mathf.Lerp(x0, x1, t));
                int y = Mathf.RoundToInt(Mathf.Lerp(y0, y1, t));
                FillEllipse(texture, x, y, thickness, thickness, color);
            }
        }

        private static Sprite CreateDragonSprite(bool light)
        {
            var body = light ? new Color(0.25f, 0.78f, 0.45f) : new Color(0.92f, 0.28f, 0.12f);
            var accent = light ? new Color(0.95f, 0.85f, 0.2f) : new Color(1f, 0.55f, 0.1f);
            return CreateWingedCreature(body, accent, light);
        }

        private static Sprite CreatePetSprite(bool light)
        {
            var body = light ? new Color(0.95f, 0.72f, 0.35f) : new Color(0.45f, 0.35f, 0.28f);
            var accent = light ? new Color(1f, 0.55f, 0.25f) : new Color(0.2f, 0.2f, 0.2f);
            return CreateRoundCreature(body, accent, 1f);
        }

        private static Sprite CreateWingedCreature(Color body, Color accent, bool light)
        {
            const int size = 96;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            ClearTexture(texture);
            FillEllipse(texture, 48, 52, 18, 16, body);
            FillEllipse(texture, 28, 54, 14, 8, accent);
            FillEllipse(texture, 68, 54, 14, 8, accent);
            FillEllipse(texture, light ? 42 : 38, 58, 5, 5, Color.white);
            FillEllipse(texture, light ? 54 : 58, 58, 5, 5, Color.white);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateClawCreature(Color body, Color accent)
        {
            const int size = 96;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            ClearTexture(texture);
            FillEllipse(texture, 48, 50, 24, 18, body);
            FillEllipse(texture, 24, 44, 10, 8, accent);
            FillEllipse(texture, 72, 44, 10, 8, accent);
            FillEllipse(texture, 40, 62, 6, 6, Color.white);
            FillEllipse(texture, 56, 62, 6, 6, Color.white);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateRoundCreature(Color body, Color accent, float scale)
        {
            const int size = 96;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            ClearTexture(texture);
            int rx = (int)(28 * scale);
            int ry = (int)(26 * scale);
            FillEllipse(texture, 48, 52, rx, ry, body);
            FillEllipse(texture, 48, 58, (int)(18 * scale), (int)(10 * scale), accent);
            FillEllipse(texture, 38, 48, 6, 6, Color.white);
            FillEllipse(texture, 58, 48, 6, 6, Color.white);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static void ClearTexture(Texture2D texture)
        {
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }

        private static void FillEllipse(Texture2D texture, int cx, int cy, int rx, int ry, Color color)
        {
            for (int y = cy - ry; y <= cy + ry; y++)
            {
                for (int x = cx - rx; x <= cx + rx; x++)
                {
                    if (x < 0 || y < 0 || x >= texture.width || y >= texture.height)
                    {
                        continue;
                    }

                    float dx = (x - cx) / (float)rx;
                    float dy = (y - cy) / (float)ry;
                    if (dx * dx + dy * dy <= 1f)
                    {
                        texture.SetPixel(x, y, color);
                    }
                }
            }
        }

        private static bool IsInsideRoundedRect(int x, int y, int width, int height, int radius)
        {
            if (x >= radius && x < width - radius)
            {
                return true;
            }

            if (y >= radius && y < height - radius)
            {
                return true;
            }

            float cx = x < radius ? radius : width - radius - 1;
            float cy = y < radius ? radius : height - radius - 1;
            float dx = x - cx;
            float dy = y - cy;
            return dx * dx + dy * dy <= radius * radius;
        }
    }
}
