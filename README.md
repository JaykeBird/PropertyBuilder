# PropertyBuilder

A quick tool to create properties for your WPF and Avalonia controls. This creates WPF DependencyProperty code, and Avalonia StyledProperty and DirectProperty code.

![image](https://github.com/JaykeBird/PropertyBuilder/assets/1905005/a3c3e511-5183-4126-a44c-69bc3fb238c4)

## How to use / Quick Overview

There are five tabs present.

1. Build a Property

Good to quickly build one property to use in your WPF and Avalonia controls. Set the options and enter the values as needed, and then copy the generated code in the boxes below.

2. Bulk Build

Build multiple properties quickly for a WPF or Avalonia control. Use "Add Property" to add on another property to generate (be sure to fill in the values), and then use "Copy All" to copy all of the properties that you've added in.

3. Resources

Includes some code snippets (such as the `PerformAs` extension methods) and links to online resources about dependency properties and Avalonia properties.

4. Options

Change the appearance of the program, or the appearance of the code displays.

5. About This Program

An about page that displays the credits and links to websites (like to here).

### More Help

For more information about how to use this tool directly, please refer to the [HELP_INFO.md](https://github.com/JaykeBird/PropertyBuilder/blob/main/HELP_INFO.md) file in this repository.

### Usage tip: Add as an External Tool in Visual Studio

I've found it useful to add this tool as an External Tool in Visual Studio, so that you can quickly access it from the Tools menu. To get it set up for yourself, you can follow these steps:

1. In Visual Studio, go to Tools > External Tools....
2. In the External Tools dialog, click "Add".
3. For Title, you can set it to "WPF/Avalonia Property Builder" or whatever title works for you.
4. For Command, paste in the full path to the extracted/compiled EXE file (or click on the "..." button to display an Open dialog and navigate to where you have the EXE file).
5. Nothing else needs to be added or changed. You can click on "OK".
6. Now this tool will be right there in the Tools menu, available to be quickly opened at any time!

## Project Structure

The actual magic is in the [PropertyBuilder.cs](https://github.com/JaykeBird/PropertyBuilder/blob/main/PropertyBuilderCore/PropertyBuilder.cs) file in the PropertyBuilderCore project. If you're looking to reuse the actual property building code in your own project, this is really all you need. (Please do credit me if you use it though!)

The PropertyBuilderWPF project is just a little app that provides a GUI for interacting with the PropertyBuilder's code in various ways, and also includes some other resources.

## Background

Creating WPF DependencyProperty objects by typing it all out was getting tedious, so a little while ago I made a CLI tool to make putting them together a lot easier and quicker for me. The CLI tool is great, but suffers from the limitations of... well, not having a full GUI, so I knew eventually I would want to go and build a GUI tool to use instead.

Well, now I'm working on Solid Shine UI 2.0, which will feature multiple new controls and also an Avalonia port of all of my controls. So, given how much I'm going to be needing to generate WPF DependencyProperty *and* Avalonia StyledProperty and DirectProperty code for all of these controls, now felt like the best time to put together this tool where I could build these in one place.

I believe this will make my workflow a little bit faster, especially when it comes to more work in Avalonia, and maybe it'll be of some help for others too!

## Future Plans

1. Add option for creating a RoutedPropertyChangedEventHandler event when building a WPF DependencyProperty
2. Add support for other frameworks (MAUI, Uno, Eto) (will probably require some rethinking about the UI)
3. Build an Avalonia GUI version of this (after Solid Shine UI 2.0 releases)

## License / Credits

Licensed under the MIT License

Created by Jayke R. Huempfner (JaykeBird) between December 2023 and January 2024.

If you use my code, I would prefer a credit! (And obviously any code you generate via this tool you're free to do with whatever you want.)

This tool uses my library [Solid Shine UI](https://github.com/JaykeBird/ssui) for the UI controls, and utilizes [AvalonEdit](https://github.com/icsharpcode/AvalonEdit) for displaying the generated C# code.
