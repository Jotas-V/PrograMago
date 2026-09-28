# Plano de implementação — primeira batalha jogável TCC-13

1. Testar a passagem da terceira atividade para `enemy-object` e o bloqueio
   da atividade seguinte; separar o limite da Fase 1 do limite jogável.
2. Testar captura/restauração v3 e migração v1/v2; guardar índice e estágio da
   atividade atual e validar registros incompatíveis.
3. Testar o botão da revisão da Fase 1 e a restauração de Mago/Inimigo na cena;
   integrar o fluxo e a interface.
4. Executar Play Mode de ponta a ponta: Fase 1, criação do Boneco, combate,
   derrota/nova tentativa, vitória e recarga da cena.
5. Rodar Edit Mode e Play Mode completos, revisar o diff, commitar apenas os
   arquivos da TCC-13 e atualizar a issue pelo GraphQL HTTP.
