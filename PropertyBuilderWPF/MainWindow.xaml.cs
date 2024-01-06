using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using SolidShineUi;

namespace PropertyBuilderWPF
{
    /// <summary>
    /// The main window of the program.
    /// </summary>
    public partial class MainWindow : FlatWindow
    {

        #region Window constructors / events

        public MainWindow()
        {
            _internalAction = true;
            InitializeComponent();

            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JaykeBird", "PropertyBuilder");
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            App.LoadSettings();

            ColorScheme = App.AppSettings.ColorScheme;

            btnThemeEdit.DisplayedColorScheme = App.AppSettings.ColorScheme;
            btnThemeEdit.ColorSchemeDataValue = 0;

            SetBindings();

            _internalAction = false;

            AddResourceLink(new Uri("https://learn.microsoft.com/en-us/dotnet/desktop/wpf/properties/dependency-properties-overview?view=netdesktop-8.0"), "WPF Dependency Properties");
            AddResourceLink(new Uri("https://learn.microsoft.com/en-us/dotnet/desktop/wpf/controls/control-authoring-overview#control-authoring-basics"), "WPF Control Authoring");
            AddResourceLink(new Uri("https://docs.avaloniaui.net/docs/guides/custom-controls/how-to-create-advanced-custom-controls"), "Avalonia DirectProperties and StyledProperties");
            AddResourceLink(new Uri("https://docs.avaloniaui.net/docs/guides/data-binding/binding-from-code#subscribing-to-a-property-on-any-object"), "Avalonia Listening to Property Changes");
            AddResourceLink(new Uri("https://docs.avaloniaui.net/docs/basics/user-interface/styling/"), "Avalonia Styling");
            AddResourceLink(new Uri("https://docs.avaloniaui.net/docs/concepts/input/routed-events"), "Avalonia Routed Events");
        }

        bool _internalAction = false;

