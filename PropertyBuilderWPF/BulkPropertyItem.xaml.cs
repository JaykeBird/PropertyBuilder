using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SolidShineUi;

using static PropertyBuilder.PropertyBuilder;

namespace PropertyBuilderWPF
{
    /// <summary>
    /// A control to use as a part of <see cref="BulkPropertyGenerator"/>.
    /// </summary>
    public partial class BulkPropertyItem : SelectableUserControl
    {
        public BulkPropertyItem()
        {
            InitializeComponent();
        }

        public override void ApplyColorScheme(ColorScheme cs)
        {
            base.ApplyColorScheme(cs);

            viewText.ColorScheme = cs;
            btnRemove.ColorScheme = cs;
        }

        public string OwnerName { get => (string)GetValue(OwnerNameProperty); set => SetValue(OwnerNameProperty, value); }

        /// <summary>The backing dependency property for <see cref="OwnerName"/>. See the related property for details.</summary>
        public static DependencyProperty OwnerNameProperty
            = DependencyProperty.Register(nameof(OwnerName), typeof(string), typeof(BulkPropertyItem),
            new FrameworkPropertyMetadata("MyType", OnPropertyChanged));

        public int GenerationType { get => (int)GetValue(GenerationTypeProperty); set => SetValue(GenerationTypeProperty, value); }

        /// <summary>The backing dependency property for <see cref="GenerationType"/>. See the related property for details.</summary>
        public static DependencyProperty GenerationTypeProperty
            = DependencyProperty.Register(nameof(GenerationType), typeof(int), typeof(BulkPropertyItem),
            new FrameworkPropertyMetadata(0, OnPropertyChanged));


        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BulkPropertyItem b)
            {
                b.GenerateProperty();
            }
        }

        void GenerateProperty()
        {
            string propertyValue = GenerationType switch
            {
                0 => BuildWpfProperty(txtName.Text, txtType.Text, OwnerName, txtDefaultValue.Text, false),
                1 => BuildAvaloniaProperty(txtName.Text, txtType.Text, OwnerName, txtDefaultValue.Text),
                2 => BuildAvaloniaDirectProperty(txtName.Text, txtType.Text, OwnerName, txtDefaultValue.Text),
                _ => BuildWpfProperty(txtName.Text, txtType.Text, OwnerName, txtDefaultValue.Text, false)
            };

            viewText.CodeText = propertyValue;
        }

        public string GetEditorText() => viewText.GetEditorText();


        #region Code Editor Visuals
        public bool ShowLineNumbers { get => (bool)GetValue(ShowLineNumbersProperty); set => SetValue(ShowLineNumbersProperty, value); }

        /// <summary>The backing dependency property for <see cref="ShowLineNumbers"/>. See the related property for details.</summary>
        public static DependencyProperty ShowLineNumbersProperty
            = DependencyProperty.Register("ShowLineNumbers", typeof(bool), typeof(BulkPropertyItem),
            new FrameworkPropertyMetadata(true));

        public FontFamily EditorFontFamily { get => (FontFamily)GetValue(EditorFontFamilyProperty); set => SetValue(EditorFontFamilyProperty, value); }

        /// <summary>The backing dependency property for <see cref="FontFamily"/>. See the related property for details.</summary>
        public static DependencyProperty EditorFontFamilyProperty
            = DependencyProperty.Register("EditorFontFamily", typeof(FontFamily), typeof(BulkPropertyItem),
            new FrameworkPropertyMetadata(new FontFamily("Consolas")));

        public double EditorFontSize { get => (double)GetValue(EditorFontSizeProperty); set => SetValue(EditorFontSizeProperty, value); }

        /// <summary>The backing dependency property for <see cref="EditorFontSize"/>. See the related property for details.</summary>
        public static DependencyProperty EditorFontSizeProperty
            = DependencyProperty.Register("EditorFontSize", typeof(double), typeof(BulkPropertyItem),
            new FrameworkPropertyMetadata(12.0));

        public FontWeight EditorFontWeight { get => (FontWeight)GetValue(EditorFontWeightProperty); set => SetValue(EditorFontWeightProperty, value); }

        /// <summary>The backing dependency property for <see cref="EditorFontWeight"/>. See the related property for details.</summary>
        public static DependencyProperty EditorFontWeightProperty
            = DependencyProperty.Register("EditorFontWeight", typeof(FontWeight), typeof(BulkPropertyItem),
            new FrameworkPropertyMetadata(FontWeights.Normal));

        public FontStyle EditorFontStyle { get => (FontStyle)GetValue(EditorFontStyleProperty); set => SetValue(EditorFontStyleProperty, value); }

        /// <summary>The backing dependency property for <see cref="EditorFontStyle"/>. See the related property for details.</summary>
        public static DependencyProperty EditorFontStyleProperty
            = DependencyProperty.Register("EditorFontStyle", typeof(FontStyle), typeof(BulkPropertyItem),
            new FrameworkPropertyMetadata(FontStyles.Normal));

        public bool WordWrap { get => (bool)GetValue(WordWrapProperty); set => SetValue(WordWrapProperty, value); }

        /// <summary>The backing dependency property for <see cref="WordWrap"/>. See the related property for details.</summary>
        public static DependencyProperty WordWrapProperty
            = DependencyProperty.Register("WordWrap", typeof(bool), typeof(BulkPropertyItem),
            new FrameworkPropertyMetadata(false));

        public bool UseSyntaxHighlighting { get => (bool)GetValue(UseSyntaxHighlightingProperty); set => SetValue(UseSyntaxHighlightingProperty, value); }

        /// <summary>The backing dependency property for <see cref="UseSyntaxHighlighting"/>. See the related property for details.</summary>
        public static DependencyProperty UseSyntaxHighlightingProperty
            = DependencyProperty.Register("UseSyntaxHighlighting", typeof(bool), typeof(BulkPropertyItem),
            new FrameworkPropertyMetadata(true));

        public bool HighlightCurrentLine { get => (bool)GetValue(HighlightCurrentLineProperty); set => SetValue(HighlightCurrentLineProperty, value); }

        /// <summary>The backing dependency property for <see cref="HighlightCurrentLine"/>. See the related property for details.</summary>
        public static DependencyProperty HighlightCurrentLineProperty
            = DependencyProperty.Register(nameof(HighlightCurrentLine), typeof(bool), typeof(BulkPropertyItem),
            new FrameworkPropertyMetadata(false));


        #endregion


        public event EventHandler? RemoveRequested;

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            RemoveRequested?.Invoke(this, e);
        }

        private void txtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            GenerateProperty();
        }
    }
}
