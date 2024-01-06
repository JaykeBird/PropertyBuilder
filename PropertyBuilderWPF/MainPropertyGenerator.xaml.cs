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
