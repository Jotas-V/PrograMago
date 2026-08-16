# PrograMago — desenho do protótipo

- Data: 16 de agosto de 2026
- Linear: TCC-1
- Engine: Unity 6000.3.11f1
- Formato: jogo 2D em pixel art
- Estado: aprovado para planejamento e implementação

## 1. Definição acadêmica

### Tema

Desenvolvimento e avaliação de um protótipo de jogo educacional para apoiar a
aprendizagem de conceitos fundamentais de programação orientada a objetos.

### Título

Desenvolvimento e avaliação de um protótipo de jogo educacional para a
aprendizagem de classes, objetos, encapsulamento, herança e polimorfismo.

### Pergunta-problema

De que maneira a utilização de um protótipo de jogo educacional pode apoiar a
compreensão e a prática dos conceitos de classes, objetos, encapsulamento,
herança e polimorfismo por estudantes de programação?

### Objetivo geral

Desenvolver e avaliar um protótipo de jogo educacional que relacione a escrita
de código aos seus efeitos visuais em uma arena, visando apoiar a compreensão e
a prática de classes, objetos, encapsulamento, herança e polimorfismo.

### Objetivos específicos

1. Investigar dificuldades relatadas na literatura sobre a aprendizagem de
   programação orientada a objetos.
2. Analisar jogos educacionais como ferramentas de apoio ao ensino e à prática
   de programação.
3. Delimitar classes, objetos, atributos, métodos, construtores, `this`,
   encapsulamento, herança, sobrescrita e polimorfismo como conteúdo do
   protótipo.
4. Projetar mecânicas que relacionem o código escrito às ações dos personagens
   na arena.
5. Implementar na Unity um protótipo 2D que permita criar um mago, definir seu
   estado e comportamento e especializá-lo em subclasses.
6. Validar a correção e a clareza do conteúdo com professores ou profissionais
   da área.
7. Aplicar o protótipo com estudantes que tenham contato com programação
   orientada a objetos.
8. Analisar indícios de aprendizagem, motivação, usabilidade e percepção dos
   participantes.

## 2. Natureza e limite da entrega

PrograMago será um protótipo funcional e experimental, não um jogo comercial
completo. A entrega deve comprovar a viabilidade da relação entre código,
aprendizagem e resposta visual. Os resultados da pesquisa serão tratados como
indícios obtidos com o grupo participante, sem generalização indevida.

O protótipo deve conter uma experiência curta e completa. Não são requisitos:

- campanha extensa;
- inventário, equipamentos ou economia;
- áudio completo e animações sofisticadas;
- invocações e sistemas complexos de aprimoramento;
- balanceamento de um produto comercial;
- execução real ou suporte integral à linguagem Java;
- composição, membros estáticos, sobrecarga, casting, classes abstratas,
  interfaces, SOLID ou injeção de dependência como conteúdos avaliados.

## 3. Direção visual e layout

O jogo será produzido em 2D e utilizará pixel art nos personagens, inimigos,
magias, arena e elementos visuais de apoio.

A experiência principal ocorrerá em uma única tela:

- a arena ocupa aproximadamente os 30% superiores;
- o mago ativo aparece à esquerda da arena;
- um ou mais inimigos aparecem à direita;
- o editor ocupa a maior parte da região inferior;
- um painel lateral apresenta objetivo, explicação, ficha do inimigo, erros e
  dicas.

O editor é o principal espaço de interação. A arena oferece resposta visual
imediata sem competir com a leitura e a escrita do código.

## 4. Estrutura de ensino

Antes de cada tarefa, a explicação de um conceito deve responder:

1. O que é?
2. Para que serve?
3. Como utilizar?
4. Como aparece no jogo?
5. O que o jogador deve fazer?

Depois da explicação, o jogador escreve o código e recebe validação. As dicas
são progressivas:

1. explicação do erro ou retomada do conceito;
2. indicação da região problemática;
3. exemplo parcial;
4. solução completa apenas depois de novas tentativas.

Essa estrutura deve ser aplicada a classes, atributos, métodos, objetos,
construtores, `this`, encapsulamento, herança, sobrescrita, `super` e
polimorfismo.

## 5. Progressão pedagógica

### Fase 1 — Construção do mago

O jogador declara a classe `Mago`, cria atributos privados, escreve o
construtor, utiliza `this` e instancia um objeto. Os atributos iniciais são vida,
dano, alcance e velocidade. Um limite configurável de pontos preserva o
balanceamento.

Quando a instanciação é validada, o mago aparece na arena e suas estatísticas
refletem os valores escritos.

Conteúdos: classe, objeto, atributos, construtor, `this` e encapsulamento.

### Fase 2 — Comportamento e primeiro inimigo

