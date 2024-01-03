
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using SolidShineUi;

namespace PropertyBuilderWPF
{
    /// <summary>
    /// A control/page for the creation of multiple properties at once.
    /// </summary>
    public partial class BulkPropertyGenerator : UserControl
    {
        Binding OwnerTypeBinding;
        Binding PropertyTypeBinding;

        Binding fontFamilyBinding;
        Binding fontSizeBinding;
        Binding fontStyleBinding;
        Binding fontWeightBinding;
        Binding lineNumbersBinding;
        Binding wordWrapBinding;
        Binding highlightBinding;
        Binding syntaxColorBinding;

        public BulkPropertyGenerator()
        {
            InitializeComponent();

            OwnerTypeBinding = new Binding(nameof(TextBox.Text)) { Source = txtOwnerType };
            PropertyTypeBinding = new Binding(nameof(Selector.SelectedIndex)) { Source = cbbPropertyType };

            fontFamilyBinding = new(nameof(EditorFontFamily)) { Source = this };
            fontSizeBinding = new(nameof(EditorFontSize)) { Source = this };
            fontStyleBinding = new(nameof(EditorFontStyle)) { Source = this };
            fontWeightBinding = new(nameof(EditorFontWeight)) { Source = this };
            lineNumbersBinding = new(nameof(ShowLineNumbers)) { Source = this };
            wordWrapBinding = new(nameof(WordWrap)) { Source = this };
            highlightBinding = new(nameof(HighlightCurrentLine)) { Source = this };
            syntaxColorBinding = new(nameof(UseSyntaxHighlighting)) { Source = this };
        }

        public ColorScheme ColorScheme { get => (ColorScheme)GetValue(ColorSchemeProperty); set => SetValue(ColorSchemeProperty, value); }

        /// <summary>The backing dependency property for <see cref="ColorScheme"/>. See the related property for details.</summary>
        public static DependencyProperty ColorSchemeProperty
            = DependencyProperty.Register(nameof(ColorScheme), typeof(ColorScheme), typeof(BulkPropertyGenerator),
            new FrameworkPropertyMetadata(new ColorScheme()));

        void AddPropertyItem()
        {
            BulkPropertyItem bpi = new BulkPropertyItem();

            bpi.SetBinding(BulkPropertyItem.OwnerNameProperty, OwnerTypeBinding);
            bpi.SetBinding(BulkPropertyItem.GenerationTypeProperty, PropertyTypeBinding);

            bpi.SetBinding(BulkPropertyItem.EditorFontFamilyProperty, fontFamilyBinding);
            bpi.SetBinding(BulkPropertyItem.EditorFontSizeProperty, fontSizeBinding);
            bpi.SetBinding(BulkPropertyItem.EditorFontStyleProperty, fontStyleBinding);
            bpi.SetBinding(BulkPropertyItem.EditorFontWeightProperty, fontWeightBinding);
            bpi.SetBinding(BulkPropertyItem.ShowLineNumbersProperty, lineNumbersBinding);
            bpi.SetBinding(BulkPropertyItem.WordWrapProperty, wordWrapBinding);
            bpi.SetBinding(BulkPropertyItem.HighlightCurrentLineProperty, highlightBinding);
            bpi.SetBinding(BulkPropertyItem.UseSyntaxHighlightingProperty, syntaxColorBinding);

            selItems.Items.Add(bpi);
            bpi.RemoveRequested += (d, e) => selItems.Items.Remove(bpi);
        }

        private void FlatButton_Click(object sender, RoutedEventArgs e)
        {
            AddPropertyItem();
        }

        private void btnCopyAll_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (var item in selItems.Items)
            {
                if (item is BulkPropertyItem bpi)
                {
                    if (!first) sb.AppendLine();
                    sb.AppendLine(bpi.GetEditorText());
                    first = false;
                }
            }

            Clipboard.SetText(sb.ToString());
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            selItems.Items.Clear();
        }


