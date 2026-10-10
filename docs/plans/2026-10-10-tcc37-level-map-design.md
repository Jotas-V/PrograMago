# TCC-37 — mapa de seleção de fases

Direção aprovada em 10/10/2026: mapa de floresta em pixel art, nove quadrados numerados conectados por uma trilha; um mago percorre a rota e entra automaticamente na fase ao chegar. A câmera acompanha em um mapa maior que a tela. M abre/fecha o mapa fora da digitação. A lista de atalhos nas configurações pertence à TCC-44.

Branch: `codex/TCC-37-map`, criada da `dev` no commit `584004f1aa7d06f424478c9e7c1e6d6ad3c76add`. Alterações preexistentes de arte/animação e documentos são preservadas e não fazem parte desta entrega.

## Integração

Atualização explícita do usuário: três cenas independentes — menu principal (TCC-44), MapScene e MainScene (gameplay existente). MapScene tem câmera ortográfica própria, mapa em world space e HUD uGUI. Ao abrir M durante o gameplay, carregar MapScene aditivamente, ocultar canvas/câmeras/EventSystem do gameplay e suspender seus ticks de combate. Fechar o mapa descarrega MapScene e restaura o gameplay sem recriar a tentativa. Selecionar outra fase altera o progresso no bootstrapper, reinicializa a apresentação transitória e retorna ao gameplay; a seleção só é permitida pelo domínio. Abrir MapScene diretamente restaura o save e carrega MainScene ao chegar a uma fase. Reutilizar frames do mago e separar o sprite de navegação do ator de combate.

O domínio LearningProgress decide quais fases são selecionáveis: antes da vitória final, somente a fase corrente pendente; a vitória permite selecionar a próxima. Após vencer a nona fase, qualquer fase fica disponível. A conclusão de todas as fases permanece no array completed existente, independentemente da fase de replay atual.

O save preserva os três documentos cumulativos, seleção, ajustes, métodos e progresso. Entrar em replay não apaga documentos nem conquistas. Antes da conclusão integral, não permitir retorno livre às fases anteriores. Ao selecionar a fase atual, apenas fechar o mapa após a caminhada, preservando a tentativa. A troca real de fase limpa somente a apresentação transitória de combate e reinicializa o presenter para a fase escolhida.

M não navega enquanto TMP_InputField está focado. Durante combate ativo ou movimento pendente, bloquear troca de fase e cliques concorrentes; o mapa não reinicia uma tentativa nem cura penalidade. Vitória/derrota permitem retornar ao mapa. Expor também um botão Mapa para que a navegação continue acessível quando o editor está focado.

## Plano e verificação

1. Testes de domínio falhando: bloqueio sequencial, seleção após vitória, bloqueio durante combate, replay/restauração e derrota.
2. Implementar consultas/transição de seleção e validar saves de replay sem alterar versões antigas.
3. Construir mapa, rota, animação, acompanhamento e entrada automática.
4. Integrar atalho/botão, vitória/derrota, preservação da edição e persistência.
5. Testar o fluxo na MainScene, movimento antes da entrada, cliques repetidos, foco do editor, limites da câmera e recarga; executar EditMode/PlayMode e revisar duas proporções.
