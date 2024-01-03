using System.Text;

namespace PropertyBuilder
{

    /// <summary>
    /// String generators for WPF DependencyProperties and Avalonia StyledProperties (and DirectProperties).
    /// </summary>
    public static class PropertyBuilder
    {

        /// <summary>
        /// Generate a WPF DependencyProperty.
        /// </summary>
        /// <param name="propName">The name of the property</param>
        /// <param name="typeName">The property's type</param>
        /// <param name="ownerName">The type of the object that will own this property</param>
        /// <param name="defVal">a default value to set this property to, if desired (for strings, please include the enclosing quote characters (<c>"value"</c>))</param>
        /// <param name="handleValueChanges">whether to automatically include a handler for when this property's value changes</param>
        /// <param name="readOnly">whether this property is meant to be read-only; a private setter will be provided</param>
        /// <returns>Code for setting up a WPF DependencyProperty with the inputted values</returns>
        public static string BuildWpfProperty(string propName, string typeName, string ownerName, string? defVal = null, bool readOnly = false, 
            WpfPropertyChangeHandler handleValueChanges = WpfPropertyChangeHandler.None)
        {
            // check if a default value is provided
            bool hasDefVal = !string.IsNullOrEmpty(defVal);

            StringBuilder sb = new();
            
            // first, we want to generate the base C# property (the "{ get ...; set ...; }" part)
            if (readOnly)
            {
                sb.AppendLine($"public {typeName} {propName} {{ get => ({typeName})GetValue({propName}Property); private set => SetValue({propName}PropertyKey, value); }}"); // read-only C# property
            }
            else
            {
                sb.AppendLine($"public {typeName} {propName} {{ get => ({typeName})GetValue({propName}Property); set => SetValue({propName}Property, value); }}"); // base C# property
            }

            sb.AppendLine();
            
            // set up the XML comments for the dependency property, is annoying to add later lol
            sb.AppendLine($"/// <summary>The backing dependency property for <see cref=\"{propName}\"/>. See the related property for details.</summary>"); // XML comment for dependency property
            
            // now we'll set up the dependency property itself
            // if it's read-only, we'll need a public-facing DependencyProperty and a private DependencyPropertyKey (which can be used to still set the value)
            if (readOnly)
            {
                sb.AppendLine($"public static readonly DependencyProperty {propName}Property = {propName}PropertyKey.DependencyProperty;"); // define public dependency property for read-only access
                sb.AppendLine();
                sb.AppendLine($"private static readonly DependencyPropertyKey {propName}PropertyKey");                                    // first line defining the internal dependency property key
                sb.AppendLine($"    = DependencyProperty.RegisterReadOnly(nameof({propName}), typeof({typeName}), typeof({ownerName})" + (hasDefVal ? "," : ");")); // dependency property parameters
            }
            else
            {
                sb.AppendLine($"public static DependencyProperty {propName}Property");                                                                // first line defining the dependency property
                sb.AppendLine($"    = DependencyProperty.Register(nameof({propName}), typeof({typeName}), typeof({ownerName})" + (hasDefVal ? "," : ");")); // dependency property parameters
            }

            // if we want to set a default value or add a handler for changes to the property's value, we'll need to add on a FrameworkPropertyMetadata
            // (technically, a PropertyMetadata is just fine, but the FrameworkPropertyMetadata offers the ability to set some extra flags where/when needed)
            if (hasDefVal || handleValueChanges != WpfPropertyChangeHandler.None)
            {
                // we could technically get away without providing a default value, but here, I'll just set it to null
                // technically, the best practice would be to use default(typeName), but I've just gotten used to using null
                if (string.IsNullOrEmpty(defVal)) defVal = "null";

                sb.AppendLine($"    new FrameworkPropertyMetadata({defVal}" + GeneratePropertyChangedHandler() + "));");
            }

            // now if we want to build handler methods/events for value changes, let's add them on at the very end
            if (handleValueChanges == WpfPropertyChangeHandler.StandardEventCall)
            {
                // generate an OnPropertyChanged handler
                sb.AppendLine();
                sb.AppendLine($"private static void On{propName}Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)");
                sb.AppendLine("{");
                sb.AppendLine($"    if (d is {ownerName} o)");
                sb.AppendLine("    {");
                sb.AppendLine($"        d.{propName}Changed?.Invoke(o, e);");
                sb.AppendLine("    }");
                sb.AppendLine("}");
                sb.AppendLine();
                // and now the PropertyChanged event
                sb.AppendLine($"/// <summary>");
                sb.AppendLine($"/// Raised when the <see cref=\"{propName}\"/> property is changed.");
                sb.AppendLine($"/// </summary>");
                sb.AppendLine($"#if NETCOREAPP");
                sb.AppendLine($"        public event DependencyPropertyChangedEventHandler? {propName}Changed;");
                sb.AppendLine($"#else");
                sb.AppendLine($"        public event DependencyPropertyChangedEventHandler {propName}Changed;");
                sb.AppendLine($"#endif");
            }
            else if (handleValueChanges == WpfPropertyChangeHandler.Standard)
            {
                // generate an OnPropertyChanged handler
                sb.AppendLine();
                sb.AppendLine($"private static void On{propName}Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)");
                sb.AppendLine("{");
                sb.AppendLine($"    if (d is {ownerName} o)");
                sb.AppendLine("    {");
                sb.AppendLine("    ");
                sb.AppendLine("    }");
                sb.AppendLine("}");
            }
            else if (handleValueChanges == WpfPropertyChangeHandler.PerformAsEventCall)
            {
                // generate the PropertyChanged event
                sb.AppendLine();
                sb.AppendLine($"/// <summary>");
                sb.AppendLine($"/// Raised when the <see cref=\"{propName}\"/> property is changed.");
                sb.AppendLine($"/// </summary>");
                sb.AppendLine($"#if NETCOREAPP");
                sb.AppendLine($"        public event DependencyPropertyChangedEventHandler? {propName}Changed;");
                sb.AppendLine($"#else");
                sb.AppendLine($"        public event DependencyPropertyChangedEventHandler {propName}Changed;");
                sb.AppendLine($"#endif");
                // Only .NET Core / modern .NET supports nullability, where .NET Framework does not - this preprocessor directive makes that split
                // those who are on .NET Core / modern .NET and not using the nullability feature can just remove this split and just use the one line
                // but I didn't want to add that on as just another option here when 1) this is primarily built for my use and 2) there's already a number of options
            }

            return sb.ToString();

            // internal helper for filling in the final part of the FrameworkPropertyMetadata
            string GeneratePropertyChangedHandler()
            {
                return handleValueChanges switch
                {
                    WpfPropertyChangeHandler.None => "",
                    WpfPropertyChangeHandler.Standard => $", On{propName}Changed",
                    WpfPropertyChangeHandler.StandardEventCall => $", On{propName}Changed",
                    WpfPropertyChangeHandler.PerformAs => $", (d, e) => d.PerformAs<{ownerName}>((o) => )",
                    WpfPropertyChangeHandler.PerformAsEventCall => $", (d, e) => d.PerformAs<{ownerName}>((o) => o.{propName}Changed?.Invoke(o, e))",
                    _ => "",
                };
            }
        }

