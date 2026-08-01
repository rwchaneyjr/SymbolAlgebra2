using System.Collections;
using DragonBoxAlgebra.Core;
using DragonBoxAlgebra.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DragonBoxAlgebra.UI
{
    public class CardWidget : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler,
        IPointerClickHandler
    {
        public BoardCard Card { get; private set; }
        public int Index { get; private set; }
        public string SideName { get; private set; }

        private AlgebraGameController _controller;
        private RectTransform _rect;
        private RectTransform _dragRoot;
        private Canvas _canvas;
        private Image _background;
        private Image _border;
        private Image _creatureImage;
        private Text _creatureText;
        private Text _labelText;
        private CreatureReaction _reaction;
        private Vector2 _dragOffset;
        private Transform _originalParent;
        private int _originalSiblingIndex;
        private bool _isDragging;
        private bool _didDrag;
        private bool _handPlayHandled;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_didDrag || SideName != "Hand" || _controller == null)
            {
                return;
            }

            if (_controller.TryFlipHandCard(Index))
            {
                Card = _controller.Hand[Index];
                DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayUndo();
                StartCoroutine(PlayHandFlip());
            }
        }

        public void Bind(BoardCard card, int index, string sideName, AlgebraGameController controller, Canvas canvas,
            RectTransform dragRoot)
        {
            Card = card;
            Index = index;
            SideName = sideName;
            _controller = controller;
            _canvas = canvas;
            _dragRoot = dragRoot;
            RefreshVisual();
        }

        public void RefreshVisual()
        {
            if (_background != null)
            {
                _background.color = SideName == "Hand"
                    ? CardVisuals.HandFaceBackground(Card)
                    : CardVisuals.Background(Card.Kind);
            }

            if (_border != null)
            {
                _border.color = SideName == "Hand"
                    ? CardVisuals.HandFaceBorder(Card)
                    : CardVisuals.Border(Card.Kind);
            }

            ApplyCreatureVisual();
            if (_labelText != null)
            {
                bool showCreatureArt = Card.Kind is CardKind.DayCreature or CardKind.NightCreature or CardKind.Box
                    && _creatureImage != null && _creatureImage.enabled;

                if (showCreatureArt)
                {
                    _labelText.text = string.Empty;
                }
                else
                {
                    _labelText.text = CardVisuals.AlgebraLabel(Card);
                    if (Card.StackCount > 1)
                    {
                        _labelText.text += $" x{Card.StackCount}";
                    }
                }
            }
        }

        private void ApplyCreatureVisual()
        {
            Sprite icon = CardVisuals.IconSprite(Card);
            bool isCreature = Card.Kind is CardKind.DayCreature or CardKind.NightCreature or CardKind.Box;

            if (_creatureImage != null)
            {
                _creatureImage.sprite = icon;
                _creatureImage.enabled = icon != null;
                _creatureImage.color = Color.white;
                _creatureImage.preserveAspect = true;
                if (icon != null && _creatureImage.rectTransform != null)
                {
                    var inset = isCreature ? new Vector2(6f, 6f) : new Vector2(8f, 28f);
                    _creatureImage.rectTransform.offsetMin = inset;
                    _creatureImage.rectTransform.offsetMax = new Vector2(-inset.x, -inset.y);
                }
            }

            if (_creatureText != null)
            {
                _creatureText.font = EmojiFont.Get();
                _creatureText.text = CardVisuals.Emoji(Card);
                _creatureText.fontSize = CardVisuals.EmojiFontSize(Card);
                _creatureText.enabled = icon == null;
            }
        }

        public void SetHighlight(bool on)
        {
            if (_background != null)
            {
                Color baseColor = SideName == "Hand"
                    ? CardVisuals.HandFaceBackground(Card)
                    : CardVisuals.Background(Card.Kind);
                _background.color = on
                    ? Color.Lerp(baseColor, Color.white, 0.35f)
                    : baseColor;
            }
        }

        private IEnumerator PlayHandFlip()
        {
            if (_rect == null)
            {
                RefreshVisual();
                yield break;
            }

            const float halfDuration = 0.1f;
            Vector3 scale = _rect.localScale;

            float elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float n = elapsed / halfDuration;
                _rect.localScale = new Vector3(Mathf.Lerp(scale.x, 0.05f, n), scale.y, scale.z);
                yield return null;
            }

            RefreshVisual();

            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float n = elapsed / halfDuration;
                _rect.localScale = new Vector3(Mathf.Lerp(0.05f, scale.x, n), scale.y, scale.z);
                yield return null;
            }

            _rect.localScale = scale;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!CanDrag())
            {
                return;
            }

            _isDragging = true;
            _didDrag = false;
            _handPlayHandled = false;
            _originalParent = transform.parent;
            _originalSiblingIndex = transform.GetSiblingIndex();
            transform.SetParent(_dragRoot, true);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_dragRoot, eventData.position,
                eventData.pressEventCamera, out _dragOffset);
            _dragOffset = (Vector2)transform.localPosition - _dragOffset;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging)
            {
                return;
            }

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_dragRoot, eventData.position,
                    eventData.pressEventCamera, out Vector2 localPoint))
            {
                _didDrag = true;
                _rect.localPosition = localPoint + _dragOffset;
            }
        }

        public void MarkHandPlayHandled() => _handPlayHandled = true;

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging)
            {
                return;
            }

            _isDragging = false;

            if (SideName == "Hand")
            {
                if (!_handPlayHandled)
                {
                    TryPlayHandDrop(eventData);
                }

                if (_controller.Hand.Count == 0 || Index >= _controller.Hand.Count)
                {
                    Destroy(gameObject);
                    return;
                }

                // HandView rebuilds all hand widgets after play; drop the drag copy.
                if (_handPlayHandled)
                {
                    Destroy(gameObject);
                    _controller.RefreshHandPresentation();
                    return;
                }

                transform.SetParent(_originalParent, false);
                transform.SetSiblingIndex(_originalSiblingIndex);
                Bind(_controller.Hand[Index], Index, "Hand", _controller, _canvas, _dragRoot);
                return;
            }

            CardWidget target = FindDropTarget(eventData);
            if (target != null && target != this)
            {
                HandleDropOnCard(target);
            }

            transform.SetParent(_originalParent, false);
            transform.SetSiblingIndex(_originalSiblingIndex);
        }

        private void TryPlayHandDrop(PointerEventData eventData)
        {
            if (_controller.HasPendingBalance)
            {
                string holeSide = _controller.PendingBalance.HoleSide;

                BalanceHoleWidget balanceHole = FindBalanceHole(eventData);
                if (balanceHole != null && balanceHole.SideName == holeSide)
                {
                    if (_controller.TryPlayFromHand(Index, holeSide))
                    {
                        MarkHandPlayHandled();
                        DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
                    }

                    return;
                }

                CardWidget target = FindDropTarget(eventData);
                if (target != null && target != this && target.SideName == holeSide)
                {
                    if (_controller.TryPlayFromHand(Index, holeSide))
                    {
                        MarkHandPlayHandled();
                        DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
                    }

                    return;
                }

                BoardDropZone boardZone = FindBoardZone(eventData);
                if (boardZone != null && boardZone.SideName == holeSide
                    && _controller.TryPlayFromHand(Index, holeSide))
                {
                    MarkHandPlayHandled();
                    DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
                }

                return;
            }

            CardWidget targetCard = FindDropTarget(eventData);
            if (targetCard != null && targetCard != this && targetCard.SideName != "Hand")
            {
                if (_controller.TryPlayFromHand(Index, targetCard.SideName))
                {
                    MarkHandPlayHandled();
                    DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
                }

                return;
            }

            BoardDropZone zone = FindBoardZone(eventData);
            if (zone != null && _controller.TryPlayFromHand(Index, zone.SideName))
            {
                MarkHandPlayHandled();
                DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            CardWidget dragged = eventData.pointerDrag?.GetComponent<CardWidget>();
            if (dragged == null || dragged == this)
            {
                return;
            }

            dragged.HandleDropOnCard(this);
        }

        public void HandleDropOnCard(CardWidget target)
        {
            if (SideName == "Hand")
            {
                if (_controller.HasPendingBalance)
                {
                    if (target.SideName == _controller.PendingBalance.HoleSide
                        && _controller.TryPlayFromHand(Index, target.SideName))
                    {
                        MarkHandPlayHandled();
                        DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
                    }

                    return;
                }

                if (target.SideName != "Hand"
                    && _controller.TryPlayFromHand(Index, target.SideName))
                {
                    MarkHandPlayHandled();
                    DragonBoxAlgebra.Audio.AudioManager.Instance?.PlayCardPlay();
                }

                return;
            }

            if (SideName != target.SideName)
            {
                return;
            }

            _controller.TryCombine(SideName, Index, target.Index);
        }

        private BalanceHoleWidget FindBalanceHole(PointerEventData eventData)
        {
            if (eventData.hovered == null)
            {
                return null;
            }

            foreach (GameObject go in eventData.hovered)
            {
                if (go == null)
                {
                    continue;
                }

                BalanceHoleWidget hole = go.GetComponent<BalanceHoleWidget>();
                if (hole == null)
                {
                    hole = go.GetComponentInParent<BalanceHoleWidget>();
                }

                if (hole != null)
                {
                    return hole;
                }
            }

            return null;
        }

        private BoardDropZone FindBoardZone(PointerEventData eventData)
        {
            if (eventData.hovered == null)
            {
                return null;
            }

            foreach (GameObject go in eventData.hovered)
            {
                if (go == null)
                {
                    continue;
                }

                BoardDropZone zone = go.GetComponent<BoardDropZone>();
                if (zone == null)
                {
                    zone = go.GetComponentInParent<BoardDropZone>();
                }

                if (zone != null)
                {
                    return zone;
                }
            }

            return null;
        }

        private CardWidget FindDropTarget(PointerEventData eventData)
        {
            if (eventData.hovered == null)
            {
                return null;
            }

            foreach (GameObject go in eventData.hovered)
            {
                if (go == null)
                {
                    continue;
                }

                CardWidget widget = go.GetComponent<CardWidget>();
                if (widget == null)
                {
                    widget = go.GetComponentInParent<CardWidget>();
                }

                if (widget != null && widget != this)
                {
                    return widget;
                }
            }

            return null;
        }

        private bool CanDrag()
        {
            if (_controller == null)
            {
                return false;
            }

            if (SideName == "Hand")
            {
                return Card.IsPlayableFromHand;
            }

            return Card.IsDraggableFromBoard;
        }

        public static CardWidget Create(Transform parent, BoardCard card, int index, string sideName,
            AlgebraGameController controller, Canvas canvas, RectTransform dragRoot)
        {
            var root = new GameObject($"Card_{sideName}_{index}", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            root.transform.SetParent(parent, false);

            var rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(110f, 120f);

            var borderGo = new GameObject("Border", typeof(RectTransform), typeof(Image));
            borderGo.transform.SetParent(root.transform, false);
            var borderRect = borderGo.GetComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = new Vector2(-3f, -3f);
            borderRect.offsetMax = new Vector2(3f, 3f);
            var borderImage = borderGo.GetComponent<Image>();
            borderImage.sprite = SpriteFactory.RoundedCard;
            borderImage.type = Image.Type.Sliced;
            borderImage.raycastTarget = false;
            borderImage.color = CardVisuals.Border(card.Kind);

            var image = root.GetComponent<Image>();
            image.sprite = SpriteFactory.RoundedCard;
            image.type = Image.Type.Sliced;
            image.color = CardVisuals.Background(card.Kind);

            var widget = root.AddComponent<CardWidget>();
            widget._rect = rect;
            widget._background = image;
            widget._border = borderImage;

            var creatureGo = new GameObject("Creature", typeof(RectTransform), typeof(CreatureReaction));
            creatureGo.transform.SetParent(root.transform, false);
            var creatureRect = creatureGo.GetComponent<RectTransform>();
            creatureRect.anchorMin = Vector2.zero;
            creatureRect.anchorMax = Vector2.one;
            creatureRect.offsetMin = new Vector2(8f, 28f);
            creatureRect.offsetMax = new Vector2(-8f, -8f);
            widget._reaction = creatureGo.GetComponent<CreatureReaction>();

            var creatureImageGo = new GameObject("Sprite", typeof(RectTransform), typeof(Image));
            creatureImageGo.transform.SetParent(creatureGo.transform, false);
            var creatureImageRect = creatureImageGo.GetComponent<RectTransform>();
            creatureImageRect.anchorMin = Vector2.zero;
            creatureImageRect.anchorMax = Vector2.one;
            creatureImageRect.offsetMin = Vector2.zero;
            creatureImageRect.offsetMax = Vector2.zero;
            var creatureImage = creatureImageGo.GetComponent<Image>();
            creatureImage.raycastTarget = false;
            creatureImage.preserveAspect = true;
            widget._creatureImage = creatureImage;

            var creatureTextGo = new GameObject("Emoji", typeof(RectTransform), typeof(Text));
            creatureTextGo.transform.SetParent(creatureGo.transform, false);
            var creatureTextRect = creatureTextGo.GetComponent<RectTransform>();
            creatureTextRect.anchorMin = Vector2.zero;
            creatureTextRect.anchorMax = Vector2.one;
            creatureTextRect.offsetMin = Vector2.zero;
            creatureTextRect.offsetMax = Vector2.zero;
            var emojiText = creatureTextGo.GetComponent<Text>();
            emojiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            emojiText.alignment = TextAnchor.MiddleCenter;
            emojiText.fontSize = CardVisuals.EmojiFontSize(card);
            emojiText.raycastTarget = false;
            widget._creatureText = emojiText;

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(root.transform, false);
            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 0f);
            labelRect.pivot = new Vector2(0.5f, 0f);
            labelRect.sizeDelta = new Vector2(0f, 24f);

            var labelText = labelGo.GetComponent<Text>();
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.fontSize = 13;
            labelText.color = Color.white;
            widget._labelText = labelText;

            var layoutElement = root.AddComponent<LayoutElement>();
            layoutElement.minWidth = 110f;
            layoutElement.minHeight = 120f;
            layoutElement.preferredWidth = 110f;
            layoutElement.preferredHeight = 120f;

            widget.Bind(card, index, sideName, controller, canvas, dragRoot);

            root.GetComponent<CanvasGroup>().blocksRaycasts = true;
            return widget;
        }

        public void ReactCombine() => _reaction?.PlayCombine();
        public void ReactCelebrate() => _reaction?.PlayCelebrate();
        public void ReactUndo() => _reaction?.PlayUndo();
    }
}
