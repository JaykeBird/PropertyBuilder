
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
    }
}
