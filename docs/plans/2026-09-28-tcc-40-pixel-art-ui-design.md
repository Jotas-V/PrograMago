# TCC-40: direção visual, animações e interface

## Objetivo

Produzir uma direção visual coesa em pixel art para o PrograMago, cobrindo personagens, animações, efeitos de combate, arena, interface e feedback dos controles. O trabalho fica concentrado na TCC-40, sem criar sub-issue.

## Contexto do projeto

O jogo usa uma arena 2D em uma floresta pixelada. O Mago atual tem silhueta reconhecível, roupa roxa, detalhes dourados, contorno escuro e fundo transparente. A importação atual usa filtro Point. A interface mistura elementos criados por código e fundos claros; os botões ainda não têm feedback de pressão nem identidade visual própria.

A TCC-11 estabelece os quatro inimigos: Boneco de Treinamento, Golem de Gelo, Elemental de Fogo e Slime Aquático. A TCC-13 continua responsável pelo fluxo da Fase 2 e pelo comportamento do combate.

## Abordagem escolhida

Foram consideradas três opções:

1. Criar uma folha de sprites completa para cada forma do Mago. É simples de usar no Animator, mas repete muitos quadros e aumenta a chance de as formas divergirem.
2. Reutilizar a silhueta e a animação-base do Mago, com variações de paleta e camadas próprias para cajado e magia. Esta opção mantém a identidade entre formas e permite animar o elemento sem duplicar o corpo.
3. Manter os sprites estáticos e acrescentar apenas tintas ou partículas. É mais rápido, mas não atende ao conjunto de animações e sprites solicitado.

A segunda opção foi aprovada. Para a interface, serão usados sprites modulares com bordas 9-slice, evitando distorção ao redimensionar painéis e botões.

## Direção visual

Manter a linguagem do projeto: pixel art 2D, silhuetas claras, contorno escuro e cores marcadas. Usar a floresta existente como base da arena, acrescentando movimento leve em primeiro plano sem reduzir o contraste dos personagens, células, instruções ou código.

Definir uma grade e uma escala comuns para os personagens e a interface. Manter transparência onde aplicável, filtro Point, pixels por unidade consistentes e compressão que não borre a arte. Documentar paleta, contornos, proporções, pivôs, animações e configurações de importação.

## Personagens e efeitos

### Mago

- Antes da instanciação, mostrar uma forma espectral distinta, como uma silhueta translúcida que indique o molde ainda incompleto.
- Após a instanciação, o Mago neutro segura o cajado.
- Piromante, Hidromante e Eletromante reutilizam a mesma identidade visual do Mago. O mapeamento aprovado é Piromante vermelho/laranja, Hidromante azul e Eletromante amarelo.
- Cada forma elemental segura sua magia na mão: chama animada, gota pulsante ou faísca intermitente.
- Criar ciclos de parado, andar e ataque. Manter a mesma temporização e volume visual entre formas.

### Inimigos

Criar silhuetas e animações próprias para Boneco de Treinamento, Golem de Gelo, Elemental de Fogo e Slime Aquático. Cada um terá ciclos de parado, andar e ataque disponíveis. Os clipes só serão acionados quando a apresentação atual indicar essas ações; esta produção não muda quais inimigos andam ou atacam.

### Magias e dano

Revisar ou produzir projéteis e impactos coerentes com os elementos. Quando alguém recebe dano, aplicar um flash curto na cor da magia correspondente e restaurar a cor normal no fim do efeito.

## Arena e interface

- Conservar o fundo da floresta e acrescentar folhas que atravessam a arena e linhas de vento em loops discretos.
- Criar molduras escaláveis para o painel do editor, a área clara onde o código aparece e os blocos da barra inferior.
- Criar sprites e estados normal, pressionado e desabilitado para Batalhar e os demais botões.
- Ao clicar, o botão cresce ligeiramente e retorna suavemente ao tamanho original, sem deslocar o layout nem alterar o funcionamento do controle.
- Redesenhar a tela de conclusão da fase com fundo ilustrado e hierarquia de texto mais clara. Preservar legibilidade de títulos, explicação, dicas e código.

