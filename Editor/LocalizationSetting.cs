using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace Suzuryg.LocalSwitchGenerator
{
    internal class LocalizationSetting
    {
        public enum Locale
        {
            ja_JP,
            en_US,
        }

        public Locale CurrentLocale
        {
            get
            {
                if (!_currentLocale.HasValue)
                {
                    var localeString = EditorPrefs.GetString(Constants.Key_Locale);
                    if (localeString is string && Enum.TryParse<Locale>(localeString, out var locale) && Enum.IsDefined(typeof(Locale), locale)) { _currentLocale = locale; }
                    else { _currentLocale = GetDefaultLocale(); }
                }
                return _currentLocale.Value;
            }
            set
            {
                _currentLocale = value;
                EditorPrefs.SetString(Constants.Key_Locale, _currentLocale.ToString());
            }
        }

        public LocalizationTable Table
        {
            get
            {
                if (_tableCache is null)
                {
                    _tableCache = new Dictionary<Locale, LocalizationTable>();
                    _tableCache[Locale.ja_JP] = AssetDatabase.LoadAssetAtPath<LocalizationTable>(AssetDatabase.GUIDToAssetPath(Constants.Guid_ja_JP));
                    _tableCache[Locale.en_US] = ScriptableObject.CreateInstance<LocalizationTable>();
                }
                return _tableCache.TryGetValue(CurrentLocale, out var table) && table != null ? table : ScriptableObject.CreateInstance<LocalizationTable>();
            }
        }

        private Locale? _currentLocale = null;
        private Dictionary<Locale, LocalizationTable> _tableCache = null;

        private Locale GetDefaultLocale()
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            return currentCulture.Name == "ja-JP" ? Locale.ja_JP : Locale.en_US;
        }

        // [MenuItem("LocalSwitchGenerator/Debug/ResetLocale")]
        private static void ResetLocale()
        {
            EditorPrefs.DeleteKey(Constants.Key_Locale);
        }
    }
}
