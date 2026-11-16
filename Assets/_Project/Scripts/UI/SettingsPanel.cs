using System;
using MasterParking.Audio;
using MasterParking.SaveSystem;
using UnityEngine;
using UnityEngine.UI;

namespace MasterParking.UI
{
    /// <summary>Audio/haptics settings overlay used by the main menu.</summary>
    public sealed class SettingsPanel
    {
        private readonly AudioService _audio;
        private readonly SettingsState _settings;
        private readonly Action<SettingsState> _onChanged;
        private readonly GameObject _root;
        private Text _musicValue;
        private Text _sfxValue;
        private Text _engineValue;
        private Text _hapticsValue;

        public SettingsPanel(
            RectTransform parent,
            AudioService audio,
            SettingsState settings,
            Action<SettingsState> onChanged,
            Action onClose)
        {
            _audio = audio;
            _settings = settings?.Clone() ?? new SettingsState();
            _onChanged = onChanged;
            _settings.Sanitize();

            _root = new GameObject("SettingsOverlay", typeof(RectTransform));
            var rootRect = (RectTransform)_root.transform;
            rootRect.SetParent(parent, false);
            UiFactory.Stretch(rootRect);

            Image overlay = UiFactory.Image(rootRect, "Dim", UiTheme.Overlay, null, true);
            UiFactory.Stretch(overlay.rectTransform);

            Image panel = UiFactory.Panel(rootRect, "Panel", UiTheme.Panel, true);
            UiFactory.SetRect(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 640f));

            Text title = UiFactory.Text(panel.transform, "Title", "AYARLAR", 48, UiTheme.Gold,
                TextAnchor.MiddleCenter, false, FontStyle.Bold);
            UiFactory.SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(500f, 64f));

            _musicValue = BuildSliderRow(panel.transform, "MÜZİK", 120f, 0);
            _sfxValue = BuildSliderRow(panel.transform, "SES EFEKTLERİ", 44f, 1);
            _engineValue = BuildSliderRow(panel.transform, "MOTOR SESİ", -32f, 2);
            _hapticsValue = BuildToggleRow(panel.transform, "TİTREŞİM", -108f);

            Button close = UiFactory.Button(panel.transform, "Close", "KAPAT", 28, UiTheme.Button, UiTheme.Text,
                () => Close(onClose), _audio);
            UiFactory.SetRect(close.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(260f, 66f));

            RefreshValues();
        }

        public void Dispose()
        {
            if (_root != null)
            {
                UnityEngine.Object.Destroy(_root);
            }
        }

        private Text BuildSliderRow(Transform parent, string label, float y, int target)
        {
            RectTransform row = UiFactory.Rect("Row_" + label, parent);
            UiFactory.SetRect(row, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(650f, 64f));

            Text caption = UiFactory.Text(row, "Label", label, 25, UiTheme.Text, TextAnchor.MiddleLeft, false, FontStyle.Bold);
            UiFactory.SetRect(caption.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f), new Vector2(0f, 0f), new Vector2(260f, 56f));

            Text value = UiFactory.Text(row, "Value", "0%", 24, UiTheme.Gold, TextAnchor.MiddleCenter);
            UiFactory.SetRect(value.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(150f, 0f), new Vector2(110f, 50f));

            Button minus = UiFactory.Button(row, "Minus", "-", 32, UiTheme.Button, UiTheme.Text,
                () => Adjust(target, -0.1f), _audio);
            UiFactory.SetRect(minus.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f), new Vector2(-70f, 0f), new Vector2(62f, 56f));

            Button plus = UiFactory.Button(row, "Plus", "+", 32, UiTheme.Button, UiTheme.Text,
                () => Adjust(target, 0.1f), _audio);
            UiFactory.SetRect(plus.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f), new Vector2(0f, 0f), new Vector2(62f, 56f));

            return value;
        }

        private Text BuildToggleRow(Transform parent, string label, float y)
        {
            RectTransform row = UiFactory.Rect("Row_" + label, parent);
            UiFactory.SetRect(row, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(650f, 64f));

            Text caption = UiFactory.Text(row, "Label", label, 25, UiTheme.Text, TextAnchor.MiddleLeft, false, FontStyle.Bold);
            UiFactory.SetRect(caption.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f), new Vector2(0f, 0f), new Vector2(260f, 56f));

            Text value = UiFactory.Text(row, "Value", "AÇIK", 24, UiTheme.Gold, TextAnchor.MiddleCenter);
            UiFactory.SetRect(value.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f), new Vector2(-220f, 0f), new Vector2(140f, 50f));

            Button toggle = UiFactory.Button(row, "Toggle", "DEĞİŞTİR", 18, UiTheme.Button, UiTheme.Text,
                () =>
                {
                    _settings.Haptics = !_settings.Haptics;
                    Publish();
                }, _audio);
            UiFactory.SetRect(toggle.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f), new Vector2(0f, 0f), new Vector2(150f, 56f));

            return value;
        }

        private void Adjust(int target, float delta)
        {
            if (target == 0)
            {
                _settings.MusicVolume = Mathf.Clamp01(_settings.MusicVolume + delta);
            }
            else if (target == 1)
            {
                _settings.SfxVolume = Mathf.Clamp01(_settings.SfxVolume + delta);
            }
            else
            {
                _settings.EngineVolume = Mathf.Clamp01(_settings.EngineVolume + delta);
            }

            Publish();
        }

        private void Publish()
        {
            _settings.Sanitize();
            RefreshValues();
            _onChanged?.Invoke(_settings);
        }

        private void RefreshValues()
        {
            if (_musicValue != null)
            {
                _musicValue.text = Mathf.RoundToInt(_settings.MusicVolume * 100f) + "%";
            }

            if (_sfxValue != null)
            {
                _sfxValue.text = Mathf.RoundToInt(_settings.SfxVolume * 100f) + "%";
            }

            if (_engineValue != null)
            {
                _engineValue.text = Mathf.RoundToInt(_settings.EngineVolume * 100f) + "%";
            }

            if (_hapticsValue != null)
            {
                _hapticsValue.text = _settings.Haptics ? "AÇIK" : "KAPALI";
            }
        }

        private void Close(Action onClose)
        {
            onClose?.Invoke();
            Dispose();
        }
    }
}