O jogador cria a classe `Inimigo` com nome, vida e elemento privados, escreve o
construtor e instancia um Boneco de Treinamento. Também declara, completa,
posiciona e chama o método `lancarMagia(Inimigo inimigo)`.

O método `getElemento()` demonstra uma aplicação concreta do encapsulamento:
outro objeto consulta o elemento sem acessar diretamente o atributo privado.

Conteúdos: métodos, parâmetros, chamadas, interação entre objetos e reforço de
encapsulamento.

### Fase 3 — Especialização

O jogador trabalha com três subclasses de `Mago`:

- `Piromante`, com magia de fogo;
- `Hidromante`, com magia de água;
- `Eletromante`, com magia elétrica.

As subclasses usam `extends Mago`, sobrescrevem `lancarMagia()` e podem recorrer
ao comportamento herdado com `super`. Os corpos mais complexos dos métodos
podem oferecer exemplos ou trechos permitidos, preservando a decisão do jogador
sem exigir um interpretador Java completo.

Conteúdos: herança, sobrescrita e `super`.

### Fase 4 — Polimorfismo e batalha final

O jogador instancia as três subclasses utilizando referências do tipo `Mago`.
O quadrado à esquerda da arena representa o mago ativo e muda de aparência
conforme a referência selecionada.

A mesma variável do tipo `Mago` pode receber `Piromante`, `Hidromante` ou
`Eletromante`. A chamada `magoAtivo.lancarMagia(inimigo)` produz um
comportamento visual diferente conforme o objeto concreto.

A dificuldade aumenta gradualmente:

1. Boneco de Treinamento sozinho;
2. um inimigo elemental;
3. dois inimigos de elementos diferentes;
4. batalha final com todos os tipos de inimigo.

Conteúdo principal: polimorfismo.

## 6. Magos e inimigos

### Hierarquia dos magos

`Mago` é a classe base. `Piromante`, `Hidromante` e `Eletromante` são suas
subclasses. Cada subclasse implementa de maneira diferente a mesma operação de
lançar magia.

Na batalha final, a estratégia pode usar `inimigo.getElemento()` para atribuir
uma subclasse à referência `magoAtivo`:

- gelo seleciona o Piromante;
- fogo seleciona o Hidromante;
- água seleciona o Eletromante;
- neutro aceita o comportamento comum do Mago.

Condicionais simples funcionam como conhecimento prévio e recurso de estratégia.
Elas não integram os resultados de aprendizagem avaliados.

### Objetos inimigos

Todos os inimigos são objetos da mesma classe `Inimigo`, demonstrando que uma
classe pode produzir instâncias com estados diferentes:

- Boneco de Treinamento: elemento neutro;
- Golem de Gelo: vulnerável a fogo;
- Elemental de Fogo: vulnerável a água;
- Slime Aquático: vulnerável a eletricidade.

O jogador cria a classe `Inimigo` uma vez. Depois, instancia progressivamente os
objetos a partir de fichas apresentadas pelo jogo. A primeira ficha oferece mais
orientação; as seguintes reduzem o apoio. A batalha final mantém vários objetos
`Inimigo` ativos simultaneamente.

A Unity armazena e percorre a coleção internamente. O jogador não precisa
aprender vetores, listas ou laços como parte deste protótipo.

## 7. Subconjunto da sintaxe Java

O jogador escreve código com aparência coerente com Java, linguagem utilizada
pelo público da instituição. O protótipo não compila nem executa esse código.
Ele reconhece somente construções necessárias às fases:

- declaração de classes;
- atributos privados e tipos permitidos;
- construtores, parâmetros e `this`;
- instanciação com `new`;
- declaração e chamada de métodos;
- `extends`, `super` e `@Override`;
- condicionais simples e chamadas oferecidas pelo domínio do jogo.

Expressões e ações aceitas por condicionais devem ser limitadas ao domínio do
protótipo. Espaços e quebras de linha válidos não podem alterar o resultado da
análise.

## 8. Arquitetura

O fluxo principal é:

1. o gerenciador de aprendizagem apresenta conceito e tarefa;
2. o jogador escreve no editor;
3. o analisador divide o texto em tokens e aplica regras específicas da fase;
4. erros retornam ao painel de explicações e dicas;
5. um código válido gera um modelo interno de magos, inimigos e estratégia;
6. o modelo cria ou atualiza os elementos visuais da arena;
7. a batalha automática executa e apresenta o resultado;
8. o resultado libera repetição, correção ou progressão.

### Componentes

- Gerenciador de aprendizagem: fases, textos, tarefas e dicas.
- Editor: entrada de código e destaque de regiões problemáticas.
- Analisador: tokenização e regras do subconjunto Java por fase.
- Modelo de domínio: dados extraídos de magos, inimigos e estratégia.
- Arena: instanciação de prefabs, posicionamento e resposta visual.
- Batalha: alvo atual, dano, alcance, velocidade e vulnerabilidades.
- Registro da sessão: tentativas, erros, dicas, tempo e conclusão.

