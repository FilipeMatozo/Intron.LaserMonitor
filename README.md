# Intron Laser Monitor
![WPF](https://img.shields.io/badge/WPF-512BD4?style=for-the-badge&logo=windows&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![MVVM](https://img.shields.io/badge/MVVM-2C3E50?style=for-the-badge&logo=github&logoColor=white)
![OxyPlot](https://img.shields.io/badge/OxyPlot-000000?style=for-the-badge&logo=plotly&logoColor=white)


Recebe dados de um sensor a laser **MyAntenna L240** via **RS-485/Serial**, plota em tempo real no **OxyPlot (SkiaSharp.WPF)** e exporta as medições para **Excel (.xlsx)**.

---

## ✨ Principais recursos

- Conexão serial (detecta/valida porta do laser).
- Iniciar / Interromper medição.
- Zerar medição (zera offset, limpa o gráfico e passa a exibir valores relativos).
- Limpar conteúdo (gráfico e listas).
- Mostrar/Esconder pontos no gráfico.
- Exportar para Excel (valores **absolutos** e **relativos** com **timestamp**).
- Indicador de conexão (texto + ellipse de status).

---

## 🖥️ Plataforma

- **Desktop Windows 64 bits**.
- **Versão portátil** (publicação “self-contained” sem exigir .NET instalado).

---

## 🚀 Como executar

1. Baixe o **.zip** da publicação interna.
2. Extraia a pasta.
3. Conecte o **sensor L240** (RS-485/Serial → COMx).
4. Abra `Intron.LaserMonitor.exe`.
5. Selecione a porta, **Conectar** e **Iniciar medição**.

> Pré-requisitos: apenas Windows x64 e acesso à porta serial. Não é necessário instalar .NET para a versão portátil.
> 

---

## 🧰 Tecnologias

- **C# / WPF / .NET**
- **MVVM** (CommunityToolkit)
- **OxyPlot SkiaSharp WPF**
- **EPPlus** (exportação Excel)
- **DI/IoC**

---

## 📷 Capturas

1. Visão Geral do software
<img width="700" alt="image" src="https://github.com/user-attachments/assets/4a50170e-ab02-4cee-bb34-ca53291502cc" />

---

2. Aquisição de dados em tempo real
<img width="700" alt="image" src="https://github.com/user-attachments/assets/cea2f3ab-dcd8-4bcd-9ffe-63b469a34046" />

---

## 📜 Licença

Uso **privado** e **fechado** — © Intron Brasil. Não distribuir fora da empresa.
