# Language Library Design: Greenshot.Translations

## Background & Current System Analysis

Greenshot currently uses a static, reflection-driven language system with the following characteristics:

### Current Architecture (to be replaced)

| Component | File | Issue |
|-----------|------|-------|
| `Language` static class | `Greenshot.Base/Core/Language.cs` | Global state, hard to test, loads everything at once |
| `LanguageFile` metadata | `Greenshot.Base/Core/LanguageFile.cs` | Fine, model is correct |
| `GreenshotForm` base | `Greenshot.Base/Controls/GreenshotForm.cs` | Reflection-based binding on every language change |
| `IGreenshotLanguageBindable` | Controls/IGreenshotLanguageBindable.cs | Control-level interface OK, but requires reflection scan |
| `LangKey` enums | `*/Configuration/LanguageKeys.cs` | Compile-time safety good, but enum-to-string costly |
| XML language files | `Languages/language-*.xml` | Verbose format, harder for LLMs to translate |

### XML File Format (current)
```xml
<?xml version="1.0" encoding="utf-8"?>
<language description="English" ietf="en-US" version="1.0.4" languagegroup="1">
    <resources>
        <resource name="about_bugs">Please report bugs to</resource>
        <resource name="about_license">Copyright © 2004-2026 Thomas Braun, Jens Klingen, Robin Krom
Greenshot comes with ABSOLUTELY NO WARRANTY. This is free software...</resource>
        <resource name="CANCEL">Cancel</resource>
    </resources>
</language>
```

### Plugin prefix pattern (current)
Plugin files are named `language_box-en-US.xml` and their keys are namespaced as `box.Configure`.

---

## New Library Specification

### Requirements

1. **Fast loading** — use `File.ReadLines` for line-by-line INI parsing; no DOM, no serialisation library; only load the selected language plus default fallback
2. **Low memory usage** — single `Dictionary<string, string>` backing store; no metadata stored in files; language display name from `CultureInfo` at runtime
3. **No reflection / Interface-based with source generation** — define translations as a C# interface; a Roslyn source generator emits the implementation
4. **Property changed support** — generated implementation raises `INotifyPropertyChanged` so WPF/Avalonia bindings update automatically
5. **Small files** — INI format (`key=value`); one key per line; no metadata in file body; easy for LLMs to translate
6. **Multi-line support** — use `\n` escape in the value for embedded newlines; `\t` for tab; `\\` for a literal backslash
7. **ISO codes, dialect support** — fallback chain: `zh-TW` → `zh` → `en-US`; IETF code and prefix derived entirely from filename without reading the file body; language display name from .NET `CultureInfo.NativeName`
8. **Available-language discovery** — filenames encode the IETF code; a single `Directory.GetFiles` gives all available languages without reading file contents
9. **WPF / Avalonia compatibility** — XAML markup extension (`{t:Translate cancel}`) and `INotifyPropertyChanged` binding via generated class
10. **Windows Forms** — helper that applies translations to all `ITranslatable` controls in a form at load time and on language change
11. **Plugin registration** — plugins call `AddPath(directoryPath)` from their initialise method; no other registration step is required; multiple callers share one `ITranslationManager` instance

---

## Library Name & Package Structure

| Project | NuGet Package | Target Framework |
|---------|--------------|------------------|
| `Greenshot.Translations` | `Greenshot.Translations` | `net481;net6.0;net8.0` |
| `Greenshot.Translations.Generator` | `Greenshot.Translations.Generator` | `netstandard2.0` (Roslyn source gen) |

No external NuGet dependencies — only BCL APIs are used.

---

## INI Translation File Format

**Filename convention:** `{ietf}.ini` (main app) or `{prefix}.{ietf}.ini` (plugin)

**Main application:** `en-US.ini`
```ini
# Greenshot translations — English (en-US)
# Lines starting with # or ; are comments and are ignored.
# Use \n for embedded newlines, \t for tabs, \\ for a literal backslash.

cancel=Cancel
ok=OK
about_bugs=Please report bugs to
settings_tooltip_language=Select your preferred language
about_license=Copyright © 2004-2026 Thomas Braun, Jens Klingen, Robin Krom\nGreenshot comes with ABSOLUTELY NO WARRANTY.
```

**Plugin file:** `box.en-US.ini`
```ini
# Box plugin translations — English (en-US)
# Keys are accessed as "box.configure", "box.upload_menu_item", etc.

configure=Configure Box
upload_menu_item=Upload to Box
upload_success=Successfully uploaded image to Box!
```

