# Plano de implementação — tokenizador do subconjunto Java

## 1. Verificar a base

1. Executar a suíte Edit Mode antes das alterações.
2. Confirmar que o tokenizador mínimo e o validador de classe estão verdes.

## 2. Palavras-chave e tipos

1. Adicionar testes que exijam a classificação das palavras-chave e dos tipos
   das oito batalhas.
2. Executar os testes e confirmar a falha pela classificação atual como
   identificador.
3. Acrescentar os tipos necessários ao `TokenKind` e a classificação mínima ao
   `CodeTokenizer`.
4. Executar novamente os testes até ficarem verdes.

Arquivos principais:

- `Assets/PrograMago/Tests/EditMode/Language/CodeTokenizerTests.cs`
- `Assets/PrograMago/Runtime/Language/TokenKind.cs`
- `Assets/PrograMago/Runtime/Language/CodeTokenizer.cs`

## 3. Símbolos das construções pedagógicas

1. Adicionar testes para parênteses, ponto e vírgula, vírgula, ponto,
   atribuição e anotação.
2. Confirmar a falha do scanner atual no primeiro símbolo novo.
3. Implementar o mapeamento explícito dos símbolos.
4. Executar a suíte focal até ficar verde.

## 4. Literais numéricos

1. Adicionar testes para inteiros e decimais com lexema e posição.
2. Confirmar a falha por caractere numérico não reconhecido.
3. Implementar a leitura de números sem converter ou normalizar o lexema.
4. Adicionar um teste de número malformado, confirmar a falha esperada e
   implementar o diagnóstico léxico específico.

## 5. Literais de string

1. Adicionar teste para string com espaços e caracteres acentuados.
2. Confirmar a falha por aspas não reconhecidas.
3. Implementar strings de uma linha preservando as aspas no lexema.
4. Em ciclos separados, adicionar testes falhos para string não terminada e
   sequência de escape fora do escopo, seguidos dos diagnósticos mínimos.

## 6. Posições e entrada integral

1. Cobrir posições depois de `LF`, `CRLF` e tabulação.
2. Atualizar o teste de caractere inesperado para um símbolo realmente fora do
   novo vocabulário.
3. Confirmar que `publicclass` continua sendo um identificador único e que
   sufixos inválidos em números não são descartados.

## 7. Verificação final

1. Executar todos os testes Edit Mode.
2. Executar todos os testes Play Mode para verificar a integração existente.
3. Executar `git diff --check` e revisar o diff restrito à TCC-7.
4. Registrar no Linear os testes executados, arquivos alterados e decisões de
   escopo.
5. Mover a TCC-7 para Done somente se todas as verificações estiverem verdes.
