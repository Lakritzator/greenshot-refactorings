# Greenshot.Translations

A fast, low-memory, reflection-free translation library for Greenshot.

## Features

| Feature | Detail |
|---------|--------|
| **Fast loading** | Pure `File.ReadLines` line-by-line parsing — no DOM, no serialisation library |
| **Low memory** | Single `Dictionary<string, string>` per language switch; no metadata stored in files |
| **No reflection** | Source generator emits typed implementations from interfaces at compile time |
| **Property changed** | Generated class raises `INotifyPropertyChanged` on language switch |
| **Small files** | Minimal INI format — `key=value`, one per line |
| **Multi-line** | Use `\n` in a value for embedded newlines; `\t` for tab; `\\` for backslash |
| **ISO / dialect** | Full fallback chain: `zh-TW` → `zh` → `en-US` |
| **Discovery** | `GetAvailableLanguages()` reads filenames only — zero file-body I/O |
| **Language name** | Resolved from `CultureInfo.NativeName` at runtime — never stored in the file |
| **WPF** | `{t:Translate key}` markup extension + `INotifyPropertyChanged` binding |
| **Windows Forms** | `FormsTranslationHelper.ApplyTranslations(form, provider)` |
| **Plugin support** | Plugins call `AddPath` from their initialise method — no other registration needed |

---

## Project structure

```
Greenshot.Translations/
├── Core/
│   ├── ITranslationProvider.cs       — lookup interface (GetString, TryGetString, LanguageChanged)
│   ├── ITranslationDiscovery.cs      — file discovery + plugin registration (AddPath)
│   ├── ITranslationManager.cs        — combines both + CurrentLanguage / FallbackLanguage
│   ├── TranslationFile.cs            — metadata record (Ietf, Prefix, FilePath, DisplayName)
│   └── IniTranslationProvider.cs     — INI-backed implementation; no external dependencies
├── Attributes/
│   └── TranslationGroupAttribute.cs  — [TranslationGroup("prefix")] for source generator
├── Wpf/
│   ├── TranslateExtension.cs         — {t:Translate key} XAML markup extension
│   ├── TranslationIndexer.cs         — INotifyPropertyChanged indexer used by the extension
│   └── TranslationManagerLocator.cs  — app-level singleton accessor
└── Forms/
    ├── ITranslatable.cs              — interface: string TranslationKey { get; }
    ├── FormsTranslationHelper.cs     — recursive translation applier
    └── TranslatableButton.cs         — example ITranslatable Button

Greenshot.Translations.Generator/
└── TranslationSourceGenerator.cs    — Roslyn IIncrementalGenerator
```

---

## Translation file format

Files are simple INI-style text files encoded in UTF-8.

### Naming convention

| Scope | Filename | Example |
|-------|----------|---------|
| Main application | `{ietf}.ini` | `en-US.ini` |
| Plugin | `{prefix}.{ietf}.ini` | `box.en-US.ini` |

The IETF language tag and plugin prefix are read from the filename.
Nothing else needs to be stored in the file itself.

### File content

```ini
# Lines starting with # or ; are comments and are ignored.
# Blank lines are also ignored.

cancel=Cancel
ok=OK
about_bugs=Please report bugs to
settings_tooltip_language=Select your preferred language

# Use \n for embedded newlines, \t for tabs, \\ for a literal backslash.
about_license=Copyright © 2004-2026 Thomas Braun, Jens Klingen, Robin Krom\nGreenshot comes with ABSOLUTELY NO WARRANTY.
```

### Plugin file example (`box.en-US.ini`)

```ini
# Box plugin translations — English (en-US)
# Keys are accessed as "box.configure", "box.upload_menu_item", etc.

configure=Configure Box
upload_menu_item=Upload to Box
upload_success=Successfully uploaded image to Box!
upload_failure=Failed to upload image to Box.
```

---

## Directory layout

```
Languages/
├── en-US.ini          ← main app English
├── de-DE.ini          ← main app German
├── zh-TW.ini
├── box.en-US.ini      ← Box plugin English
├── box.de-DE.ini
└── imgur.en-US.ini
```

---

## Quick-start

### 1 — Create the provider (host application)

```csharp
using Greenshot.Translations.Core;

// Create once, share with all plugins
var manager = new IniTranslationProvider();

// Host app registers its own language directory
manager.AddPath(Path.Combine(AppContext.BaseDirectory, "Languages"));

// Set the desired language; "en-US" is the built-in fallback
manager.CurrentLanguage = "de-DE";
```

### 2 — Plugin registration (called from plugin's initialise method)

Each plugin receives the shared `ITranslationManager` and registers its directory:

```csharp
// In BoxPlugin.Initialize(ITranslationManager translationManager)
public void Initialize(ITranslationManager translationManager)
{
    // Register this plugin's language files — that's all that's needed.
    translationManager.AddPath(Path.Combine(_pluginFolder, "Languages"));
}
```

Plugin INI files follow the `{prefix}.{ietf}.ini` naming convention.
The prefix is derived automatically from the filename (e.g. `box` from `box.en-US.ini`).

