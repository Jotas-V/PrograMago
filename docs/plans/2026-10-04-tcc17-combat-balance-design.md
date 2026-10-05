# TCC-17 — combate e balanceamento da fase 9

Design aprovado pelo jogador em 04/10/2026. Escopo: fase 9; fases anteriores mantêm seus atributos e submissões sem penalidade.

- Inimigos derrotados desaparecem depois que seus impactos pendentes terminam, inclusive no golpe final e no resultado de vitória.
- Perfis elementais distintos: golem resistente e lento; elemental com ataque à distância; slime ágil. A vida declarada no código continua sendo respeitada; resistência também vem do posicionamento, dano e ritmo do perfil.
- Batalhar com código inválido desconta 20% da vida máxima aprovada, arredondado para cima, mínimo 1. O diagnóstico e os três documentos de código são preservados. A penalidade acumula entre submissões; corrigir o código não cura. Não validar a cada tecla.
- Vida zero apresenta derrota. Tentar novamente restaura a vida, mantendo o código para correção. A build usada como base da penalidade fica fixa durante a tentativa para evitar cura por edição dos atributos. Antes da primeira build válida, usar a build aprovada da fase anterior como referência.
- Estratégia sintaticamente válida, mas elementalmente errada, entra no combate normalmente e pode perder. Vitória exige inimigos derrotados e vida positiva.
- Registrar as regras e resultados dos testes na TCC-17 e em docs/regras-do-jogo.md. Documento acadêmico do TCC fica para atualização posterior.

## Plano de implementação

1. Testes de regressão de remoção dos mortos, perfis distintos, build frágil derrotada e build equilibrada vencedora.
2. Configuração opcional do encontro final no domínio; manter configuração padrão das fases anteriores.
3. Vida da tentativa e penalidade no fluxo Batalhar; integrar derrota, correção e reinício sem perder documentos.
4. Sincronizar visibilidade de cada morto com os próprios eventos visuais pendentes, sem depender das animações dos outros atores.
5. Executar testes EditMode/PlayMode, rever exemplos e documentar atributos e limites da validação.

## Calibração e evidências

A formação inicial do encontro final fica três casas mais próxima do Mago. O elemental usa dano 1, alcance 10, iniciativa 7 e velocidade 4; golem: 2/2/3/4; slime: 1/2/6/6 (dano/alcance/iniciativa/velocidade). O boneco mantém dano zero. A vida declarada continua validada pelo catálogo: 10 para o boneco, 12 para os elementais.

A build de referência é `(10, 4, 9, 1, 1)`, somando 25 pontos. Testes verificam vitória recebendo dano e perda de vida acumulada após código inválido. A antiga `(1, 6, 15, 2, 1)` agora perde com a formação final. As escolhas de especialização e valores dos construtores precisam continuar consistentes com a build.

Editar código após derrota também começa uma nova tentativa. Edição durante a tentativa sem derrota não recupera vida. A tentativa é local à sessão; o salvamento existente mantém código e progressão, sem implementar combate em andamento entre recargas.
