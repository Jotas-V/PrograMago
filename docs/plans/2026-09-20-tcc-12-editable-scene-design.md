# TCC-12 — MainScene editável fora do Play

## Problema e decisão

O Awake construía a arena, a timeline e o tutorial. Esses objetos desapareciam
ao sair do Play e não podiam ser editados na cena. A apresentação permanente
passa a ser salva na MainScene, com referências serializadas no bootstrapper.
O Awake conecta eventos e carrega a partida usando os objetos existentes.

Uma prévia temporária que executasse o Awake no Editor também mostraria a cena,
mas misturaria edição com carregamento de save e regras da partida. Por isso,
a preparação da apresentação é uma operação explícita, separada dos presenters.

## Estrutura

- Fundo, casas, painéis, botões, timeline e tutorial permanecem na Hierarchy.
- O menu de contexto `Preparar apresentação da cena` prepara a apresentação
  sem iniciar combate ou consultar o save. Repeti-lo reutiliza a estrutura.
- `ArenaEditorPreview`, marcado EditorOnly, contém MagoPreview e BonecoPreview.
  Ele mantém o alinhamento com o chão no Editor e fica inativo no Play; esse
  grupo não entra no build. Os personagens reais continuam dependendo do código.
- O jogador ainda pode adicionar/remover blocos durante a partida. As setas
  existentes são reutilizadas; novas setas copiam a primeira seta configurada.
- A cena exibe a batalha com Boneco como exemplo visual. O jogo escolhe a etapa
  correta pelo progresso salvo, ou inicia em Classes quando não há save.

## Verificação

EditMode verifica elementos persistidos, referências, conteúdo da prévia e
preparação repetida sem duplicações. PlayMode verifica que há apenas uma
estrutura visual, que a prévia não vaza para a partida e que edição de código,
timeline, combate e persistência continuam funcionando.

Validação final: 285 testes EditMode e 42 PlayMode aprovados. MainScene reaberta
fora do Play com a estrutura persistida. Prévia renderizada em
TestResults/mainscene-edit-mode.png. Nenhuma migração de save foi necessária.

## TCC-13 — workspace de classes, preparação e comandos

A faixa inferior agora contém somente comandos de combate. A área de aprendizado
fica no topo do editor com as áreas Classes, Métodos e Preparação. Depois que a
fase 3 é concluída, as classes ficam protegidas; os métodos são aprovados em
ordem, um por vez, e a preparação só aceita chamadas aos setters aprendidos.

A preparação calcula toda a distribuição antes de criar o novo Mago. Cada
atributo termina entre 1 e 15 e o total precisa ser no máximo 25. Uma tentativa
inválida não altera o Mago declarado. O motor usa o Mago preparado para o
combate e a nova tentativa após derrota reinicia vida, posições e inimigos com
esses mesmos atributos.

O primeiro Boneco usa um único bloco `lancarMagia();`. O comando atômico localiza
o alvo, escolhe a magia disponível e respeita a distância da arena de 16 casas.
Blocos adicionais podem ser criados e reordenados na pausa. Blocos de classe não
entram nessa ordem e não alteram a sequência de ataques. O tutorial explica essa separação,
o alcance de 15 casas e a necessidade de preparar os atributos antes da batalha.

Os métodos aprovados, o rascunho e a preparação são campos opcionais do save;
saves anteriores continuam válidos. A suíte final cobre 285 testes EditMode e
42 testes PlayMode.
