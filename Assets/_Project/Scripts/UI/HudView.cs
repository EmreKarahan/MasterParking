using System;
using System.Collections.Generic;
using MasterParking.App;
using MasterParking.Audio;
using MasterParking.CameraSystem;
using MasterParking.Core;
using MasterParking.Gameplay;
using MasterParking.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace MasterParking.UI
{
    /// <summary>
    /// Landscape HUD and modal flow for one attempt. All buttons raise events;
    /// the gameplay screen decides whether they restart, advance or navigate.
    /// </summary>
    public sealed class HudView : IGameplayHud
    {
        private readonly AppRoot _app;
        private readonly AudioService _audio;
        private readonly RectTransform _root;
        private readonly Dictionary<Gear, Button> _gearButtons = new Dictionary<Gear, Button>();
        private readonly Dictionary<Gear, Text> _gearLabels = new Dictionary<Gear, Text>();

        private Text _levelIdText;
        private Text _levelNameText;
        private Text _timerText;
        private Text _speedText;
        private Text _cameraLabel;
        private Text _parkingLabel;
        private Text _parkingPercent;
        private Image _parkingFill;
        private GameObject _parkingPanel;
        private Button _pauseButton;
        private Button _cameraButton;
        private PedalButton _gasPedal;
        private PedalButton _brakePedal;
        private SteeringWheelControl _steering;

        private GameObject _modal;
        private Gear _lastGear = (Gear)(-1);
        private int _lastTenths = -1;
        private int _lastSpeed = -1;
        private bool _controlsEnabled = true;

        public event Action PausePressed;
        public event Action CameraPressed;
        public event Action ResumePressed;
        public event Action RestartPressed;
        public event Action NextPressed;
        public event Action LevelsPressed;
        public event Action MenuPressed;
        public event Action<bool> GasChanged;
        public event Action<bool> BrakeChanged;
        public event Action<float, bool> SteeringChanged;
        public event Action<Gear> GearSelected;

        public HudView(
            RectTransform parent,
            AppRoot app,
            LevelDefinition level,
            int levelCount,
            CameraMode cameraMode,
            AudioService audio)
        {
            _app = app;
            _audio = audio;

            _root = UiFactory.Rect("HUD", parent);
            UiFactory.Stretch(_root);

            BuildTopBar(levelCount);
            BuildParkingPanel();
            BuildGearSelector();
            BuildSteeringWheel();
            BuildPedals();

            SetLevelInfo(level.Name, level.Id, levelCount);
            SetCameraMode(cameraMode);
        }

        public void Dispose()
        {
            if (_root != null)
            {
                UnityEngine.Object.Destroy(_root.gameObject);
            }
        }

        public void SetLevelInfo(string levelName, int levelId, int levelCount)
        {
            if (_levelIdText != null)
            {
                _levelIdText.text = $"BÖLÜM {levelId:00} / {levelCount:00}";
            }

            if (_levelNameText != null)
            {
                _levelNameText.text = levelName ?? string.Empty;
            }
        }

        public void SetTimer(float seconds)
        {
            int tenths = Mathf.FloorToInt(Mathf.Max(0f, seconds) * 10f);
            if (tenths == _lastTenths || _timerText == null)
            {
                return;
            }

            _lastTenths = tenths;
            int minutes = tenths / 600;
            int wholeSeconds = (tenths / 10) % 60;
            int fraction = tenths % 10;
            _timerText.text = $"{minutes:00}:{wholeSeconds:00}.{fraction}";
        }

        public void SetGear(Gear gear)
        {
            if (gear == _lastGear)
            {
                return;
            }

            _lastGear = gear;
            foreach (KeyValuePair<Gear, Button> entry in _gearButtons)
            {
                bool selected = entry.Key == gear;
                entry.Value.GetComponent<Image>().color = selected ? UiTheme.Gold : UiTheme.Button;
                if (_gearLabels.TryGetValue(entry.Key, out Text label))
                {
                    label.color = selected ? UiTheme.BackgroundTop : UiTheme.Text;
                }
            }
        }

        public void SetSpeed(float metresPerSecond)
        {
            if (_speedText == null)
            {
                return;
            }

            int kmh = Mathf.RoundToInt(Mathf.Max(0f, metresPerSecond) * 3.6f);
            if (kmh == _lastSpeed)
            {
                return;
            }

            _lastSpeed = kmh;
            _speedText.text = kmh + " km/s";
        }

        public void SetParking(in ParkingStatus status)
        {
            if (_parkingPanel == null)
            {
                return;
            }

            bool visible = status.IsVisible && _modal == null;
            if (_parkingPanel.activeSelf != visible)
            {
                _parkingPanel.SetActive(visible);
            }

            if (!visible)
            {
                return;
            }

            float progress = Mathf.Clamp01(status.Progress);
            _parkingFill.fillAmount = progress;
            _parkingFill.color = status.Phase == ParkingPhase.Aligning ? UiTheme.Gold : UiTheme.Green;

            if (_parkingPercent != null)
            {
                _parkingPercent.text = Mathf.RoundToInt(progress * 100f) + "%";
            }

            if (_parkingLabel != null)
            {
                switch (status.Phase)
                {
                    case ParkingPhase.Holding:
                        _parkingLabel.text = "PARK EDİLİYOR  •  P vitesi hızlandırır";
                        break;
                    case ParkingPhase.Aligning:
                        _parkingLabel.text = "HİZALANIYOR  •  dur ve bekle";
                        break;
                    default:
                        _parkingLabel.text = "PARK ALANI";
                        break;
                }
            }
        }

        public void SetCameraMode(CameraMode mode)
        {
            if (_cameraLabel != null)
            {
                _cameraLabel.text = mode == CameraMode.Follow ? "TAKİP" : "GENEL";
            }
        }

        public void SetControlsEnabled(bool enabled)
        {
            _controlsEnabled = enabled;
            _gasPedal?.SetInteractable(enabled);
            _brakePedal?.SetInteractable(enabled);
            _steering?.SetInteractable(enabled);

            foreach (KeyValuePair<Gear, Button> entry in _gearButtons)
            {
                entry.Value.interactable = enabled;
            }

            if (_pauseButton != null)
            {
                _pauseButton.interactable = enabled;
            }

            if (_cameraButton != null)
            {
                _cameraButton.interactable = enabled;
            }
        }

        public void ShowCrash(string reason)
        {
            SetControlsEnabled(false);
            RectTransform panel = CreateModal("KAZA!", UiTheme.Red, new Vector2(820f, 520f));

            Text body = UiFactory.Text(panel, "Reason", reason, 30, UiTheme.Text,
                TextAnchor.MiddleCenter, true, FontStyle.Bold);
            UiFactory.SetRect(body.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, 58f), new Vector2(700f, 90f));

            Text hint = UiFactory.Text(panel, "Hint", "Daha yavaş gir, direksiyonu erken topla.", 21, UiTheme.Muted,
                TextAnchor.MiddleCenter, true);
            UiFactory.SetRect(hint.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, -8f), new Vector2(700f, 54f));

            AddModalButton(panel, "YENİDEN DENE", -96f, 360f, 74f, UiTheme.Gold, UiTheme.BackgroundTop,
                () => RestartPressed?.Invoke());
            AddModalButton(panel, "BÖLÜMLER", -190f, 170f, 62f, UiTheme.Button, UiTheme.Text,
                () => LevelsPressed?.Invoke(), -100f);
            AddModalButton(panel, "ANA MENÜ", -190f, 170f, 62f, UiTheme.PanelSoft, UiTheme.Muted,
                () => MenuPressed?.Invoke(), 100f);
        }

        public void ShowWin(LevelCompletionResult result, bool hasNextLevel)
        {
            SetControlsEnabled(false);
            RectTransform panel = CreateModal("BÖLÜM TAMAMLANDI!", UiTheme.Gold, new Vector2(820f, 660f));

            RectTransform stars = UiFactory.Rect("Stars", panel);
            UiFactory.SetRect(stars, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, 124f), new Vector2(330f, 100f));

            for (int i = 0; i < ScoreCalculator.MaxStars; i++)
            {
                Image star = UiFactory.Star(stars, "Star_" + i, i < result.Stars, 84f);
                UiFactory.SetRect(star.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f), new Vector2((i - 1) * 108f, 0f), new Vector2(84f, 84f));
            }

            Text time = UiFactory.Text(panel, "Time", "SÜRE  " + FormatTime(result.ElapsedTime), 28, UiTheme.Text,
                TextAnchor.MiddleCenter, false, FontStyle.Bold);
            UiFactory.SetRect(time.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, 48f), new Vector2(600f, 46f));

            string best = result.NewBestTime ? "YENİ REKOR!" : "En iyi: " + FormatTime(result.BestTime);
            Text bestText = UiFactory.Text(panel, "Best", best, 22,
                result.NewBestTime ? UiTheme.Green : UiTheme.Muted, TextAnchor.MiddleCenter, false, FontStyle.Bold);
            UiFactory.SetRect(bestText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, 4f), new Vector2(600f, 38f));

            if (result.UnlockedNextLevel)
            {
                Text unlocked = UiFactory.Text(panel, "Unlocked", "YENİ BÖLÜM AÇILDI!", 22, UiTheme.Green,
                    TextAnchor.MiddleCenter, false, FontStyle.Bold);
                UiFactory.SetRect(unlocked.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f), new Vector2(0f, -42f), new Vector2(620f, 36f));
            }

            if (hasNextLevel)
            {
                AddModalButton(panel, "SONRAKİ BÖLÜM", -122f, 380f, 72f, UiTheme.Gold, UiTheme.BackgroundTop,
                    () => NextPressed?.Invoke());
            }
            else
            {
                Text done = UiFactory.Text(panel, "Done", "TÜM BÖLÜMLER TAMAMLANDI!", 22, UiTheme.Gold,
                    TextAnchor.MiddleCenter, false, FontStyle.Bold);
                UiFactory.SetRect(done.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f), new Vector2(0f, -122f), new Vector2(620f, 36f));
            }

            AddModalButton(panel, "TEKRAR", -222f, 180f, 62f, UiTheme.Button, UiTheme.Text,
                () => RestartPressed?.Invoke(), -104f);
            AddModalButton(panel, "BÖLÜMLER", -222f, 180f, 62f, UiTheme.PanelSoft, UiTheme.Muted,
                () => LevelsPressed?.Invoke(), 104f);
        }

        public void ShowPause(bool visible)
        {
            if (!visible)
            {
                ClearModal();
                return;
            }

            SetControlsEnabled(false);
            RectTransform panel = CreateModal("DURAKLATILDI", UiTheme.Gold, new Vector2(600f, 560f));

            AddModalButton(panel, "DEVAM ET", 96f, 380f, 70f, UiTheme.Gold, UiTheme.BackgroundTop,
                () => ResumePressed?.Invoke());
            AddModalButton(panel, "YENİDEN BAŞLAT", 10f, 380f, 62f, UiTheme.Button, UiTheme.Text,
                () => RestartPressed?.Invoke());
            AddModalButton(panel, "BÖLÜMLER", -76f, 380f, 62f, UiTheme.PanelSoft, UiTheme.Text,
                () => LevelsPressed?.Invoke());
            AddModalButton(panel, "ANA MENÜ", -162f, 380f, 62f, UiTheme.PanelSoft, UiTheme.Muted,
                () => MenuPressed?.Invoke());
        }

        private void BuildTopBar(int levelCount)
        {
            Image levelPanel = UiFactory.Panel(_root, "LevelPanel", UiTheme.PanelSoft);
            UiFactory.SetRect(levelPanel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(28f, -24f), new Vector2(540f, 96f));

            _levelIdText = UiFactory.Text(levelPanel.transform, "LevelId", "BÖLÜM 01 / " + levelCount.ToString("00"), 19,
                UiTheme.Muted, TextAnchor.MiddleLeft, false, FontStyle.Bold);
            UiFactory.SetRect(_levelIdText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(22f, -12f), new Vector2(480f, 30f));

            _levelNameText = UiFactory.Text(levelPanel.transform, "LevelName", "—", 28, UiTheme.Text,
                TextAnchor.MiddleLeft, false, FontStyle.Bold);
            UiFactory.SetRect(_levelNameText.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(0f, 0f), new Vector2(22f, 14f), new Vector2(490f, 42f));

            Image timerPanel = UiFactory.Panel(_root, "TimerPanel", UiTheme.Panel);
            UiFactory.SetRect(timerPanel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(300f, 96f));

            Text timerLabel = UiFactory.Text(timerPanel.transform, "Label", "SÜRE", 17, UiTheme.Muted,
                TextAnchor.MiddleCenter, false, FontStyle.Bold);
            UiFactory.SetRect(timerLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -10f), new Vector2(240f, 26f));

            _timerText = UiFactory.Text(timerPanel.transform, "Timer", "00:00.0", 36, UiTheme.Gold,
                TextAnchor.MiddleCenter, false, FontStyle.Bold);
            UiFactory.SetRect(_timerText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0f, 10f), new Vector2(260f, 54f));

            _cameraButton = UiFactory.Button(_root, "Camera", "TAKİP", 20, UiTheme.Button, UiTheme.Text,
                () => CameraPressed?.Invoke(), _audio);
            UiFactory.SetRect(_cameraButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(1f, 1f), new Vector2(-152f, -24f), new Vector2(150f, 62f));
            _cameraLabel = _cameraButton.GetComponentInChildren<Text>();

            _pauseButton = UiFactory.Button(_root, "Pause", "II", 34, UiTheme.Panel, UiTheme.Text,
                () => PausePressed?.Invoke(), _audio);
            UiFactory.SetRect(_pauseButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(1f, 1f), new Vector2(-28f, -24f), new Vector2(108f, 62f));
        }

        private void BuildParkingPanel()
        {
            Image panel = UiFactory.Panel(_root, "ParkingPanel", UiTheme.Panel);
            UiFactory.SetRect(panel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -136f), new Vector2(480f, 96f));
            _parkingPanel = panel.gameObject;

            _parkingLabel = UiFactory.Text(panel.transform, "Phase", "PARK ALANI", 19, UiTheme.Text,
                TextAnchor.MiddleLeft, false, FontStyle.Bold);
            UiFactory.SetRect(_parkingLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(18f, -10f), new Vector2(380f, 30f));

            _parkingPercent = UiFactory.Text(panel.transform, "Percent", "0%", 22, UiTheme.Gold,
                TextAnchor.MiddleRight, false, FontStyle.Bold);
            UiFactory.SetRect(_parkingPercent.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(1f, 1f), new Vector2(-18f, -10f), new Vector2(80f, 30f));

            Image track = UiFactory.Image(panel.transform, "Track", new Color(1f, 1f, 1f, 0.12f),
                TextureFactory.RoundedRect(48, 14));
            UiFactory.SetRect(track.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0f, 16f), new Vector2(-36f, 16f));

            Image fill = UiFactory.Image(track.transform, "Fill", UiTheme.Green, TextureFactory.RoundedRect(48, 14));
            UiFactory.Stretch(fill.rectTransform);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 0f;
            _parkingFill = fill;

            panel.gameObject.SetActive(false);
        }

        private void BuildGearSelector()
        {
            Image panel = UiFactory.Panel(_root, "GearPanel", UiTheme.PanelSoft);
            UiFactory.SetRect(panel.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0f, 28f), new Vector2(390f, 92f));

            var gears = new[] { Gear.Park, Gear.Reverse, Gear.Drive };
            var labels = new[] { "P", "R", "D" };
            for (int i = 0; i < gears.Length; i++)
            {
                Gear gear = gears[i];
                Button button = UiFactory.Button(panel.transform, "Gear_" + labels[i], labels[i], 34,
                    UiTheme.Button, UiTheme.Text, () => GearSelected?.Invoke(gear), _audio);
                UiFactory.SetRect(button.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f), new Vector2((i - 1) * 118f, 0f), new Vector2(96f, 66f));

                _gearButtons[gear] = button;
                _gearLabels[gear] = button.GetComponentInChildren<Text>();
            }
        }

        private void BuildSteeringWheel()
        {
            Image touch = UiFactory.Circle(_root, "Steering", new Color(1f, 1f, 1f, 0.07f), true);
            UiFactory.SetRect(touch.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(0f, 0f), new Vector2(38f, 26f), new Vector2(320f, 320f));

            RectTransform wheel = UiFactory.Rect("Wheel", touch.transform);
            UiFactory.SetRect(wheel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(268f, 268f));

            var ring = wheel.gameObject.AddComponent<Image>();
            ring.sprite = TextureFactory.Ring(256, 0.73f);
            ring.color = new Color(0.78f, 0.84f, 0.92f, 0.92f);
            ring.raycastTarget = false;

            Image spokeH = UiFactory.Image(wheel, "SpokeH", new Color(0.78f, 0.84f, 0.92f, 0.9f),
                TextureFactory.RoundedRect(48, 14));
            UiFactory.SetRect(spokeH.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(205f, 15f));

            Image spokeV = UiFactory.Image(wheel, "SpokeV", new Color(0.78f, 0.84f, 0.92f, 0.9f),
                TextureFactory.RoundedRect(48, 14));
            UiFactory.SetRect(spokeV.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, -28f), new Vector2(15f, 118f));

            Image hub = UiFactory.Circle(wheel, "Hub", UiTheme.Gold);
            UiFactory.SetRect(hub.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(56f, 56f));

            _steering = touch.gameObject.AddComponent<SteeringWheelControl>();
            _steering.Initialize(touch.rectTransform, wheel, touch);
            _steering.SteeringChanged += (ratio, held) => SteeringChanged?.Invoke(ratio, held);
        }

        private void BuildPedals()
        {
            _gasPedal = CreatePedal("Gas", "GAZ", new Vector2(1f, 0f), new Vector2(-38f, 30f),
                new Vector2(190f, 190f), UiTheme.GreenDark, UiTheme.Green, out Image gasImage);
            _gasPedal.Pressed += held => GasChanged?.Invoke(held);

            _brakePedal = CreatePedal("Brake", "FREN", new Vector2(1f, 0f), new Vector2(-244f, 44f),
                new Vector2(150f, 150f), UiTheme.RedDark, UiTheme.Red, out Image brakeImage);
            _brakePedal.Pressed += held => BrakeChanged?.Invoke(held);

            _speedText = UiFactory.Text(_root, "Speed", "0 km/s", 24, UiTheme.Text,
                TextAnchor.MiddleRight, false, FontStyle.Bold);
            UiFactory.SetRect(_speedText.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(1f, 0f), new Vector2(-38f, 238f), new Vector2(280f, 42f));
        }

        private PedalButton CreatePedal(
            string name,
            string label,
            Vector2 anchor,
            Vector2 position,
            Vector2 size,
            Color background,
            Color labelColor,
            out Image image)
        {
            image = UiFactory.Circle(_root, name, background, true);
            UiFactory.SetRect(image.rectTransform, anchor, anchor, anchor, position, size);

            Image inner = UiFactory.Circle(image.transform, "Inner", new Color(0f, 0f, 0f, 0.12f));
            UiFactory.Stretch(inner.rectTransform, 14f, 14f, 14f, 14f);
            inner.raycastTarget = false;

            Text text = UiFactory.Text(image.transform, "Label", label, 30, labelColor,
                TextAnchor.MiddleCenter, false, FontStyle.Bold);
            UiFactory.Stretch(text.rectTransform, 6f, 6f, 6f, 6f);

            var pedal = image.gameObject.AddComponent<PedalButton>();
            pedal.Initialize(image);
            return pedal;
        }

        private RectTransform CreateModal(string title, Color accent, Vector2 size)
        {
            ClearModal();

            var overlay = new GameObject("Modal", typeof(RectTransform));
            var overlayRect = (RectTransform)overlay.transform;
            overlayRect.SetParent(_root, false);
            UiFactory.Stretch(overlayRect);

            Image dim = UiFactory.Image(overlayRect, "Dim", UiTheme.Overlay, null, true);
            UiFactory.Stretch(dim.rectTransform);

            Image panel = UiFactory.Panel(overlayRect, "Panel", UiTheme.Panel, true);
            UiFactory.SetRect(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, size);

            Text titleText = UiFactory.Text(panel.transform, "Title", title, 48, accent,
                TextAnchor.MiddleCenter, false, FontStyle.Bold);
            UiFactory.SetRect(titleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(size.x - 80f, 66f));

            _modal = overlay;
            overlay.transform.SetAsLastSibling();
            if (_parkingPanel != null)
            {
                _parkingPanel.SetActive(false);
            }

            return panel.rectTransform;
        }

        private void AddModalButton(
            RectTransform panel,
            string label,
            float y,
            float width,
            float height,
            Color background,
            Color foreground,
            Action action,
            float x = 0f)
        {
            Button button = UiFactory.Button(panel, label, label, 26, background, foreground, action, _audio);
            UiFactory.SetRect(button.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(x, y), new Vector2(width, height));
        }

        private void ClearModal()
        {
            if (_modal != null)
            {
                UnityEngine.Object.Destroy(_modal);
                _modal = null;
            }
        }

        private static string FormatTime(float seconds)
        {
            if (seconds <= 0f)
            {
                return "--:--.-";
            }

            int tenths = Mathf.FloorToInt(seconds * 10f);
            int minutes = tenths / 600;
            int wholeSeconds = (tenths / 10) % 60;
            int fraction = tenths % 10;
            return $"{minutes:00}:{wholeSeconds:00}.{fraction}";
        }
    }
}
