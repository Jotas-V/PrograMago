# TCC-6 — gerenciador de fases e conteúdo pedagógico

- Data: 6 de setembro de 2026
- Linear: TCC-6
- Estado do desenho: aprovado

## Objetivo

Implementar a progressão pedagógica do PrograMago como uma sequência de oito
batalhas orientadas por dados. Cada batalha apresenta um conceito, uma tarefa,
dicas progressivas e uma revisão após a vitória. Validar o código apenas inicia
o combate; a próxima atividade só é liberada quando o sistema de batalha
informa que todos os inimigos foram eliminados.

O incremento prepara os contratos de vitória e derrota para a TCC-12, sem
implementar combate, dano, vida ou balanceamento.

## Progressão pedagógica

As quatro fases macro do documento do protótipo são divididas em oito batalhas
curtas, adequadas a uma experiência completa de aproximadamente 15 a 30
minutos.

1. **O nascimento do Mago — Classe**
   - Tarefa: declarar `public class Mago {}`.
2. **Estado protegido — Atributos e encapsulamento**
   - Tarefa: adicionar atributos privados ao `Mago`.
3. **Dando vida ao Mago — Construtor, `this` e objeto**
   - Tarefa: criar o construtor e instanciar o mago.
4. **Surge um inimigo — Reforço de classe, atributos e objetos**
   - Tarefa: criar e instanciar `Inimigo`.
5. **Primeira magia — Métodos, parâmetros e chamadas**
   - Tarefa: implementar e chamar `lancarMagia`.
6. **Especialização elemental — Herança**
   - Tarefa: criar uma subclasse com `extends Mago`.
7. **Cada mago, uma magia — Sobrescrita e `super`**
   - Tarefa: especializar o comportamento das subclasses.
8. **Batalha final — Polimorfismo**
   - Tarefa: usar uma referência `Mago` com diferentes subclasses.

As oito atividades serão cadastradas nesta entrega. Somente o critério da
primeira batalha já possui validação de linguagem. Os demais critérios ficam
representados nos dados e não podem ser aceitos indevidamente pelo validador
atual.

## Conteúdo orientado por dados

Um `LearningPathAsset` baseado em `ScriptableObject` armazena a coleção
ordenada de batalhas. Esse formato permite revisar os textos pelo Inspector e
versioná-los junto ao projeto sem recompilar regras de domínio.

Cada definição de batalha contém:

- identificador estável, arco e ordem;
- título e nome do conceito;
- explicação do que é;
- explicação de para que serve;
- exemplo de como usar;
- efeito esperado no jogo;
- tarefa do jogador;
- coleção ordenada de dicas progressivas;
- identificador do critério de validação;
- título, conquista e resumo apresentados após a vitória.

O asset da Unity é convertido, durante a composição, em modelos imutáveis do
domínio. Campos obrigatórios vazios, identificadores repetidos, ordem inválida,
ausência de dicas ou critérios desconhecidos são erros de configuração.

## Estado e progressão

O gerenciador mantém a batalha atual e controla as seguintes etapas:

1. `Editing`: conteúdo e editor disponíveis.
2. `BattleInProgress`: código validado e batalha automática iniciada.
3. `VictoryReview`: vitória confirmada e revisão exibida.
4. `JourneyCompleted`: revisão da oitava batalha concluída.

Uma submissão válida deve identificar explicitamente qual critério satisfez.
O gerenciador só inicia a batalha quando esse critério corresponde ao exigido
pela atividade atual. Isso impede que a declaração simples de `Mago` libere
atividades futuras de atributos, herança ou polimorfismo.

O sistema de combate da TCC-12 chamará o contrato de resultado. Uma vitória
move o fluxo para `VictoryReview`; uma derrota reinicia a batalha atual sem
desbloquear conteúdo. O botão **Próxima batalha** funciona apenas durante a
revisão de vitória. Após a oitava vitória, a interface mostra a conclusão da
jornada em vez de tentar acessar uma nona definição.

## Interface pedagógica

O `TutorialPanel` recebe os textos da batalha atual por uma interface de
apresentação. A composição visual deve permitir ler o conteúdo sem bloquear o
editor. Dicas são liberadas gradualmente após tentativas inválidas e nunca
substituem o diagnóstico específico do analisador.

