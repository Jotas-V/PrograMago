# TCC-40 — correção visual da área inferior

## Escopo aprovado

Aplicar uma faixa contínua de terra sob a arena, criar uma moldura de madeira horizontal para o editor, remover molduras duplicadas, e organizar editor, tutorial, faixa de blocos e botão Batalhar para impedir sobreposição. Preservar todos os sprites existentes de personagens, inimigos e botões, inclusive feedback de clique. Não criar ícones novos.

## Execução

1. Gerar duas artes de fundo a partir das referências visuais existentes: terra compatível com `ForestArena` e painel horizontal derivado do painel de madeira já existente.
2. Importar ambas para Unity com pixels nítidos; manter alpha no painel e aplicar borda 9-slice adequada.
3. Atualizar o catálogo/aplicação visual para carregar cada arte uma vez no destino correto.
4. Tornar o `BottomArea` o fundo de terra; aplicar o painel somente ao `CodeEditorPanel`; manter o `CodeInput` transparente mas interativo.
5. Remover a arte duplicada do `TutorialPanel`; usar uma superfície única para o guia com margens que preservem títulos, texto e botões.
6. Reencaixar o editor e a barra inferior, mantendo a hierarquia e controles existentes.
7. Abrir a execução Unity e inspecionar o resultado em tamanho real, inclusive a tela de conclusão. Corrigir problemas visuais encontrados antes de concluir.

## Critérios de aceitação

- A terra continua visualmente a faixa de chão da arena pela área inferior sem uma quebra branca.
- A placa de madeira horizontal envolve o editor sem invadir a coluna tutorial nem cobrir o código.
- Títulos, tutorial, área de código, seletores e Batalhar são legíveis e não se sobrepõem.
- Sprites são nítidos e os botões conservam seus estados e o efeito de clique.
- Nenhum personagem, inimigo, projétil ou ícone redundante foi criado.

### Recorte dos botões existentes

Antes da aplicação na faixa inferior, consultar e ajustar pela Unity Sprite Editor API os retângulos dos quatro estados de Batalhar e dos quatro estados de ação, pois a metadata atual usa a célula quadrada inteira da folha. Manter a arte e os nomes já existentes.
