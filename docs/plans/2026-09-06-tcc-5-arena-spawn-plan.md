# Plano de implementação — Spawn responsivo do mago

1. Adicionar um teste de Play Mode para a posição normalizada do `WizardSpawnPoint` e executá-lo em duas resoluções.
2. Confirmar que o teste falha com o spawn fixo atual.
3. Adicionar ao `GameplayBootstrapper` a câmera e a posição normalizada da arena.
4. Reposicionar o spawn ao iniciar e quando as dimensões da tela mudarem.
5. Persistir a referência da câmera na `SampleScene`.
6. Executar todos os testes Edit Mode e Play Mode.
7. Jogar em `Free Aspect`, redimensionar a Game View e confirmar arena, código e tutorial simultaneamente visíveis.
