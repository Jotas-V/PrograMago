# Plano de implementação — TCC-9

1. Confirmar a suíte de base no projeto isolado.
2. Criar testes falhos para o modelo tipado e mapear os cinco valores por nome.
3. Criar testes falhos para a passagem do resultado validado à arena.
4. Separar prévia temporária de estado aprovado; preservar este estado após erro
   e ao avançar para a próxima batalha.
5. Adaptar a view Unity para distinguir silhueta e instância e exibir os cinco
   atributos e o saldo de pontos no quadro da arena.
6. Testar o fluxo completo e o reinício em Play Mode.
7. Executar as suítes, revisar o diff e registrar o resultado na TCC-9.

## Ajuste solicitado após validação

8. Manter o código digitado ao avançar de batalha; testar a progressão com o
   mesmo texto acumulado nas três atividades da fase 1.
9. Confirmar na interface que o saldo de pontos aceita distribuição parcial e
   chega a zero com 25 pontos, sem mudar após entrada inválida.
