using DragonBoxAlgebra.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace DragonBoxAlgebra.UI
{
    public class AlgebraUI : MonoBehaviour
    {
        public AlgebraGameController Controller { get; private set; }

        private Text _titleText;
        private Text _progressText;
        private Text _messageText;
        private Text _spriteDebugText;
        private LevelCompleteView _completeView;
        private RectTransform _dragRoot;
        private Canvas _canvas;

        public void Initialize(AlgebraGameController controller)
        {
            Controller = controller;
            BuildUI();
            controller.LevelLoaded += OnLevelLoaded;
            controller.MessageChanged += OnMessageChanged;
            controller.LevelCompleted += OnLevelCompleted;
            controller.LoadLevel(GameProgress.SavedLevelIndex);
        }

        private void OnDestroy()
        {
            if (Controller != null)
            {
                Controller.LevelLoaded -= OnLevelLoaded;
                Controller.MessageChanged -= OnMessageChanged;
                Controller.LevelCompleted -= OnLevelCompleted;
            }
        }

        private void OnLevelLoaded(int current, int total)
        {
            _progressText.text = $"{current}/{total}";
            _titleText.text = $"{Controller.CurrentLevel.Title}  •  {CreatureArt.ThemeName}";
            _completeView.Hide();
            RefreshSpriteDebugBanner();
        }

        private void RefreshSpriteDebugBanner()
        {
            if (_spriteDebugText == null)
            {
                return;
            }

            string msg = CreatureSpriteDebug.GetOnScreenMessage();
            _spriteDebugText.text = msg;
            _spriteDebugText.enabled = !string.IsNullOrEmpty(msg);
        }

        private void OnMessageChanged(string message)
        {
            _messageText.text = message;
        }

        private void OnLevelCompleted(int stars, int moves)
        {
            DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayWin();
        }

        public void OnRestartClicked() => Controller.RestartLevel();

        public void OnNextClicked()
        {
            _completeView.Hide();
            Controller.LoadNextLevel();
        }

        public void OnUndoClicked()
        {
            Controller.Undo();
            DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayUndo();
        }

        public void OnRewindClicked()
        {
            Controller.RewindLevel();
            DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayUndo();
        }

        public void OnRandomClicked()
        {
            Controller.LoadRandomLevel();
        }

        private void BuildUI()
        {
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            _canvas = canvasGo.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);

            _dragRoot = new GameObject("DragRoot", typeof(RectTransform)).GetComponent<RectTransform>();
            _dragRoot.SetParent(canvasGo.transform, false);
            _dragRoot.anchorMin = Vector2.zero;
            _dragRoot.anchorMax = Vector2.one;
            _dragRoot.offsetMin = Vector2.zero;
            _dragRoot.offsetMax = Vector2.zero;

            var background = CreatePanel(canvasGo.transform, "Background", new Color(0.12f, 0.34f, 0.42f),
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            _titleText = CreateText(background.transform, "Title", "DragonBox Algebra",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f), 28, TextAnchor.MiddleCenter);
            _progressText = CreateText(background.transform, "Progress", "1/6",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -52f), 20, TextAnchor.MiddleCenter);

            _spriteDebugText = CreateText(background.transform, "SpriteDebug", "",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -78f), 14, TextAnchor.MiddleCenter);
            _spriteDebugText.color = new Color(1f, 0.92f, 0.35f);
            _spriteDebugText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _spriteDebugText.verticalOverflow = VerticalWrapMode.Overflow;
            RefreshSpriteDebugBanner();

            var boardRow = CreatePanel(background.transform, "BoardRow", new Color(0f, 0f, 0f, 0.15f),
                new Vector2(0.05f, 0.28f), new Vector2(0.95f, 0.82f), Vector2.zero, Vector2.zero);

            var leftPanel = CreateTexturedPanel(boardRow.transform, "LeftPanel",
                new Vector2(0f, 0f), new Vector2(0.49f, 1f));
            var rightPanel = CreateTexturedPanel(boardRow.transform, "RightPanel",
                new Vector2(0.51f, 0f), new Vector2(1f, 1f));

            var boardView = gameObject.AddComponent<BoardView>();
            boardView.Initialize(Controller, leftPanel, rightPanel, _canvas, _dragRoot);

            var handPanel = CreatePanel(background.transform, "Hand", new Color(0.08f, 0.18f, 0.24f, 0.85f),
                new Vector2(0.12f, 0.06f), new Vector2(0.88f, 0.22f), Vector2.zero, Vector2.zero);
            var handView = gameObject.AddComponent<HandView>();
            handView.Initialize(Controller, handPanel, _canvas, _dragRoot);

            _messageText = CreateText(background.transform, "Message",
                "Drag cards together on the same side, or drag from your hand to the board.",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 12f), 18, TextAnchor.LowerCenter);

            CreateRoundButton(background.transform, "Menu", new Vector2(0.06f, 0.92f), OnRestartClicked, "⬆");
            CreateRoundButton(background.transform, "Next", new Vector2(0.12f, 0.92f), OnRandomClicked, "⏭");
            CreateRoundButton(background.transform, "Undo", new Vector2(0.88f, 0.92f), OnUndoClicked, "↩");
            CreateRoundButton(background.transform, "Rewind", new Vector2(0.94f, 0.92f), OnRewindClicked, "⏪");

            var completePanel = CreatePanel(background.transform, "CompletePanel", new Color(0.05f, 0.12f, 0.18f, 0.92f),
                new Vector2(0.2f, 0.25f), new Vector2(0.8f, 0.75f), Vector2.zero, Vector2.zero);
            var starsText = CreateText(completePanel.transform, "Stars", "Level Complete",
                new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), Vector2.zero, 22, TextAnchor.MiddleCenter);
            var creatureText = CreateText(completePanel.transform, "Creature", "🐲",
                new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f), Vector2.zero, 48, TextAnchor.MiddleCenter);
            CreateButton(completePanel.transform, "Next", new Vector2(0.5f, 0.18f), OnNextClicked);

            _completeView = gameObject.AddComponent<LevelCompleteView>();
            _completeView.Initialize(Controller, completePanel.gameObject, starsText, creatureText);

            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem),
                    typeof(UnityEngine.EventSystems.StandaloneInputModule));
            }
        }

        private static RectTransform CreateTexturedPanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var image = go.GetComponent<Image>();
            image.sprite = Sprite.Create(SpriteFactory.BoardTexture,
                new Rect(0, 0, SpriteFactory.BoardTexture.width, SpriteFactory.BoardTexture.height),
                new Vector2(0.5f, 0.5f));
            image.type = Image.Type.Tiled;
            image.color = new Color(0.85f, 0.95f, 1f);
            return rect;
        }

        private static RectTransform CreatePanel(Transform parent, string name, Color color,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            go.GetComponent<Image>().color = color;
            return rect;
        }

        private static Text CreateText(Transform parent, string name, string value, Vector2 anchorMin,
            Vector2 anchorMax, Vector2 anchoredPosition, int fontSize, TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(600f, 120f);
            rect.anchoredPosition = anchoredPosition;

            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            return text;
        }

        private static Button CreateButton(Transform parent, string label, Vector2 anchor, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(160f, 48f);
            var image = go.GetComponent<Image>();
            image.sprite = SpriteFactory.RoundedButton;
            image.type = Image.Type.Sliced;
            image.color = new Color(0.82f, 0.32f, 0.18f);

            var button = go.GetComponent<Button>();
            button.onClick.AddListener(onClick);
            CreateText(go.transform, "Label", label, Vector2.zero, Vector2.one, Vector2.zero, 18, TextAnchor.MiddleCenter);
            return button;
        }

        private static void CreateRoundButton(Transform parent, string name, Vector2 anchor,
            UnityEngine.Events.UnityAction onClick, string symbol)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(52f, 52f);
            var image = go.GetComponent<Image>();
            image.sprite = SpriteFactory.RoundedCard;
            image.color = new Color(0.82f, 0.32f, 0.18f);
            go.GetComponent<Button>().onClick.AddListener(onClick);
            CreateText(go.transform, "Symbol", symbol, Vector2.zero, Vector2.one, Vector2.zero, 22, TextAnchor.MiddleCenter);
        }
    }
}