        void SetBindings()
        {
            // for some reason, WPF won't allow binding to work through the TabControl
            // sooooo now I'm just doing all these bindings here lol

            Binding csBinding = new()
            {
                Source = this,
                Path = new PropertyPath(nameof(ColorScheme))
            };

            btnThemeEdit.SetBinding(ChangeTheme.ColorSchemeButton.ColorSchemeProperty, csBinding);
            btnEditorFont.SetBinding(FlatButton.ColorSchemeProperty, csBinding);
            chkLineNumbers.SetBinding(CheckBox.ColorSchemeProperty, csBinding);
            chkWordWrap.SetBinding(CheckBox.ColorSchemeProperty, csBinding);
            chkLineHighlight.SetBinding(CheckBox.ColorSchemeProperty, csBinding);
            chkSyntaxColoring.SetBinding(CheckBox.ColorSchemeProperty, csBinding);

            mpg.SetBinding(MainPropertyGenerator.ColorSchemeProperty, csBinding);
            bpg.SetBinding(BulkPropertyGenerator.ColorSchemeProperty, csBinding);

            //Binding fontFamilyBinding = new(nameof(Settings.EditorFontFamily)) { Source = App.AppSettings };
            //Binding fontSizeBinding = new(nameof(Settings.EditorFontSize)) { Source = App.AppSettings };
            //Binding fontStyleBinding = new(nameof(Settings.EditorFontStyle)) { Source = App.AppSettings };
            //Binding fontWeightBinding = new(nameof(Settings.EditorFontWeight)) { Source = App.AppSettings };
            //Binding lineNumbersBinding = new(nameof(Settings.ShowLineNumbers)) { Source = App.AppSettings };
            //Binding wordWrapBinding = new(nameof(Settings.WordWrap)) { Source = App.AppSettings };
            //Binding highlightBinding = new(nameof(Settings.HighlightCurrentLine)) { Source = App.AppSettings };
            //Binding syntaxColorBinding = new(nameof(Settings.UseSyntaxHighlighting)) { Source = App.AppSettings };

            //mpg.SetBinding(MainPropertyGenerator.EditorFontFamilyProperty, fontFamilyBinding);
            //mpg.SetBinding(MainPropertyGenerator.EditorFontSizeProperty, fontSizeBinding);
            //mpg.SetBinding(MainPropertyGenerator.EditorFontStyleProperty, fontStyleBinding);
            //mpg.SetBinding(MainPropertyGenerator.EditorFontWeightProperty, fontWeightBinding);
            //mpg.SetBinding(MainPropertyGenerator.ShowLineNumbersProperty, lineNumbersBinding);
            //mpg.SetBinding(MainPropertyGenerator.WordWrapProperty, wordWrapBinding);
            //mpg.SetBinding(MainPropertyGenerator.HighlightCurrentLineProperty, highlightBinding);
            //mpg.SetBinding(MainPropertyGenerator.UseSyntaxHighlightingProperty, syntaxColorBinding);

            //cdPerformAs.SetBinding(CodeDisplay.EditorFontFamilyProperty, fontFamilyBinding);
            //cdPerformAs.SetBinding(CodeDisplay.EditorFontSizeProperty, fontSizeBinding);
            //cdPerformAs.SetBinding(CodeDisplay.EditorFontStyleProperty, fontStyleBinding);
            //cdPerformAs.SetBinding(CodeDisplay.EditorFontWeightProperty, fontWeightBinding);
            //cdPerformAs.SetBinding(CodeDisplay.ShowLineNumbersProperty, lineNumbersBinding);
            //cdPerformAs.SetBinding(CodeDisplay.WordWrapProperty, wordWrapBinding);
            //cdPerformAs.SetBinding(CodeDisplay.HighlightCurrentLineProperty, highlightBinding);
            //cdPerformAs.SetBinding(CodeDisplay.UseSyntaxHighlightingProperty, syntaxColorBinding);
            cdPerformAs.SetBinding(CodeDisplay.ColorSchemeProperty, csBinding);

            //cdAvaloniaChanged.SetBinding(CodeDisplay.EditorFontFamilyProperty, fontFamilyBinding);
            //cdAvaloniaChanged.SetBinding(CodeDisplay.EditorFontSizeProperty, fontSizeBinding);
            //cdAvaloniaChanged.SetBinding(CodeDisplay.EditorFontStyleProperty, fontStyleBinding);
            //cdAvaloniaChanged.SetBinding(CodeDisplay.EditorFontWeightProperty, fontWeightBinding);
            //cdAvaloniaChanged.SetBinding(CodeDisplay.ShowLineNumbersProperty, lineNumbersBinding);
            //cdAvaloniaChanged.SetBinding(CodeDisplay.WordWrapProperty, wordWrapBinding);
            //cdAvaloniaChanged.SetBinding(CodeDisplay.HighlightCurrentLineProperty, highlightBinding);
            //cdAvaloniaChanged.SetBinding(CodeDisplay.UseSyntaxHighlightingProperty, syntaxColorBinding);
            cdAvaloniaChanged.SetBinding(CodeDisplay.ColorSchemeProperty, csBinding);

            chkLineNumbers.SetBinding(CheckBox.IsCheckedProperty, new Binding(nameof(Settings.ShowLineNumbers)) { Source = App.AppSettings, Mode = BindingMode.TwoWay });
            chkWordWrap.SetBinding(CheckBox.IsCheckedProperty, new Binding(nameof(Settings.WordWrap)) { Source = App.AppSettings, Mode = BindingMode.TwoWay });
            chkLineHighlight.SetBinding(CheckBox.IsCheckedProperty, new Binding(nameof(Settings.HighlightCurrentLine)) { Source = App.AppSettings, Mode = BindingMode.TwoWay });
            chkSyntaxColoring.SetBinding(CheckBox.IsCheckedProperty, new Binding(nameof(Settings.UseSyntaxHighlighting)) { Source = App.AppSettings, Mode = BindingMode.TwoWay });
        }

        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App.SaveSettings();
        }

