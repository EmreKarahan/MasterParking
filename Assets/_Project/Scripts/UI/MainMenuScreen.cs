using MasterParking.App;
using MasterParking.Core;
using MasterParking.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace MasterParking.UI
{
    /// <summary>Title screen. Mirrors the source game's single play entry point.</summary>
    public sealed class MainMenuScreen : MonoBehaviour
    {
        private AppRoot _app;
        private AppFlow _flow;
        private UiRoot _ui;
        private SettingsPanel _settings;

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
            int levelCount = _app.Levels.Count;

            UiFactory.SignSprite(safe, "p", new Color(1f, 1f, 1f, 0.09f), 620f)
                .rectTransform.anchorMin = new Vector2(1f, 0.5f);
            // Re-anchor the watermark to the right edge.
            Image watermark = safe.Find("Sign_p")?.GetComponent<Image>();
            if (watermark != null)
            {
                UiFactory.SetRect(watermark.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                    new Vector2(1f, 0.5f), new Vector2(-60f, -40f), new Vector2(620f, 620f));
                watermark.color = new Color(1f, 1f, 1f, 0.09f);
            }

            Text header = UiFactory.Text(safe, "Header", "MASTER", 74, UiTheme.Text,
                TextAnchor.MiddleCenter, false, FontStyle.Bold);
            UiFactory.SetRect(header.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -130f), new Vector2(900f, 90f));

            Text title = UiFactory.Text(safe, "Title", "PARKING", 112, UiTheme.Gold,
                TextAnchor.MiddleCenter, false, FontStyle.Bold);
            UiFactory.SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -212f), new Vector2(1100f, 130f));

            Image rule = UiFactory.Image(safe, "Rule", UiTheme.Gold, TextureFactory.RoundedRect(64, 18));
            UiFactory.SetRect(rule.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -330f), new Vector2(300f, 8f));

            string subtitle = levelCount > 0
                ? $"{levelCount} BÖLÜM  •  GERÇEK PARK DENEYİMİ"
                : "BÖLÜM VERİSİ BULUNAMADI";
            Text sub = UiFactory.Text(safe, "Subtitle", subtitle, 22, UiTheme.Muted,
                TextAnchor.MiddleCenter, false, FontStyle.Bold);
            UiFactory.SetRect(sub.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -362f), new Vector2(900f, 44f));

            int continueId = ResolveContinueLevel(levelCount);
            string playLabel = _app.Save.LastPlayedLevelId > 1 && levelCount > 0 ? "DEVAM ET" : "OYNA";

            Button play = UiFactory.Button(safe, "Play", playLabel, UiTheme.ButtonSize, UiTheme.Gold, UiTheme.BackgroundTop,
                () => _flow.StartLevelById(continueId), _app.Audio);
            UiFactory.SetRect(play.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(440f, 88f));
            play.interactable = levelCount > 0;

            Button levels = UiFactory.Button(safe, "Levels", "BÖLÜMLER", UiTheme.ButtonSize, UiTheme.Button, UiTheme.Text,
                _flow.ShowLevelSelect, _app.Audio);
            UiFactory.SetRect(levels.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, -82f), new Vector2(440f, 82f));
            levels.interactable = levelCount > 0;

            Button settings = UiFactory.Button(safe, "Settings", "AYARLAR", UiTheme.ButtonSize, UiTheme.Button, UiTheme.Text,
                OpenSettings, _app.Audio);
            UiFactory.SetRect(settings.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, -180f), new Vector2(440f, 82f));

            Button quit = UiFactory.Button(safe, "Quit", "ÇIKIŞ", UiTheme.ButtonSize, UiTheme.PanelSoft, UiTheme.Muted,
                Quit, _app.Audio);
            UiFactory.SetRect(quit.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, -278f), new Vector2(440f, 72f));

            int stars = TotalStars();
            Text stats = UiFactory.Text(safe, "Stats", $"★  {stars} / {levelCount * ScoreCalculator.MaxStars}", 24,
                UiTheme.Gold, TextAnchor.MiddleRight, false, FontStyle.Bold);
            UiFactory.SetRect(stats.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(1f, 1f), new Vector2(-36f, -28f), new Vector2(400f, 48f));

            Text version = UiFactory.Text(safe, "Version", "MASTER PARKING  •  v1.0", 16, UiTheme.Muted,
                TextAnchor.MiddleLeft);
            UiFactory.SetRect(version.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(0f, 0f), new Vector2(32f, 22f), new Vector2(420f, 30f));
        }

        private void OpenSettings()
        {
            if (_settings != null)
            {
                return;
            }

            _settings = new SettingsPanel(
                _ui.SafeArea,
                _app.Audio,
                _app.Settings,
                settings => _app.SaveSettings(settings),
                () => _settings = null);
        }

        private int ResolveContinueLevel(int levelCount)
        {
            if (levelCount <= 0)
            {
                return 1;
            }

            int id = _app.Save.LastPlayedLevelId;
            if (!_app.Save.Progression.IsUnlocked(id))
            {
                id = _app.Save.Progression.UnlockedLevel;
            }

            return Mathf.Clamp(id, 1, levelCount);
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

        private static void Quit()
        {
#if UNITY_EDITOR
            Debug.Log("[MasterParking] Quit requested (ignored in editor).");
#else
            Application.Quit();
#endif
        }
    }
}
