using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Intron.LaserMonitor.Effects;

public class InvertEffect : ShaderEffect
{
    private static readonly PixelShader _shader = new PixelShader
    {
        // Ajuste "SeuProjeto" e o caminho do recurso conforme seu assembly/projeto
        UriSource = new Uri(
            "pack://application:,,,/Shaders/Invert.ps",
            UriKind.Absolute)
    };

    public InvertEffect()
    {
        PixelShader = _shader;
        UpdateShaderValue(InputProperty);
    }

    // Sampler 0 (obrigatório para Image)
    public static readonly DependencyProperty InputProperty =
        RegisterPixelShaderSamplerProperty(nameof(Input), typeof(InvertEffect), 0);

    public Brush Input
    {
        get => (Brush)GetValue(InputProperty);
        set => SetValue(InputProperty, value);
    }
}
