# Testes do PrograMago

O projeto separa os testes rápidos do núcleo (`EditMode`) dos testes de
integração da cena (`PlayMode`).

## Pelo editor

1. Abra `Window > General > Test Runner`.
2. Na aba `EditMode`, use `Run All` para validar domínio, linguagem, aplicação e
   apresentação.
3. Na aba `PlayMode`, use `Run All` para validar a cena e seus componentes reais.

## Pela linha de comando

Feche o editor do Unity para evitar que o projeto fique bloqueado e execute, na
raiz do repositório:

```powershell
.\scripts\run-unity-tests.ps1 -Platform All
```

Também é possível executar somente uma suíte:

```powershell
.\scripts\run-unity-tests.ps1 -Platform EditMode
.\scripts\run-unity-tests.ps1 -Platform PlayMode
```

O script lê a versão do Unity em `ProjectSettings/ProjectVersion.txt`, procura a
instalação correspondente no Unity Hub e grava XML e logs em `TestResults/`.
Essa pasta é local e não deve ser versionada.

## Evolução da suíte

Cada regra nova do jogo deve começar por um teste que falhe pela ausência do
comportamento esperado. Depois da implementação mínima, execute Edit Mode e Play
Mode novamente para confirmar que o fluxo completo continua funcionando.