        private async void window_Loaded(object sender, RoutedEventArgs e)
        {
            string avaloniaPropertyMethod = "";
            string performAs = "";

            using (Stream? s = App.GetResourceStream("AvaloniaPropertyChanged.txt"))
            {
                if (s != null)
                {
                    StreamReader sr = new StreamReader(s);
                    avaloniaPropertyMethod = await sr.ReadToEndAsync();
                }
            }

            using (Stream? s = App.GetResourceStream("PerformAs.txt"))
            {
                if (s != null)
                {
                    StreamReader sr = new StreamReader(s);
                    performAs = await sr.ReadToEndAsync();
                }
            }

            cdPerformAs.CodeText = performAs;
            cdAvaloniaChanged.CodeText = avaloniaPropertyMethod;
        }

        void AddResourceLink(Uri url, string title)
        {
            LinkTextBlock ltb = new LinkTextBlock
            {
                Text = title,
                ToolTip = url.AbsoluteUri,
                Margin = new Thickness(0, 2, 0, 2)
            };
            ltb.Click += (o, e) => App.OpenBrowserLink(url);

            stkLinks.Children.Add(ltb);
        }

        #endregion

        #region Code Editor Visuals
        //public bool ShowLineNumbers { get => (bool)GetValue(ShowLineNumbersProperty); set => SetValue(ShowLineNumbersProperty, value); }

        ///// <summary>The backing dependency property for <see cref="ShowLineNumbers"/>. See the related property for details.</summary>
        //public static DependencyProperty ShowLineNumbersProperty
        //    = DependencyProperty.Register("ShowLineNumbers", typeof(bool), typeof(MainWindow),
        //    new FrameworkPropertyMetadata(true));

        //public FontFamily EditorFontFamily { get => (FontFamily)GetValue(EditorFontFamilyProperty); set => SetValue(EditorFontFamilyProperty, value); }

        ///// <summary>The backing dependency property for <see cref="FontFamily"/>. See the related property for details.</summary>
        //public static DependencyProperty EditorFontFamilyProperty
        //    = DependencyProperty.Register("EditorFontFamily", typeof(FontFamily), typeof(MainWindow),
        //    new FrameworkPropertyMetadata(new FontFamily("Consolas")));

        //public double EditorFontSize { get => (double)GetValue(EditorFontSizeProperty); set => SetValue(EditorFontSizeProperty, value); }

        ///// <summary>The backing dependency property for <see cref="EditorFontSize"/>. See the related property for details.</summary>
        //public static DependencyProperty EditorFontSizeProperty
        //    = DependencyProperty.Register("EditorFontSize", typeof(double), typeof(MainWindow),
        //    new FrameworkPropertyMetadata(12.0));

        //public FontWeight EditorFontWeight { get => (FontWeight)GetValue(EditorFontWeightProperty); set => SetValue(EditorFontWeightProperty, value); }

        ///// <summary>The backing dependency property for <see cref="EditorFontWeight"/>. See the related property for details.</summary>
        //public static DependencyProperty EditorFontWeightProperty
        //    = DependencyProperty.Register("EditorFontWeight", typeof(FontWeight), typeof(MainWindow),
        //    new FrameworkPropertyMetadata(FontWeights.Normal));

        //public FontStyle EditorFontStyle { get => (FontStyle)GetValue(EditorFontStyleProperty); set => SetValue(EditorFontStyleProperty, value); }

        ///// <summary>The backing dependency property for <see cref="EditorFontStyle"/>. See the related property for details.</summary>
        //public static DependencyProperty EditorFontStyleProperty
        //    = DependencyProperty.Register("EditorFontStyle", typeof(FontStyle), typeof(MainWindow),
        //    new FrameworkPropertyMetadata(FontStyles.Normal));

        //public bool WordWrap { get => (bool)GetValue(WordWrapProperty); set => SetValue(WordWrapProperty, value); }

        ///// <summary>The backing dependency property for <see cref="WordWrap"/>. See the related property for details.</summary>
        //public static DependencyProperty WordWrapProperty
        //    = DependencyProperty.Register("WordWrap", typeof(bool), typeof(MainWindow),
        //    new FrameworkPropertyMetadata(false));

        //public bool UseSyntaxHighlighting { get => (bool)GetValue(UseSyntaxHighlightingProperty); set => SetValue(UseSyntaxHighlightingProperty, value); }

