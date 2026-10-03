# TCC-12/TCC-13 — primeira vitória e continuação de métodos

Plano aprovado pelo usuário em 03/10/2026, sobre a branch dev.

- Manter edição livre do código e o balanceamento existente.
- Corrigir a espera desatualizada do teste de vitória: separar resultado lógico de conclusão visual, com espera limitada a cinco segundos. A animação aprovada de 0,5 s em velocidade 0,30 demora aproximadamente 1,67 s.
- Na TCC-13, liberar a etapa 6 após vencer o Boneco. Abrir o documento Mago para acrescentar lancarMagia(Inimigo alvo) e a chamada usando o inimigo já declarado.
- Reutilizar ElementalSpellValidator. Rejeitar código incompleto sem iniciar combate. Validar Ajustes com a classe que agora contém magia, sem exigir que o jogador remova o método para redistribuir atributos.
- Preservar saves v4 que contêm somente as etapas anteriormente jogáveis: ampliar arrays pelo prefixo de IDs existente, mantendo código, tentativas, vitória e Ajustes. Não fabricar conclusão das novas etapas.
- Manter etapas 7–9 bloqueadas até integração das próximas issues.
- TDD: testes de progressão e migração; percurso PlayMode com falha de código, correção, vitória e recarga. Rodar suítes completas para identificar regressões.

## Resultado da implementação

- Etapa 6 liberada após a vitória contra o Boneco; etapas 7–9 continuam aguardando as próximas integrações.
- Ajustes validam o documento acumulado com o método de magia, reutilizando o validador existente.
- Saves v4 anteriores são ampliados pelo prefixo de IDs, preservando vitória, tentativas e documentos. As etapas novas começam sem conclusão fabricada.
- O teste de primeira vitória passou ao esperar o término visual com prazo limitado. Não foi necessário alterar motor, animação ou balanceamento.
- Testes novos de progressão e migração foram observados falhando antes da implementação e passaram na suíte final. O teste PlayMode da etapa 6 foi observado falhando no bloqueio após o Boneco e passou após a integração.
- EditMode: 361/363 aprovados. PlayMode: 43/44 aprovados. Nenhuma falha nova; permanecem MagoPrefab_UsesProjectPixelArtWithPointFiltering, Scene_PreparingPresentationAgainReusesAuthoredObjects e Arena_BackdropFillsFrameEvenUnderScaledParent.
- Linear: descrições da TCC-12 e TCC-13 alinhadas ao plano aprovado; estados preservados.
- Relatórios locais estão em C:/Users/Pichau/Documents/TCC/PrograMago/tmp/tcc13-dev-editmode.xml e tcc13-dev-playmode.xml.
- Validação executada no Test Runner em batch no checkout dev. O projeto aberto na interface do Unity continua sendo o checkout principal da branch codex/TCC-15; seus arquivos locais não foram alterados.
