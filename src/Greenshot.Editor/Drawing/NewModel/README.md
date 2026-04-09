# New Drawing Surface Architecture

This directory contains a complete redesign of Greenshot's drawing surface system with clear separation of concerns.

## Directory Structure

```
NewModel/
├── Models/           - Data models (shapes, layers, styles, canvas)
├── Renderers/        - Rendering logic (shape renderers, adorner renderer)
├── Filters/          - Filter system for effects
├── Integration/      - Helpers for existing DrawableContainer system
├── Examples/         - Usage examples
└── *.md             - Documentation
```

## Architecture Overview

The new architecture separates into distinct namespaces:

### 1. **Models** - Pure Data Layer
- **Shapes**: `IShape`, `RectangleShape`, `EllipseShape`, `TextShape`, `ImageShape`, `BackgroundShape`, `CursorShape`
- **Styles**: `IShapeStyle`, `ShapeStyle`, `StyleManager`
- **Layers**: `Layer` with visibility and Z-index
- **Canvas**: `ShapeCanvas` - manages shapes, layers, and filters
- **Images**: `IImageData`, `BitmapImageData` - image abstraction
- **State**: `ShapeEditorState` - transient UI state
- **Adorners**: `IAdornerConfiguration` - custom adorner positioning

### 2. **Renderers** - Rendering Layer
- Shape renderers: `RectangleRenderer`, `EllipseRenderer`, `TextRenderer`, `ImageRenderer`, `CursorRenderer`
- `AdornerRenderer` - selection borders and resize handles
- `CanvasRenderer` - coordinates all rendering

### 3. **Filters** - Effects Layer
- `IFilter` interface, `BlurFilter`, `HighlightFilter`
- Support for inverted filters (apply everywhere except area)

### 4. **Integration** - Compatibility Layer
- `IntegrationHelpers` - convert between old and new systems

## Key Features

### Layer Support
- Background layer (implicit), default layer, custom layers
- Visibility and Z-index control

### Image Support  
- Abstract image handling (System.Drawing agnostic)
- Shared images for memory efficiency
- Background and cursor as special image shapes

### Custom Adorners
- Shapes can define their own adorner positions and colors

### Filter System
- Area-based filters with inverted mode support

See full documentation in README.md for examples and details.