### Discovery rules
- Files matching `*.ini` in a registered path whose name matches the IETF pattern are valid translation files
- The IETF code is extracted from the filename without reading the file body
- Prefix is extracted from the stem segment before the first `.` when two dots exist (e.g. `box` from `box.en-US.ini`)
- The language display name is resolved from `CultureInfo.GetCultureInfo(ietf).NativeName` at runtime — never stored in the file

---

## Source Generator Workflow

### Step 1 — Define an interface (user writes this)

```csharp
using Greenshot.Translations;

namespace Greenshot.Configuration;

[TranslationGroup("greenshot")]          // maps to greenshot.en-US.json
public interface IGreenshotTranslations
{
    string AboutBugs { get; }            // maps to key "about_bugs"
    string AboutLicense { get; }         // maps to key "about_license"
    string Cancel { get; }
    string Ok { get; }
    string SettingsTooltipLanguage { get; }
}
```

Key naming: `PascalCase` interface property → `snake_case` JSON key
(e.g. `AboutBugs` → `about_bugs`, `SettingsTooltipLanguage` → `settings_tooltip_language`)

### Step 2 — Source generator emits (user never writes this)

```csharp
// <auto-generated />
using System.ComponentModel;
using Greenshot.Translations.Core;

namespace Greenshot.Configuration;

public partial class GreenshotTranslations : IGreenshotTranslations, INotifyPropertyChanged
{
    private readonly ITranslationProvider _provider;

    public event PropertyChangedEventHandler PropertyChanged;

    public GreenshotTranslations(ITranslationProvider provider)
    {
        _provider = provider;
        _provider.LanguageChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged(object sender, EventArgs e)
    {
        // Raise PropertyChanged for every property so bindings refresh
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AboutBugs)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AboutLicense)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Cancel)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Ok)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SettingsTooltipLanguage)));
    }

    public string AboutBugs => _provider.GetString("about_bugs");
    public string AboutLicense => _provider.GetString("about_license");
    public string Cancel => _provider.GetString("cancel");
    public string Ok => _provider.GetString("ok");
    public string SettingsTooltipLanguage => _provider.GetString("settings_tooltip_language");
}
```

---

## Core Interfaces

### `ITranslationProvider`
```csharp
public interface ITranslationProvider
{
    /// <summary>Raised when the active language changes.</summary>
    event EventHandler LanguageChanged;

    /// <summary>Returns the translated string for <paramref name="key"/>,
    /// or a sentinel like "###key###" when not found.</summary>
    string GetString(string key);

    /// <summary>Returns <c>true</c> and sets <paramref name="value"/> when the key exists.</summary>
    bool TryGetString(string key, out string value);

    /// <summary>Returns <c>true</c> when the key exists in the current language.</summary>
    bool HasKey(string key);
}
```

### `ITranslationDiscovery`
```csharp
public interface ITranslationDiscovery
{
    /// <summary>Returns all available IETF tags by scanning registered paths.</summary>
    IReadOnlyList<TranslationFile> GetAvailableLanguages();

    /// <summary>Registers an additional directory to scan for translation files.</summary>
    void AddPath(string directoryPath);
}
```

### `ITranslationManager`
```csharp
public interface ITranslationManager : ITranslationProvider, ITranslationDiscovery
{
    /// <summary>The currently active IETF language tag, e.g. "en-US".</summary>
    string CurrentLanguage { get; set; }

    /// <summary>The IETF tag used as ultimate fallback, default "en-US".</summary>
    string FallbackLanguage { get; set; }
}
```

### `TranslationFile` (metadata)
```csharp
public sealed class TranslationFile
{
    public string Ietf { get; init; }          // "en-US"  — from filename
    public string Prefix { get; init; }        // "box" or null for main app — from filename
    public string FilePath { get; init; }      // full path
    // Language display name from CultureInfo.NativeName — never stored in the file
    public string DisplayName => CultureInfo.GetCultureInfo(Ietf).NativeName;
}
```

---

## `IniTranslationProvider` Implementation Sketch

