# TCC-5 — Spawn responsivo do mago na arena

## Objetivo

Fazer o `Mago` aparecer no canto esquerdo da arena visível, sem depender de uma posição fixa no mundo. Arena, editor de código e `TutorialPanel` devem permanecer visíveis ao mesmo tempo em qualquer proporção suportada pela Game View.

## Design aprovado

O `WizardSpawnPoint` continua sendo um objeto vazio dentro de `ArenaWorld`. O `GameplayBootstrapper` recebe a referência da câmera da arena e reposiciona o spawn usando coordenadas normalizadas da viewport: aproximadamente 10% da largura e 85% da altura da tela. Essa posição corresponde ao lado esquerdo e ao centro vertical da faixa superior reservada à arena.

O cálculo converte a posição da viewport para o mundo, preservando a profundidade atual do spawn. Ele é executado antes da criação do mago e novamente quando as dimensões da tela mudam. O prefab `Mago` continua sendo criado como filho do spawn, com posição local zero.

## Layout

O Canvas mantém o arranjo atual:

- arena na faixa superior;
- editor de código na região inferior esquerda;
- tutorial e feedback na região inferior direita.

Nenhuma dessas três áreas será escondida, maximizada ou substituída durante o gameplay.

## Validação

- Teste de Play Mode verifica que o spawn corresponde à coordenada de viewport configurada.
- Teste redimensiona a resolução e confirma que o spawn continua no canto esquerdo da arena.
- Testes existentes garantem que o mago permanece filho do spawn e que o layout completo continua visível.
- Playtest manual confirma o resultado em `Free Aspect` com mais de uma largura da Game View.
