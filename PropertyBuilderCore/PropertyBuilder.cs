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
        /// <returns>C# code that sets up a WPF DependencyProperty with the inputted values</returns>
        public static string BuildWpfProperty(string propName, string typeName, string ownerName, string? defVal = null, bool readOnly = false,
            WpfPropertyChangeHandler handleValueChanges = WpfPropertyChangeHandler.None)
        {
            // check if a default value is provided
            bool hasDefVal = !string.IsNullOrEmpty(defVal);
            bool needsMetadata = hasDefVal || handleValueChanges != WpfPropertyChangeHandler.None;

            StringBuilder sb = new();

            if (readOnly) // determine if this is a standard get-set property, or only a read-only property (private set)
            {
                // first, we want to generate the base C# property (the "{ get ...; set ...; }" part)
                sb.AppendLine($"public {typeName} {propName} {{ get => ({typeName})GetValue({propName}Property); private set => SetValue({propName}PropertyKey, value); }}"); // read-only C# property
                sb.AppendLine();
                // now we'll set up the read-only dependency property itself
                // we'll need a private DependencyPropertyKey for internal use, and then later we'll add a public-facing DependencyProperty
                sb.AppendLine($"private static readonly DependencyPropertyKey {propName}PropertyKey");                                                   // defining the internal dependency property key
                sb.AppendLine($"    = DependencyProperty.RegisterReadOnly(nameof({propName}), typeof({typeName}), typeof({ownerName})" + (needsMetadata ? "," : ");")); // register the dependency property
            }
            else
            {
                // first, we want to generate the base C# property (the "{ get ...; set ...; }" part)
                sb.AppendLine($"public {typeName} {propName} {{ get => ({typeName})GetValue({propName}Property); set => SetValue({propName}Property, value); }}"); // base C# property
                sb.AppendLine();
                // now we'll set up the dependency property itself
                // set up the XML comments for the dependency property, it's annoying to add later lol
                sb.AppendLine($"/// <summary>The backing dependency property for <see cref=\"{propName}\"/>. See the related property for details.</summary>"); // XML comment for dependency property
                sb.AppendLine($"public static DependencyProperty {propName}Property");                                                                          // defining the dependency property
                sb.AppendLine($"    = DependencyProperty.Register(nameof({propName}), typeof({typeName}), typeof({ownerName})" + (needsMetadata ? "," : ");")); // register the dependency property
            }

            // the FrameworkPropertyMetadata is used to define extra behaviors and other stuff for our property
            // here, we'll use it to define the default value (if one is provided) and also add a handler that gets triggered when the value changes
            // (technically, a PropertyMetadata is just fine, but the FrameworkPropertyMetadata offers the ability to set some extra flags if needed later)
            if (needsMetadata)
            {
                // we could technically get away without providing a default value, but here, I'll just set it to null
                // technically, the best practice would be to use default(typeName), but I've just gotten used to using null
                if (string.IsNullOrEmpty(defVal)) defVal = "null";

                sb.AppendLine($"    new FrameworkPropertyMetadata({defVal}" + GeneratePropertyChangedHandler() + "));");
            }

            if (readOnly) // now, after the actual read-only property was registered, we'll define the public-facing dependency property
            {
                sb.AppendLine();
                // set up the XML comments for the dependency property, it's annoying to add later lol
                sb.AppendLine($"/// <summary>The backing dependency property for <see cref=\"{propName}\"/>. See the related property for details.</summary>"); // XML comment for dependency property
                sb.AppendLine($"public static readonly DependencyProperty {propName}Property = {propName}PropertyKey.DependencyProperty;");  // define public dependency property for read-only access
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

                GeneratePropertyChangedEvent(ref sb);
            }
            else if (handleValueChanges == WpfPropertyChangeHandler.StandardRoutedEventCall)
            {
                // generate an OnPropertyChanged handler
                sb.AppendLine();
                sb.AppendLine($"private static void On{propName}Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)");
                sb.AppendLine("{");
                sb.AppendLine($"    if (d is {ownerName} o)");
                sb.AppendLine("    {");
                sb.AppendLine($"        RoutedPropertyChangedEventArgs<{typeName}> re = new RoutedPropertyChangedEventArgs<{typeName}>");
                sb.AppendLine($"            (({typeName})e.OldValue, ({typeName})e.NewValue, {propName}ChangedEvent);");
                sb.AppendLine("        re.Source = o;");
                sb.AppendLine("        o.RaiseEvent(re);");
                sb.AppendLine("    }");
                sb.AppendLine("}");

                GenerateRoutedPropertyChangedEvent(ref sb);
            }
            else if (handleValueChanges == WpfPropertyChangeHandler.Standard)
            {
                // generate an OnPropertyChanged handler
                sb.AppendLine();
                sb.AppendLine($"private static void On{propName}Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)");
                sb.AppendLine("{");
                sb.AppendLine($"    if (d is {ownerName} o)");
                sb.AppendLine("    {");
                sb.AppendLine("        ");
                sb.AppendLine("    }");
                sb.AppendLine("}");
            }
            else if (handleValueChanges == WpfPropertyChangeHandler.PerformAsEventCall)
            {
                GeneratePropertyChangedEvent(ref sb);
            }
            else if (handleValueChanges == WpfPropertyChangeHandler.PerformAsRoutedEventCall)
            {
                GenerateRoutedPropertyChangedEvent(ref sb);
                sb.AppendLine();
                sb.AppendLine($"private void On{propName}Changed(DependencyPropertyChangedEventArgs e)");
                sb.AppendLine("{");
                sb.AppendLine($"    RoutedPropertyChangedEventArgs<{typeName}> re = new RoutedPropertyChangedEventArgs<{typeName}>");
                sb.AppendLine($"        (({typeName})e.OldValue, ({typeName})e.NewValue, {propName}ChangedEvent);");
                sb.AppendLine("    re.Source = this;");
                sb.AppendLine("    RaiseEvent(re);");
                sb.AppendLine("}");
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
                    WpfPropertyChangeHandler.StandardRoutedEventCall => $", On{propName}Changed",
                    WpfPropertyChangeHandler.PerformAs => $", (d, e) => d.PerformAs<{ownerName}>((o) => )",
                    WpfPropertyChangeHandler.PerformAsEventCall => $", (d, e) => d.PerformAs<{ownerName}>((o) => o.{propName}Changed?.Invoke(o, e))",
                    WpfPropertyChangeHandler.PerformAsRoutedEventCall => $", (d, e) => d.PerformAs<{ownerName}>((o) => o.On{propName}Changed(e))",
                    _ => "",
                };
            }

            void GeneratePropertyChangedEvent(ref StringBuilder sb)
            {
                // generate the PropertyChanged event
                sb.AppendLine();
                sb.AppendLine("/// <summary>");
                sb.AppendLine($"/// Raised when the <see cref=\"{propName}\"/> property is changed.");
                sb.AppendLine("/// </summary>");
                sb.AppendLine("#if NETCOREAPP");
                sb.AppendLine($"    public event DependencyPropertyChangedEventHandler? {propName}Changed;");
                sb.AppendLine("#else");
                sb.AppendLine($"    public event DependencyPropertyChangedEventHandler {propName}Changed;");
                sb.AppendLine("#endif");
                // Only .NET Core / modern .NET supports nullability, where .NET Framework does not - this preprocessor directive makes that split
                // if you're not targetting both .NET and .NET Framework, or aren't using nullability, you can just remove this split and just use the one line
                // but I didn't want to add that on as another option when 1) this is primarily built for my use and 2) there's already SO MANY options/parameters here
            }

            void GenerateRoutedPropertyChangedEvent(ref StringBuilder sb)
            {
                // generate the PropertyChanged routed event
                sb.AppendLine();
                sb.AppendLine("/// <summary>");
                sb.AppendLine($"/// The backing routed event object for <see cref=\"{propName}Changed\"/>. Please see the related event for details.");
                sb.AppendLine("/// </summary>");
                sb.AppendLine($"public static readonly RoutedEvent {propName}ChangedEvent = EventManager.RegisterRoutedEvent(");
                sb.AppendLine($"    nameof({propName}Changed), RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<{typeName}>), typeof({ownerName}));");
                sb.AppendLine();
                // here is the C# event for this routed event
                sb.AppendLine("/// <summary>");
                sb.AppendLine($"/// Raised when the <see cref=\"{propName}\"/> property is changed.");
                sb.AppendLine("/// </summary>");
                sb.AppendLine($"public event RoutedPropertyChangedEventHandler<{typeName}> {propName}Changed");
                sb.AppendLine("{");
                sb.AppendLine($"    add {{ AddHandler({propName}ChangedEvent, value); }}");
                sb.AppendLine($"    remove {{ RemoveHandler({propName}ChangedEvent, value); }}");
                sb.AppendLine("}");
            }
        }

        /// <summary>
        /// Generate a WPF DependencyProperty, by writing the property code into a TextWriter.
        /// </summary>
        /// <param name="writer">the <see cref="TextWriter"/> to write to</param>
        /// <param name="propName">The name of the property</param>
        /// <param name="typeName">The property's type</param>
        /// <param name="ownerName">The type of the object that will own this property</param>
        /// <param name="defVal">a default value to set this property to, if desired (for strings, please include the enclosing quote characters (<c>"value"</c>))</param>
        /// <param name="handleValueChanges">whether to automatically include a handler for when this property's value changes</param>
        /// <param name="readOnly">whether this property is meant to be read-only; a private setter will be provided</param>
        public static void BuildWpfProperty(ref TextWriter writer, string propName, string typeName, string ownerName, string? defVal = null, bool readOnly = false,
            WpfPropertyChangeHandler handleValueChanges = WpfPropertyChangeHandler.None)
        {
            // check if a default value is provided
            bool hasDefVal = !string.IsNullOrEmpty(defVal);
            bool needsMetadata = hasDefVal || handleValueChanges != WpfPropertyChangeHandler.None;

            if (readOnly) // determine if this is a standard get-set property, or only a read-only property (private set)
            {
                // first, we want to generate the base C# property (the "{ get ...; set ...; }" part)
                writer.WriteLine($"public {typeName} {propName} {{ get => ({typeName})GetValue({propName}Property); private set => SetValue({propName}PropertyKey, value); }}"); // read-only C# property
                writer.WriteLine();
                // now we'll set up the read-only dependency property itself
                // we'll need a private DependencyPropertyKey for internal use, and then later we'll add a public-facing DependencyProperty
                writer.WriteLine($"private static readonly DependencyPropertyKey {propName}PropertyKey");                                                   // defining the internal dependency property key
                writer.WriteLine($"    = DependencyProperty.RegisterReadOnly(nameof({propName}), typeof({typeName}), typeof({ownerName})" + (needsMetadata ? "," : ");")); // register the dependency property
            }
            else
            {
                // first, we want to generate the base C# property (the "{ get ...; set ...; }" part)
                writer.WriteLine($"public {typeName} {propName} {{ get => ({typeName})GetValue({propName}Property); set => SetValue({propName}Property, value); }}"); // base C# property
                writer.WriteLine();
                // now we'll set up the dependency property itself
                // set up the XML comments for the dependency property, it's annoying to add later lol
                writer.WriteLine($"/// <summary>The backing dependency property for <see cref=\"{propName}\"/>. See the related property for details.</summary>"); // XML comment for dependency property
                writer.WriteLine($"public static DependencyProperty {propName}Property");                                                                          // defining the dependency property
                writer.WriteLine($"    = DependencyProperty.Register(nameof({propName}), typeof({typeName}), typeof({ownerName})" + (needsMetadata ? "," : ");")); // register the dependency property
            }

            // the FrameworkPropertyMetadata is used to define extra behaviors and other stuff for our property
            // here, we'll use it to define the default value (if one is provided) and also add a handler that gets triggered when the value changes
            // (technically, a PropertyMetadata is just fine, but the FrameworkPropertyMetadata offers the ability to set some extra flags if needed later)
            if (needsMetadata)
            {
                // we could technically get away without providing a default value, but here, I'll just set it to null
                // technically, the best practice would be to use default(typeName), but I've just gotten used to using null
                if (string.IsNullOrEmpty(defVal)) defVal = "null";

                writer.WriteLine($"    new FrameworkPropertyMetadata({defVal}" + GeneratePropertyChangedHandler() + "));");
            }

            if (readOnly) // now, after the actual read-only property was registered, we'll define the public-facing dependency property
            {
                writer.WriteLine();
                // set up the XML comments for the dependency property, it's annoying to add later lol
                writer.WriteLine($"/// <summary>The backing dependency property for <see cref=\"{propName}\"/>. See the related property for details.</summary>"); // XML comment for dependency property
                writer.WriteLine($"public static readonly DependencyProperty {propName}Property = {propName}PropertyKey.DependencyProperty;");  // define public dependency property for read-only access
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
                GeneratePropertyChangedEvent(ref writer);
            }
            else if (handleValueChanges == WpfPropertyChangeHandler.Standard)
            {
                // generate an OnPropertyChanged handler
                writer.WriteLine();
                writer.WriteLine($"private static void On{propName}Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)");
                writer.WriteLine("{");
                writer.WriteLine($"    if (d is {ownerName} o)");
                writer.WriteLine("    {");
                writer.WriteLine("        ");
                writer.WriteLine("    }");
                writer.WriteLine("}");
            }
            else if (handleValueChanges == WpfPropertyChangeHandler.StandardRoutedEventCall)
            {
                // generate an OnPropertyChanged handler
                writer.WriteLine();
                writer.WriteLine($"private static void On{propName}Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)");
                writer.WriteLine("{");
                writer.WriteLine($"    if (d is {ownerName} o)");
                writer.WriteLine("    {");
                writer.WriteLine($"        RoutedPropertyChangedEventArgs<{typeName}> re = new RoutedPropertyChangedEventArgs<{typeName}>");
                writer.WriteLine($"            (({typeName})e.OldValue, ({typeName})e.NewValue, {propName}ChangedEvent);");
                writer.WriteLine("        re.Source = o;");
                writer.WriteLine("        o.RaiseEvent(re);");
                writer.WriteLine("    }");
                writer.WriteLine("}");

                GenerateRoutedPropertyChangedEvent(ref writer);
            }
            else if (handleValueChanges == WpfPropertyChangeHandler.PerformAsEventCall)
            {
                // generate the PropertyChanged event
                GeneratePropertyChangedEvent(ref writer);
            }
            else if (handleValueChanges == WpfPropertyChangeHandler.PerformAsRoutedEventCall)
            {
                GenerateRoutedPropertyChangedEvent(ref writer);
                writer.WriteLine();
                writer.WriteLine($"private void On{propName}Changed(DependencyPropertyChangedEventArgs e)");
                writer.WriteLine("{");
                writer.WriteLine($"    RoutedPropertyChangedEventArgs<{typeName}> re = new RoutedPropertyChangedEventArgs<{typeName}>");
                writer.WriteLine($"        (({typeName})e.OldValue, ({typeName})e.NewValue, {propName}ChangedEvent);");
                writer.WriteLine("    re.Source = this;");
                writer.WriteLine("    RaiseEvent(re);");
                writer.WriteLine("}");
            }

            // internal helper for filling in the final part of the FrameworkPropertyMetadata
            string GeneratePropertyChangedHandler()
            {
                return handleValueChanges switch
                {
                    WpfPropertyChangeHandler.None => "",
                    WpfPropertyChangeHandler.Standard => $", On{propName}Changed",
                    WpfPropertyChangeHandler.StandardEventCall => $", On{propName}Changed",
                    WpfPropertyChangeHandler.StandardRoutedEventCall => $", On{propName}Changed",
                    WpfPropertyChangeHandler.PerformAs => $", (d, e) => d.PerformAs<{ownerName}>((o) => )",
                    WpfPropertyChangeHandler.PerformAsEventCall => $", (d, e) => d.PerformAs<{ownerName}>((o) => o.{propName}Changed?.Invoke(o, e))",
                    WpfPropertyChangeHandler.PerformAsRoutedEventCall => $", (d, e) => d.PerformAs<{ownerName}>((o) => o.On{propName}Changed(e))",
                    _ => "",
                };
            }

            void GeneratePropertyChangedEvent(ref TextWriter tw)
            {
                // generate the PropertyChanged event
                tw.WriteLine();
                tw.WriteLine("/// <summary>");
                tw.WriteLine($"/// Raised when the <see cref=\"{propName}\"/> property is changed.");
                tw.WriteLine("/// </summary>");
                tw.WriteLine("#if NETCOREAPP");
                tw.WriteLine($"    public event DependencyPropertyChangedEventHandler? {propName}Changed;");
                tw.WriteLine("#else");
                tw.WriteLine($"    public event DependencyPropertyChangedEventHandler {propName}Changed;");
                tw.WriteLine("#endif");
                // Only .NET Core / modern .NET supports nullability, where .NET Framework does not - this preprocessor directive makes that split
                // if you're not targetting both .NET and .NET Framework, or aren't using nullability, you can just remove this split and just use the one line
                // but I didn't want to add that on as another option when 1) this is primarily built for my use and 2) there's already SO MANY options/parameters here
            }

            void GenerateRoutedPropertyChangedEvent(ref TextWriter tw)
            {
                // generate the PropertyChanged routed event
                tw.WriteLine();
                tw.WriteLine("/// <summary>");
                tw.WriteLine($"/// The backing routed event object for <see cref=\"{propName}Changed\"/>. Please see the related event for details.");
                tw.WriteLine("/// </summary>");
                tw.WriteLine($"public static readonly RoutedEvent {propName}ChangedEvent = EventManager.RegisterRoutedEvent(");
                tw.WriteLine($"    nameof({propName}Changed), RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<{typeName}>), typeof({ownerName}));");
                tw.WriteLine();
                tw.WriteLine("/// <summary>");
                tw.WriteLine($"/// Raised when the <see cref=\"{propName}\"/> property is changed.");
                tw.WriteLine("/// </summary>");
                tw.WriteLine($"public event RoutedPropertyChangedEventHandler<{typeName}> {propName}Changed");
                tw.WriteLine("{");
                tw.WriteLine($"    add {{ AddHandler({propName}ChangedEvent, value); }}");
                tw.WriteLine($"    remove {{ RemoveHandler({propName}ChangedEvent, value); }}");
                tw.WriteLine("}");
            }
        }

        /// <summary>
        /// Generate an Avalonia StyledProperty. Use <see cref="BuildAvaloniaDirectProperty(string, string, string, string?, string?, bool)"/> for a DirectProperty.
        /// </summary>
        /// <param name="propName">The name of the property</param>
        /// <param name="typeName">The property's type</param>
        /// <param name="ownerName">The type of the object that will own this property (generally, the class this property is being added into)</param>
        /// <param name="defVal">a default value to set this property to, if desired (for strings, please include the enclosing quote characters (<c>"value"</c>))</param>
        /// <returns>C# code that sets up an Avalonia StyledProperty with the inputted values</returns>
        /// <remarks>
        /// To handle value changes, use the DirectProperty's <c>Changed</c> property, or override the protected method <c>OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)</c>.
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
        /// Generate an Avalonia StyledProperty, by writing the property code into a TextWriter. 
        /// Use <see cref="BuildAvaloniaDirectProperty(ref TextWriter, string, string, string, string?, string?, bool)"/> for a DirectProperty.
        /// </summary>
        /// <param name="writer">the <see cref="TextWriter"/> to write to</param>
        /// <param name="propName">The name of the property</param>
        /// <param name="typeName">The property's type</param>
        /// <param name="ownerName">The type of the object that will own this property (usually, the class this property is being added into)</param>
        /// <param name="defVal">a default value to set this property to, if desired (for strings, please include the enclosing quote characters (<c>"value"</c>))</param>
        /// <remarks>
        /// To handle value changes, use the DirectProperty's <c>Changed</c> property, or override the protected method <c>OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)</c>.
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
        /// <param name="ownerName">The type of the object that will own this property (usually, the class this property is being added into)</param>
        /// <param name="fieldName">the name to use for the internal field; keep this as <c>null</c> to generate a name based upon <paramref name="propName"/></param>
        /// <param name="readOnly">set if this property is only meant to be read-only; a private setter will be provided</param>
        /// <param name="defVal">a default value to set this property to, if desired (for strings, please include the enclosing quote characters (<c>"value"</c>))</param>
        /// <returns>C# code that sets up an Avalonia DirectProperty with the inputted values</returns>
        /// <remarks>
        /// To handle value changes, use the DirectProperty's <c>Changed</c> property, or override the protected method <c>OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)</c>.
        /// To see the difference between StyledProperties and DirectProperties, see <a href="https://docs.avaloniaui.net/docs/guides/custom-controls/how-to-create-advanced-custom-controls" 
        /// >this page in Avalonia's documentation</a>.
        /// </remarks>
        public static string BuildAvaloniaDirectProperty(string propName, string typeName, string ownerName, string? defVal = null, string? fieldName = null, bool readOnly = false)
        {
            // generate a fieldName if one isn't put in - this field name is done by taking the property name and lowercasing the first letter
            if (string.IsNullOrEmpty(propName))
            {
                fieldName = "propertyField";
            }
            else
            {
                fieldName ??= string.Concat(propName[..1].ToLowerInvariant(), propName.AsSpan(1));
            }

            StringBuilder sb = new();
            sb.AppendLine($"private {typeName} _{fieldName}" + (string.IsNullOrEmpty(defVal) ? ";" : $" = {defVal};"));
            sb.AppendLine();
            sb.AppendLine($"public {typeName} {propName} {{ get => _{fieldName}; " + (readOnly ? "private" : "") + $" set => SetAndRaise({propName}Property, ref _{fieldName}, value); }}");
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
        /// Generate an Avalonia DirectProperty, by writing the property code into a TextWriter.
        /// </summary>
        /// <param name="writer">the <see cref="TextWriter"/> to write to</param>
        /// <param name="propName">The name of the property</param>
        /// <param name="typeName">The property's type</param>
        /// <param name="ownerName">The type of the object that will own this property (usually, the class this property is being added into)</param>
        /// <param name="fieldName">the name to use for the internal field; keep this as <c>null</c> to generate a name based upon <paramref name="propName"/></param>
        /// <param name="readOnly">set if this property is only meant to be read-only; a private setter will be provided</param>
        /// <param name="defVal">a default value to set this property to, if desired (for strings, please include the enclosing quote characters (<c>"value"</c>))</param>
        /// <remarks>
        /// To handle value changes, use the DirectProperty's <c>Changed</c> property, or override the protected method <c>OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)</c>.
        /// To see the difference between StyledProperties and DirectProperties, see <a href="https://docs.avaloniaui.net/docs/guides/custom-controls/how-to-create-advanced-custom-controls" 
        /// >this page in Avalonia's documentation</a>.
        /// </remarks>
        public static void BuildAvaloniaDirectProperty(ref TextWriter writer, string propName, string typeName, string ownerName, string? defVal = null,
            string? fieldName = null, bool readOnly = false)
        {
            // generate a fieldName if one isn't put in - this field name is done by taking the property name and lowercasing the first letter
            if (string.IsNullOrEmpty(propName))
            {
                fieldName = "propertyField";
            }
            else
            {
                fieldName ??= string.Concat(propName[..1].ToLowerInvariant(), propName.AsSpan(1));
            }

            // start with writing the internal field
            writer.WriteLine($"private {typeName} _{fieldName}" + (string.IsNullOrEmpty(defVal) ? ";" : $" = {defVal};"));
            writer.WriteLine();
            // now, the C# property
            writer.WriteLine($"public {typeName} {propName} {{ get => _{fieldName}; " + (readOnly ? "private" : "") + $" set => SetAndRaise({propName}Property, ref _{fieldName}, value); }}");
            writer.WriteLine();
            // finally, let's set up and register the direct property
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
    /// These handlers are used to execute code (and/or raise events) when the property's value has changed. The same results can be achieved with all of these,
    /// the only difference being in semantics and structure. 
    /// <para/>
    /// For Solid Shine UI 2.0, I made a small extension method named PerformAs which helps to generate cleaner, shorter code.
    /// See this code file for the PerformAs extension method:
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
        /// Generate a <c>On{Property}Changed</c> handler, that raises a <c>{Property}Changed</c> routed event.
        /// </summary>
        StandardRoutedEventCall = 3,
        /// <summary>
        /// Use Solid Shine UI's PerformAs extension method, to make value change handling simpler.
        /// </summary>
        PerformAs = 4,
        /// <summary>
        /// Use Solid Shine UI's PerformAs extension method to raise a <c>{Property}Changed</c> event.
        /// </summary>
        PerformAsEventCall = 5,
        /// <summary>
        /// Use Solid Shine UI's PerformAs extension method to raise a <c>{Property}Changed</c> routed event.
        /// </summary>
        PerformAsRoutedEventCall = 6,
    }
}
