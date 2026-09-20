# TCC-12 — blocos de ação e apresentação do combate

Desenho aprovado pelo usuário em 19/09/2026. Complementa e substitui os pontos
de interface e Boneco do desenho de 14/09. Trabalho vinculado à TCC-12, equipe
TCC, projeto PrograMago. A conclusão das demais batalhas da Fase 2 segue na TCC-13.

## Funcionamento

- As três setas de preparação preservam as declarações e instâncias de Mago e
  Inimigo. O programa de preparação é validado antes de iniciar o combate.
- As setas seguintes contêm código de ação. Um clique abre seu conteúdo no mesmo
  editor; arrastar altera a ordem. O jogador pode adicionar e remover ações antes
  de batalhar (1–16 blocos, até 48 chamadas no total).
- O vocabulário inicial é `analisarAlvo();`, `selecionarMagia();` e
  `lancarMagia();`. Cada bloco aceita múltiplas chamadas e comentários. Trata-se
  de um subconjunto didático de comandos, sem execução arbitrária de Java/C#.
- A ordem das chamadas e o bloco de origem chegam ao CombatEngine. Cada ciclo
  começa sem alvo ou magia selecionada. Analisar encontra o inimigo vivo mais
  próximo e invalida a seleção de magia anterior; selecionar escolhe a fraqueza
  disponível ou a magia neutra; lançar exige as duas operações anteriores.
- Cada chamada de lançamento válida ataca ou avança uma posição caso esteja
  fora do alcance. Múltiplos lançamentos no mesmo ciclo repetem essa operação.
  O ciclo inteiro consome uma oportunidade do Mago. Esse comportamento ainda
  precisa de balanceamento, especialmente para programas com muitos ataques.
- Erros de sintaxe impedem o início e indicam a ação correspondente. Ordem
  incorreta é executada pelo motor, sem causar dano naquele lançamento; a UI
  destaca o bloco e explica a ação que falta. Durante a luta, pode-se ler os
  blocos e pausar para reordená-los; edição e adição ficam bloqueadas.
- O Boneco de Treinamento tem 10 de vida, dano zero e não se move. O Mago base
  usa magia neutra. As regras elementais, alcance e intervalos anteriores são
  preservados; subclasses e novos comportamentos inimigos ficam para a evolução
  do jogo.

## Apresentação e tutorial

A timeline horizontal ocupa o rodapé ao lado de Batalhar, sem reduzir a largura
do editor. Preparação e ciclo têm títulos e cores distintos; há rolagem para
programas maiores. Os blocos persistem no save v3 como campo opcional, mantendo
compatibilidade com saves anteriores.

O tutorial da atividade 4, primeira batalha da Fase 2, tem seis passos com código:
preservar Mago, declarar Inimigo, construir, consultar elemento, instanciar Boneco
e ordenar ações. O botão de exemplo copia somente Inimigo para comparação. Erros
do validador abrem o passo relacionado sem substituir o código do jogador.

Quatro prefabs em `Assets/PrograMago/Resources/Combat` contêm sprites próprios e
CombatProjectileView: NeutralProjectile, FireProjectile, WaterProjectile e
ElectricProjectile. Foram criados via AssetDatabase/PrefabUtility no Editor.
Eles representam voo e impacto, congelam na pausa e são limpos no reinício ou
saída. Não calculam dano, colisões, fraquezas nem vitória. A UI observa os eventos
resolvidos pelo CombatEngine; a vitória é apresentada após o último impacto.
Os sprites são provisórios, com filtro Point, sem mipmaps e sem compressão.

## Verificação

EditMode cobre Boneco passivo, seleção obrigatória de magia, múltiplas ações e
origem dos eventos, compilação do código e os quatro prefabs. PlayMode cobre
arraste e pausa, persistência de blocos, retorno à edição, voo de projétil e o
percurso pedagógico até derrotar o Boneco e recarregar a revisão. A cena utilizada
é MainScene, conforme a renomeação já feita no projeto pelo usuário.

O Unity CLI com `com.unity.pipeline` permite executar as suítes e inspecionar a
cena no Editor aberto. Evidências locais ficam em `TestResults/`, fora do Git.

Validação concluída: 262/262 EditMode e 37/37 PlayMode. Inspeção visual em
1920×1080 e 1366×768, incluindo projétil neutro congelado na pausa e os três
projéteis elementais contra suas fraquezas. O save do jogador foi restaurado
byte a byte após a inspeção; a resolução original da Game View também foi reposta.