```csharp
public sealed class IniTranslationProvider : ITranslationManager
{
    private readonly List<string> _searchPaths = new();
    private Dictionary<string, string> _resources = new(StringComparer.OrdinalIgnoreCase);
    private string _currentLanguage = "en-US";

    public event EventHandler LanguageChanged;

    public string CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            if (_currentLanguage == value) return;
            _currentLanguage = value;
            Reload();
            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public string FallbackLanguage { get; set; } = "en-US";

    public void AddPath(string path)
    {
        if (!_searchPaths.Contains(path))
            _searchPaths.Add(path);
    }

    public IReadOnlyList<TranslationFile> GetAvailableLanguages()
    {
        // Scan filenames only — no file I/O on contents
        var result = new List<TranslationFile>();
        foreach (var dir in _searchPaths)
        {
            if (!Directory.Exists(dir)) continue;
            foreach (var file in Directory.GetFiles(dir, "*.ini"))
            {
                if (TryParseFileName(file, out var tf))
                    result.Add(tf);
            }
        }
        return result;
    }

    private void Reload()
    {
        _resources = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Load fallback first (en-US), then overlay the selected language chain
        if (_currentLanguage != FallbackLanguage)
            LoadAll(FallbackLanguage);

        // Dialect chain: zh → zh-TW (most specific last)
        LoadAll(_currentLanguage);
    }

    private void LoadAll(string ietf)
    {
        foreach (var dir in _searchPaths)
        {
            // Match both "en-US.ini" and "box.en-US.ini"
            foreach (var file in Directory.GetFiles(dir, $"*{ietf}.ini"))
            {
                LoadFile(file);
            }
        }
    }

    private void LoadFile(string filePath)
    {
        // Derive prefix from filename; no metadata is read from file body
        TryParseFileName(filePath, out var tf);
        var prefix = tf?.Prefix;

        foreach (var line in File.ReadLines(filePath))
        {
            var trimmed = line.TrimStart();
            if (trimmed.Length == 0 || trimmed[0] == '#' || trimmed[0] == ';')
                continue;
            var eq = trimmed.IndexOf('=');
            if (eq <= 0) continue;
            var key = trimmed.Substring(0, eq).TrimEnd();
            var rawValue = trimmed.Substring(eq + 1);
            var value = rawValue.Replace(@"\n", "\n").Replace(@"\t", "\t").Replace(@"\\", "\\");
            var fullKey = prefix is null ? key : $"{prefix}.{key}";
            _resources[fullKey] = value;
        }
    }

    public string GetString(string key)
        => _resources.TryGetValue(key, out var v) ? v : $"###{key}###";

    public bool TryGetString(string key, out string value)
        => _resources.TryGetValue(key, out value);

    public bool HasKey(string key)
        => _resources.ContainsKey(key);

    private static bool TryParseFileName(string filePath, out TranslationFile result)
    {
        // Expected names: "en-US.ini" or "box.en-US.ini"
        result = null;
        var name = Path.GetFileNameWithoutExtension(filePath);
        // … regex extraction …
        return false; // placeholder
    }
}
```

---

## WPF Integration

### XAML Markup Extension
```csharp
[MarkupExtensionReturnType(typeof(string))]
public class TranslateExtension : MarkupExtension
{
    [ConstructorArgument("key")]
    public string Key { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var target = (IProvideValueTarget)serviceProvider
            .GetService(typeof(IProvideValueTarget));

        if (target?.TargetObject is DependencyObject depObj
            && target.TargetProperty is DependencyProperty depProp)
        {
            // Set binding so property updates on language change
            var binding = new Binding($"[{Key}]")
            {
                Source = TranslationManagerLocator.Indexer,
                Mode = BindingMode.OneWay
            };
            return binding.ProvideValue(serviceProvider);
        }

        return TranslationManagerLocator.Current?.GetString(Key) ?? Key;
    }
}
```

### Usage in XAML
```xml
<Window xmlns:t="clr-namespace:Greenshot.Translations.Wpf;assembly=Greenshot.Translations">
    <Button Content="{t:Translate cancel}" />
    <Label Content="{t:Translate about_bugs}" />
</Window>
```

### `INotifyPropertyChanged` approach (alternative XAML binding)
```xml
<!-- Bind to the generated GreenshotTranslations instance in DataContext -->
<Button Content="{Binding Translations.Cancel}" />
```

---

## Avalonia Integration

```csharp
// Avalonia uses a very similar approach
public class TranslateExtension : IMarkupExtension<object>
{
    public string Key { get; set; }

    public object ProvideValue(IServiceProvider serviceProvider)
        => TranslationManagerLocator.Current?.GetString(Key) ?? Key;
}
```

Usage:
```xml
<Button Content="{t:Translate cancel}" />
```

For reactive updates in Avalonia, bind to a `ReactiveObject`-derived translations class
(generated by the source generator with Avalonia-specific `RaisePropertyChanged`).

---

