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