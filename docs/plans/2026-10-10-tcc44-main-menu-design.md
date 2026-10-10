# TCC-44 — menu inicial

Design solicitado e autorizado pelo usuário em 10/10/2026. Branch `codex/TCC-44-menu`, baseada na dev `28406ac`.

MenuScene independente e primeira cena da build. Floresta existente ocupa a tela; título PrograMago e quatro botões à esquerda (Jogar, Opções, Controles, Sair). À direita, mago padrão ataca o Boneco de Treinamento com magia neutra usando exclusivamente sprites existentes. Composição animada na Unity permite manter a identidade das artes e adaptar a proporção sem gerar novos personagens.

Jogar abre MapScene quando não há progresso; quando existe save válido, apresenta Continuar e Novo jogo. Reiniciar requer confirmação e grava um registro inicial completo apenas após confirmar. Cancelar preserva o save. Saves incompatíveis não são sobrescritos silenciosamente.

Opções possui volumes geral, música e efeitos persistidos localmente. Geral aplica AudioListener.volume; música/efeitos ficam disponíveis à futura TCC-45. Controles explica clique, blocos de código, M fora da digitação, R por cinco segundos e comandos de combate por botões. Escape fecha o painel atual. Sair usa Application.Quit na build; no Editor informa que o fechamento funciona na versão executável.

Retorno ao menu pelo mapa preserva progresso/código e descarrega cenas anteriores. Menu → mapa → gameplay mantém uma cena de cada função, sem serviços duplicados. O mapa carregado aditivamente durante uma tentativa continua preservando o gameplay até a seleção/volta.

Plano: testes de integração primeiro (cena, navegação, confirmação, persistência, controles e retorno); observar falhas; implementar comportamento e construir cena no Editor; verificar testes e visual em 16:9 e 4:3; registrar resultados na TCC-44. Não integrar automaticamente na dev.
