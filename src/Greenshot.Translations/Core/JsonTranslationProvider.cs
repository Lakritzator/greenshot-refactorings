/*
 * Greenshot - a free and open source screenshot tool
 * Copyright (C) 2004-2026 Thomas Braun, Jens Klingen, Robin Krom
 *
 * For more information see: https://getgreenshot.org/
 * The Greenshot project is hosted on GitHub https://github.com/greenshot/greenshot
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 1 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Greenshot.Translations.Core
{
    /// <summary>
    /// <see cref="ITranslationManager"/> implementation that reads translation data
    /// from JSON files.
    ///
    /// <para>
    /// <b>File naming convention:</b>
    /// <list type="bullet">
    ///   <item><description>Main application: <c>{ietf}.json</c>, e.g. <c>en-US.json</c></description></item>
    ///   <item><description>Plugin: <c>{prefix}.{ietf}.json</c>, e.g. <c>box.en-US.json</c></description></item>
    /// </list>
    /// </para>
    ///
    /// <para>
    /// <b>File content format:</b>
    /// <code>
    /// {
    ///   "ietf": "en-US",
    ///   "description": "English",
    ///   "version": "1.0",
    ///   "prefix": "box",
    ///   "resources": {
    ///     "cancel": "Cancel",
    ///     "multi_line": "First line\nSecond line"
    ///   }
    /// }
    /// </code>
    /// </para>
    ///
    /// <para>
    /// <b>Fallback chain:</b> <c>zh-TW</c> → <c>zh</c> → <c>en-US</c> (FallbackLanguage).
    /// </para>
    /// </summary>
    public sealed class JsonTranslationProvider : ITranslationManager
    {
        // Matches the stem of a translation filename.
        // Group 1 (optional): plugin prefix, e.g. "box" in "box.en-US"
        // Group 2 (required): IETF language tag, e.g. "en-US", "zh-TW", "de-x-franconia"
        private static readonly Regex _ietfPattern = new(
            @"^(?:([a-zA-Z0-9]+)\.)?([a-zA-Z]{2,3}(?:-[a-zA-Z0-9]+)*)$",
            RegexOptions.Compiled);

        // Returned as the value for any missing translation key.
        private const string MissingSentinelFormat = "###{0}###";

        private readonly List<string> _searchPaths = new();
        private Dictionary<string, string> _resources = new(StringComparer.OrdinalIgnoreCase);
        private string _currentLanguage = "en-US";

        /// <inheritdoc/>
        public event EventHandler LanguageChanged;

        /// <inheritdoc/>
        public string CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (string.Equals(_currentLanguage, value, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                _currentLanguage = value;
                Reload();
                LanguageChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <inheritdoc/>
        public string FallbackLanguage { get; set; } = "en-US";

        /// <inheritdoc/>
        public void AddPath(string directoryPath)
        {
            if (!string.IsNullOrEmpty(directoryPath)
                && !_searchPaths.Contains(directoryPath))
            {
                _searchPaths.Add(directoryPath);
            }
        }

        /// <inheritdoc/>
        public IReadOnlyList<TranslationFile> GetAvailableLanguages()
        {
            var result = new List<TranslationFile>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var dir in _searchPaths)
            {
                if (!Directory.Exists(dir))
                {
                    continue;
                }

                foreach (var filePath in Directory.GetFiles(dir, "*.json"))
                {
                    if (TryParseFileName(filePath, out var translationFile)
                        && seen.Add(filePath))
                    {
                        result.Add(translationFile);
                    }
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public string GetString(string key)
            => _resources.TryGetValue(key, out var value)
                ? value
                : string.Format(MissingSentinelFormat, key);

        /// <inheritdoc/>
        public bool TryGetString(string key, out string value)
            => _resources.TryGetValue(key, out value);

        /// <inheritdoc/>
        public bool HasKey(string key)
            => _resources.ContainsKey(key);

        /// <summary>
        /// Clears the in-memory cache and reloads translations for the
        /// current language, layered on top of the fallback language.
        /// </summary>
        private void Reload()
        {
            _resources = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Load fallback language first so selected language can override
            if (!string.Equals(_currentLanguage, FallbackLanguage, StringComparison.OrdinalIgnoreCase))
            {
                LoadAll(FallbackLanguage);
            }

            // Walk the dialect chain: zh-TW → zh → (already handled by fallback)
            var chain = BuildFallbackChain(_currentLanguage);
            foreach (var ietf in chain)
            {
                LoadAll(ietf);
            }
        }

        /// <summary>
        /// Returns the fallback chain for <paramref name="ietf"/> from most
        /// specific to least specific (excluding the ultimate fallback language
        /// which is handled separately).
        /// </summary>
        private static IEnumerable<string> BuildFallbackChain(string ietf)
        {
            // Most specific first
            yield return ietf;

            // Strip region subtag: "zh-TW" → "zh"
            var dashIndex = ietf.IndexOf('-');
            if (dashIndex > 0)
            {
                var baseCode = ietf.Substring(0, dashIndex);
                if (!string.Equals(baseCode, ietf, StringComparison.OrdinalIgnoreCase))
                {
                    yield return baseCode;
                }
            }
        }

        /// <summary>
        /// Loads all JSON translation files whose names match <paramref name="ietf"/>
        /// from all registered search paths.
        /// </summary>
        private void LoadAll(string ietf)
        {
            foreach (var dir in _searchPaths)
            {
                if (!Directory.Exists(dir))
                {
                    continue;
                }

                // Match both "en-US.json" and "box.en-US.json"
                foreach (var filePath in Directory.GetFiles(dir, $"*{ietf}.json"))
                {
                    if (TryParseFileName(filePath, out var tf)
                        && string.Equals(tf.Ietf, ietf, StringComparison.OrdinalIgnoreCase))
                    {
                        LoadFile(filePath, tf.Prefix);
                    }
                }
            }
        }

        /// <summary>
        /// Parses a single JSON translation file and merges its resources
        /// into the in-memory dictionary.  Keys from plugin files are
        /// namespaced as <c>{prefix}.{key}</c>.
        /// </summary>
        private void LoadFile(string filePath, string prefix)
        {
            using var stream = File.OpenRead(filePath);
            using var doc = JsonDocument.Parse(stream);

            if (!doc.RootElement.TryGetProperty("resources", out var resources))
            {
                return;
            }

            foreach (var entry in resources.EnumerateObject())
            {
                var key = prefix is null
                    ? entry.Name
                    : $"{prefix}.{entry.Name}";
                _resources[key] = entry.Value.GetString() ?? string.Empty;
            }
        }

        /// <summary>
        /// Extracts IETF tag and optional prefix from a JSON filename.
        /// Returns <c>false</c> when the filename does not match the convention.
        /// </summary>
        private static bool TryParseFileName(string filePath, out TranslationFile result)
        {
            result = null;
            var stem = Path.GetFileNameWithoutExtension(filePath);
            var match = _ietfPattern.Match(stem);

            if (!match.Success)
            {
                return false;
            }

            var prefix = match.Groups[1].Success ? match.Groups[1].Value : null;
            var ietf = match.Groups[2].Value;

            result = new TranslationFile
            {
                Ietf = ietf,
                Prefix = prefix,
                FilePath = filePath
            };

            return true;
        }
    }
}
