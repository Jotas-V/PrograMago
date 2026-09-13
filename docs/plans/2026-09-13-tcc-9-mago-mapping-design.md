# TCC-9 — estado e representação do Mago

- Data: 13 de setembro de 2026
- Linear: TCC-9
- Estado do desenho: aprovado pelo usuário

## Objetivo

Transformar a instanciação validada pela TCC-8 em um Mago com cinco atributos
inteiros e saldo de pontos, visível na arena. A declaração da classe continua
representada como silhueta; apenas `new Mago(...)` cria o personagem completo.

## Fluxo

O validador permanece a única fonte de valores aceitos. Uma submissão válida
entrega `ValidatedMagoProgram` ao presenter. Para classe e atributos, ele confirma
a silhueta. Para instanciação, converte os valores por nome em um modelo imutável
com `vida`, `dano`, `alcance`, `iniciativa`, `velocidadeAtaque`, nome da instância e
pontos restantes. A view da arena atualiza o prefab existente e mostra esses
valores na interface, incluindo `Pontos restantes: X`.

A prévia da primeira atividade é temporária. Digitar ou submeter código inválido
não modifica o último estado aprovado. O Mago aprovado permanece visível ao
avançar entre as batalhas da fase 1. O texto do editor também permanece: o
jogador acrescenta atributos e construtor ao que já escreveu. Reiniciar a
batalha/fase limpa o estado da arena, mas preserva o texto para correção, como
no fluxo existente. Um desafio final de reescrita completa fica para outra issue.

## Representação

O prefab simples existente será reutilizado. Uma cor translúcida distingue a
definição da classe; a cor opaca indica uma instância. Os atributos só aparecem
após uma instanciação válida. A interface será ancorada à área da arena, sem
introduzir regras de combate nem arte final nesta issue.

## Verificação

Testes Edit Mode cobrirão associação dos valores pelo nome, orçamento parcial,
fluxo de submissão, erro após sucesso, prévia e persistência entre batalhas.
Testes Play Mode confirmarão a mudança visual, o texto e o reinício na cena real.
Cada comportamento novo seguirá teste falhando, implementação mínima e teste
passando.

O saldo de pontos é informativo: cinco atributos podem somar menos de 25. A
interface mostra `Pontos restantes: X` após uma instanciação válida, inclusive
zero. Uma distribuição inválida mantém os valores e o saldo aprovados antes.
