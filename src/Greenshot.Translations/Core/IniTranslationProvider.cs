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
using System.Text.RegularExpressions;

namespace Greenshot.Translations.Core
{
    /// <summary>
    /// <see cref="ITranslationManager"/> implementation that reads translation data
    /// from lightweight INI-style files.
    ///
    /// <para>
    /// <b>File naming convention:</b>
    /// <list type="bullet">
    ///   <item><description>Main application: <c>{ietf}.ini</c>, e.g. <c>en-US.ini</c></description></item>
    ///   <item><description>Plugin: <c>{prefix}.{ietf}.ini</c>, e.g. <c>box.en-US.ini</c></description></item>
    /// </list>
    /// The IETF language tag and optional plugin prefix are derived entirely from
    /// the filename — no metadata is stored inside the file.
    /// </para>
    ///
    /// <para>
    /// <b>File content format:</b>
    /// <code>
    /// # Lines starting with # or ; are comments and are ignored.
    /// # Blank lines are ignored.
    ///
    /// cancel=Cancel
    /// about_bugs=Please report bugs to
    ///
    /// # Use \n for embedded newlines, \t for tabs, \\ for a literal backslash.
    /// about_license=Copyright © 2004-2026 Thomas Braun...\nLine 2 of license text.
    /// </code>
    /// The language display name (e.g. "English") is obtained from
    /// <see cref="CultureInfo"/> at runtime and does not need to appear in the file.
    /// </para>
    ///
    /// <para>
    /// <b>Fallback chain:</b> <c>zh-TW</c> → <c>zh</c> → <c>en-US</c> (FallbackLanguage).
    /// </para>
    ///
    /// <para>
    /// <b>Plugin registration:</b> plugins call <see cref="AddPath"/> from their
    /// initialise method, passing the directory that contains their INI files.
    /// No other registration step is needed.
    /// </para>
    /// </summary>
    public sealed class IniTranslationProvider : ITranslationManager
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

                foreach (var filePath in Directory.GetFiles(dir, "*.ini"))
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

            // Load fallback language first so the selected language can override it.
            if (!string.Equals(_currentLanguage, FallbackLanguage, StringComparison.OrdinalIgnoreCase))
            {
                LoadAll(FallbackLanguage);
            }

            // Walk the dialect chain from least to most specific: zh → zh-TW
            // so the most specific variant wins.
            var chain = BuildFallbackChain(_currentLanguage);
            foreach (var ietf in chain)
            {
                LoadAll(ietf);
            }
        }

        /// <summary>
        /// Returns the fallback chain for <paramref name="ietf"/> from least
        /// specific to most specific (so the most specific file loaded last wins).
        /// The ultimate fallback language is handled separately by <see cref="Reload"/>.
        /// </summary>
        private static IEnumerable<string> BuildFallbackChain(string ietf)
        {
            // Strip the region subtag first: "zh-TW" → "zh"
            var dashIndex = ietf.IndexOf('-');
            if (dashIndex > 0)
            {
                var baseCode = ietf.Substring(0, dashIndex);
                // Only yield the base code when it differs from the full tag
                if (!string.Equals(baseCode, ietf, StringComparison.OrdinalIgnoreCase))
                {
                    yield return baseCode;
                }
            }

            // Most specific last — so its values override the base code
            yield return ietf;
        }

        /// <summary>
        /// Loads all INI translation files whose names match <paramref name="ietf"/>
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

                // Match both "en-US.ini" and "box.en-US.ini"
                foreach (var filePath in Directory.GetFiles(dir, $"*{ietf}.ini"))
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
        /// Parses a single INI translation file and merges its key/value pairs
        /// into the in-memory dictionary.  Keys from plugin files are
        /// namespaced as <c>{prefix}.{key}</c>.
        /// </summary>
        private void LoadFile(string filePath, string prefix)
        {
            foreach (var (key, value) in ParseIniFile(filePath))
            {
                var fullKey = prefix is null ? key : $"{prefix}.{key}";
                _resources[fullKey] = value;
            }
        }

        /// <summary>
        /// Reads an INI-style translation file and yields key/value pairs.
        ///
        /// <para>
        /// Rules:
        /// <list type="bullet">
        ///   <item><description>Lines whose first non-whitespace character is <c>#</c> or <c>;</c> are comments.</description></item>
        ///   <item><description>Blank lines are ignored.</description></item>
        ///   <item><description>Each data line must contain exactly one <c>=</c>; everything before it is the key (trimmed), everything after is the value.</description></item>
        ///   <item><description><c>\n</c> in a value is converted to a real newline; <c>\t</c> to a tab; <c>\\</c> to a single backslash.</description></item>
        /// </list>
        /// </para>
        /// </summary>
        private static IEnumerable<(string Key, string Value)> ParseIniFile(string filePath)
        {
            foreach (var line in File.ReadLines(filePath))
            {
                var trimmed = line.TrimStart();

                // Skip blank lines and comment lines (# or ;)
                if (trimmed.Length == 0 || trimmed[0] == '#' || trimmed[0] == ';')
                {
                    continue;
                }

                var equalsIndex = trimmed.IndexOf('=');

                // Skip lines without an '=' or with an empty key
                if (equalsIndex <= 0)
                {
                    continue;
                }

                var key = trimmed.Substring(0, equalsIndex).TrimEnd();
                var rawValue = trimmed.Substring(equalsIndex + 1);

                // Process escape sequences in the value
                var value = ProcessEscapes(rawValue);

                if (!string.IsNullOrEmpty(key))
                {
                    yield return (key, value);
                }
            }
        }

        /// <summary>
        /// Converts INI escape sequences in <paramref name="raw"/> to their
        /// corresponding characters.
        /// </summary>
        private static string ProcessEscapes(string raw)
        {
            // Use a simple state-machine approach to avoid multiple string allocations.
            if (raw.IndexOf('\\') < 0)
            {
                return raw;
            }

            var chars = new char[raw.Length];
            var writeIndex = 0;
            var i = 0;

            while (i < raw.Length)
            {
                if (raw[i] == '\\' && i + 1 < raw.Length)
                {
                    switch (raw[i + 1])
                    {
                        case 'n':
                            chars[writeIndex++] = '\n';
                            i += 2;
                            continue;
                        case 't':
                            chars[writeIndex++] = '\t';
                            i += 2;
                            continue;
                        case '\\':
                            chars[writeIndex++] = '\\';
                            i += 2;
                            continue;
                    }
                }

                chars[writeIndex++] = raw[i++];
            }

            return new string(chars, 0, writeIndex);
        }

        /// <summary>
        /// Extracts the IETF tag and optional prefix from an INI filename.
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
