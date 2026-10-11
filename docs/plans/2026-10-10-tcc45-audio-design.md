# TCC-45 — áudio aprovado

Branch `codex/TCC-45-audio`, criada de `origin/dev` em
`29744b3f43d2b3b51cb77bc34fb2db73051da2fc`. O usuário aprovou os arquivos,
a integração e a nova branch. Plano manual autorizado em substituição à skill
writing-plans indisponível.

## Seleção

| Evento | Original | Autor | Licença |
| --- | --- | --- | --- |
| Música contínua | deja_vus_in-game_ost_loop.ogg | YannZ | CC-BY 4.0 |
| Hover | select_001.ogg | Kenney | CC0 |
| Abrir mapa | book_03.ogg | rubberduck | CC0 |
| Abrir grimório | cloth1.ogg | Kenney | CC0 |
| Página | bookFlip1.ogg | Kenney | CC0 |
| Fogo | Fire.aif, convertido integralmente para WAV | artisticdude | CC-BY 3.0 |
| Água | splash_14.ogg | rubberduck | CC0 |
| Magia normal | quick_zap.wav | dklon | CC-BY 3.0 |
| Magia elétrica | continuousspark.wav, 0,218 s | Brian MacIntosh / BMacZero | CC0 |
| Dano | impactSoft_medium_002.ogg | Kenney | CC0 |
| Passos no mapa | footstep_grass_000.ogg | Kenney | CC0 |
| Vitória | Win sound.wav | Listener | CC0 |

Mapa e grimório foram invertidos por solicitação expressa do usuário.
Não usar os trovões rejeitados, nem as opções anteriores de magia normal/elétrica.
Clique pode reutilizar o hover; conclusão reutiliza a vitória. Derrota reutiliza
o impacto de dano, sem acrescentar uma faixa não avaliada pelo usuário.

## Integração

Um serviço persistente inicia antes da primeira cena, carrega apenas os arquivos
selecionados em Resources/Audio e mantém uma única fonte de música em loop.
Volumes existentes de GameAudioSettings atualizam as fontes imediatamente; volume
geral aplica-se uma única vez no AudioListener. Volume geral zero silencia e é
persistido pelo menu existente.

Oito vozes limitam efeitos simultâneos; intervalo mínimo evita rajadas duplicadas
de impacto/hover, com ganho reduzido ao sobrepor vozes. Sons de resultado têm
prioridade e reduzem brevemente a música. Botões desabilitados ficam silenciosos.
Lançamento toca apenas quando o projétil é realmente criado; impacto tem callback
de chegada independente de destruição/cancelamento. Abrir mapa/grimório toca na
configuração bem-sucedida; página só quando muda; passos só durante deslocamento.

## Plano de execução e validação

1. Registrar falhas dos testes sem serviço/arquivos (17 casos).
2. Importar arquivos aprovados e licenças; música em streaming, efeitos em memória.
3. Implementar serviço persistente, volumes e limitação de efeitos.
4. Conectar botões, mapa, grimório, lançamento, impacto e resultados.
5. Verificar testes de áudio e regressão EditMode/PlayMode, compilação e build.
6. Registrar limitações e entrega no Linear; manter a entrega na branch solicitada.
