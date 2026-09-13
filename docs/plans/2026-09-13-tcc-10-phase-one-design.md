# TCC-10 — conclusão jogável da fase 1

- Data: 13 de setembro de 2026
- Linear: TCC-10
- Estado do desenho: aprovado pelo usuário

## Resultado

O jogador conclui, sem intervenção do desenvolvedor, as três batalhas do primeiro capítulo: declarar `Mago`, proteger seus cinco atributos e construir uma instância com `this`. A terceira vitória exibe “Fase 1 concluída” no overlay existente e encerra o trecho jogável. O capítulo 2 permanece cadastrado, mas não é liberado enquanto seus validadores e combate não estiverem prontos.

## Fluxo e representação

Explicação, tarefa, editor, diagnóstico e dicas progressivas usam os contratos atuais. Uma submissão inválida registra tentativa e libera a próxima dica. As três atividades usam vitória automática após validação. O código digitado acompanha o jogador; a arena preserva a última versão aprovada do Mago e seus cinco atributos. Um sprite inicial em pixel art substitui a forma básica do prefab e mantém o tratamento translúcido da silhueta.

Ao vencer a terceira batalha, o overlay de revisão informa a conclusão da fase e apresenta o total de tentativas. Não há botão funcional que leve à atividade incompleta do capítulo 2.

## Registro e retomada

Um registro local em JSON guarda as tentativas por identificador de batalha, quais das três batalhas foram concluídas, o código em edição e a última versão aprovada. A inicialização valida o registro contra o caminho pedagógico, retoma a primeira atividade pendente e reconstrói a arena revalidando o código aprovado. Se a fase já foi concluída, a revisão final e o Mago aprovado são restaurados. Um arquivo ausente inicia um jogo novo; dados inválidos geram aviso e não bloqueiam a cena.

O registro é salvo após submissões e vitórias, ao avançar e quando a aplicação pausa ou encerra. O texto do editor também é salvo durante a edição para que a retomada não perca o trabalho do jogador. Escritas usam substituição segura do arquivo para evitar registros parciais.

## Verificação

Testes Edit Mode cobrem contagem de tentativas, avanço restrito ao capítulo 1, snapshots e restauração. Testes Play Mode exercitam as três atividades pela interface, os erros com dicas, os atributos do Mago, o marco de conclusão e a retomada local. A suíte completa Edit Mode e Play Mode encerra a validação. As alterações locais pré-existentes em `SampleScene.unity` e `tmp/` serão preservadas.
