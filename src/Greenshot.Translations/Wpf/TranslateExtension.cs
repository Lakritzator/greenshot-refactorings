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
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Greenshot.Translations.Core;

namespace Greenshot.Translations.Wpf
{
    /// <summary>
    /// WPF XAML markup extension that returns a translated string for the
    /// given <see cref="Key"/> from the application-level
    /// <see cref="TranslationManagerLocator.Current"/> provider.
    ///
    /// <para>
    /// When used on a <see cref="DependencyProperty"/>, the extension creates
    /// a <see cref="Binding"/> so that the target is automatically updated
    /// when <see cref="ITranslationProvider.LanguageChanged"/> fires.
    /// </para>
    /// </summary>
    ///
    /// <remarks>
    /// Register the namespace in XAML:
    /// <code>
    /// xmlns:t="clr-namespace:Greenshot.Translations.Wpf;assembly=Greenshot.Translations"
    /// </code>
    ///
    /// Then use the extension:
    /// <code>
    /// &lt;Button Content="{t:Translate cancel}" /&gt;
    /// &lt;Label Content="{t:Translate about_bugs}" /&gt;
    /// </code>
    /// </remarks>
    [MarkupExtensionReturnType(typeof(string))]
    public sealed class TranslateExtension : MarkupExtension
    {
        /// <summary>Initialises the extension with no key set.</summary>
        public TranslateExtension() { }

        /// <summary>
        /// Initialises the extension and sets <see cref="Key"/> inline.
        /// </summary>
        /// <param name="key">
        /// The translation key, e.g. <c>"cancel"</c> or <c>"box.upload_menu_item"</c>.
        /// </param>
        public TranslateExtension(string key)
        {
            Key = key;
        }

        /// <summary>
        /// The translation key used to retrieve the string from the active
        /// <see cref="ITranslationProvider"/>.
        /// </summary>
        [ConstructorArgument("key")]
        public string Key { get; set; }

        /// <inheritdoc/>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (serviceProvider.GetService(typeof(IProvideValueTarget))
                is IProvideValueTarget pvt
                && pvt.TargetObject is DependencyObject
                && pvt.TargetProperty is DependencyProperty
                && TranslationManagerLocator.Indexer is { } indexer)
            {
                // Return a binding so the property auto-refreshes on language change.
                // The Indexer is an INotifyPropertyChanged wrapper that raises
                // PropertyChanged(Binding.IndexerName) on every language switch.
                var binding = new Binding($"[{Key}]")
                {
                    Source = indexer,
                    Mode = BindingMode.OneWay,
                    FallbackValue = Key
                };

                return binding.ProvideValue(serviceProvider);
            }

            // Fallback for non-dependency-property targets (e.g. event handler args)
            return TranslationManagerLocator.Current?.GetString(Key) ?? Key;
        }
    }
}
