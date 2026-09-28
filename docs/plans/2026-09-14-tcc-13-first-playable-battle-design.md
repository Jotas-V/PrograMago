# TCC-13 — primeira batalha da Fase 2 jogável

- Data: 14 de setembro de 2026
- Linear: TCC-13 (TCC / PrograMago)
- Estado: aprovado pelo usuário

## Resultado

Após as três atividades da Fase 1, **Próxima batalha** abre `enemy-object`.
O jogador preserva o código do Mago, declara `Inimigo`, instancia o Boneco de
Treinamento e usa **Batalhar**. A validação existente fornece Mago e Boneco ao
motor de combate TCC-12. Vitória mostra a revisão pedagógica dessa atividade;
derrota permite tentar de novo com o mesmo código ou voltar a editá-lo. A
batalha `first-spell-method` permanece bloqueada. Os parâmetros de movimento,
alcance e ataque existentes não mudam nesta entrega; serão balanceados depois.

## Progressão e registro

O limite jogável passa a incluir apenas a primeira atividade do segundo
capítulo, mantendo a contagem e o marco de conclusão da Fase 1 separados. O
botão da vitória da terceira atividade fica habilitado para entrar na batalha
do Boneco. Ao vencer o Boneco, o botão informa que a continuação virá depois
e não avança para um validador ainda ausente.

O registro local evolui à versão 3 e inclui a quarta atividade, o índice atual
e o estágio. Um combate em andamento é retomado na edição para reiniciar a
tentativa sem restaurar vida e posições parciais. Registros v1 e v2 continuam
válidos: preservam as três atividades concluídas, texto e blocos e retomam a
revisão da Fase 1 quando ela era o último marco. O código aprovado é revalidado
contra a atividade mais avançada que o aceitar, permitindo restaurar o Mago e
o Boneco sem confundir código de edição com progresso concluído. Um registro
inválido continua sendo ignorado sem travar a cena.

## Verificação

Testes Edit Mode cobrem o avanço só até `enemy-object`, o limite da Fase 1,
salvamento v3, migração v1/v2 e retomada de combate como edição. Testes Play
Mode percorrem a interface da declaração inicial do Mago até lutar e vencer
o Boneco, verificam derrota e nova tentativa, e recarregam a cena após a
vitória. Cada mudança de comportamento começa por um teste que falha.

## Alternativas

Um atalho da revisão da Fase 1 direto para a arena não registraria a etapa
pedagógica. Uma tela demonstrativa separada do progresso não faria do Boneco
uma batalha do jogo. A extensão restrita do fluxo atual mantém validação,
combate e persistência conectados.
