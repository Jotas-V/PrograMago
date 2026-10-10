# TCC-37 — mapa de fases

Base: `dev` em `584004f`; branch `codex/TCC-37-map`.

O fluxo usa cenas independentes: `MapScene` para seleção e `MainScene` para gameplay. O menu principal será integrado na TCC-44. MapScene é a entrada provisória da build enquanto o menu não está pronto.

M abre/fecha o mapa fora da digitação. O botão Mapa também permite abrir a cena com o editor focado. Abrir durante uma batalha preserva o gameplay carregado e pausa motor/projéteis/apresentação; a seleção fica bloqueada até o resultado da tentativa. Voltar retoma a mesma tentativa, respeitando uma pausa manual anterior. Após derrota, selecionar a fase atual pela trilha inicia nova tentativa, sem liberar a próxima. Ao escolher uma fase disponível, o mago padrão existente percorre os pontos da trilha; a câmera acompanha e a fase abre automaticamente ao chegar. Cliques concorrentes e saída durante a caminhada são bloqueados.

Antes da conclusão integral, somente a fase atual pendente pode ser selecionada; vitória libera a próxima. Após concluir as nove fases, todas ficam disponíveis para replay. A conclusão global é derivada das conquistas persistidas; replay não apaga conquistas nem os documentos cumulativos de código.

## Arte

O mago usa os sprites já existentes de Mago-Idle e Mago-Walk; nenhum personagem novo foi gerado. O fundo novo está em `Assets/PrograMago/Resources/Map/ForestMapBackground.png`, gerado pela ferramenta ImageGen integrada e importado como Sprite, Point, sem mipmaps/compressão. A rota, quadrados, números, estados e textos são elementos da Unity.

Prompt final de geração: panorama de floresta em pixel art de RPG 16-bit, visão superior levemente elevada, proporção aproximada 3:1, árvores antigas em verde-petróleo/oliva e clareiras em verde-sálvia, faixa central aberta para sobrepor rota e nove fases. Fundo apenas, sem personagens, construções, rotas desenhadas, números, texto ou interface.

## Autoria

A cena é editável e salva em `Assets/Scenes/MapScene.unity`. `PrograMago > TCC-37 > Construir cena do mapa` reconstrói a apresentação com as referências existentes. Os callbacks de seleção/volta são associados em runtime; abrir a cena diretamente restaura o progresso local. Reutilizar `LevelMapView.SceneName` no menu futuro para evitar duplicar a navegação.

## Verificação

Cinco testes de domínio passaram após falharem pela ausência das consultas/transição de seleção. Nove testes PlayMode passaram, incluindo abertura de MapScene separada, preservação do rascunho, movimento antes da entrada, cliques repetidos, bloqueios, foco do editor, limites da câmera, pausa de um combate real, retorno após derrota e migração de save de seis para nove etapas. Os três casos adicionais reproduziram falhas antes das correções. Os relatórios locais e capturas ficam em `tmp/tcc37-*`.

EditMode completo: 391/393 passaram. Permanecem as duas falhas preexistentes de GameplaySceneAssetTests (caminho antigo do mago e preparação duplicando quatro objetos), já registradas na TCC-19. O teste de conclusão final encontrou uma regressão na primeira integração do botão do mapa; corrigida preservando JourneyCompleted antes da navegação.

PlayMode completo: 69/70 passaram. A falha preexistente restante é Arena_BackdropFillsFrameEvenUnderScaledParent (escala do fundo). Nenhum teste novo falhou na verificação final. Revisão visual em 1920×1080 e 1440×1080 confirmou números legíveis, mago padrão acima do ponto selecionado, exploração da trilha e entrada automática em MainScene pela seleção. O save do usuário e a configuração original da Game View foram restaurados após a conferência.
