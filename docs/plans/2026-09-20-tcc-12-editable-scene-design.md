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

Validação final: 273 testes EditMode e 42 PlayMode aprovados. MainScene reaberta
fora do Play com a estrutura persistida. Prévia renderizada em
TestResults/mainscene-edit-mode.png. Nenhuma migração de save foi necessária.
