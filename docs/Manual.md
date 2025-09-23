# Intron Laser Monitor — Manual do Usuário e Desenvolvedor

Documento interno da Intron Brasil. Este manual descreve instalação, operação, arquitetura resumida e pontos de integração do Intron Laser Monitor.

---

## Sumário

- [1. Visão geral](#1-visão-geral)
- [2. Instalação e requisitos](#2-instalação-e-requisitos)
- [3. Início rápido](#3-início-rápido)
- [4. Interface do usuário](#4-interface-do-usuário)
- [5. Comunicação serial (RS-485)](#5-comunicação-serial-rs-485)
- [6. Comandos do aplicativo](#6-comandos-do-aplicativo)
- [7. Gráfico em tempo real](#7-gráfico-em-tempo-real)
- [8. Exportação para Excel](#8-exportação-para-excel)
- [9. Estrutura técnica (resumo)](#9-estrutura-técnica-resumo)
- [10. Anexos](#10-anexos)

Acesse também:
- [Solução de problemas (FAQ)](/FAQ.md)
- [README](../README.md)
---

## 1. Visão geral

O **Intron Laser Monitor** comunica-se com o sensor **MyAntenna L240** via **RS-485 (Serial)**, exibindo a distância medida em um **gráfico em tempo real**. Ao final da aquisição, permite **exportar** os dados para **Excel (.xlsx)**, com **timestamp**, **valor absoluto** e **valor relativo (com offset)**.

**Casos de uso**: monitoramento de **vigas** e estruturas durante aplicação de carga para identificar **deformações**.

> Público-alvo: técnicos e engenheiros da Intron Brasil em campo e laboratório.

---
## 2. Instalação e requisitos

### 2.1. Requisitos

- **Windows 10/11 64 bits**.
- Porta **Serial/USB** disponível conectada ao conversor **RS-485** do sensor.
- Permissão de leitura/escrita na pasta onde o app será executado (para exportar .xlsx).

> Observação: a versão portátil é “self-contained”: não requer .NET instalado.

### 2.2. Instalação (versão portátil)

1. Obtenha o **pacote .zip** de [publicação interna via link](https://intronbrasil-my.sharepoint.com/:f:/g/personal/saymonton_intronbrasil_com_br/Epzra742SlRIgcgnefQTzqIBOFM5lH7W5HFkVyOUDrB3gg) (requer permissão), ou pela aba de [releases](../../releases) do repositório oficial.
2. **Extraia** o conteúdo para uma pasta local (ex.: `C:\\Intron\\LaserMonitor`).
3. Dê **duplo clique** em `Intron.LaserMonitor.exe`.

---

## 3. Início rápido

1. Conecte o **L240** e verifique qual **COM** o Windows atribuiu.
2. Abra o software e **selecione a porta** na **ComboBox**.
3. Clique em **Conectar**.
    - O indicador (texto + ellipse) ficará **verde** quando conectado.
4. Vá para a **aba de comandos** e selecione **Iniciar**.
5. Acompanhe o **gráfico em tempo real**.
6. Ao final, use **Interromper**: escolha a pasta e salve o `.xlsx`.

> Dica: use Zerar medição para iniciar um novo ciclo com valor relativo a partir de zero.
> 

---

## 4. Interface do usuário

<img width="1010" alt="Tela inicial do software" src="https://github.com/user-attachments/assets/33a6981b-c45c-41c8-b3c9-ff6f18288756" />


- **Cabeçalho / Conexão Serial**
    - **ComboBox** para seleção de **porta COM**.
    - **Botão Conectar/Desconectar** (habilitado apenas quando o dispositivo correto é detectado).
    - **Indicador de status** (texto + bolinha que muda de cor).
- **Aba de Comandos** (habilitada após conexão)
    - **Iniciar/Interromper medição**
    - **Zerar medição**
    - **Limpar conteúdo**
    - **Mostrar/Esconder pontos**
    - **Exportar Excel**
- **Gráfico principal** (OxyPlot SkiaSharp)
    - Exibe valores ao longo do **tempo**.
    - Permite visualizar **amostras** ponto a ponto quando habilitado.

---

## 5. Comunicação serial (RS-485)

A comunicação serial é feita a partir da interface **ISerialService**, que fornece ao projeto funções importantes da classe **SerialService**, utilizadas diretamente para abertura e fechamento da serial, início e encerramento de cada ciclo de medição.

Sobre a função de conexão serial, é importante mencionarmos a função `VerifyDevice()`, que nos retornará uma booleana nos confirmando se o dispositivo conectado é de fato o modelo de laser utilizado.

> Para mais informações, consulte [SerialService.cs](../Intron.LaserMonitor/Services/SerialService.cs), localizado na pasta Services do projeto.

### 5.1. Parâmetros necessários

- **Baudrate:** 115200.
- **Data bits:** 8.
- **Parity:** *None.*
- **Stop bits:** 1.
- **Protocolo:** ASCII/RTU + comandos `iGET`, `iSET`, `iSM`, `iACM`.

### 5.2. Boas práticas

- **Abrir/fechar** a porta de forma segura (try/finally).
- Desassociar **eventos** antes de `Close()`.
- Fazer bom uso do `Dispose()` ao navegar entre futuras janelas, a fim de evitar travamentos excessivos.
- Usar **Dispatcher** no WPF para atualizar UI sem travamentos.
- Buffer/parse não bloqueante, priorizando **responsividade**.

---

## 6. Comandos do aplicativo

| Comando | Descrição |
| --- | --- |
| **Conectar** | Abre a porta serial e valida se é o L240. |
| **Iniciar medição** | Começa a leitura contínua e plotagem em tempo real. |
| **Interromper medição** | Pausa a leitura contínua. |
| **Zerar medição** | Define o valor atual como **offset** (0 relativo), limpa o gráfico e volta a plotar. |
| **Limpar conteúdo** | Limpa gráfico, pontos e lista de medições. |
| **Mostrar/Esconder pontos** | Habilita/desabilita a renderização de marcadores por amostra. |
| **Exportar Excel** | Salva `.xlsx` com **Timestamp**, **Distância (mm)** e **Distância Absoluta (mm)**. |

---

## 7. Gráfico em tempo real

- **Biblioteca:** OxyPlot SkiaSharp WPF.
- **Eixo X:** tempo (timestamp).
- **Eixo Y:** distância (mm) **relativa** (após zerar) e/ou **absoluta**.
- **Desempenho:** usar `InvalidatePlot(true)` com cuidado, e preferir atualizações em lote quando possível.

> Gráfico plotando valores obtidos pelo sensor laser.

<img width="790" alt="Valores plotados pelo gráfico" src="https://github.com/user-attachments/assets/9e386ea8-7649-453e-9e23-3580de97a31e" />

---

## 8. Exportação para Excel

- **Formato:** `.xlsx` (usando **EPPlus**).
- **Colunas:** `Timestamp`, `Distância Absoluta (mm)`, `Distância Relativa #n(mm)` sendo **n** a quantidade de vezes em que foi definido o zero.
- **Fluxo:** ao finalizar um ciclo, o app **abre caixa de salvamento** automaticamente.

> Boas práticas:
> - Use formatação de número para mm (sem notação científica).
> - Garanta fechamento/Dispose correto antes de abrir o arquivo no Explorer.

---

## 9. Estrutura técnica (resumo)

- **Linguagem/Stack:** C#, WPF (.NET), MVVM (CommunityToolkit), DI/IoC.
- **Camadas principais:**
    - **ViewModels:** orquestram estados de conexão, comandos e dados do gráfico.
    - **Services:** `ISerialService`, `IExcelExportService`, etc.
    - **Models:** `Measurement, DataReceivedEventArgs`, etc.
    - **Views:** XAML (WPF) + OxyPlot SkiaSharp WPF.

> Diagrama simples com fluxo Serial → Parser → VM → Plot/Export.

<img width="1543" alt="image" src="https://github.com/user-attachments/assets/69035187-ab27-4ad4-a007-0af60d3d1dcf" />

---

## 10. Anexos

- L2-40:
    - [Clique aqui](https://intronbrasil-my.sharepoint.com/:f:/g/personal/saymonton_intronbrasil_com_br/Epzra742SlRIgcgnefQTzqIBOFM5lH7W5HFkVyOUDrB3gg) para visualizar os arquivos relacionados ao projeto incluindo as documentações do laser, sendo elas datasheet, especificações, esquema de ligação RS-485 dentre outras informações (acesso concedido mediante solicitação à equipe de desenvolvimento).

---

[Retornar à docs](../docs)