## Windows Forms Integration

```csharp
/// <summary>
/// Marks a WinForms control as having a translation key.
/// Used by <see cref="FormsTranslationHelper"/> to apply translations.
/// </summary>
public interface ITranslatable
{
    string TranslationKey { get; }
}

/// <summary>
/// Scans a form and applies translations to all <see cref="ITranslatable"/> controls.
/// Subscribes to <see cref="ITranslationProvider.LanguageChanged"/> so the form
/// stays up to date when the user switches language.
/// </summary>
public static class FormsTranslationHelper
{
    public static void ApplyTranslations(Form form, ITranslationProvider provider)
    {
        ApplyToControl(form, provider);
        provider.LanguageChanged += (_, _) => ApplyToControl(form, provider);
    }

    private static void ApplyToControl(Control control, ITranslationProvider provider)
    {
        if (control is ITranslatable translatable
            && provider.TryGetString(translatable.TranslationKey, out var text))
        {
            control.Text = text;
        }

        foreach (Control child in control.Controls)
            ApplyToControl(child, provider);
    }
}
```

Concrete control example:
```csharp
public class TranslatableButton : Button, ITranslatable
{
    public string TranslationKey { get; set; }
}
```

---

## Fallback / Dialect Chain

```
zh-TW  →  zh  →  en-US  (ultimate fallback)
de-x-franconia  →  de  →  en-US
nn-NO  →  no  →  en-US
```

Implementation note: when looking for `zh-TW.ini` and it doesn't exist,
strip the region to get `zh.ini`; if that doesn't exist, use `en-US.ini`.

---

## File Discovery Example

```
Languages/
├── en-US.ini          ← main app English
├── de-DE.ini          ← main app German
├── zh-TW.ini
├── box.en-US.ini      ← Box plugin English
├── box.de-DE.ini
├── imgur.en-US.ini
└── confluence.en-US.ini
```

`GetAvailableLanguages()` returns distinct IETF codes from all filenames — no file
content is read until a language is actually selected.

---

## Migration Path from Current System

| Old | New |
|-----|-----|
| `Language.GetString("cancel")` | `provider.GetString("cancel")` or `translations.Cancel` |
| `Language.GetString(LangKey.Cancel)` | `translations.Cancel` (property, no reflection) |
| `Language.GetString("box", LangKey.Configure)` | `boxTranslations.Configure` |
| `Language.LanguageChanged += handler` | `provider.LanguageChanged += handler` |
| `GreenshotForm.ApplyLanguage()` | `FormsTranslationHelper.ApplyTranslations(this, provider)` |
| `language-en-US.xml` | `en-US.ini` |
| `language_box-en-US.xml` | `box.en-US.ini` |

---

## LLM Prompt for Implementing This Library

Use the following prompt to ask an LLM to implement `Greenshot.Translations`:

---