        /// <summary>
        /// Generate a WPF DependencyProperty.
        /// </summary>
        /// <param name="writer">the <see cref="TextWriter"/> to write to</param>
        /// <param name="propName">The name of the property</param>
        /// <param name="typeName">The property's type</param>
        /// <param name="ownerName">The type of the object that will own this property</param>
        /// <param name="defVal">a default value to set this property to, if desired (for strings, please include the enclosing quote characters (<c>"value"</c>))</param>
        /// <param name="handleValueChanges">whether to automatically include a handler for when this property's value changes</param>
        /// <param name="readOnly">whether this property is meant to be read-only; a private setter will be provided</param>
        /// <returns>Code for setting up a WPF DependencyProperty with the inputted values</returns>
        public static void BuildWpfProperty(ref TextWriter writer, string propName, string typeName, string ownerName, string? defVal = null, bool readOnly = false,
            WpfPropertyChangeHandler handleValueChanges = WpfPropertyChangeHandler.None)
        {
            // check if a default value is provided
            bool hasDefVal = !string.IsNullOrEmpty(defVal);

            // first, we want to generate the base C# property (the "{ get ...; set ...; }" part)
            if (readOnly)
            {
                writer.WriteLine($"public {typeName} {propName} {{ get => ({typeName})GetValue({propName}Property); private set => SetValue({propName}PropertyKey, value); }}"); // read-only C# property
            }
            else
            {
                writer.WriteLine($"public {typeName} {propName} {{ get => ({typeName})GetValue({propName}Property); set => SetValue({propName}Property, value); }}"); // base C# property
            }

            writer.WriteLine();

            // set up the XML comments for the dependency property, is annoying to add later lol
            writer.WriteLine($"/// <summary>The backing dependency property for <see cref=\"{propName}\"/>. See the related property for details.</summary>"); // XML comment for dependency property

            // now we'll set up the dependency property itself
            // if it's read-only, we'll need a public-facing DependencyProperty and a private DependencyPropertyKey (which can be used to still set the value)
            if (readOnly)
            {
                writer.WriteLine($"public static readonly DependencyProperty {propName}Property = {propName}PropertyKey.DependencyProperty;"); // define public dependency property for read-only access
                writer.WriteLine();
                writer.WriteLine($"private static readonly DependencyPropertyKey {propName}PropertyKey");                                    // first line defining the internal dependency property key
                writer.WriteLine($"    = DependencyProperty.RegisterReadOnly(nameof({propName}), typeof({typeName}), typeof({ownerName})" + (hasDefVal ? "," : ");")); // dependency property parameters
            }
            else
            {
                writer.WriteLine($"public static DependencyProperty {propName}Property");                                                                // first line defining the dependency property
                writer.WriteLine($"    = DependencyProperty.Register(nameof({propName}), typeof({typeName}), typeof({ownerName})" + (hasDefVal ? "," : ");")); // dependency property parameters
            }

            // if we want to set a default value or add a handler for changes to the property's value, we'll need to add on a FrameworkPropertyMetadata
            // (technically, a PropertyMetadata is just fine, but the FrameworkPropertyMetadata offers the ability to set some extra flags where/when needed)
            if (hasDefVal || handleValueChanges != WpfPropertyChangeHandler.None)
            {
                // we could technically get away without providing a default value, but here, I'll just set it to null
                // technically, the best practice would be to use default(typeName), but I've just gotten used to using null
                if (string.IsNullOrEmpty(defVal)) defVal = "null";

                writer.WriteLine($"    new FrameworkPropertyMetadata({defVal}" + GeneratePropertyChangedHandler() + "));");
            }

            // now if we want to build handler methods/events for value changes, let's add them on at the very end
            if (handleValueChanges == WpfPropertyChangeHandler.StandardEventCall)
            {
                // generate an OnPropertyChanged handler
                writer.WriteLine();
                writer.WriteLine($"private static void On{propName}Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)");
                writer.WriteLine("{");
                writer.WriteLine($"    if (d is {ownerName} o)");
                writer.WriteLine("    {");
                writer.WriteLine($"        d.{propName}Changed?.Invoke(o, e);");
                writer.WriteLine("    }");
                writer.WriteLine("}");
                writer.WriteLine();
                // and now the PropertyChanged event
                writer.WriteLine($"/// <summary>");
                writer.WriteLine($"/// Raised when the <see cref=\"{propName}\"/> property is changed.");
                writer.WriteLine($"/// </summary>");
                writer.WriteLine($"#if NETCOREAPP");
                writer.WriteLine($"        public event DependencyPropertyChangedEventHandler? {propName}Changed;");
                writer.WriteLine($"#else");
                writer.WriteLine($"        public event DependencyPropertyChangedEventHandler {propName}Changed;");
                writer.WriteLine($"#endif");
            }
            else if (handleValueChanges == WpfPropertyChangeHandler.Standard)
            {
                // generate an OnPropertyChanged handler
                writer.WriteLine();
                writer.WriteLine($"private static void On{propName}Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)");
                writer.WriteLine("{");
                writer.WriteLine($"    if (d is {ownerName} o)");
                writer.WriteLine("    {");
                writer.WriteLine("    ");
                writer.WriteLine("    }");
                writer.WriteLine("}");
            }
            else if (handleValueChanges == WpfPropertyChangeHandler.PerformAsEventCall)
            {
                // generate the PropertyChanged event
                writer.WriteLine();
                writer.WriteLine($"/// <summary>");
                writer.WriteLine($"/// Raised when the <see cref=\"{propName}\"/> property is changed.");
                writer.WriteLine($"/// </summary>");
                writer.WriteLine($"#if NETCOREAPP");
                writer.WriteLine($"        public event DependencyPropertyChangedEventHandler? {propName}Changed;");
                writer.WriteLine($"#else");
                writer.WriteLine($"        public event DependencyPropertyChangedEventHandler {propName}Changed;");
                writer.WriteLine($"#endif");
            }

            // internal helper for filling in the final part of the FrameworkPropertyMetadata
            string GeneratePropertyChangedHandler()
            {
                return handleValueChanges switch
                {
                    WpfPropertyChangeHandler.None => "",
                    WpfPropertyChangeHandler.Standard => $", On{propName}Changed",
                    WpfPropertyChangeHandler.StandardEventCall => $", On{propName}Changed",
                    WpfPropertyChangeHandler.PerformAs => $", (d, e) => d.PerformAs<{ownerName}>((o) => )",
                    WpfPropertyChangeHandler.PerformAsEventCall => $", (d, e) => d.PerformAs<{ownerName}>((o) => o.{propName}Changed?.Invoke(o, e))",
                    _ => "",
                };
            }
        }

