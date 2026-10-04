# TCC-42 — origem e destino visuais da magia

Em 04/10/2026, o usuário aprovou manter o tempo do ataque atual e pediu que a magia saia do meio do personagem e siga até o inimigo.

Causa: LaunchCombatProjectile usava Transform.position de origem e destino, alinhados aos pés dos atores. Usar SpriteRenderer.bounds.center em coordenadas de mundo nos dois pontos resolve a altura e respeita escala, pivô, formas e sprites em objetos filhos. Sem SpriteRenderer, manter Transform.position como fallback.

Mudança limitada à geometria do disparo; preservar a rotina de lançamento, pausa, duração da animação, projéteis e regras de combate. O helper deve atender qualquer ator, incluindo os inimigos que disparam magia.

Plano: teste PlayMode falhando no código antigo; helper para centro visual; teste de origem, destino e chegada com atores escalados; regressão dos testes de lançamento e trajetória; recompile e integrar em dev local.
Validação: o novo teste falhou antes da correção (origem a 0,724 unidades do centro do Mago) e passou depois. Os dez testes PlayMode filtrados por Combat passaram, incluindo lançamento, pausa, ataques consecutivos, trajetória e chegada. Recompilação sem erros.