Um único overlay de vitória, oculto no estado inicial, é reutilizado em todas
as batalhas. Ele ocupa a tela inteira com um fundo preto semitransparente, que
mantém a arena e o código perceptíveis, porém escurecidos. Um card opaco aparece
centralizado sobre esse fundo e concentra a mensagem de conclusão. Seus dados
variáveis são:

- título de conclusão;
- descrição da conquista;
- resumo do conteúdo aprendido.

O botão **Próxima batalha** permanece fixo no card. O fundo semitransparente
bloqueia interações com o jogo enquanto a revisão estiver aberta. Ao avançar, o
painel é atualizado com os dados seguintes e o editor inicia vazio. A cena não
precisa ser alterada ou reconfigurada manualmente.

O mapa de seleção, desbloqueio e replay foi separado na subissue TCC-37.

## Integração com batalha

A TCC-6 expõe uma fronteira explícita para o resultado do combate:

- vitória: apresenta revisão e libera a ação de avançar;
- derrota: reinicia a batalha atual preservando o código;
- nenhum resultado: mantém a batalha em andamento e não libera progressão.

A implementação real de inimigos, dano, vida, curva de dificuldade e animações
permanece na TCC-12. Não haverá botão ou simulação de vitória na interface de
produção; os testes chamarão o contrato diretamente.

## Reinício da batalha atual

O jogador pode reiniciar a batalha atual mantendo a tecla `R` pressionada por
cinco segundos contínuos. O comando obedece às seguintes regras:

- exibe feedback visual do tempo restante;
- soltar `R` antes do limite cancela e zera a contagem;
- dispara uma vez por pressionamento e exige que a tecla seja solta antes de
  aceitar nova tentativa;
- usa tempo não afetado pela escala de tempo da Unity;
- preserva o código digitado, a batalha atual e as dicas já liberadas;
- limpa arena, inimigos, feedback transitório e estado do combate;
- retorna a `Editing`, exigindo novo clique em **Batalhar**;
- não remove conclusões ou desbloqueios anteriores;
- fica desabilitado durante a revisão de vitória.

Uma derrota usa a mesma operação de reinício. A lógica temporal do gesto fica
separada da leitura do teclado para permitir testes determinísticos.

## Tratamento de erros

Tentativas de avançar antes da vitória, concluir uma batalha que não está em
andamento ou iniciar uma batalha com critério incompatível são rejeitadas sem
alterar o estado. Entradas inválidas no editor continuam usando os diagnósticos
existentes e também registram uma tentativa para a progressão das dicas.

Erros no asset pedagógico impedem a inicialização do fluxo e geram uma mensagem
clara de configuração. Nenhum erro de dados pode desbloquear uma batalha ou
apagar o código do jogador.

## Estratégia de testes

O trabalho seguirá RED–GREEN–REFACTOR:

1. testes do modelo de conteúdo para campos obrigatórios, dicas, ordem e
   identificadores;
2. testes do gerenciador para transições, critério incompatível, vitória,
   derrota, avanço, reinício e conclusão da oitava batalha;
3. testes do controlador de pressionamento contínuo para limite de cinco
   segundos, cancelamento e disparo único;
4. testes do presenter para carregar conteúdo, revelar dicas, iniciar batalha,
   apresentar revisão e preservar o editor no reinício;
5. testes do asset garantindo as oito atividades e seus conteúdos obrigatórios;
6. testes da cena para referências serializadas e overlay inicialmente oculto;
7. testes Play Mode para atualização do painel, resultado de vitória, botão de
   avanço e reinício por pressionamento contínuo;
8. execução completa das suítes Edit Mode e Play Mode.

Cada comportamento novo deve falhar pela ausência da funcionalidade antes da
implementação mínima correspondente.

## Fora do escopo

- mapa visual e seleção de batalhas, tratados na TCC-37;
- combate, dano, vida, inimigos e balanceamento, tratados na TCC-12;
- implementação dos validadores de atributos, construtores, métodos, herança,
  sobrescrita e polimorfismo;
- persistência do progresso entre execuções do jogo;
- campanha além das oito batalhas aprovadas.