```
You are implementing a C# translation/localization library called Greenshot.Translations.

## Requirements

1. Fast loading — use File.ReadLines for line-by-line INI parsing; no DOM, no serialisation
   library; only load the selected language + default fallback (en-US) at any given time.

2. Low memory usage — single Dictionary<string,string> as the in-memory store.
   No metadata is stored inside translation files.

3. No reflection — use a Roslyn IIncrementalGenerator source generator instead.
   Users declare a C# interface annotated with [TranslationGroup("prefix")].
   The generator emits a partial class implementing that interface plus INotifyPropertyChanged.

4. Property changed support — the generated class raises PropertyChanged for every
   property when the active language changes, so WPF/Avalonia bindings refresh.

5. Small INI files — key=value format, one per line, no metadata.
   File format:
     # comment lines start with # or ;
     cancel=Cancel
     about_license=Line 1\nLine 2
   The language display name comes from CultureInfo.NativeName — not stored in the file.
   File versioning is managed with the source code — not stored in the file.

6. Multi-line support — \n in a value is converted to a real newline on load;
   \t to tab; \\ to a literal backslash.

7. ISO codes with dialect fallback — filename encodes IETF tag:
   "en-US.ini", "box.en-US.ini".
   Fallback chain: zh-TW → zh → en-US.

8. Language discovery from filenames — no file content needed to enumerate languages;
   a single Directory.GetFiles("*.ini") call gives all available IETF codes.
   DisplayName resolved from CultureInfo at runtime.

9. WPF compatibility — provide a MarkupExtension {t:Translate key} that creates
   a Binding to a TranslationIndexer (INotifyPropertyChanged indexer wrapper).
   Also document how to use the generated class as a DataContext.

10. Avalonia compatibility — same MarkupExtension pattern, using Avalonia APIs.

11. Windows Forms — provide a static FormsTranslationHelper.ApplyTranslations(Form, ITranslationProvider)
    that recursively visits all controls implementing ITranslatable, sets their Text,
    and re-applies on LanguageChanged (using BeginInvoke for thread safety).

12. Plugin registration — plugins call AddPath(directoryPath) from their initialise method.
    The host application creates the IniTranslationProvider, passes it to each plugin,
    and each plugin registers its own Languages directory. No other registration needed.

## Project Structure

Create two projects:

### Greenshot.Translations (target: net481;net6.0;net8.0, no external NuGet dependencies)
- Core/ITranslationProvider.cs        — interface with GetString, TryGetString, HasKey, LanguageChanged
- Core/ITranslationDiscovery.cs       — interface with GetAvailableLanguages, AddPath (plugin registration)
- Core/ITranslationManager.cs         — extends both, adds CurrentLanguage, FallbackLanguage
- Core/TranslationFile.cs             — record: Ietf, Prefix, FilePath, DisplayName (from CultureInfo)
- Core/IniTranslationProvider.cs      — INI implementation; uses only BCL APIs
- Attributes/TranslationGroupAttribute.cs — [TranslationGroup("prefix")] for source generator
- Wpf/TranslateExtension.cs           — MarkupExtension for WPF
- Wpf/TranslationIndexer.cs           — INotifyPropertyChanged indexer (raises Indexer on language change)
- Wpf/TranslationManagerLocator.cs    — app-level singleton; also exposes Indexer for bindings
- Avalonia/TranslateExtension.cs      — MarkupExtension for Avalonia (separate file)
- Forms/ITranslatable.cs              — interface: string TranslationKey { get; }
- Forms/FormsTranslationHelper.cs     — static helper applying translations to a form
- Forms/TranslatableButton.cs         — example Button : ITranslatable

### Greenshot.Translations.Generator (target: netstandard2.0)
- TranslationGroupAttribute.cs       — duplicate attribute for analyzer
- TranslationSourceGenerator.cs      — IIncrementalGenerator implementation

## Source Generator Rules

Input: any interface decorated with [TranslationGroup]
Output: partial class named {InterfaceName without 'I'} (e.g. IGreenshotTranslations → GreenshotTranslations)

Key mapping: PascalCase property name → snake_case INI key
  AboutBugs → about_bugs
  SettingsTooltipLanguage → settings_tooltip_language

The generated class:
- Has a constructor accepting ITranslationProvider
- Subscribes to ITranslationProvider.LanguageChanged
- Raises PropertyChanged for all properties on language change
- Each property returns _provider.GetString("key") (or "prefix.key" for plugins)

## Coding conventions (match Greenshot style)
- Allman braces
- 4 spaces, no tabs
- _camelCase private fields
- Explicit visibility modifiers
- using directives at top of file, sorted alphabetically

## Deliverables
- Full compilable source for both projects
- XML doc comments on all public members
- README.md with:
  - Quick-start: create IniTranslationProvider, register paths, define interface, use in XAML and WinForms
  - Plugin registration pattern
  - Language file authoring guide
  - LLM translation prompt template
  - Migration guide from Greenshot's old Language.cs system
```

---

## Acceptance Criteria

A complete implementation of this library should satisfy the following checks:

- [ ] `GetAvailableLanguages()` returns correct IETF codes by reading only filenames
- [ ] `TranslationFile.DisplayName` returns the native language name from `CultureInfo`
- [ ] Switching `CurrentLanguage` fires `LanguageChanged` and clears/reloads the cache
- [ ] Missing key returns `"###key###"` sentinel (not an exception)
- [ ] Dialect fallback: requesting `zh-TW` falls back through `zh` to `en-US`
- [ ] Multi-line values (with `\n`) are returned correctly
- [ ] Prefix keys work: `box.configure` finds the value from `box.en-US.ini`
- [ ] Plugin calls `AddPath` from its initialise method; no other registration needed
- [ ] Source generator produces valid C# for any `[TranslationGroup]` interface
- [ ] Generated class raises `PropertyChanged` when language changes
- [ ] WPF binding updates label text when `CurrentLanguage` changes
- [ ] `FormsTranslationHelper.ApplyTranslations` sets `Text` on all `ITranslatable` controls
- [ ] Build succeeds on `net481`, `net6.0`, and `net8.0` with no external NuGet dependencies
