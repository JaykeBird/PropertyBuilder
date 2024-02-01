# Info about this program / Help Overview

The program's main functionality is split between the "Build a Property" tab, suitable for building properties one-at-a-time for both WPF and Avalonia simultaneously, and the "Bulk Build" tab which is more useful for quickly creating a collection of properties at once for just WPF or just Avalonia.

In this document, I'll describe the different options that I've presented here. For information on how to use the built WPF DependencyProperty and Avalonia StyledProperty and DirectProperty code, you should refer to the links listed at the bottom of the "Resources" tab in the program (which I've also listed at the bottom of this document, for convenience). As a result, this document assumes you already know what a DependencyProperty or StyledProperty is, and is instead more just about how to tweak the options present to build what you need.

## Build a Property

Build a Proeprty is suitable for making a single property for both WPF and Avalonia application simultaneously, with some options present to tweak the output as you'd like (and remove more of the tedium).

The four options in the text boxes at the top are:

#### Property Name

Simply, the name of the property. This is what you'll utilize when you want to refer to this property elsewhere in your code. When building a property, you should already know what you're going to name it.

To go along with standard conventions for WPF and Avalonia, the property name is also used to define the underlying WPF DependencyProperty or Avalonia StyledProperty/DirectProperty object (as well as the private field used by the DirectProperty). For example, a property with the name `Title` will have a DependencyProperty or StyledProperty named `TitleProperty`. It's highly recommended to stick to this convention.

#### Property Type

The data type or value type or what-have-you that this property handles. Similar to the name, you should already know what type you want to put here.

To give a basic example, a property with the name `Title` is probably going to be of type `string`.

#### Owner Type

Different from the property type above, Owner Type refers to the type of the control/object that will "own" this property. Most likely, you'll just want to put in the name of the class that you're putting this property into.

As a basic example, if you're building a property called `Width` to add to your control `MyButton`, the owner type you're probably going to fill in here will be `MyButton`.

You can add on additional owners for a DependencyProperty or StyledProperty later, by using the property's `AddOwner` method (such as `TitleProperty.AddOwner(typeof(MyType2))`) in that relevant class's code. This can be helpful in scenarios where you're interacting with bindings or properties programmatically, rather than handling this via XAML.

#### Default Value

The initial value the property is set to when it is created/initialized. You can omit this (leave the text box blank) to not set a default value at all; in this case, the value will be set to `DependencyProperty.UnsetValue` in WPF and `AvaloniaProperty.UnsetValue` in Avalonia, and will return as null if you attempt to get (and cast) the value.

When setting the initial value, you can set it to any static value, even including `null` or the name of a constant in your code. When entering in a string, though, make sure to type in the quotes in the text box in this program, such as `"my default value"` rather than just `my default value`.

### WPF Options

#### Value Changed Handler

While not strictly related to creating a DependencyProperty, I provide some options here to ease the creation of more of this boilerplate stuff. In particular, the options here set up some code for you that will execute whenever the value of this dependency property changes. To reiterate, none of this is strictly needed to create a DependencyProperty, but are just here for help and convenience.

Here's an overview of all the options:

| Title                    | Description                                                                                              |
|--------------------------|----------------------------------------------------------------------------------------------------------|
| None                     | Don't actually add in any extra code.                                                                    |
| Standard                 | Add a standard static method, named `On{PropertyName}Changed`.                                           |
| StandardEventCall        | Like Standard, but uses the static method to raise a `{PropertyName}Changed` event.                      |
| StandardRoutedEventCall  | Like StandardEventCall, but `{PropertyName}Changed` is now a routed event.                               |
| PerformAs                | Use the PerformAs extension method to achieve the same thing as Standard, but with less lines of code.   |
| PerformAsEventCall       | Use the PerformAs extension method to raise a `{PropertyName}Changed` event.                             |
| PerformAsRoutedEventCall | Like PerformAsEventCall, but `{PropertyName}Changed` is now a routed event, called via a private method. |

`PerformAs` is an extension method that I created as part of my SolidShineUi control library. View the Resources tab in the program for more information on it and the code for you to copy and use, or the [PerformAs.txt file](https://github.com/JaykeBird/PropertyBuilder/blob/main/PropertyBuilderWPF/PerformAs.txt) for just the code. Essentially, via using anonymous functions and this extension method, I was able to provide the same functionality (including a type check) as you would see if you used the *Standard* or *StandardEventCall* options, but in less lines of code overall.

#### Is read-only

Set if this DependencyProperty should be read-only or not. Setting a DependencyProperty as read-only should not be the default, but there are certain situations where this is useful (such as collection types). Refer to the WPF documentation (linked below) to learn more about when you should consider making a DependencyProperty read-only.

### Avalonia Options

Since Avalonia handles listening to property changes in a different way, it was not really possible for me to create a self-contained block of code that has support for raising an event or executing some code when a property is changed. Instead, you'll have to set up this yourself, and I give suggestions about how to do this in the Resources tab of the program.

#### Type

Select the type of property being created for use in Avalonia.

Avalonia has two different types of AvaloniaProperties: the StyledProperty and DirectProperty. Please refer to the link listed below for more information on the difference between the two; in general, StyledProperty is the most direct comparison to the WPF DependencyProperty (including the larger overhead that comes with it), while a DirectProperty is more useful when just needed for data handling or binding or achieving certain scenarios that StyledProperty cannot. For example, you cannot create a read-only StyledProperty; instead, you'll need to create a read-only DirectProperty.

From the drop-down here, you get to select the type of property that is built:

| Title              | Description                                                       |
|--------------------|-------------------------------------------------------------------|
| StyledProperty     | Create a StyledProperty.                                          |
| DirectProperty     | Create a DirectProperty.                                          |
| DirectAsReadOnly   | Create a DirectProperty that has a private setter (is read-only). |

### All Set!

When you have all the values above set as you wish, you can copy the WPF DependencyProperty and Avalonia StyledProperty/DirectProperty code by clicking the "Copy" buttons underneath the different code displays.

You are free to type in and modify the code in the displays before you copy it, but note that if you go back and change any of the text or options above, that'll reset the built code and discard your changes.

## Bulk Build

When in a situation where you're trying to build multiple properties for a single control or class quickly, you can use the Bulk Build tab. This allows you to quickly create multiple properties, and then copy all of them once they're all set.

To make this tab easier to use, the WPF-specific options above (and the Avalonia read-only DirectProperty option) are not present here. This tab is more meant for just creating a bunch of properties at once which don't need any extra logic.

### Getting Started

To get started, enter in the Owner Type in the text box at the top of the window. Owner Type refers to the type of the control/object that will "own" these properties. Most likely, you'll just want to put in the name of the class that you're putting them all into.

Then, next to the text box, you can select the type of properties you want to create: WPF DependencyProperty, or the Avalonia StyledProperty or DirectProperty. The read-only options aren't provided here, since read-only properties should be one-off things, not a standard/default.

If you need to create properties for both a WPF control/class and an Avalonia control/class, you can start with selecting one type here, setting up the properties and then hitting "Copy All" to paste into your first class, and then afterwards switch the type over and then hit "Copy All" again for the second class.

Once you have these initial options set up, you can start adding properties.

### Adding Properties

To add a property, click on the "Add Property" in the bottom-left corner of the window. This adds on a blank property entry onto the bottom of the list. From here, you can start filling in the details of the name and type of the property, and setting a default value (if desired).

You're able to just copy this property alone by clicking on the "Copy" button beneath the generated code. You can remove this property from the list by clicking on the "X" Remove button to the right of the generated code display.

You can continue to add more and more properties as needed by continuing to click on the "Add Property" button.

### All Set!

Once you have all the properties added and named that you want, you can copy all of the built code at once by clicking on the blue "Copy All" button in the bottom-right corner of the window.

If you want to clear the list of properties and start over fresh, then click on the "Clear" button to do so; note that there is no confirmation or any undo.

## More Help / Contact

If you need additional help with this program or want to reach out, please don't be afraid to create an issue here on GitHub or reach out to me at my social media profiles at [jaykebird.com/aboutMe](https://jaykebird.com/aboutMe).

Note that this is a project that I built for free and to primarily solve a use case of mine, so I do not guarantee any level of support or warranty. I intend to keep this tool relatively small, so ideas or feature suggestions will really only be considered if they're closely aligned to what this tool already does.

## Resources

Here is a link of all the resources that I've also linked on the Resources tab in the program. If you're needing more information on the WPF DependencyProperty, or how to use a StyledProperty or DirectProperty with Avalonia, these resources can help. These all link directly to the official documentation pages.

1. [WPF Dependency Properties](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/properties/dependency-properties-overviewview=netdesktop-8.0)
2. [WPF Control Authoring](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/controls/control-authoring-overview#control-authoring-basics)
3. [Avalonia DirectProperties and StyledProperties](https://docs.avaloniaui.net/docs/guides/custom-controls/how-to-create-advanced-customcontrols)
4. [Avalonia Listening to Property Changes](https://docs.avaloniaui.net/docs/guides/data-binding/binding-from-code#subscribing-to-a-property-onany-object)
5. [Avalonia Styling](https://docs.avaloniaui.net/docs/basics/user-interface/styling/)
6. [Avalonia Routed Events](https://docs.avaloniaui.net/docs/concepts/input/routed-events)