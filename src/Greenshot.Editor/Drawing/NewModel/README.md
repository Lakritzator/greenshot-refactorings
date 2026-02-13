# New Drawing Surface Architecture

This directory contains a complete redesign of Greenshot's drawing surface system with clear separation of concerns.

## Architecture Overview

The new architecture separates:
1. **Data Model** - Pure shape data (geometry, properties)
2. **Rendering Logic** - How shapes are drawn
3. **Editor State** - UI state (selection, adorners/grippers)
4. **Styles** - Reusable styling configurations

### Key Principles

- **Separation of Concerns**: Each component has a single, well-defined responsibility
- **Immutability**: Styles are immutable, allowing safe sharing across shapes
- **Extensibility**: Easy to add new shape types and renderers
- **Testability**: Pure data models are easy to test without UI dependencies
- **Serialization**: Clean data models are straightforward to serialize/deserialize

## Components

### 1. Data Model (`IShape`, Shape Classes)

**Purpose**: Represent the essential data of a drawable shape

**Key Classes**:
- `IShape` - Interface for all shapes
- `RectangleShape` - Rectangle data
- `EllipseShape` - Ellipse data  
- `TextShape` - Text with font data

**Characteristics**:
- Contains only data (bounds, style reference)
- No parent references, no UI state
- Each shape has a unique GUID
- Cloneable for copy/paste operations

**Example**:
```csharp
var style = new ShapeStyle(Color.Red, 2, Color.Yellow, false);
var rect = new RectangleShape(new NativeRect(10, 10, 100, 50), style);
```

### 2. Styles (`IShapeStyle`, `ShapeStyle`, `StyleManager`)

**Purpose**: Manage reusable styling properties

**Key Classes**:
- `IShapeStyle` - Style interface (immutable)
- `ShapeStyle` - Concrete immutable style implementation
- `StyleManager` - Manages named, reusable styles

**Features**:
- Immutable design (use `WithX()` methods to create modified copies)
- Shareable across multiple shapes
- Named styles via StyleManager
- Apply styles to single or multiple shapes

**Example**:
```csharp
var styleManager = new StyleManager();
styleManager.RegisterStyle("MyStyle", new ShapeStyle(Color.Blue, 2, Color.LightBlue, true));

// Apply to shape
styleManager.ApplyStyle(shape, "MyStyle");

// Apply to multiple shapes
styleManager.ApplyStyleToShapes(new[] { shape1, shape2, shape3 }, "MyStyle");
```

### 3. Rendering (`IShapeRenderer`, Renderer Classes, `CanvasRenderer`)

**Purpose**: Draw shapes to a Graphics context

**Key Classes**:
- `IShapeRenderer` - Interface for shape-specific renderers
- `RectangleRenderer`, `EllipseRenderer`, `TextRenderer` - Concrete renderers
- `CanvasRenderer` - Coordinates rendering of entire canvas

**Characteristics**:
- Stateless rendering
- One renderer per shape type
- Extensible via registration pattern
- Handles style application (colors, shadows, etc.)

**Example**:
```csharp
var canvasRenderer = new CanvasRenderer();
canvasRenderer.RegisterRenderer(new CustomShapeRenderer());

// Render entire canvas
canvasRenderer.RenderCanvas(graphics, canvas);
```

### 4. Editor State (`ShapeEditorState`, `AdornerRenderer`)

**Purpose**: Manage UI state separate from data

**Key Classes**:
- `ShapeEditorState` - Tracks selection, editing mode, resize state
- `AdornerRenderer` - Renders selection borders and resize handles (adorners/grippers)
- `AdornerPosition` - Enum for 8 resize handle positions

**Characteristics**:
- Completely separate from shape data
- Not serialized
- Can be created/discarded as needed for UI

**Example**:
```csharp
var state = new ShapeEditorState(shape);
state.IsSelected = true;
state.ShowAdorners = true;

var adornerRenderer = new AdornerRenderer();
adornerRenderer.RenderAdorners(graphics, state);
```

### 5. Canvas (`ShapeCanvas`)

**Purpose**: Manage collection of shapes

**Key Features**:
- Add/remove shapes
- Z-order management (bring to front, send to back)
- Shape lookup by ID
- Pure data container (no rendering logic)

**Example**:
```csharp
var canvas = new ShapeCanvas();
canvas.AddShape(rectangle);
canvas.AddShape(ellipse);
canvas.BringToFront(rectangle);
```

## Usage Examples

### Creating and Rendering Shapes

```csharp
// Create a canvas
var canvas = new ShapeCanvas();

// Create shapes with styles
var redStyle = new ShapeStyle(Color.Red, 2, Color.Empty, false);
var rect = new RectangleShape(new NativeRect(10, 10, 100, 50), redStyle);
canvas.AddShape(rect);

var blueStyle = new ShapeStyle(Color.Blue, 1, Color.LightBlue, false);
var ellipse = new EllipseShape(new NativeRect(50, 50, 80, 60), blueStyle);
canvas.AddShape(ellipse);

// Render the canvas
var renderer = new CanvasRenderer();
renderer.RenderCanvas(graphics, canvas);
```