        ///// <summary>The backing dependency property for <see cref="UseSyntaxHighlighting"/>. See the related property for details.</summary>
        //public static DependencyProperty UseSyntaxHighlightingProperty
        //    = DependencyProperty.Register(nameof(UseSyntaxHighlighting), typeof(bool), typeof(MainWindow),
        //    new FrameworkPropertyMetadata(true));

        //public bool HighlightCurrentLine { get => (bool)GetValue(HighlightCurrentLineProperty); set => SetValue(HighlightCurrentLineProperty, value); }

        ///// <summary>The backing dependency property for <see cref="HighlightCurrentLine"/>. See the related property for details.</summary>
        //public static DependencyProperty HighlightCurrentLineProperty
        //    = DependencyProperty.Register(nameof(HighlightCurrentLine), typeof(bool), typeof(MainWindow),
        //    new FrameworkPropertyMetadata(true));

        #endregion

        #region Settings

        private void btnThemeEdit_Click(object sender, EventArgs e)
        {
            ChangeTheme.ColorSchemeDialog csd = new()
            {
                ColorScheme = App.AppSettings.ColorScheme,
                Owner = this
            };
            csd.ShowDialog();

            if (csd.DialogResult)
            {
                if (csd.SelectedColorScheme.IsHighContrast)
                {
                    App.AppSettings.ColorScheme = csd.SelectedColorScheme;
                }
                else if (csd.SelectedColorScheme.AccentMainColor.GetHexString() == "A8A8A8")
                {
                    // light theme
                    ColorScheme cs = ColorScheme.CreateLightTheme(ColorsHelper.GrayBlue);
                    cs.MenusUseAccent = true;
                    App.AppSettings.ColorScheme = cs;
                }
                else if (csd.SelectedColorScheme.AccentMainColor.GetHexString() == "C8C8C8")
                {
                    // dark theme
                    ColorScheme cs = ColorScheme.CreateDarkTheme(ColorsHelper.GrayBlue);
                    cs.MenusUseAccent = true;
                    App.AppSettings.ColorScheme = cs;
                }
                else
                {
                    App.AppSettings.ColorScheme = new ColorScheme(csd.SelectedColorScheme.MainColor, ColorsHelper.GrayBlue) { MenusUseAccent = true };
                }

                ColorScheme = App.AppSettings.ColorScheme;
                btnThemeEdit.DisplayedColorScheme = csd.SelectedColorScheme;
                btnThemeEdit.ColorSchemeDataValue = csd.InternalColorSchemeValue;

                App.SaveSettings();
            }
        }

        private void btnEditorFont_Click(object sender, RoutedEventArgs e)
        {
            FontSelectDialog fsd = new FontSelectDialog()
            {
                SelectedFontFamily = App.AppSettings.EditorFontFamily,
                SelectedFontSize = App.AppSettings.EditorFontSize,
                SelectedFontWeight = App.AppSettings.EditorFontWeight,
                SelectedFontStyle = App.AppSettings.EditorFontStyle,
                ShowDecorations = false,
                ColorScheme = ColorScheme,
                Owner = this
            };

            fsd.ShowDialog();

            if (fsd.DialogResult)
            {
                App.AppSettings.EditorFontFamily = fsd.SelectedFontFamily;
                App.AppSettings.EditorFontSize = fsd.SelectedFontSize;
                App.AppSettings.EditorFontStyle = fsd.SelectedFontStyle;
                App.AppSettings.EditorFontWeight = fsd.SelectedFontWeight;
            }

            App.SaveSettings();
        }


        private void chkLineNumbers_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (_internalAction) return;

            //App.AppSettings.ShowLineNumbers = chkLineNumbers.IsChecked;
            //App.SaveSettings();
        }

        private void chkWordWrap_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (_internalAction) return;

            //App.AppSettings.WordWrap = chkWordWrap.IsChecked;
            //App.SaveSettings();
        }

        private void chkLineHighlight_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (_internalAction) return;

            //App.AppSettings.HighlightCurrentLine = chkLineHighlight.IsChecked;
            //App.SaveSettings();
        }

        private void chkSyntaxColoring_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (_internalAction) return;

            //App.AppSettings.UseSyntaxHighlighting = chkSyntaxColoring.IsChecked;
            //App.SaveSettings();
        }

        #endregion
    }
}