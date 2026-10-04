# TCC-16 — quatro inimigos e um único Mago ativo

## Recorte aprovado em 04/10/2026

O usuário confirmou concluir TCC-16 primeiro. TCC-17 integrará a etapa 9 com if/else; TCC-38 ampliará batalhas de prática. Manter as oito etapas jogáveis e o save atual, sem adiantar a nona etapa nesta entrega.

## Auditoria

EnemyConstructionValidator reconhece os quatro perfis e múltiplas instanciações. CombatEngine já mantém uma coleção de estados independentes, posiciona os inimigos nas casas finais da arena e seleciona o inimigo vivo mais próximo. Os quatro magos e inimigos já possuem assets. O catálogo visual só referencia o Boneco: os demais usam placeholders. RenderCombatState troca a aparência do mesmo Mago, mas pode interromper uma animação que está na fila de apresentação.

## Implementação

1. Associar sprites de Idle e Animator Controllers de Golem de Gelo, Elemental de Fogo e Slime Aquático ao catálogo visual existente.
2. RenderEnemies deve escolher a apresentação pelo perfil sem alterar atributos, posições ou objetos de combate. Preservar fallback para assets ausentes.
3. A referência do Mago permanece única. A aparência deve acompanhar o elemento do ataque que está sendo apresentado; uma atualização posterior do motor não pode sobrescrever a forma de um lançamento em curso. Ao terminar a fila, apresentar a forma atual do motor.
4. Confirmar seleção por proximidade e desempate estável pela ordem da coleção. Não criar seleção automática pela fraqueza no código do jogador nem exigir listas/laços. A estratégia utilizada pelos testes configura o motor diretamente, sem ensinar if/else antes da TCC-17.
5. Manter os estados e objetos dos quatro inimigos ao alternar as formas. Não recriar inimigos durante RenderCombatState.

## Verificação e plano

Criar testes PlayMode inicialmente vermelhos para quatro inimigos com arte própria e troca de formas durante lançamentos consecutivos. Testar a coleção/ordem dos quatro alvos no domínio. Corrigir apenas catálogo, renderização e apresentação das formas. Executar EditMode de combate e PlayMode da cena. Preservar arte local, validar recompilação, registrar commit e integrar dev local.
## Complemento autorizado — sprites de projéteis

Após retomar o trabalho, o usuário solicitou também integrar os sprites que faltavam dos inimigos e projéteis. Os quatro prefabs de magia ainda referenciavam placeholders de 8 x 8. Substituir pelos assets existentes *Projectile-PixelArt.png, preservando componentes, trajetória e temporização; usar cor branca para não tingir a pixel art. Atualizar o construtor do catálogo para também preencher e validar os três perfis elementais.

## Resultado da validação

Em 04/10/2026, a suíte completa EditMode terminou com 368/370 testes passando e a PlayMode com 50/51. Os novos testes de quatro inimigos, preservação de estados/objetos, formas dos lançamentos enfileirados e quatro sprites de projéteis passaram. O catálogo foi reconstruído e a arena foi conferida visualmente no Editor; a prévia foi descartada ao sair do Play Mode, sem salvar a cena ou o progresso.

Persistem três falhas observadas antes desta entrega: MagoPrefab_UsesProjectPixelArtWithPointFiltering espera o caminho antigo da arte do mago; Scene_PreparingPresentationAgainReusesAuthoredObjects diverge na contagem de objetos; Arena_BackdropFillsFrameEvenUnderScaledParent diverge na escala do fundo. Nenhuma nova falha foi introduzida nesta validação. Relatórios locais: tmp/tcc16-editmode.json e tmp/tcc16-playmode.json.