        #region Code Editor Visuals
        public bool ShowLineNumbers { get => (bool)GetValue(ShowLineNumbersProperty); set => SetValue(ShowLineNumbersProperty, value); }

        /// <summary>The backing dependency property for <see cref="ShowLineNumbers"/>. See the related property for details.</summary>
        public static DependencyProperty ShowLineNumbersProperty
            = DependencyProperty.Register("ShowLineNumbers", typeof(bool), typeof(BulkPropertyGenerator),
            new FrameworkPropertyMetadata(true));

        public FontFamily EditorFontFamily { get => (FontFamily)GetValue(EditorFontFamilyProperty); set => SetValue(EditorFontFamilyProperty, value); }

        /// <summary>The backing dependency property for <see cref="FontFamily"/>. See the related property for details.</summary>
        public static DependencyProperty EditorFontFamilyProperty
            = DependencyProperty.Register("EditorFontFamily", typeof(FontFamily), typeof(BulkPropertyGenerator),
            new FrameworkPropertyMetadata(new FontFamily("Consolas")));

        public double EditorFontSize { get => (double)GetValue(EditorFontSizeProperty); set => SetValue(EditorFontSizeProperty, value); }

        /// <summary>The backing dependency property for <see cref="EditorFontSize"/>. See the related property for details.</summary>
        public static DependencyProperty EditorFontSizeProperty
            = DependencyProperty.Register("EditorFontSize", typeof(double), typeof(BulkPropertyGenerator),
            new FrameworkPropertyMetadata(12.0));

        public FontWeight EditorFontWeight { get => (FontWeight)GetValue(EditorFontWeightProperty); set => SetValue(EditorFontWeightProperty, value); }

        /// <summary>The backing dependency property for <see cref="EditorFontWeight"/>. See the related property for details.</summary>
        public static DependencyProperty EditorFontWeightProperty
            = DependencyProperty.Register("EditorFontWeight", typeof(FontWeight), typeof(BulkPropertyGenerator),
            new FrameworkPropertyMetadata(FontWeights.Normal));

        public FontStyle EditorFontStyle { get => (FontStyle)GetValue(EditorFontStyleProperty); set => SetValue(EditorFontStyleProperty, value); }

        /// <summary>The backing dependency property for <see cref="EditorFontStyle"/>. See the related property for details.</summary>
        public static DependencyProperty EditorFontStyleProperty
            = DependencyProperty.Register("EditorFontStyle", typeof(FontStyle), typeof(BulkPropertyGenerator),
            new FrameworkPropertyMetadata(FontStyles.Normal));

        public bool WordWrap { get => (bool)GetValue(WordWrapProperty); set => SetValue(WordWrapProperty, value); }

        /// <summary>The backing dependency property for <see cref="WordWrap"/>. See the related property for details.</summary>
        public static DependencyProperty WordWrapProperty
            = DependencyProperty.Register("WordWrap", typeof(bool), typeof(BulkPropertyGenerator),
            new FrameworkPropertyMetadata(false));

        public bool UseSyntaxHighlighting { get => (bool)GetValue(UseSyntaxHighlightingProperty); set => SetValue(UseSyntaxHighlightingProperty, value); }

        /// <summary>The backing dependency property for <see cref="UseSyntaxHighlighting"/>. See the related property for details.</summary>
        public static DependencyProperty UseSyntaxHighlightingProperty
            = DependencyProperty.Register("UseSyntaxHighlighting", typeof(bool), typeof(BulkPropertyGenerator),
            new FrameworkPropertyMetadata(true));

        public bool HighlightCurrentLine { get => (bool)GetValue(HighlightCurrentLineProperty); set => SetValue(HighlightCurrentLineProperty, value); }

        /// <summary>The backing dependency property for <see cref="HighlightCurrentLine"/>. See the related property for details.</summary>
        public static DependencyProperty HighlightCurrentLineProperty
            = DependencyProperty.Register(nameof(HighlightCurrentLine), typeof(bool), typeof(BulkPropertyGenerator),
            new FrameworkPropertyMetadata(false));


        #endregion
    }
}
