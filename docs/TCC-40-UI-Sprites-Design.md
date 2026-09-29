# TCC-40 — direção dos sprites da UI

## Decisão visual

Direção aprovada: **terra discreta + madeira fina**. A arena permanece intacta. A faixa inferior deve continuar visualmente ligada ao chão da arena, mas a textura de terra aparece só como uma transição estreita; não como um fundo rochoso que compete com o código.

O editor usa uma superfície escura, lisa e de baixo contraste visual para manter o código legível. O painel de instruções usa uma superfície clara, com textura de papel quase imperceptível. Molduras de madeira ficam finas, sem folhas grandes, ornamentos volumosos ou detalhes atrás de texto.

## Medidas da cena

Canvas de referência: **1920 × 1080**.

| Objeto | Retângulo de referência |
| --- | ---: |
| Arena existente | 1920 × 324 |
| BottomArea | 1920 × 756 |
| CodeEditorPanel | 1420 × 732, margem 12 px |
| CodeInput | 1396 × 636, posição x=24 / y=96 |
| CodeTimeline | 1200 × 68, posição x=24 / y=22 |
| BattleButton | 180 × 56, posição x=1240 / y=26 |
| CodeBlockButton (cada) | 82 × 40 |
| TutorialPanel | 460 × 732, posição x=1448 / y=12 |
| TutorialBodyText | aproximadamente 401 × 345 |
| TutorialStepText | aproximadamente 410 × 59 |
| Navegação do tutorial (cada) | aproximadamente 201 × 35 |
| CopyEnemyExampleButton | aproximadamente 419 × 35 |
| VictoryCard | 1080 × 720 |

As medidas vêm dos RectTransforms da MainScene. A integração deve preservar essas caixas e não redimensionar a arena.

## Peças de arte

1. **Faixa de transição de terra:** tira horizontal pequena e repetível, com a paleta do solo da arena; alvo visual de 24–32 px de altura no canvas de referência.
2. **CodeEditorPanel:** fundo escuro de floresta/pedra, madeira em borda de 8 px e centro de baixo ruído. A superfície do código não deve receber tábuas, folhas ou brilho.
3. **CodeInput:** superfície interna escura e quase lisa, com contraste suficiente para o código claro existente.
4. **CodeTimeline e abas:** superfícies compactas de madeira escura. Abas inativas usam detalhe verde musgo discreto; a aba ativa recebe um acento dourado/musgo. Rótulos continuam como texto da UI, fora da arte.
5. **Painel de instruções:** centro de papel claro, moldura fina de madeira e divisórias simples para título, texto e navegação.
6. **Botões:** uma pele primária para Batalhar e uma pele secundária para navegação, copiar exemplo e controles de bloco. Variantes idle/hover/pressed/disabled podem ser usadas sem texto embutido; a animação de clique/juice será implementada no TCC-42.
7. **Card de vitória/derrota:** moldura fina coerente com os outros painéis, interior escuro, título claro com acento dourado e espaço livre para texto e botões.

## Regras de escala e leitura

- Painéis e botões usam sprites próprios com bordas preparadas para **9-slice**, para preservar a moldura quando forem ajustados aos RectTransforms existentes.
- A faixa de terra é repetida horizontalmente; não se estica uma arte panorâmica sobre os 756 px da BottomArea.
- Arte de fundo não contém letras, rótulos ou ícones de controle. Os textos e ícones permanecem elementos separados da UI.
- Texturas seguem pixel art, filtro Point, sem mipmaps e sem compressão. A escala final precisa ser conferida na Unity durante a integração.
- A arena e seus sprites não serão modificados.

## Referências existentes

- `Assets/PrograMago/Resources/Arena/ForestArena.png` — paleta e textura de pixel art da arena.
- `Assets/PrograMago/Resources/Arena/WorkspaceEarthBackground.png` — referência da terra; não deve ser esticada por toda a UI.
- `Assets/PrograMago/Art/UI/UI-Panel-Button-Sheet.png` — catálogo anterior, útil para comparar a linguagem visual; os novos sprites devem reduzir o peso de folhagens e ornamentos.

## Ordem de produção e integração

1. Produzir os sprites modulares da transição, superfícies, molduras, abas e botões com a paleta da arena.
2. Conferir transparência, recortes e bordas 9-slice nos metadados.
3. No TCC-42, aplicar as artes nas caixas medidas, validar legibilidade e posicionamento na resolução de referência e acrescentar o feedback de clique. A arena fica fora dessa integração.

## Arquivos produzidos para esta direção

As artes abaixo são fontes PNG, sem texto embutido. O mapeamento reaproveita peles de botão e molduras onde a mesma função visual se repete, evitando variantes sem uso.

| Arquivo | Uso na cena | Caixa de referência |
| --- | --- | ---: |
| `UI-Soil-Transition-PixelArt.png` | faixa estreita que liga a BottomArea ao chão da arena | faixa horizontal, alvo visual de 24–32 px |
| `UI-Code-Editor-Panel-PixelArt.png` | moldura externa do editor e moldura reutilizada para o card de vitória/derrota | 1420 × 732; card 1080 × 720 |
| `UI-CodeInput-Surface-PixelArt.png` | superfície interna escura para texto de código | 1396 × 636 |
| `UI-Code-Timeline-Track-PixelArt.png` | fundo da faixa de abas e ações de batalha | 1200 × 68 |
| `UI-Button-Primary-PixelArt.png` | botão Batalhar | 180 × 56 |
| `UI-Button-Secondary-PixelArt.png` | navegação, copiar exemplo e botões dos blocos | 201 × 35; 419 × 35; blocos 82 × 40 |
| `UI-Tutorial-Panel-PixelArt.png` | fundo do painel de instruções, com texto separado | 460 × 732 |

A textura grande de terra foi descartada: a direção aprovada usa o solo apenas na faixa de transição, sem uma textura competindo com o editor. As duas peles de botão foram recompostas para preencher melhor a caixa visual e reduzir as margens transparentes.

## Integração pendente no Unity

As artes ainda precisam ser importadas como sprites, configurar filtro Point/mipmaps/compressão conforme a regra do projeto, definir as bordas 9-slice para molduras e botões e associar cada imagem aos RectTransforms indicados. Esses ajustes ficam para TCC-42 no Unity Sprite Editor. A arena não foi editada.
