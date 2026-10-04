# TCC-42 — sincronização do lançamento de magia

O usuário aprovou em 04/10/2026 ataque de 0,5 s com lançamento na pose do cajado apontado para o alvo, aproximadamente 0,33 s.

Diagnóstico: os clipes existentes têm 0,5 s, mas a velocidade global 0,30 prolonga o ataque para 1,67 s. O projétil era criado imediatamente ao receber o evento do motor. A pose Attack_2 aparece em 0,333 s no clipe de seis frames.

Manter clipes, arte, ritmo de Idle/Walk e regras do motor. Usar velocidade 1 no ataque dos quatro magos, com fila de apresentação para que disparos consecutivos não interrompam o lançamento anterior. Disparar o projétil ao alcançar a pose de lançamento, voltar a Idle ao terminar o ataque e restaurar velocidade 0,30. Pausar animação e contagem de preparação junto com o combate. Incluir ataques pendentes na condição que adia a tela de vitória. Cancelar ataques pendentes ao limpar/reiniciar a apresentação.

Sem Animator válido, manter o disparo imediato como fallback. Usar atualização de animação em tempo não escalado durante o ataque para acompanhar a apresentação de combate existente.

Validação: testes PlayMode reproduzem o disparo prematuro, duração excessiva e avanço da pose durante pausa; verificam dois ataques consecutivos e regressão dos projéteis, edição após derrota e vitórias. Conferir os quatro clipes de ataque de 0,5 s. Preservar arquivos de arte locais já modificados.

Plano: testes vermelhos; fila e rotina de lançamento; testes verdes; regressão da cena; recompile; integrar commit em dev local.
Resultado: os três testes novos falharam antes da correção e passaram depois. Regressão da cena: 47/48 passaram; permanece apenas Arena_BackdropFillsFrameEvenUnderScaledParent, já falhando antes desta alteração. Unity recompilou sem erros. Os quatro clipes de ataque carregados têm duração de 0,5 s.
