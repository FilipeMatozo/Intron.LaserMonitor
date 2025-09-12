using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Intron.LaserMonitor.Effects;

public class InvertChromaKeyEffect : ShaderEffect
{
    private static readonly PixelShader _shader = new PixelShader
    {
        // Ajuste "SeuProjeto" e o caminho conforme seu assembly/projeto
        UriSource = new Uri(
            "pack://application:,,,/Shaders/InvertChromaKey.ps",
            UriKind.Absolute)
    };

    public InvertChromaKeyEffect()
    {
        PixelShader = _shader;

        // Inputs / consts do shader
        UpdateShaderValue(InputProperty);
        UpdateShaderValue(KeyColorProperty);
        UpdateShaderValue(ToleranceProperty);
        UpdateShaderValue(SoftnessProperty);

        // Defaults: remover preto com leve suavização
        KeyColor = Colors.Black;
        Tolerance = 0.10; // 0..1 (ajuste)
        Softness = 0.05;  // 0..1 (ajuste)
    }

    // Sampler 0 (obrigatório)
    public static readonly DependencyProperty InputProperty =
        RegisterPixelShaderSamplerProperty(nameof(Input), typeof(InvertChromaKeyEffect), 0);
    public Brush Input
    {
        get => (Brush)GetValue(InputProperty);
        set => SetValue(InputProperty, value);
    }

    // c0: KeyColor (usa RGBA, mas shader lê RGB)
    public static readonly DependencyProperty KeyColorProperty =
        DependencyProperty.Register(
            nameof(KeyColor),
            typeof(Color),
            typeof(InvertChromaKeyEffect),
            new UIPropertyMetadata(Colors.Black, PixelShaderConstantCallback(0)));
    public Color KeyColor
    {
        get => (Color)GetValue(KeyColorProperty);
        set => SetValue(KeyColorProperty, value);
    }

    // c1: Tolerance (float)
    public static readonly DependencyProperty ToleranceProperty =
        DependencyProperty.Register(
            nameof(Tolerance),
            typeof(double),
            typeof(InvertChromaKeyEffect),
            new UIPropertyMetadata(0.10, PixelShaderConstantCallback(1)));
    public double Tolerance
    {
        get => (double)GetValue(ToleranceProperty);
        set => SetValue(ToleranceProperty, value);
    }

    // c2: Softness (float)
    public static readonly DependencyProperty SoftnessProperty =
        DependencyProperty.Register(
            nameof(Softness),
            typeof(double),
            typeof(InvertChromaKeyEffect),
            new UIPropertyMetadata(0.05, PixelShaderConstantCallback(2)));
    public double Softness
    {
        get => (double)GetValue(SoftnessProperty);
        set => SetValue(SoftnessProperty, value);
    }
}
