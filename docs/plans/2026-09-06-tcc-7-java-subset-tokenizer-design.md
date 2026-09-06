# TCC-7 — tokenizador do subconjunto Java

- Data: 6 de setembro de 2026
- Linear: TCC-7
- Estado do desenho: aprovado

## Objetivo

Expandir o tokenizador mínimo existente para reconhecer todo o vocabulário
necessário às oito batalhas pedagógicas do PrograMago. A análise continua sendo
local, determinística e educacional: o jogo não compila nem executa Java real.

## Recorte léxico

O `CodeTokenizer` continuará percorrendo a entrada uma única vez e produzirá
tokens na mesma ordem do texto, com lexema, deslocamento, linha e coluna.

O vocabulário aceito será:

- palavras-chave `public`, `private`, `class`, `new`, `this`, `extends`,
  `super`, `return` e `void`;
- tipos `int`, `float` e `String`;
- identificadores iniciados por letra ou `_`, seguidos por letras, dígitos ou
  `_`;
- literais inteiros, decimais e strings entre aspas duplas;
- símbolos `{`, `}`, `(`, `)`, `;`, `,`, `.`, `=`, e `@`.

`@Override` será representado pelo símbolo `@` seguido do identificador
`Override`. Os lexemas de strings incluem as aspas originais; nenhum token
reescreve o código do jogador.

Comentários, caracteres literais, booleanos, operações aritméticas,
comparações e condicionais permanecem fora do escopo. As formas permitidas de
`if` e seus operadores serão definidas na TCC-17.

## Componentes e compatibilidade

O trabalho permanece no assembly `PrograMago.Language` e evolui as abstrações
já usadas pelo editor:

- `TokenKind` recebe os novos tipos de token;
- `CodeTokenizer` reconhece palavras, números, strings e símbolos;
- `Token`, `SourcePosition`, `Diagnostic` e `TokenizationResult` mantêm seus
  contratos atuais;
- `ClassDeclarationValidator` continua funcionando sem alteração porque os
  tipos de token da declaração vazia são preservados.

Não haverá tabela configurável, parser, árvore sintática ou dependência da
Unity nesta entrega. Os validadores das TCC-8, TCC-11, TCC-15 e TCC-17 serão
responsáveis por decidir se uma sequência de tokens é válida para cada
atividade.

## Fluxo e erros

1. Espaços, tabulações e quebras de linha são ignorados entre tokens, mas
   atualizam a posição corrente.
2. Palavras completas são classificadas como palavra-chave, tipo ou
   identificador.
3. Números aceitam dígitos e, opcionalmente, uma parte decimal com dígitos dos
   dois lados do ponto.
4. Strings terminam na próxima aspa dupla e não atravessam linhas.
5. Símbolos permitidos produzem um token próprio.
6. Qualquer trecho não reconhecido encerra a tokenização com um diagnóstico e
   sem tokens parciais.

Diagnósticos léxicos distinguem caractere inesperado, string não terminada e
número malformado. Todos apontam para a posição original que iniciou o
problema. Sequências de escape em strings não serão aceitas nesta entrega.

## Testes

O desenvolvimento seguirá RED–GREEN–REFACTOR em Edit Mode. A suíte cobrirá:

- classificação de todas as palavras-chave e tipos;
- identificadores e fronteiras entre palavras;
- literais inteiros, decimais e strings com espaços e acentos;
- todos os símbolos necessários às oito batalhas;
- equivalência entre diferentes formatações de espaço e quebra de linha;
- posições após `LF`, `CRLF` e tabulação;
- caractere inesperado, string não terminada, sequência de escape e número
  malformado;
- regressão da declaração `public class Mago {}` e das suítes Edit Mode e Play
  Mode existentes.

## Fora do escopo

- validação sintática ou conceitual das atividades;
- criação de AST ou interpretação do programa;
- suporte geral à especificação Java;
- execução ou compilação do texto digitado;
- alterações na interface, arena ou progressão pedagógica.
