# Greenshot.Translations

A fast, low-memory, reflection-free translation library for Greenshot.

## Features

| Feature | Detail |
|---------|--------|
| **Fast loading** | `System.Text.Json` with streaming — only the active language + fallback loaded |
| **Low memory** | Single `Dictionary<string, string>` per language switch |
| **No reflection** | Source generator emits typed implementations from interfaces |
| **Property changed** | Generated class raises `INotifyPropertyChanged` on language switch |
| **Small files** | Compact JSON — one key per line; easy for LLMs to translate |
| **Multi-line** | JSON native `\n` in string values |
| **ISO / dialect** | Full fallback chain: `zh-TW` → `zh` → `en-US` |
| **Discovery** | `GetAvailableLanguages()` reads filenames only — no file-body I/O |
| **WPF** | `{t:Translate key}` markup extension + `INotifyPropertyChanged` binding |
| **Avalonia** | Same extension pattern using Avalonia APIs |
| **Windows Forms** | `FormsTranslationHelper.ApplyTranslations(form, provider)` |

---

## Project structure

```
Greenshot.Translations/
├── Core/
│   ├── ITranslationProvider.cs       — lookup interface (GetString, TryGetString, LanguageChanged)
│   ├── ITranslationDiscovery.cs      — file discovery interface
│   ├── ITranslationManager.cs        — combines both + CurrentLanguage / FallbackLanguage
│   ├── TranslationFile.cs            — immutable metadata record
│   └── JsonTranslationProvider.cs    — JSON-backed implementation
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

## Quick-start

### 1 — Translation JSON file

Place language files in a `Languages/` folder next to your application:

```
Languages/
├── en-US.json
├── de-DE.json
└── box.en-US.json   ← plugin, keyed as "box.upload_menu_item"
```

`en-US.json`:
```json
{
  "ietf": "en-US",
  "description": "English",
  "version": "1.0",
  "resources": {
    "cancel": "Cancel",
    "about_bugs": "Please report bugs to",
    "about_license": "Line 1\nLine 2"
  }
}
```

### 2 — Define a translation interface

```csharp
using Greenshot.Translations.Attributes;

namespace MyApp;

[TranslationGroup]                // no prefix = main application
public interface IMyTranslations
{
    string Cancel { get; }        // → key "cancel"
    string AboutBugs { get; }     // → key "about_bugs"
    string AboutLicense { get; }  // → key "about_license"
}
```

The source generator emits `MyTranslations` automatically — you never write the
implementation class.

### 3 — Bootstrap the provider

```csharp
using Greenshot.Translations.Core;
using Greenshot.Translations.Wpf;

var manager = new JsonTranslationProvider();
manager.AddPath(Path.Combine(AppContext.BaseDirectory, "Languages"));
manager.FallbackLanguage = "en-US";
manager.CurrentLanguage = "de-DE";          // fires LanguageChanged after load

// For WPF
TranslationManagerLocator.Current = manager;

// Obtain the typed instance (constructed by DI or manually)
var translations = new MyTranslations(manager);
```

### 4 — Use in WPF

```xml
<Window xmlns:t="clr-namespace:Greenshot.Translations.Wpf;assembly=Greenshot.Translations">
    <StackPanel>
        <Button Content="{t:Translate cancel}" />
        <Label Content="{t:Translate about_bugs}" />
    </StackPanel>
</Window>
```

Or with the generated class as `DataContext`:

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

## Language file authoring guide

| Field | Required | Notes |
|-------|----------|-------|
| `ietf` | yes | Language tag, e.g. `"en-US"`, `"zh-TW"` |
| `description` | recommended | Human-readable name displayed in language picker |
| `version` | recommended | Semantic version string |
| `prefix` | only for plugins | Namespace prefix, e.g. `"box"` |
| `resources` | yes | Object mapping `snake_case` key → translated string |

Multi-line values use JSON `\n`:
```json
"long_text": "First paragraph.\nSecond paragraph."
```

### LLM translation prompt template

```
You are a professional software translator.

Translate the following JSON translation file from English to {TARGET_LANGUAGE}.
Keep all JSON keys (the left-hand side) unchanged.
Translate only the values (right-hand side strings).
Preserve \n newlines exactly as they appear.
Return valid JSON only.

--- SOURCE (en-US.json) ---
{PASTE FILE CONTENT}
--- END SOURCE ---

Output the complete translated JSON for {TARGET_LANGUAGE} ({IETF_CODE}).
```

---

## Migration guide from the old `Language.cs` system

| Old API | New API |
|---------|---------|
| `Language.GetString("cancel")` | `provider.GetString("cancel")` or `translations.Cancel` |
| `Language.GetString(LangKey.Cancel)` | `translations.Cancel` (no reflection, compile-time safe) |
| `Language.GetString("box", LangKey.Configure)` | `boxTranslations.Configure` |
| `Language.TryGetString(key, out var s)` | `provider.TryGetString(key, out var s)` |
| `Language.HasKey(key)` | `provider.HasKey(key)` |
| `Language.LanguageChanged += handler` | `provider.LanguageChanged += handler` |
| `Language.CurrentLanguage = "de-DE"` | `manager.CurrentLanguage = "de-DE"` |
| `GreenshotForm.ApplyLanguage()` | `FormsTranslationHelper.ApplyTranslations(this, provider)` |
| `language-en-US.xml` | `en-US.json` |
| `language_box-en-US.xml` | `box.en-US.json` |
| `language_box-*.xml` prefix keys | `box.configure`, `box.upload_menu_item` |

---

## Acceptance criteria

- [ ] `GetAvailableLanguages()` returns correct IETF codes by reading only filenames
- [ ] Switching `CurrentLanguage` fires `LanguageChanged` and reloads translations
- [ ] Missing key returns `"###key###"` sentinel, not an exception
- [ ] Dialect fallback: `zh-TW` falls back through `zh` to `en-US`
- [ ] Multi-line values (with `\n`) are returned correctly
- [ ] Prefix keys (`box.configure`) resolve from `box.en-US.json`
- [ ] Source generator emits valid C# for any `[TranslationGroup]` interface
- [ ] Generated class raises `PropertyChanged` on language change
- [ ] WPF `{t:Translate key}` updates label text when `CurrentLanguage` changes
- [ ] `FormsTranslationHelper.ApplyTranslations` sets `Text` on all `ITranslatable` controls
- [ ] Build succeeds on `net481`, `net6.0`, and `net8.0`
