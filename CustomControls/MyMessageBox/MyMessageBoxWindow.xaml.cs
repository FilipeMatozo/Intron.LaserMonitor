using Intron.LaserMonitor.CustomControls.MyMessageBox.Enums;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Intron.LaserMonitor.CustomControls.MyMessageBox
{
    /// <summary>
    /// Lógica interna para MyMessageBoxWindow.xaml
    /// </summary>
    public partial class MyMessageBoxWindow : Window
    {
        // Bindings simples para XAML
        public string TitleText { get; }
        public string MessageText { get; }
        public string? DetailText { get; }
        public string? DoNotAskAgainText { get; }
        public bool IsDoNotAskAgainChecked => DontAskAgainCheck.IsChecked == true;

        public MyMessageBoxResult Result { get; private set; } = MyMessageBoxResult.None;

        private readonly MyMessageBoxOptions _opts;

        public MyMessageBoxWindow(MyMessageBoxOptions options)
        {
            InitializeComponent();
            _opts = options;

            TitleText = options.Title;
            MessageText = options.Message;
            DetailText = options.Detail;
            DoNotAskAgainText = options.DoNotAskAgainText;
            DataContext = this;

            switch (options.Icon)
            {
                case MyMessageBoxIcon.Success:
                    ImageIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/icons8-success-50.png", UriKind.Absolute));
                    break;
                case MyMessageBoxIcon.Info:
                    ImageIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/icons8-info-50.png", UriKind.Absolute));
                    break;
                case MyMessageBoxIcon.Warning:
                    ImageIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/icons8-warning-50.png", UriKind.Absolute));
                    break;
                case MyMessageBoxIcon.Error:
                    ImageIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/icons8-exclamation-50.png", UriKind.Absolute));
                    break;
                case MyMessageBoxIcon.Question:
                    ImageIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/icons8-question-50.png", UriKind.Absolute));
                    break;
                case MyMessageBoxIcon.None: default: break;
            }

            // Botão Copiar
            CopyButton.Visibility = options.ShowCopyButton ? Visibility.Visible : Visibility.Collapsed;
            

            // Checkbox
            DontAskAgainCheck.Visibility = options.ShowDoNotAskAgain ? Visibility.Visible : Visibility.Collapsed;
            DontAskAgainCheck.IsChecked = options.IsDoNotAskAgainChecked;

            // Ajuste de botões
            ConfigureButtons(options);

            // Acento
            if (options.AccentBrush is not null)
            {
                Resources["DialogAccentBrush"] = options.AccentBrush;
            }

            // Conteúdo extra (se Detail estiver muito longo)
            if (!string.IsNullOrWhiteSpace(DetailText) && DetailText!.Length > 320)
            {
                ContentScroll.Visibility = Visibility.Visible;
                var tb = new System.Windows.Controls.TextBlock
                {
                    Text = DetailText,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = (Brush)Resources["DialogSecondaryForegroundBrush"],
                    FontSize = 12,
                    Margin = new Thickness(0, 0, 0, 8)
                };
                ContentScroll.Content = tb;
            }

            // Teclas
            PreviewKeyDown += OnPreviewKeyDown;
            MouseDown += (s, e) => { if (e.ChangedButton == MouseButton.Left) DragMoveSafe(); };
        }

        private void DragMoveSafe()
        {
            try { DragMove(); } catch { /* ignore */ }
        }

        private void ConfigureButtons(MyMessageBoxOptions o)
        {
            // Mapear presets
            switch (o.Buttons)
            {
                case MyMessageBoxButtons.Ok:
                    ShowPrimary(o.PrimaryText ?? "OK", isDefault: true, result: MyMessageBoxResult.Ok);
                    break;
                case MyMessageBoxButtons.OkCancel:
                    ShowSecondary(o.SecondaryText ?? "Cancelar", isCancel: true, result: MyMessageBoxResult.Cancel);
                    ShowPrimary(o.PrimaryText ?? "OK", isDefault: true, result: MyMessageBoxResult.Ok);
                    break;
                case MyMessageBoxButtons.YesNo:
                    ShowSecondary(o.SecondaryText ?? "Não", isCancel: true, result: MyMessageBoxResult.No);
                    ShowPrimary(o.PrimaryText ?? "Sim", isDefault: true, result: MyMessageBoxResult.Yes);
                    break;
                case MyMessageBoxButtons.YesNoCancel:
                    ShowTertiary(o.TertiaryText ?? "Cancelar", isCancel: true, result: MyMessageBoxResult.Cancel);
                    ShowSecondary(o.SecondaryText ?? "Não", result: MyMessageBoxResult.No);
                    ShowPrimary(o.PrimaryText ?? "Sim", isDefault: true, result: MyMessageBoxResult.Yes);
                    break;
                case MyMessageBoxButtons.Custom:
                    // Até 3 botões custom
                    if (!string.IsNullOrWhiteSpace(o.TertiaryText)) ShowTertiary(o.TertiaryText!, result: MyMessageBoxResult.Tertiary);
                    if (!string.IsNullOrWhiteSpace(o.SecondaryText)) ShowSecondary(o.SecondaryText!, result: MyMessageBoxResult.Secondary);
                    if (!string.IsNullOrWhiteSpace(o.PrimaryText)) ShowPrimary(o.PrimaryText!, isDefault: true, result: MyMessageBoxResult.Primary);
                    break;
            }

            // Default e Cancel custom
            ApplyDefaultButton(o.DefaultButton);
            // CancelResult é respeitado em Esc
            _cancelOnEsc = o.CancelResult;
        }

        private void ShowPrimary(string text, bool isDefault = false, bool isCancel = false, MyMessageBoxResult result = MyMessageBoxResult.Primary)
        {
            PrimaryButton.Content = text;
            PrimaryButton.Visibility = Visibility.Visible;
            if (isDefault) PrimaryButton.IsDefault = true;
            if (isCancel) PrimaryButton.IsCancel = true;
            _primaryResult = result;
        }

        private void ShowSecondary(string text, bool isDefault = false, bool isCancel = false, MyMessageBoxResult result = MyMessageBoxResult.Secondary)
        {
            SecondaryButton.Content = text;
            SecondaryButton.Visibility = Visibility.Visible;
            if (isDefault) SecondaryButton.IsDefault = true;
            if (isCancel) SecondaryButton.IsCancel = true;
            _secondaryResult = result;
        }

        private void ShowTertiary(string text, bool isDefault = false, bool isCancel = false, MyMessageBoxResult result = MyMessageBoxResult.Tertiary)
        {
            TertiaryButton.Content = text;
            TertiaryButton.Visibility = Visibility.Visible;
            if (isDefault) TertiaryButton.IsDefault = true;
            if (isCancel) TertiaryButton.IsCancel = true;
            _tertiaryResult = result;
        }

        private void ApplyDefaultButton(MyMessageBoxDefaultButton def)
        {
            if (def == MyMessageBoxDefaultButton.Auto) return; // já marcado pelos presets

            // Limpa anteriores e aplica explicitamente
            PrimaryButton.IsDefault = SecondaryButton.IsDefault = TertiaryButton.IsDefault = false;
            switch (def)
            {
                case MyMessageBoxDefaultButton.First:
                    if (PrimaryButton.IsVisible) PrimaryButton.IsDefault = true;
                    else if (SecondaryButton.IsVisible) SecondaryButton.IsDefault = true;
                    else if (TertiaryButton.IsVisible) TertiaryButton.IsDefault = true;
                    break;
                case MyMessageBoxDefaultButton.Second:
                    if (SecondaryButton.IsVisible) SecondaryButton.IsDefault = true;
                    else if (TertiaryButton.IsVisible) TertiaryButton.IsDefault = true;
                    break;
                case MyMessageBoxDefaultButton.Third:
                    if (TertiaryButton.IsVisible) TertiaryButton.IsDefault = true;
                    break;
            }
        }

        // Result mapping
        private MyMessageBoxResult _primaryResult = MyMessageBoxResult.Primary;
        private MyMessageBoxResult _secondaryResult = MyMessageBoxResult.Secondary;
        private MyMessageBoxResult _tertiaryResult = MyMessageBoxResult.Tertiary;
        private MyMessageBoxResult _cancelOnEsc = MyMessageBoxResult.Cancel;

        private void PrimaryButton_Click(object sender, RoutedEventArgs e) => CloseWith(_primaryResult);
        private void SecondaryButton_Click(object sender, RoutedEventArgs e) => CloseWith(_secondaryResult);
        private void TertiaryButton_Click(object sender, RoutedEventArgs e) => CloseWith(_tertiaryResult);

        private void Close_Click(object sender, RoutedEventArgs e) => CloseWith(_cancelOnEsc);

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true; CloseWith(_cancelOnEsc);
            }
            else if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                DoCopy(); e.Handled = true;
            }
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e) => DoCopy();

        private void DoCopy()
        {
            var sb = new StringBuilder();
            sb.AppendLine(TitleText);
            sb.AppendLine(MessageText);
            if (!string.IsNullOrWhiteSpace(DetailText)) sb.AppendLine(DetailText);
            Clipboard.SetText(sb.ToString());
        }

        private void CloseWith(MyMessageBoxResult result)
        {
            Result = result;
            DialogResult = true;
            Close();
        }
    }
}
