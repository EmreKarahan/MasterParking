using MasterParking.App;
using MasterParking.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MasterParking.UI
{
    /// <summary>
    /// Scrollable level grid. Lock and star state comes straight from saved
    /// progression; no per-level branching lives here.
    /// </summary>
    public sealed class LevelSelectScreen : MonoBehaviour
    {
        private AppRoot _app;
        private AppFlow _flow;
        private UiRoot _ui;

        public void Initialize(AppRoot app, AppFlow flow)
        {
            _app = app;
            _flow = flow;
            _ui = GetComponent<UiRoot>();
            Build();
        }

        private void Build()
        {
            RectTransform safe = _ui.SafeArea;

            Button back = UiFactory.Button(safe, "Back", "GERİ", 24, UiTheme.Button, UiTheme.Text,
                _flow.ShowMainMenu, _app.Audio);
            UiFactory.SetRect(back.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(32f, -28f), new Vector2(150f, 62f));

            Text title = UiFactory.Text(safe, "Title", "BÖLÜM SEÇ", 52, UiTheme.Gold,
                TextAnchor.MiddleCenter, false, FontStyle.Bold);
            UiFactory.SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(800f, 70f));

            int stars = TotalStars();
            Text stats = UiFactory.Text(safe, "Stats",
                $"★ {stars} / {_app.Levels.Count * ScoreCalculator.MaxStars}", 24, UiTheme.Gold,
                TextAnchor.MiddleRight, false, FontStyle.Bold);
            UiFactory.SetRect(stats.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(1f, 1f), new Vector2(-36f, -30f), new Vector2(360f, 48f));

            RectTransform viewport = UiFactory.Rect("Viewport", safe);
            UiFactory.Stretch(viewport, 46f, 40f, 46f, 132f);
            Image hitArea = UiFactory.Image(viewport, "HitArea", UiTheme.Transparent, null, true);
            UiFactory.Stretch(hitArea.rectTransform);
            viewport.gameObject.AddComponent<RectMask2D>();

            RectTransform content = UiFactory.Rect("Content", viewport);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;

            var grid = content.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(272f, 210f);
            grid.spacing = new Vector2(20f, 20f);
            grid.padding = new RectOffset(8, 8, 8, 8);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 6;
            grid.childAlignment = TextAnchor.UpperCenter;

            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.elasticity = 0.12f;
            scroll.inertia = true;
            scroll.decelerationRate = 0.14f;
            scroll.scrollSensitivity = 26f;

            if (_app.Levels.Count == 0)
            {
                Text empty = UiFactory.Text(safe, "Empty", "Seviye verisi yüklenemedi.", 28, UiTheme.Red,
                    TextAnchor.MiddleCenter);
                UiFactory.SetRect(empty.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900f, 60f));
                return;
            }

            for (int i = 0; i < _app.Levels.Count; i++)
            {
                LevelDefinition level = _app.Levels.GetByIndex(i);
                BuildCard(content, level);
            }
        }

        private void BuildCard(Transform parent, LevelDefinition level)
        {
            bool unlocked = _app.Save.Progression.IsUnlocked(level.Id);
            _app.Save.Progression.TryGet(level.Id, out LevelProgress progress);
            int stars = progress?.Stars ?? 0;

            Image card = UiFactory.Panel(parent, "Card_" + level.Id,
                unlocked ? UiTheme.PanelLight : new Color(0.05f, 0.07f, 0.1f, 0.85f), true);

            var button = card.gameObject.AddComponent<Button>();
            button.targetGraphic = card;
            button.transition = Selectable.Transition.ColorTint;
            button.interactable = unlocked;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.92f, 0.95f, 1f);
            colors.pressedColor = new Color(0.74f, 0.8f, 0.88f);
            colors.disabledColor = new Color(0.6f, 0.6f, 0.65f, 0.7f);
            button.colors = colors;

            if (unlocked)
            {
                int levelId = level.Id;
                button.onClick.AddListener(() =>
                {
                    _app.Audio.PlayClick();
                    _flow.StartLevelById(levelId);
                });
            }

            Text number = UiFactory.Text(card.transform, "Number", level.Id.ToString("00"), 38, unlocked ? UiTheme.Gold : UiTheme.Muted,
                TextAnchor.MiddleLeft, false, FontStyle.Bold);
            UiFactory.SetRect(number.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(18f, -12f), new Vector2(100f, 50f));

            Text name = UiFactory.Text(card.transform, "Name", level.Name, 22, unlocked ? UiTheme.Text : UiTheme.Muted,
                TextAnchor.MiddleCenter, true, FontStyle.Bold);
            UiFactory.SetRect(name.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, 6f), new Vector2(236f, 66f));

            RectTransform starRow = UiFactory.Rect("Stars", card.transform);
            UiFactory.SetRect(starRow, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0f, 20f), new Vector2(160f, 42f));

            for (int i = 0; i < ScoreCalculator.MaxStars; i++)
            {
                Image star = UiFactory.Star(starRow, "Star_" + i, unlocked && i < stars, 36f);
                UiFactory.SetRect(star.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f), new Vector2((i - 1) * 46f, 0f), new Vector2(36f, 36f));
            }

            if (!unlocked)
            {
                Text locked = UiFactory.Text(card.transform, "Locked", "KİLİTLİ", 18, UiTheme.Muted,
                    TextAnchor.MiddleCenter, false, FontStyle.Bold);
                UiFactory.SetRect(locked.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                    new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(180f, 26f));
            }
            else if (progress != null && progress.Completed && progress.BestTime > 0f)
            {
                Text best = UiFactory.Text(card.transform, "Best", progress.BestTime.ToString("0.0") + " sn", 16,
                    UiTheme.Muted, TextAnchor.MiddleRight);
                UiFactory.SetRect(best.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f),
                    new Vector2(1f, 1f), new Vector2(-14f, -16f), new Vector2(100f, 28f));
            }
        }

        private int TotalStars()
        {
            int total = 0;
            var levels = _app.Save.Progression.Levels;
            if (levels != null)
            {
                for (int i = 0; i < levels.Count; i++)
                {
                    if (levels[i] != null)
                    {
                        total += levels[i].Stars;
                    }
                }
            }

            return total;
        }
    }
}