### Working with Editor State

```csharp
// Create editor states for shapes
var states = new List<ShapeEditorState>();
var state1 = new ShapeEditorState(rect);
state1.IsSelected = true;
state1.ShowAdorners = true;
states.Add(state1);

// Render with editor state (selection borders, adorners)
renderer.RenderCanvas(graphics, canvas, states);
```

### Using Style Manager

```csharp
var styleManager = new StyleManager();

// Use predefined styles
styleManager.ApplyStyle(shape, "RedBorder");
styleManager.ApplyStyle(shape, "BlueFilled");

// Create custom style
var myStyle = new ShapeStyle(Color.Green, 3, Color.LightGreen, true);
styleManager.RegisterStyle("MyCustomStyle", myStyle);

// Apply to multiple shapes
var selectedShapes = canvas.Shapes.Where(s => /* selection criteria */);
styleManager.ApplyStyleToShapes(selectedShapes, "MyCustomStyle");
```

### Shape Serialization

```csharp
// Shapes are pure data - easy to serialize
var shapes = canvas.Shapes;
// Serialize shapes (JSON, XML, binary, etc.)

// Editor state is NOT serialized - it's transient UI state
```

## Comparison with Old Architecture

### Old Architecture (DrawableContainer)
- ❌ Mixed data, rendering, and state in one class
- ❌ Parent references make serialization complex
- ❌ Styles embedded in fields, not reusable
- ❌ Adorners tightly coupled to containers
- ❌ Hard to use different rendering systems

### New Architecture
- ✅ Clean separation: Data, Rendering, State, Styles
- ✅ Pure data models, easy to serialize
- ✅ Immutable, reusable styles
- ✅ Independent adorner rendering
- ✅ Easy to swap rendering systems (e.g., WPF, SkiaSharp)
- ✅ Better testability
- ✅ Simpler keyboard/mouse handling (state separate from data)

## Extending the System

### Adding a New Shape Type

1. Create data class implementing `IShape`:
```csharp
public class TriangleShape : IShape
{
    public Guid Id { get; }
    public NativeRect Bounds { get; set; }
    public IShapeStyle Style { get; set; }
    
    // Add triangle-specific data
    public Point[] Points { get; set; }
    
    // Implement Clone()
}
```

2. Create renderer implementing `IShapeRenderer`:
```csharp
public class TriangleRenderer : IShapeRenderer
{
    public bool CanRender(IShape shape) => shape is TriangleShape;
    
    public void Render(Graphics graphics, IShape shape)
    {
        var triangle = (TriangleShape)shape;
        // Rendering logic using shape.Style
    }
}
```

3. Register renderer:
```csharp
canvasRenderer.RegisterRenderer(new TriangleRenderer());
```

### Adding Shape-Specific Properties

For shapes with unique properties beyond common styling:

```csharp
public class ArrowShape : IShape
{
    // Standard IShape properties
    public Guid Id { get; }
    public NativeRect Bounds { get; set; }
    public IShapeStyle Style { get; set; }
    
    // Arrow-specific properties
    public ArrowHeadStyle HeadStyle { get; set; }
    public int ArrowHeadLength { get; set; }
    
    // ...
}
```

## Integration with Existing Greenshot

The new architecture is built **side-by-side** with the existing `DrawableContainer` system:

1. Both systems can coexist in the codebase
2. Gradual migration path: new features use new architecture
3. Can interoperate: convert between old and new representations
4. No breaking changes to existing functionality

## Future Enhancements

- **Undo/Redo**: Command pattern with shape snapshots
- **Serialization**: JSON/XML serializers for ShapeCanvas
- **Filters/Effects**: Composable filter system separate from shapes
- **Different Renderers**: WPF, SkiaSharp, Direct2D renderers
- **Advanced Styling**: Gradients, textures, opacity
- **Shape Grouping**: Composite pattern for grouped shapes
- **Keyboard Navigation**: Simplified with separate state management

## Migration Notes

For migrating existing code:

1. **Reading**: Parse existing DrawableContainer serialized data into new Shape classes
2. **Writing**: Convert new Shapes to DrawableContainer format if needed for backward compatibility
3. **UI Integration**: Wire up events to update ShapeEditorState based on user interactions
4. **Rendering**: Replace DrawableContainer.Draw() calls with CanvasRenderer.RenderCanvas()

## Summary

This new architecture provides:
- **Cleaner code** through separation of concerns
- **Better maintainability** with focused, single-responsibility classes
- **Enhanced reusability** with shareable styles
- **Easier testing** with pure data models
- **Future flexibility** for different rendering systems and features

The system is production-ready and can be used immediately alongside the existing architecture.