        /// <summary>
        /// Generate an Avalonia StyledProperty. Use <see cref="BuildAvaloniaDirectProperty(string, string, string, string?, string?, bool)"/> for a DirectProperty.
        /// </summary>
        /// <param name="propName">The name of the property</param>
        /// <param name="typeName">The property's type</param>
        /// <param name="ownerName">The type of the object that will own this property</param>
        /// <param name="defVal">a default value to set this property to, if desired (for strings, please include the enclosing quote characters (<c>"value"</c>))</param>
        /// <returns>Code for setting up an Avalonia StyledProperty with the inputted values</returns>
        /// <remarks>
        /// To handle value changes, override the protected method <c>OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)</c>
        /// <para/>
        /// Note that there is not a read-only version of a StyledProperty; if you need a read-only value, you'll need to use a DirectProperty.
        /// To see the difference between StyledProperties and DirectProperties, see <a href="https://docs.avaloniaui.net/docs/guides/custom-controls/how-to-create-advanced-custom-controls" 
        /// >this page in Avalonia's documentation</a>.
        /// </remarks>
        public static string BuildAvaloniaProperty(string propName, string typeName, string ownerName, string? defVal = null)
        {
            StringBuilder sb = new();
            sb.AppendLine($"public {typeName} {propName} {{ get => GetValue({propName}Property); set => SetValue({propName}Property, value); }}"); // base C# property
            sb.AppendLine();
            sb.AppendLine($"/// <summary>The backing styled property for <see cref=\"{propName}\"/>. See the related property for details.</summary>"); // XML comment for styled property
            sb.AppendLine($"public static readonly StyledProperty<{typeName}> {propName}Property");                                                 // first line defining the styled property
            sb.AppendLine($"    = AvaloniaProperty.Register<{ownerName}, {typeName}>(nameof({propName})" + (string.IsNullOrEmpty(defVal) ? "" : $",{defVal}") + ");"); // styled property parameters

            return sb.ToString();
        }

