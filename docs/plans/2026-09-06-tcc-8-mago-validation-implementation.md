# Plano de implementação — validação da criação do Mago

## 1. Preparar os contratos

1. Executar a suíte existente e registrar qualquer bloqueio do Unity aberto.
2. Criar testes para o contrato comum de validação por critério.
3. Confirmar a falha pela ausência do despachante e do resultado geral.
4. Implementar apenas os contratos mínimos para deixar os testes verdes.

## 2. Validar atributos privados

1. Escrever testes falhos para a classe com os cinco campos `private int`.
2. Cobrir ordem flexível e formatação equivalente.
3. Implementar o cursor de tokens e o validador de atributos.
4. Em ciclos separados, cobrir campo ausente, duplicado, público, com tipo ou
   nome incorreto e membro extra.
5. Verificar o diagnóstico e sua posição em cada ciclo.

## 3. Validar construtor e `this`

1. Escrever um teste falho para o construtor completo.
2. Implementar assinatura, parâmetros e corpo mínimos.
3. Adicionar casos falhos para nome do construtor, tipo de parâmetro, ausências,
   duplicatas e ordem variável.
4. Adicionar casos falhos para cada forma incorreta de `this` e implementar os
   diagnósticos estruturais ou conceituais correspondentes.

## 4. Validar instanciação e orçamento

1. Escrever um teste falho para `Mago variavel = new Mago(...)`.
2. Implementar associação dos argumentos à ordem dos parâmetros.
3. Cobrir ausência de `new`, classe errada, aridade e conteúdo extra.
4. Adicionar ciclos falhos para zero, 16, orçamento parcial, total 25 e soma
   acima de 25.
5. Preservar valores e pontos restantes no resultado validado.

## 5. Integrar as três batalhas

1. Escrever testes falhos para selecionar o validador pelo critério atual.
2. Atualizar o caso de uso e a composição sem levar regras à apresentação ou à
   Unity.
3. Confirmar que cada entrada aprova somente sua batalha correspondente.
4. Cobrir a progressão da primeira para a terceira batalha e as prévias sem
   alteração indevida do estado.

## 6. Verificação e entrega

1. Executar todos os testes Edit Mode.
2. Executar todos os testes Play Mode.
3. Executar `git diff --check` e revisar o diff restrito à TCC-8.
4. Commitar a implementação.
5. Registrar no Linear os arquivos, decisões e testes executados.
6. Mover a TCC-8 para To Test somente se todas as verificações estiverem
   verdes.

## 7. Corrigir a conclusão das batalhas introdutórias

1. Escrever um teste de regressão que espere revisão de vitória imediatamente
   após código válido em uma batalha `OnCodeValidated`.
2. Executar o teste e confirmar que ele falha porque o fluxo permanece em
   `BattleInProgress`.
3. Adicionar o modo de conclusão ao domínio e ao asset de conteúdo.
4. Fazer o presenter reutilizar `ReportBattleVictory` somente no modo
   `OnCodeValidated`.
5. Escrever um teste separado que confirme que `OnCombatVictory` continua em
   andamento até receber um resultado externo.
6. Marcar as três primeiras batalhas como automáticas e as cinco restantes como
   dependentes do combate.
7. Executar as suítes Edit Mode e Play Mode e validar manualmente o caminho da
   primeira para a segunda batalha.
