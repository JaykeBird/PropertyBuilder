using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.ILSpy;
using PropertyBuilderWPF.Highlighting;
using SolidShineUi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PropertyBuilderWPF
{
    /// <summary>
    /// Interaction logic for CodeDisplay.xaml
    /// </summary>
    public partial class CodeDisplay : UserControl
    {
        public CodeDisplay()
        {
            InitializeComponent();

            Loaded += CodeDisplay_Loaded;


            bhr = new BracketHighlightRenderer(editText.TextArea.TextView);
            cbr = new CSharpBracketSearcher();

            editText.TextArea.TextView.BackgroundRenderers.Add(bhr);
            editText.TextArea.Caret.PositionChanged += editText_PositionChanged;
            editText.Options.HighlightCurrentLine = HighlightCurrentLine;
            SetSyntaxHighlighting();
        }

        //public DocumentTextWriter TextWriter { get; private set; }

        private BracketHighlightRenderer bhr;
        private CSharpBracketSearcher cbr;

        public void ClearText()
        {
            editText.Text = "";
        }

        private void CodeDisplay_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;
            //throw new NotImplementedException();
            SetSyntaxHighlighting();
        }

        private void editText_PositionChanged(object? sender, EventArgs e)
        {
            var result = cbr.SearchBracket(editText.Document, editText.CaretOffset);
            bhr.SetHighlight(result);
        }

        /// <summary>
        /// Get or set the color scheme to apply to the visuals of this control.
        /// </summary>
        public ColorScheme ColorScheme { get => (ColorScheme)GetValue(ColorSchemeProperty); set => SetValue(ColorSchemeProperty, value); }

        /// <summary>The backing dependency property for <see cref="ColorScheme"/>. See the related property for details.</summary>
        public static readonly DependencyProperty ColorSchemeProperty
            = DependencyProperty.Register("ColorScheme", typeof(ColorScheme), typeof(CodeDisplay),
            new FrameworkPropertyMetadata(new ColorScheme()));

        /// <summary>
        /// Get or set the text to display within the code display.
        /// </summary>
        public string CodeText { get => (string)GetValue(CodeTextProperty); set => SetValue(CodeTextProperty, value); }

        /// <summary>The backing dependency property for <see cref="CodeText"/>. See the related property for details.</summary>
        public static readonly DependencyProperty CodeTextProperty
            = DependencyProperty.Register("CodeText", typeof(string), typeof(CodeDisplay),
            new FrameworkPropertyMetadata("", OnCodeTextChange));

        private static void OnCodeTextChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CodeDisplay cd)
            {
                cd.editText.Text = cd.CodeText;
            }
        }

        public string GetEditorText() => editText.Text;

        public bool ShowGenerateMenu { get => (bool)GetValue(ShowGenerateMenuProperty); set => SetValue(ShowGenerateMenuProperty, value); }

        /// <summary>The backing dependency property for <see cref="ShowGenerateMenu"/>. See the related property for details.</summary>
        public static readonly DependencyProperty ShowGenerateMenuProperty
            = DependencyProperty.Register("ShowGenerateMenu", typeof(bool), typeof(CodeDisplay),
            new FrameworkPropertyMetadata(false));

        #region Code Editor Visuals
        public bool ShowLineNumbers { get => (bool)GetValue(ShowLineNumbersProperty); set => SetValue(ShowLineNumbersProperty, value); }

        /// <summary>The backing dependency property for <see cref="ShowLineNumbers"/>. See the related property for details.</summary>
        public static DependencyProperty ShowLineNumbersProperty
            = DependencyProperty.Register("ShowLineNumbers", typeof(bool), typeof(CodeDisplay),
            new FrameworkPropertyMetadata(true));

        public FontFamily EditorFontFamily { get => (FontFamily)GetValue(EditorFontFamilyProperty); set => SetValue(EditorFontFamilyProperty, value); }

        /// <summary>The backing dependency property for <see cref="FontFamily"/>. See the related property for details.</summary>
        public static DependencyProperty EditorFontFamilyProperty
            = DependencyProperty.Register("EditorFontFamily", typeof(FontFamily), typeof(CodeDisplay),
            new FrameworkPropertyMetadata(new FontFamily("Consolas")));

        public double EditorFontSize { get => (double)GetValue(EditorFontSizeProperty); set => SetValue(EditorFontSizeProperty, value); }

        /// <summary>The backing dependency property for <see cref="EditorFontSize"/>. See the related property for details.</summary>
        public static DependencyProperty EditorFontSizeProperty
            = DependencyProperty.Register("EditorFontSize", typeof(double), typeof(CodeDisplay),
            new FrameworkPropertyMetadata(12.0));

        public FontWeight EditorFontWeight { get => (FontWeight)GetValue(EditorFontWeightProperty); set => SetValue(EditorFontWeightProperty, value); }

        /// <summary>The backing dependency property for <see cref="EditorFontWeight"/>. See the related property for details.</summary>
        public static DependencyProperty EditorFontWeightProperty
            = DependencyProperty.Register("EditorFontWeight", typeof(FontWeight), typeof(CodeDisplay),
            new FrameworkPropertyMetadata(FontWeights.Normal));
        
        public FontStyle EditorFontStyle { get => (FontStyle)GetValue(EditorFontStyleProperty); set => SetValue(EditorFontStyleProperty, value); }

        /// <summary>The backing dependency property for <see cref="EditorFontStyle"/>. See the related property for details.</summary>
        public static DependencyProperty EditorFontStyleProperty
            = DependencyProperty.Register("EditorFontStyle", typeof(FontStyle), typeof(CodeDisplay),
            new FrameworkPropertyMetadata(FontStyles.Normal));

        public bool WordWrap { get => (bool)GetValue(WordWrapProperty); set => SetValue(WordWrapProperty, value); }

        /// <summary>The backing dependency property for <see cref="WordWrap"/>. See the related property for details.</summary>
        public static DependencyProperty WordWrapProperty
            = DependencyProperty.Register("WordWrap", typeof(bool), typeof(CodeDisplay),
            new FrameworkPropertyMetadata(false));

        public bool UseSyntaxHighlighting { get => (bool)GetValue(UseSyntaxHighlightingProperty); set => SetValue(UseSyntaxHighlightingProperty, value); }

        /// <summary>The backing dependency property for <see cref="UseSyntaxHighlighting"/>. See the related property for details.</summary>
        public static DependencyProperty UseSyntaxHighlightingProperty
            = DependencyProperty.Register("UseSyntaxHighlighting", typeof(bool), typeof(CodeDisplay),
            new FrameworkPropertyMetadata(true, OnSyntaxHighlightingChange));

        public bool HighlightCurrentLine { get => (bool)GetValue(HighlightCurrentLineProperty); set => SetValue(HighlightCurrentLineProperty, value); }

        /// <summary>The backing dependency property for <see cref="HighlightCurrentLine"/>. See the related property for details.</summary>
        public static DependencyProperty HighlightCurrentLineProperty
            = DependencyProperty.Register("HighlightCurrentLine", typeof(bool), typeof(CodeDisplay),
            new FrameworkPropertyMetadata(false, OnHighlightCurrentLineChanged));

        private static void OnSyntaxHighlightingChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CodeDisplay cd)
            {
                cd.SetSyntaxHighlighting();
            }
        }

        private static void OnHighlightCurrentLineChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CodeDisplay cd)
            {
                cd.editText.Options.HighlightCurrentLine = cd.HighlightCurrentLine;
            }
        }

        #endregion

        void SetSyntaxHighlighting()
        {
            if (App.CsharpHighlighter != null && App.NoneHighlighter != null)
            {
                editText.SyntaxHighlighting = UseSyntaxHighlighting
                    ? HighlightingLoader.Load(App.CsharpHighlighter, HighlightingManager.Instance)
                    : HighlightingLoader.Load(App.NoneHighlighter, HighlightingManager.Instance);
            }

            //TextMarkerService tms = new TextMarkerService(editText.TextArea.TextView);
            //editText.TextArea.TextView.BackgroundRenderers.Add(tms);
            //editText.TextArea.TextView.LineTransformers.Add(tms);
            
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(CodeText);
        }
    }
}