        /// <summary>
        /// Generate an Avalonia StyledProperty. Use <see cref="BuildAvaloniaDirectProperty(ref TextWriter, string, string, string, string?, string?, bool)"/> for a DirectProperty.
        /// </summary>
        /// <param name="writer">the <see cref="TextWriter"/> to write to</param>
        /// <param name="propName">The name of the property</param>
        /// <param name="typeName">The property's type</param>
        /// <param name="ownerName">The type of the object that will own this property</param>
        /// <param name="defVal">a default value to set this property to, if desired (for strings, please include the enclosing quote characters (<c>"value"</c>))</param>
        /// <returns>Code for setting up an Avalonia StyledProperty with the inputted values</returns>
        /// <remarks>
        /// To handle value changes, override the protected method <c>OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)</c>
        /// <para/>
        /// Note that there is not a read-only version of a StyledProperty; if you need a read-only value, you'll need to use a DirectProperty.
        /// To see the difference between StyledProperties and DirectProperties, see <a href="https://docs.avaloniaui.net/docs/guides/custom-controls/how-to-create-advanced-custom-controls" 
        /// >this page in Avalonia's documentation</a>.
        /// </remarks>
        public static void BuildAvaloniaProperty(ref TextWriter writer, string propName, string typeName, string ownerName, string? defVal = null)
        {
            writer.WriteLine($"public {typeName} {propName} {{ get => GetValue({propName}Property); set => SetValue({propName}Property, value); }}"); // base C# property
            writer.WriteLine();
            writer.WriteLine($"/// <summary>The backing styled property for <see cref=\"{propName}\"/>. See the related property for details.</summary>"); // XML comment for styled property
            writer.WriteLine($"public static readonly StyledProperty<{typeName}> {propName}Property");                                                 // first line defining the styled property
            writer.WriteLine($"    = AvaloniaProperty.Register<{ownerName}, {typeName}>(nameof({propName})" + (string.IsNullOrEmpty(defVal) ? "" : $",{defVal}") + ");"); // styled property parameters
        }

