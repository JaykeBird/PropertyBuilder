using SolidShineUi;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using static PropertyBuilder.PropertyBuilder;

namespace PropertyBuilderWPF
{
    /// <summary>
    /// A control/page for generating a WPF DepedencyProperty or Avalonia StyledProperty/DirectProperty.
    /// </summary>
    public partial class MainPropertyGenerator : UserControl
    {
        public MainPropertyGenerator()
        {
            InitializeComponent();
        }

        public ColorScheme ColorScheme { get => (ColorScheme)GetValue(ColorSchemeProperty); set => SetValue(ColorSchemeProperty, value); }

        /// <summary>The backing dependency property for <see cref="ColorScheme"/>. See the related property for details.</summary>
        public static DependencyProperty ColorSchemeProperty
            = DependencyProperty.Register("ColorScheme", typeof(ColorScheme), typeof(MainPropertyGenerator),
            new FrameworkPropertyMetadata(new ColorScheme()));

        #region Code Editor Visuals
        public bool ShowLineNumbers { get => (bool)GetValue(ShowLineNumbersProperty); set => SetValue(ShowLineNumbersProperty, value); }

        /// <summary>The backing dependency property for <see cref="ShowLineNumbers"/>. See the related property for details.</summary>
        public static DependencyProperty ShowLineNumbersProperty
            = DependencyProperty.Register("ShowLineNumbers", typeof(bool), typeof(MainPropertyGenerator),
            new FrameworkPropertyMetadata(true));

        public FontFamily EditorFontFamily { get => (FontFamily)GetValue(EditorFontFamilyProperty); set => SetValue(EditorFontFamilyProperty, value); }

        /// <summary>The backing dependency property for <see cref="FontFamily"/>. See the related property for details.</summary>
        public static DependencyProperty EditorFontFamilyProperty
            = DependencyProperty.Register("EditorFontFamily", typeof(FontFamily), typeof(MainPropertyGenerator),
            new FrameworkPropertyMetadata(new FontFamily("Consolas")));

        public double EditorFontSize { get => (double)GetValue(EditorFontSizeProperty); set => SetValue(EditorFontSizeProperty, value); }

        /// <summary>The backing dependency property for <see cref="EditorFontSize"/>. See the related property for details.</summary>
        public static DependencyProperty EditorFontSizeProperty
            = DependencyProperty.Register("EditorFontSize", typeof(double), typeof(MainPropertyGenerator),
            new FrameworkPropertyMetadata(12.0));

        public FontWeight EditorFontWeight { get => (FontWeight)GetValue(EditorFontWeightProperty); set => SetValue(EditorFontWeightProperty, value); }

        /// <summary>The backing dependency property for <see cref="EditorFontWeight"/>. See the related property for details.</summary>
        public static DependencyProperty EditorFontWeightProperty
            = DependencyProperty.Register("EditorFontWeight", typeof(FontWeight), typeof(MainPropertyGenerator),
            new FrameworkPropertyMetadata(FontWeights.Normal));

        public FontStyle EditorFontStyle { get => (FontStyle)GetValue(EditorFontStyleProperty); set => SetValue(EditorFontStyleProperty, value); }

        /// <summary>The backing dependency property for <see cref="EditorFontStyle"/>. See the related property for details.</summary>
        public static DependencyProperty EditorFontStyleProperty
            = DependencyProperty.Register("EditorFontStyle", typeof(FontStyle), typeof(MainPropertyGenerator),
            new FrameworkPropertyMetadata(FontStyles.Normal));

        public bool WordWrap { get => (bool)GetValue(WordWrapProperty); set => SetValue(WordWrapProperty, value); }

        /// <summary>The backing dependency property for <see cref="WordWrap"/>. See the related property for details.</summary>
        public static DependencyProperty WordWrapProperty
            = DependencyProperty.Register("WordWrap", typeof(bool), typeof(MainPropertyGenerator),
            new FrameworkPropertyMetadata(false));

        public bool UseSyntaxHighlighting { get => (bool)GetValue(UseSyntaxHighlightingProperty); set => SetValue(UseSyntaxHighlightingProperty, value); }

        /// <summary>The backing dependency property for <see cref="UseSyntaxHighlighting"/>. See the related property for details.</summary>
        public static DependencyProperty UseSyntaxHighlightingProperty
            = DependencyProperty.Register("UseSyntaxHighlighting", typeof(bool), typeof(MainPropertyGenerator),
            new FrameworkPropertyMetadata(true));

        public bool HighlightCurrentLine { get => (bool)GetValue(HighlightCurrentLineProperty); set => SetValue(HighlightCurrentLineProperty, value); }

        /// <summary>The backing dependency property for <see cref="HighlightCurrentLine"/>. See the related property for details.</summary>
        public static DependencyProperty HighlightCurrentLineProperty
            = DependencyProperty.Register(nameof(HighlightCurrentLine), typeof(bool), typeof(MainPropertyGenerator),
            new FrameworkPropertyMetadata(false));


        #endregion

        void GenerateProperty()
        {
            string wpfProp = BuildWpfProperty(txtName.Text, txtType.Text, txtOwner.Text, txtDefault.Text, chkWpfReadOnly.IsChecked,
                cbbWpfChangeHandler.SelectedEnumValueAsEnum<PropertyBuilder.WpfPropertyChangeHandler>());

            string avaloniaProp = cbbAvaloniaType.SelectedEnumValueAsEnum<AvaloniaPropertyType>() switch
            {
                AvaloniaPropertyType.StyledProperty => BuildAvaloniaProperty(txtName.Text, txtType.Text, txtOwner.Text, txtDefault.Text),
                AvaloniaPropertyType.DirectProperty => BuildAvaloniaDirectProperty(txtName.Text, txtType.Text, txtOwner.Text, txtDefault.Text, null, false),
                AvaloniaPropertyType.DirectAsReadOnly => BuildAvaloniaDirectProperty(txtName.Text, txtType.Text, txtOwner.Text, txtDefault.Text, null, true),
                _ => BuildAvaloniaProperty(txtName.Text, txtType.Text, txtOwner.Text, txtDefault.Text)
            };

            edtWpf.CodeText = wpfProp;
            edtAvalonia.CodeText = avaloniaProp;
        }

        private void txtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (edtWpf == null) return;
            GenerateProperty();
        }

        private void chkWpfReadOnly_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (edtWpf == null) return;
            GenerateProperty();
        }

        private void cbbWpfChangeHandler_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (edtWpf == null) return;
            GenerateProperty();
        }

        private void cbbAvaloniaType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (edtAvalonia == null) return;
            GenerateProperty();
        }
    }
}