O código escrito pelo jogador nunca é executado diretamente no computador.

## 9. Validação e mensagens de erro

O protótipo deve distinguir:

- erro sintático: chave, parêntese ou ponto e vírgula ausente;
- erro estrutural: método ou construtor na posição incorreta;
- erro conceitual: atributo público quando a tarefa exige encapsulamento;
- regra do jogo: valores fora do limite configurado;
- erro comportamental: magia incompatível com a subclasse ou inimigo.

As mensagens devem explicar o problema em linguagem educacional e relacioná-lo
ao conceito atual. O sistema não deve apenas informar que o código está errado.

## 10. Avaliação científica

### Validação de conteúdo

Professores ou profissionais da área verificam correção conceitual, clareza,
adequação dos exemplos em Java e coerência entre código e resposta visual.

### Aplicação com estudantes

1. apresentação da pesquisa e caracterização dos participantes;
2. pré-teste sobre os conteúdos do protótipo;
3. utilização do jogo;
4. pós-teste com questões equivalentes;
5. questionário de percepção e usabilidade;
6. comentários abertos.

O pré-teste e o pós-teste avaliam classe e objeto, atributos e métodos,
encapsulamento, construtores e instanciação, herança, sobrescrita e
polimorfismo.

O questionário investiga facilidade de uso, clareza, relação entre código e
arena, motivação, percepção de aprendizagem e aspectos difíceis ou divertidos.
Antes da coleta, devem ser confirmadas com o orientador as exigências da
instituição para pesquisas com participantes.

### Dados do jogo

Quando tecnicamente viável, o protótipo registra de forma adequada à pesquisa:

- fases concluídas;
- quantidade de tentativas;
- erros mais frequentes;
- dicas utilizadas;
- tempo por fase;
- inimigos derrotados.

## 11. Verificação técnica

O analisador deve possuir testes para códigos válidos e inválidos, incluindo:

- espaços e quebras de linha diferentes;
- chaves ou terminadores ausentes;
- tipos incorretos;
- construtor com nome errado;
- atributos públicos indevidos;
- uso incorreto de `this`;
- atributos acima do limite;
- herança sem `extends Mago`;
- sobrescrita com assinatura incompatível;
- instanciações de inimigos não permitidas;
- elemento incompatível com o inimigo.

Também devem ser verificados o mapeamento do código para os prefabs, as
estatísticas, vulnerabilidades, troca do mago ativo, progressão, dicas e batalha
com vários inimigos.

## 12. Critérios de conclusão do protótipo

O protótipo estará pronto para a aplicação quando:

- ensinar os conceitos previstos antes das tarefas;
- receber e analisar o subconjunto definido de Java;
- apresentar erros e dicas progressivas;
- criar magos e inimigos a partir do código validado;
- refletir atributos na arena;
- executar três subclasses com comportamentos distintos;
- demonstrar visualmente herança, sobrescrita e polimorfismo;
- manter vários inimigos na batalha final;
- executar a batalha automática;
- registrar os dados mínimos necessários para a avaliação;
- completar um teste piloto sem bloqueios críticos.

## 13. Cronograma paralelo de seis semanas

O desenvolvimento e a escrita do TCC avançam juntos.

| Semana | Protótipo | Pesquisa científica |
| --- | --- | --- |
| 1 | Interface e prova do analisador | Introdução, problema, objetivos e justificativa |
| 2 | Mago, atributos, construtor e instanciação | Revisão sobre dificuldades em POO |
| 3 | Métodos, Inimigo e primeira batalha | Revisão sobre jogos educacionais |
| 4 | Subclasses, magias e polimorfismo | Metodologia e instrumentos de avaliação |
| 5 | Múltiplos inimigos, ensino, dicas e registros | Validação dos instrumentos e preparação da aplicação |
| 6 | Testes, piloto e correções | Revisão do texto e preparação da coleta |

Resultados, discussão e conclusão são escritos após a aplicação e a análise dos
dados.

## 14. Principais riscos e contenções

- Interpretador excessivamente amplo: limitar tokens, estruturas e ações por
  fase.
- Excesso de conteúdo: manter somente os conceitos aprovados como obrigatórios.
- Jogo pouco divertido: priorizar decisão, resposta imediata, progressão e
  consequência visual em vez de quantidade de sistemas.
- Sobrecarga textual: usar explicações curtas e progressivas no painel lateral.
- Atraso na pesquisa: desenvolver instrumentos e referencial em paralelo ao
  protótipo.
- Dados insuficientes: executar piloto, registrar telemetria básica e combinar
  medidas de conhecimento e percepção.