### 3 — Define a translation interface

```csharp
using Greenshot.Translations.Attributes;

namespace MyApp;

[TranslationGroup]                    // no prefix = main application
public interface IMyTranslations
{
    string Cancel { get; }            // → key "cancel"
    string AboutBugs { get; }         // → key "about_bugs"
    string AboutLicense { get; }      // → key "about_license"
}
```

For a plugin:

```csharp
[TranslationGroup("box")]             // prefix = "box"
public interface IBoxTranslations
{
    string Configure { get; }         // → key "box.configure"
    string UploadMenuItem { get; }    // → key "box.upload_menu_item"
}
```

The source generator emits `MyTranslations` / `BoxTranslations` automatically.

### 4 — Use in WPF

```xml
<Window xmlns:t="clr-namespace:Greenshot.Translations.Wpf;assembly=Greenshot.Translations">
    <StackPanel>
        <Button Content="{t:Translate cancel}" />
        <Label Content="{t:Translate about_bugs}" />
    </StackPanel>
</Window>
```

Startup:
```csharp
TranslationManagerLocator.Current = manager;
```

Or bind directly to the generated class (set as DataContext):
```xml
<Button Content="{Binding Cancel}" />
```

### 5 — Use in Windows Forms

```csharp
public class MyForm : Form
{
    private readonly TranslatableButton _btnCancel = new() { TranslationKey = "cancel" };

    public MyForm(ITranslationProvider provider)
    {
        InitializeComponent();
        Controls.Add(_btnCancel);
        FormsTranslationHelper.ApplyTranslations(this, provider);
    }
}
```

---

## Language discovery

```csharp
// Enumerate available languages without reading any file content
foreach (var tf in manager.GetAvailableLanguages())
{
    Console.WriteLine($"{tf.DisplayName} ({tf.Ietf}) — {tf.FilePath}");
    // e.g. "English (United States) (en-US) — /app/Languages/en-US.ini"
    //      "Deutsch (Deutschland) (de-DE) — /app/Languages/de-DE.ini"
}
```

`TranslationFile.DisplayName` is resolved from `CultureInfo.NativeName` at runtime.

---

## Dialect fallback chain

```
zh-TW  →  zh  →  en-US  (FallbackLanguage)
de-x-franconia  →  de  →  en-US
nn-NO  →  nn  →  en-US
```

The most specific variant is loaded last and overrides broader matches.

---

## LLM translation prompt template

```
You are a professional software translator.

Translate the following INI translation file from English to {TARGET_LANGUAGE}.
Keep all keys (the left-hand side of each = sign) unchanged.
Translate only the values (the right-hand side).
Preserve \n newlines exactly as they appear.
Return the complete translated file, keeping all comments.

--- SOURCE (en-US.ini) ---
{PASTE FILE CONTENT}
--- END SOURCE ---

Output the complete translated INI for {TARGET_LANGUAGE} ({IETF_CODE}).
```

---

## Migration guide from the old `Language.cs` system

| Old API | New API |
|---------|---------|
| `Language.GetString("cancel")` | `provider.GetString("cancel")` or `translations.Cancel` |
| `Language.GetString(LangKey.Cancel)` | `translations.Cancel` (compile-time safe, no reflection) |
| `Language.GetString("box", LangKey.Configure)` | `boxTranslations.Configure` |
| `Language.TryGetString(key, out var s)` | `provider.TryGetString(key, out var s)` |
| `Language.HasKey(key)` | `provider.HasKey(key)` |
| `Language.LanguageChanged += handler` | `provider.LanguageChanged += handler` |
| `Language.CurrentLanguage = "de-DE"` | `manager.CurrentLanguage = "de-DE"` |
| `GreenshotForm.ApplyLanguage()` | `FormsTranslationHelper.ApplyTranslations(this, provider)` |
| `language-en-US.xml` | `en-US.ini` |
| `language_box-en-US.xml` | `box.en-US.ini` |

---

## Acceptance criteria

- [ ] `GetAvailableLanguages()` returns correct IETF codes by reading only filenames
- [ ] `TranslationFile.DisplayName` returns the native language name from CultureInfo
- [ ] Switching `CurrentLanguage` fires `LanguageChanged` and reloads translations
- [ ] Missing key returns `"###key###"` sentinel, not an exception
- [ ] Dialect fallback: `zh-TW` falls back through `zh` to `en-US`
- [ ] Multi-line values (with `\n`) are returned correctly
- [ ] Prefix keys (`box.configure`) resolve from `box.en-US.ini`
- [ ] Source generator emits valid C# for any `[TranslationGroup]` interface
- [ ] Generated class raises `PropertyChanged` on language change
- [ ] WPF `{t:Translate key}` updates label text when `CurrentLanguage` changes
- [ ] `FormsTranslationHelper.ApplyTranslations` sets `Text` on all `ITranslatable` controls
- [ ] Plugin calls `AddPath` from its initialise method; no other registration needed
- [ ] Build succeeds on `net481`, `net6.0`, and `net8.0` with no external dependencies

