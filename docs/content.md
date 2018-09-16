# Content

## Improved markup extension nesting

This project offers a straightforward solution to the problem that you don't know where you are in case of a nested markup, giving you the advantage to exactly control the output of your markup extension depending on the nesting tree, thus extremely expanding their application range. It's very easy to implement, even if you never built markup extensions before.

Take a look at this sample (coming soon) for a demonstration.

## Feature summary

* Each markup extension knows its nesting subtree (downto each dependency object)
  * During *runtime and designtime*
  * Multiple branches possible
  * Markup extension (trees) can be declared as resources to reduce memory and calculation overhead
* ProvideValue is replaced by a function delivering the particular endpoint information
* Notification engine to update the whole nesting tree if needed
* Support of List objects
* Interface for existing markup extensions included

## Why

Markup extensions are not able to resolve their target dependency object when nested into each other. This restricts their usage to a small application range, because useful information such as the types of the target object and property as well as the content of additional attached or inherited properties could not be read out in order to adopt the own result to the particular situation.

Therefore, a new markup extension base class derived from the WPF MarkupExtension class was developed. In this class, the whole nesting tree is properly tracked during the ProvideValue function that is called by the XAML parser at designtime or runtime. This enables each markup extension in the tree to adopt its output to each endpoint (the target dependency object) of its subtree including multiple branches caused by markup extensions referenced as a resource.

## Localization

With the help of the nesting feature, the WPF Localization Extension was enhanced. Please refer to the homepage and the latest GitHub commits of this project for further information.

## String operations

* Concatenate extension
  * Concatenate a list of strings (or markup extensions delivering these strings)
  * Provide a format code to control the output

## Design

* Alternating grid color extension
  * Apply an alternating color to rows of a grid.

## Under developement

* Dynamic binding extension - allows changes of Source and Path
* Aspect ratio extension - bind width and height of a control while keeping a particular aspect ratio

## Classes

### XAMLMarkupExtensions.Base

* INestedMarkupExtension:
  Interface that has to be implemented to add the nesting feature to other classes derived from MarkupExtension.

* NestedMarkupExtension:
  A default implementation of the INestedMarkupExtension interface. We suggest to use this class to design nestable markup extensions.

* ObjectDependencyManager and SimpleProvideValueServiceProvider:
  Internal used helper classes.

* ParentChangedNotifier:
  Helper class for changes on the Parent property of FrameworkElement objects.
  
* Type, Static & Null extension (known from WPF)

### XAMLMarkupExtensions.Binding

* DynBindingExtension:
  A markup extension that can change the source and/or path of a binding.

* BindingProxy:
  Internal used helper class.

### XAMLMarkupExtensions.Strings

* CatExtension:
  Concatenate strings coming from various sources.

### XAMLMarkupExtensions.Design

* AlternatingGridColorExtension:
  Return a specific color for odd and even rows in grids.

To Be Continued...
(contributions are welcome)