# TCC-4 — plano de implementação da base de testes

## Sequência

1. Ampliar os testes de `CodeTokenizer` com formatação alternativa, entrada nula
   e diagnóstico léxico posicionado.
2. Ampliar os testes de `ClassDeclarationValidator` com entrada vazia, ordem
   incorreta, nome incorreto, chaves ausentes e tokens extras.
3. Cobrir preservação de estado em falha, idempotência e reinício nos casos de
   uso e no presenter.
4. Criar uma assembly Play Mode e um teste da cena que percorra submissão válida,
   submissão inválida e reinício usando os componentes reais da Unity.
5. Documentar a execução pelo Test Runner e pela linha de comando.
6. Executar Edit Mode e Play Mode, revisar alterações, commitá-las e registrar o
   resultado na TCC-4.

## Disciplina de execução

Testes de caracterização podem ficar verdes imediatamente quando registrarem
comportamentos já existentes. Qualquer comportamento novo ou correção seguirá
RED–GREEN–REFACTOR: teste falhando pela razão esperada, implementação mínima e
suíte completa verde antes da refatoração.

## Comandos de verificação

Com o projeto fechado no editor, usar a versão definida em
`ProjectSettings/ProjectVersion.txt`:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe' `
  -batchmode -nographics -projectPath (Get-Location).Path `
  -runTests -testPlatform EditMode -testResults editmode-results.xml -quit

& 'C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe' `
  -batchmode -nographics -projectPath (Get-Location).Path `
  -runTests -testPlatform PlayMode -testResults playmode-results.xml -quit
```

Com o projeto aberto, executar as duas abas diretamente em
`Window > General > Test Runner`.