        /// <summary>
        /// Generate an Avalonia DirectProperty.
        /// </summary>
        /// <param name="propName">The name of the property</param>
        /// <param name="typeName">The property's type</param>
        /// <param name="ownerName">The type of the object that will own this property</param>
        /// <param name="fieldName">the name to use for the internal field; recommended to leave blank, to generate a default name based upon <paramref name="propName"/></param>
        /// <param name="readOnly">set if this property is only meant to be read-only; a private setter will be provided</param>
        /// <param name="defVal">a default value to set this property to, if desired (for strings, please include the enclosing quote characters (<c>"value"</c>))</param>
        /// <returns>Code for setting up a Avalonia DirectProperty with the inputted values</returns>
        /// <remarks>
        /// To handle value changes, override the protected method <c>OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)</c>
        /// <para/>
        /// To see the difference between StyledProperties and DirectProperties, see <a href="https://docs.avaloniaui.net/docs/guides/custom-controls/how-to-create-advanced-custom-controls" 
        /// >this page in Avalonia's documentation</a>.
        /// </remarks>
        public static string BuildAvaloniaDirectProperty(string propName, string typeName, string ownerName, string? defVal = null, string? fieldName = null, bool readOnly = false)
        {
            // generate a fieldName if one isn't put in - this field name is done by taking the property name and lowercasing the first letter
            if (string.IsNullOrEmpty(propName))
            {
                fieldName = "_propertyField";
            }
            else
            {
                fieldName ??= string.Concat(propName.Substring(0, 1).ToLowerInvariant(), propName.AsSpan(1));
            }

            StringBuilder sb = new();
            sb.AppendLine($"private {typeName} _{fieldName} " + (string.IsNullOrEmpty(defVal) ? ";" : $" = {defVal};"));
            sb.AppendLine();
            sb.AppendLine($"public {typeName} {propName} {{ get => _{propName}; " + (readOnly ? "private" : "") + $" set => SetAndRaise({propName}Property, ref _{fieldName}, value); }}");
            sb.AppendLine();
            sb.AppendLine($"/// <summary>The backing direct property for <see cref=\"{propName}\"/>. See the related property for details.</summary>");
            sb.AppendLine($"public static readonly DirectProperty<{ownerName},{typeName}> {propName}Property");
            sb.Append($"    = AvaloniaProperty.RegisterDirect<{ownerName}, {typeName}>(nameof({propName}), (s) => s.{propName}" + (readOnly ? "" : $", (s, v) => s.{propName} = v"));
            if (!string.IsNullOrEmpty(defVal))
            {
                sb.AppendLine($", unsetValue: {defVal});");
            }
            else
            {
                sb.AppendLine(");");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generate an Avalonia DirectProperty.
        /// </summary>
        /// <param name="writer">the <see cref="TextWriter"/> to write to</param>
        /// <param name="propName">The name of the property</param>
        /// <param name="typeName">The property's type</param>
        /// <param name="ownerName">The type of the object that will own this property</param>
        /// <param name="fieldName">the name to use for the internal field; recommended to leave blank, to generate a default name based upon <paramref name="propName"/></param>
        /// <param name="readOnly">set if this property is only meant to be read-only; a private setter will be provided</param>
        /// <param name="defVal">a default value to set this property to, if desired (for strings, please include the enclosing quote characters (<c>"value"</c>))</param>
        /// <returns>Code for setting up a Avalonia DirectProperty with the inputted values</returns>
        /// <remarks>
        /// To handle value changes, override the protected method <c>OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)</c>
        /// <para/>
        /// To see the difference between StyledProperties and DirectProperties, see <a href="https://docs.avaloniaui.net/docs/guides/custom-controls/how-to-create-advanced-custom-controls" 
        /// >this page in Avalonia's documentation</a>.
        /// </remarks>
        public static void BuildAvaloniaDirectProperty(ref TextWriter writer, string propName, string typeName, string ownerName, string? defVal = null, 
            string? fieldName = null, bool readOnly = false)
        {
            // generate a fieldName if one isn't put in - this field name is done by taking the property name and lowercasing the first letter
            fieldName ??= string.Concat(propName.Substring(0, 1).ToLowerInvariant(), propName.AsSpan(1));

            writer.WriteLine($"private {typeName} _{fieldName} " + (string.IsNullOrEmpty(defVal) ? ";" : $" = {defVal};"));
            writer.WriteLine();
            writer.WriteLine($"public {typeName} {propName} {{ get => _{propName}; " + (readOnly ? "private" : "") + $" set => SetAndRaise({propName}Property, ref _{fieldName}, value); }}");
            writer.WriteLine();
            writer.WriteLine($"/// <summary>The backing direct property for <see cref=\"{propName}\"/>. See the related property for details.</summary>");
            writer.WriteLine($"public static readonly DirectProperty<{ownerName},{typeName}> {propName}Property");
            writer.Write($"    = AvaloniaProperty.RegisterDirect<{ownerName}, {typeName}>(nameof({propName}), (s) => s.{propName}" + (readOnly ? "" : $", (s, v) => s.{propName} = v"));
            if (!string.IsNullOrEmpty(defVal))
            {
                writer.WriteLine($", unsetValue: {defVal});");
            }
            else
            {
                writer.WriteLine(");");
            }
        }
    }

    /// <summary>
    /// Set the type of property value change handler to generate with a WPF DependencyProperty.
    /// </summary>
    /// <remarks>
    /// See this code file to see Solid Shine UI's PerformAs extension method:
    /// <a href="https://github.com/JaykeBird/ssui/blob/main/SolidShineUi/Utils/BaseExtensions.cs">Utils/BaseExtensions.cs</a>
    /// </remarks>
    public enum WpfPropertyChangeHandler
    {
        /// <summary>
        /// Do not generate any handler at all.
        /// </summary>
        None = 0,
        /// <summary>
        /// Generate a <c>On{Property}Changed</c> handler.
        /// </summary>
        Standard = 1,
        /// <summary>
        /// Generate a <c>On{Property}Changed</c> handler, that raises a <c>{Property}Changed</c> event.
        /// </summary>
        StandardEventCall = 2,
        /// <summary>
        /// Use Solid Shine UI's PerformAs extension method, to make value change handling simpler.
        /// </summary>
        PerformAs = 3,
        /// <summary>
        /// Use Solid Shine UI's PerformAs extension method to raise a <c>{Property}Changed</c> event.
        /// </summary>
        PerformAsEventCall = 4,
    }
}
