using SolidShineUi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace PropertyBuilderWPF
{
    public class Settings : DependencyObject
    {
        [JsonPropertyName("colorScheme")]
        public ColorScheme ColorScheme { get; set; } = new ColorScheme(ColorsHelper.Red, ColorsHelper.GrayBlue) { MenusUseAccent = true };

        [JsonPropertyName("editor.showLineNumbers")]
        public bool ShowLineNumbers { get => (bool)GetValue(ShowLineNumbersProperty); set => SetValue(ShowLineNumbersProperty, value); }

        /// <summary>The backing dependency property for <see cref="ShowLineNumbers"/>. See the related property for details.</summary>
        public static DependencyProperty ShowLineNumbersProperty
            = DependencyProperty.Register("ShowLineNumbers", typeof(bool), typeof(Settings),
            new FrameworkPropertyMetadata(true));

        [JsonPropertyName("editor.fontFamily")]
        public string EditorFontFamilyName { get; set; } = "Consolas";

        [JsonPropertyName("editor.fontWeight")]
        public int EditorFontWeightVal { get; set; } = 400;

        [JsonPropertyName("editor.fontStyle")]
        public string EditorFontStyleName { get; set; } = "Normal";

        [JsonIgnore]
        public FontFamily EditorFontFamily { get => (FontFamily)GetValue(EditorFontFamilyProperty); set => SetValue(EditorFontFamilyProperty, value); }

        /// <summary>The backing dependency property for <see cref="FontFamily"/>. See the related property for details.</summary>
        public static DependencyProperty EditorFontFamilyProperty
            = DependencyProperty.Register("EditorFontFamily", typeof(FontFamily), typeof(Settings),
            new FrameworkPropertyMetadata(new FontFamily("Consolas")));

        [JsonPropertyName("editor.fontSize")]
        public double EditorFontSize { get => (double)GetValue(EditorFontSizeProperty); set => SetValue(EditorFontSizeProperty, value); }

        /// <summary>The backing dependency property for <see cref="EditorFontSize"/>. See the related property for details.</summary>
        public static DependencyProperty EditorFontSizeProperty
            = DependencyProperty.Register("EditorFontSize", typeof(double), typeof(Settings),
            new FrameworkPropertyMetadata(12.0));

        [JsonIgnore]
        public FontWeight EditorFontWeight { get => (FontWeight)GetValue(EditorFontWeightProperty); set => SetValue(EditorFontWeightProperty, value); }

        /// <summary>The backing dependency property for <see cref="EditorFontWeight"/>. See the related property for details.</summary>
        public static DependencyProperty EditorFontWeightProperty
            = DependencyProperty.Register("EditorFontWeight", typeof(FontWeight), typeof(Settings),
            new FrameworkPropertyMetadata(FontWeights.Normal));

        [JsonIgnore]
        public FontStyle EditorFontStyle { get => (FontStyle)GetValue(EditorFontStyleProperty); set => SetValue(EditorFontStyleProperty, value); }

        /// <summary>The backing dependency property for <see cref="EditorFontStyle"/>. See the related property for details.</summary>
        public static DependencyProperty EditorFontStyleProperty
            = DependencyProperty.Register("EditorFontStyle", typeof(FontStyle), typeof(Settings),
            new FrameworkPropertyMetadata(FontStyles.Normal));

        [JsonPropertyName("editor.wordWrap")]
        public bool WordWrap { get => (bool)GetValue(WordWrapProperty); set => SetValue(WordWrapProperty, value); }

        /// <summary>The backing dependency property for <see cref="WordWrap"/>. See the related property for details.</summary>
        public static DependencyProperty WordWrapProperty
            = DependencyProperty.Register("WordWrap", typeof(bool), typeof(Settings),
            new FrameworkPropertyMetadata(false));

        [JsonPropertyName("editor.syntaxColoring")]
        public bool UseSyntaxHighlighting { get => (bool)GetValue(UseSyntaxHighlightingProperty); set => SetValue(UseSyntaxHighlightingProperty, value); }

        /// <summary>The backing dependency property for <see cref="UseSyntaxHighlighting"/>. See the related property for details.</summary>
        public static DependencyProperty UseSyntaxHighlightingProperty
            = DependencyProperty.Register(nameof(UseSyntaxHighlighting), typeof(bool), typeof(Settings),
            new FrameworkPropertyMetadata(true));

        [JsonPropertyName("editor.highlightLine")]
        public bool HighlightCurrentLine { get => (bool)GetValue(HighlightCurrentLineProperty); set => SetValue(HighlightCurrentLineProperty, value); }

        /// <summary>The backing dependency property for <see cref="HighlightCurrentLine"/>. See the related property for details.</summary>
        public static DependencyProperty HighlightCurrentLineProperty
            = DependencyProperty.Register(nameof(HighlightCurrentLine), typeof(bool), typeof(Settings),
            new FrameworkPropertyMetadata(false));

        public static Settings? LoadSettings(string file)
        {
            try
            {
                return LoadSettings(new FileStream(file, FileMode.Open, FileAccess.Read));
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        public static JsonSerializerOptions SettingsSerializingOptions { get; } = new JsonSerializerOptions
        {
            IgnoreReadOnlyProperties = true,
            IgnoreReadOnlyFields = true,
            //WriteIndented = true
        };

        public static Settings? LoadSettings(Stream s)
        {
            Settings? ss = JsonSerializer.Deserialize<Settings>(s);

            if (ss != null)
            {
                ss.EditorFontFamily = new FontFamily(ss.EditorFontFamilyName);
                ss.EditorFontStyle = GetStyleFromString(ss.EditorFontStyleName);
                ss.EditorFontWeight = FontWeight.FromOpenTypeWeight(ss.EditorFontWeightVal);
            }

            return ss;
        }

        static FontStyle GetStyleFromString(string value)
        {
            return value.ToUpperInvariant() switch
            {
                "NORMAL" => FontStyles.Normal,
                "ITALIC" => FontStyles.Italic,
                "OBLIQUE" => FontStyles.Oblique,
                _ => FontStyles.Normal
            };
        }
        public void Save(Stream s)
        {
            EditorFontFamilyName = EditorFontFamily.Source;
            EditorFontStyleName = EditorFontStyle.ToString();
            EditorFontWeightVal = EditorFontWeight.ToOpenTypeWeight();

            JsonSerializer.Serialize(s, this, SettingsSerializingOptions);
        }

        public void Save(string file)
        {
            using FileStream fs = new(file, FileMode.Create);
            Save(fs);
            fs.Flush();
        }
    }
}