## Integração e limites

Importar os sprites e configurar prefabs, divisões de sprites, bordas 9-slice e animações com escala e filtro consistentes. Ligar clipes às ações visuais existentes. O CombatEngine e suas regras, balanceamento, alvos e resultados permanecem fora do escopo.

## Validação visual

- Verificar personagens, projéteis e painéis em tamanho real de jogo, com pixels nítidos e transparência correta.
- Comparar as quatro formas do Mago e os quatro inimigos lado a lado para confirmar leitura e consistência.
- Conferir cada ciclo de animação, o flash de dano, as folhas/vento e o retorno do botão após o clique.
- Confirmar que o editor, os objetivos e as dicas continuam legíveis nos painéis e na tela final.
## Correção da composição após teste visual (2026-09-28)

### Evidência revisada

A captura da execução mostra molduras quadradas ampliadas em painéis largos. O código atribui `UI_WorkspacePanel` tanto ao `CodeEditorPanel` (75% da largura da área inferior) quanto a painéis de tutorial aninhados. Também atribui `UI_CodePanel` à imagem de fundo do `CodeInput`, que ocupa quase toda a área do editor. Cada arte é quadrada (362×362 px); o resultado empilha molduras decorativas e cobre texto e código. O `BottomArea` não possui imagem de fundo própria.

### Direção aprovada

- Preencher a área abaixo da arena com uma continuação visual da terra já existente em `ForestArena`.
- Usar uma placa de madeira horizontal, com proporção adequada ao editor, como moldura única do espaço de código. O conteúdo de código permanece legível dentro da placa.
- Remover o uso duplicado de molduras entre `TutorialPanel` e `EnemyGuidePanel`; manter uma única superfície discreta na coluna de instruções e ajustar o espaço interno.
- Reposicionar/acomodar a faixa inferior de blocos e Batalhar para que cada controle permaneça dentro da sua própria área e fora do texto editável.
- Reutilizar os sprites existentes de personagens, inimigos e botões. Não criar ícones sem função nem redesenhar artes já aprovadas.

### Plano de implementação

1. Criar somente dois fundos rasterizados específicos para esta composição: continuação de terra em estilo compatível com `ForestArena` e placa horizontal de madeira baseada na placa já aprovada da UI.
2. Importá-los com filtro Point, sem mipmaps e sem compressão que borre os pixels; configurar a placa como 9-slice apenas nas bordas necessárias.
3. Aplicar o fundo de terra ao `BottomArea`, a placa apenas ao `CodeEditorPanel`, e deixar a imagem de `CodeInput` transparente, preservando seu raycast e edição.
4. Tirar molduras duplicadas dos painéis do tutorial e do editor; revisar âncoras, margens, ordem de desenho, cores e contraste do texto.
5. Manter os sprites/estados e feedback de clique dos botões. Ajustar a faixa inferior para não cobrir o campo de código nem sair da placa.
6. Verificar visualmente o editor, o tutorial, os botões, a arena e a tela de conclusão na execução Unity, comparando proporções de janela ampla e a proporção atual usada na captura.

### Fora deste ajuste

Não gerar novos sprites de personagens, inimigos, projéteis ou ícones; não alterar regras de combate ou progressão.

### Ajuste técnico dos botões

A folha atual recorta cada estado de botão como uma célula quadrada de 362×362 px, embora o desenho visível seja uma faixa horizontal menor. A integração vai verificar e, se confirmado na Sprite Editor API, ajustar os retângulos dos quatro estados horizontais existentes `UI_Action_*`, mantendo nomes, cores e estados. O sprite quadrado `UI_Battle_*` não é adequado ao controle retangular Batalhar (180×56); Batalhar usará os mesmos estados horizontais existentes para não comprimir a arte. Não serão criadas novas artes de botão.
