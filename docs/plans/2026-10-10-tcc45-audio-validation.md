# TCC-45 - Integracao de audio

Branch: codex/TCC-45-audio, criada de origin/dev (29744b3).

Selecao e comportamento aprovados: docs/plans/2026-10-10-tcc45-audio-design.md.
Origens, autores, licencas e transformacoes: Assets/PrograMago/Resources/Audio/Credits.txt.
O processo de build copia os creditos para Audio-Credits.txt, junto do executavel.

Servico persistente com uma musica em loop e oito vozes de efeitos. Musica em
streaming; efeitos curtos carregados antecipadamente. Volumes individuais e geral
persistidos pelos controles existentes do menu; volume geral zero silencia.
A fonte de musica sobrevive a trocas de cena, mantendo a posicao da faixa.

Eventos: hover/clique, abrir mapa/grimorio, virar pagina, passos durante movimento,
lancamento das quatro magias, chegada do impacto, vitoria, derrota e conclusao.
O som de lancamento acompanha a criacao do projetil depois da animacao de ataque.
Impacto so toca na chegada; cancelar/destruir o projetil nao produz impacto.
Sons de resultado tem prioridade e reduzem brevemente o volume da musica.

Validacao automatizada:
- 17 testes de audio falharam antes da implementacao; todos passaram depois.
- Edit Mode: 396/398 passaram.
- Play Mode: 103/104 passaram, incluindo os 17 testes de audio.

Falhas preexistentes confirmadas nos resultados anteriores da TCC-48:
- MagoPrefab_UsesProjectPixelArtWithPointFiltering: referencia antiga de sprite.
- Scene_PreparingPresentationAgainReusesAuthoredObjects: objetos recriados pela preparacao da cena.
- Arena_BackdropFillsFrameEvenUnderScaledParent: enquadramento do fundo da arena.

Nenhum desses arquivos de arte/preparacao foi alterado nesta entrega.

Validacao final:
- Build StandaloneWindows64: Succeeded (275.509.695 bytes, cerca de 150 segundos).
- Audio-Credits.txt copiado e validado por hash contra o arquivo de origem.
- Executavel iniciou em modo batch e permaneceu ativo; log de inicializacao sem erros/excecoes.
- Verificacao ao vivo: MapOpen/MapStep e GrimoireOpen/PageTurn ativos nos eventos reais.
- Musica em streaming continuou de 6,6 s para 51,8 s entre as telas, com sinal de saida nao nulo.
- Loop de jogo avancando: frame 319 para 13595; nove fontes (uma musica e oito efeitos), um listener ativo.
- Console da sessao de verificacao sem novos erros de runtime.
- Relatorio da build inclui um erro de timeout do comando de automacao enviado durante
  o processamento da build; o resultado da build e Succeeded. Demais avisos sao
  obsolescencias ja existentes e ausencia de configuracao de Pipeline para o Player.
- Configuracoes/artefatos de Unity alterados automaticamente durante a validacao foram
  preservados em tmp/tcc45-build-generated-backup e restaurados antes da entrega.
