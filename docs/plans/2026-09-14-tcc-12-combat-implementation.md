# Plano de implementação — TCC-12

1. Escrever testes Edit Mode falhos para perfis inimigos, relógio e ordem das
   ações; implementar o motor determinístico mínimo no domínio.
2. Cobrir em ciclos de teste alcance, movimento, escolha de alvo, elementos,
   reordenação, pausa, vitória e derrota.
3. Escrever testes de apresentação falhos para iniciar combate somente após
   validação e `OnCombatVictory`, encaminhar cada evento à arena e resolver o
   resultado pelo fluxo pedagógico; implementar a integração.
4. Criar a coluna visual da fila, arrastar e soltar, botão de pausa, feedback
   de dano/elemento/alcance e interface de derrota na Unity sem alterar a cena
   local já modificada; verificar em Play Mode.
5. Executar Edit Mode e Play Mode, revisar a diferença e registrar o resultado
   na TCC-12 pela API HTTP do Linear.
