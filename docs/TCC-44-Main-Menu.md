# TCC-44 — menu inicial

Branch `codex/TCC-44-menu`, criada diretamente de `dev` em `28406ac`. MenuScene é a primeira cena da build; MapScene e MainScene continuam independentes.

## Composição

Floresta existente como fundo, título PrograMago e Jogar/Opções/Controles/Sair à esquerda. A ilustração à direita reutiliza Mago-Idle/Mago-Attack, o Boneco de Treinamento do catálogo visual e a magia neutra existente, com animação leve de disparo e impacto. Nenhuma imagem ou personagem foi gerado para esta entrega.

## Navegação e save

Jogar sem save cria uma jornada vazia e abre o mapa. Com save, oferece Continuar e Novo jogo. Continuar restaura o progresso pelo mapa. Novo jogo exige confirmação antes de substituir o arquivo; cancelar mantém o save. Save incompatível desabilita Continuar e exige confirmação explícita para reiniciar. O botão Menu no mapa preserva a jornada e descarrega as cenas anteriores, inclusive gameplay aditivo.

## Opções e controles

Volumes geral, música e efeitos usam preferências locais (0–1). Geral aplica AudioListener.volume. TCC-45 deve consumir GameAudioSettings.Music e GameAudioSettings.Effects na integração de fontes/mixer, sem aplicar duas vezes o volume geral. Esta entrega não adiciona música ou efeitos sonoros.

Controles informa clique, M fora da digitação, R pressionado por cinco segundos, documentos de código, Ajustes e botões de atividade/combate. Escape fecha os painéis do menu. Sair fecha a aplicação na build; no Editor apresenta uma mensagem, sem interromper uma sessão de desenvolvimento.

## Autoria

Menu editável em `Assets/Scenes/MenuScene.unity`. O comando `PrograMago > TCC-44 > Construir menu inicial` reconstrói a cena e a conexão de retorno no mapa. O construtor de mapa também mantém o botão Menu. Dados e callbacks ficam em MainMenuView; ilustração em MenuDuelAnimation; preferências em GameAudioSettings.

## Verificação

Os seis testes iniciais falharam pela ausência de MenuScene e passaram após a implementação. Casos adicionais verificam save incompatível e retorno do gameplay pelo mapa. Relatórios e capturas ficam em `tmp/tcc44-*`.

Oito testes novos passaram. PlayMode completo: 77/78, com apenas a falha preexistente da escala do fundo da arena. EditMode completo: 391/393, com as duas falhas preexistentes de referência antiga do mago e preparação duplicando quatro objetos. Compilação sem erros.

Menu, opções e controles conferidos em 1920×1080 e 1440×1080. A revisão corrigiu a quebra de linhas dos controles e alinhou os personagens ao chão. Frames em avanço confirmados antes das capturas; a captura do menu ilustra um instante do disparo animado. Save e configurações originais do Editor restaurados após a conferência.
