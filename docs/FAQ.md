# Solução de problemas (FAQ)

## Problemas conhecidos
1. **Q1. O botão Conectar não habilita.**

- Verifique se a **porta COM** correta foi selecionada.
- Teste o cabo/RS-485.
- A rotina de verificação impede conectar em dispositivos que **não** sejam o L240.

2. **Q2. Gráfico “trava” durante a aquisição.**

- Evite operações pesadas no thread de UI.
- Atualize a UI via **Dispatcher**.
- Reduza marcadores de ponto se não forem necessários.

3. **Q3. Excel abre duas janelas/planilhas separadas.**

- Confirme se o processo do Excel anterior foi fechado.
- Garanta `Dispose()` do pacote EPPlus **antes** de abrir/exibir o arquivo.

4. **Q4. Valores com ponto flutuante “estranhos”.**

- Aplique formatação consistente (ex.: `N3` ou máscara explícita no Excel).
- Verifique cultura (`CultureInfo.InvariantCulture`) na leitura/parse e exportação.

---
## Não encontrou o que procurava?
Não achou a solução para seu problema ou quer reportar algo? Acesse a aba [Issues](../../issues) e sinta se livre para abrir uma nova issue, caso seu problema ainda não esteja listado.

---

## Acesse também:

[Manual.md](/Manual.md)  
[README](../README.md)
