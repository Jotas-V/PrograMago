# TCC-48 — Diagnóstico e proposta para as instruções das fases

Data: 10/10/2026. Issue: https://linear.app/jotas/issue/TCC-48/jogo-revisar-explicacoes-tarefas-e-dicas-de-todas-as-fases

Branch: `codex/TCC-48-hints`, criada diretamente da `dev` atualizada, commit base `a04f3a3337e049e412e9331f45a3541f4f443053`.

## Pedido e etapa atual

Primeiro conferir as nove fases no jogo e depois planejar uma revisão consistente. As explicações devem ensinar o raciocínio, com exemplos, mas o jogador precisa desenvolver o código da atividade. Este documento registra o diagnóstico e uma proposta; nenhuma alteração de conteúdo ou interface de produção foi implementada nesta etapa.

## Como a conferência foi feita

- Leitura dos conteúdos, critérios, progressão das dicas, apresentação comum e tutorial específico de Inimigo.
- Unity em Play Mode: apresentação das nove fases em 1920 × 1080, inicialmente sem dicas e depois após quatro falhas registradas na progressão. Foram capturadas 18 telas e extraídos os quatro textos de ajuda de cada fase.
- Inspeção dos seis passos do tutorial da fase 5, incluindo tamanho de fonte e presença de transbordamento.
- 17 testes existentes de `LearningFlowPresenterTests` e 32 de `LearningProgressTests` passaram: 49/49.
- Os estados das fases foram preparados pela instrumentação de auditoria. Isso verifica a apresentação e a sequência de ajuda, mas não representa uma partida completa vencendo as nove batalhas, nem uma avaliação de compreensão com iniciantes.
- O save original, a cena original e as configurações temporárias da Game View foram restaurados após a conferência. Alterações preexistentes de animações e outros arquivos não pertencem a esta entrega.

Evidências locais de sessão: `tmp/tcc48-phase-*-hints-*.json`, `tmp/tcc48-tutorial-steps.json`, resultados dos testes em `tmp/tcc48-presenter-tests.json` e `tmp/tcc48-progress-tests.json`. Capturas em `Assets/tmp/tcc48-phase-*-hints-*.png`; esses arquivos temporários não fazem parte do commit.

## Problemas gerais observados

O painel comum empilha “O que é”, “Para que serve”, “Como usar” e “No jogo” em uma área fixa; depois mostra tarefa e somente a dica mais recente. Embora não tenha ocorrido transbordamento nas medições, o ajuste automático reduz a fonte para acomodar textos grandes. Na fase 8, a tarefa chega a 13,35 px e a última dica a 11,85 px. Na fase 9, a explicação chega a 14,45 px. O sexto passo da fase 5 fica em 14,3 px. Caber na caixa não garante boa leitura.

As dicas aparecem após tentativas inválidas, sem um pedido explícito de ajuda ou histórico visível. Algumas últimas dicas e exemplos iniciais fornecem a resposta literal. A fase 5 substitui os campos comuns por outro tutorial e oferece copiar a implementação completa de Inimigo; nessa fase, os campos de explicação, tarefa e dica do painel comum ficam ocultos.

## Diagnóstico e revisão por fase

| Fase | Diagnóstico atual | Revisão proposta |
| --- | --- | --- |
| 1 — Classe | O exemplo e a última dica entregam a classe Mago inteira. | Explicar molde, nome e corpo com uma classe de outro contexto. Informar o nome obrigatório na tarefa e orientar a montagem sem apresentar a declaração final. |
| 2 — Atributos privados | A última dica lista as cinco declarações completas. | Explicar atributo, tipo e acesso com um único atributo de outra classe. Listar os cinco nomes requeridos como requisitos, sem transformar a lista em código pronto. |
| 3 — Construtor e instância | O exemplo mostra a assinatura de cinco parâmetros, atribui somente vida e já mostra uma instância; parece uma implementação completa, mas está incompleto. | Separar “definir o construtor” de “criar um objeto”. Explicar atributo versus parâmetro, `this`, ordem dos argumentos, limites de 1 a 15 e soma de até 25. Usar exemplo pequeno e contextualizado. |
| 4 — Setters | O exemplo implementa setAlcance e as dicas chegam a fornecer atribuições dos cinco métodos. A tarefa fica pequena. | Diferenciar declaração dentro de Mago e chamada em Ajustes. Explicar setter com um livro. Nomear os cinco métodos exigidos, preservar construtor/instância e explicar o orçamento sem fornecer as cinco implementações. |
| 5 — Inimigo e getter | Tutorial exclusivo de seis passos com classe, construtor, getter e instância prontos; botão copia a solução completa de Inimigo. As dicas comuns estão ocultas. | Migrar para o padrão compartilhado. Separar os passos de atributos, construtor, consulta e criação. Ensinar getter como consulta que devolve um valor, contrastando com setter. Informar nome, vida e elemento exigidos em texto. Remover a entrega da solução completa. |
| 6 — Método e chamada | O exemplo entrega a declaração e a chamada da magia. A diferença entre parâmetro e objeto concreto pode ficar confusa. | Mostrar em outro contexto a diferença entre criar um comportamento e acioná-lo. Indicar onde declarar o método e onde chamar com a instância do inimigo existente. |
| 7 — Herança | Exemplo, tarefa e última dica fornecem a declaração inteira de Piromante. | Explicar relação entre classe base e especialização. Nomear os tipos requeridos, indicar a relação e esclarecer que a escolha por elemento será trabalhada depois. Evitar prometer sem explicação que todo recurso simplificado do jogo é Java completo. |
| 8 — Sobrescrita | Exemplo entrega o método inteiro; a última dica combina implementação, criação do Golem e chamada. Fonte da dica cai para 11,85 px. | Separar três ações: sobrescrever, trocar o alvo e atualizar a chamada. Explicar assinatura, anotação e reutilização via `super` com uma analogia. Dicas curtas, cada uma com uma função pedagógica. |
| 9 — Polimorfismo | Tarefa muito genérica para várias exigências. Exemplo longo contém reticências literais no meio do código. Regras de combate competem com a explicação. | Dar checklist de subclasses, construtores, métodos e seleção por elemento, alinhado ao validador. Explicar a referência comum e o alvo fornecido pelo motor. Separar regras de combate, inclusive penalidade de tentativa inválida. Usar exemplo pequeno, sem código incompleto apresentado como executável. |

## Proposta inicial (substituída pelo grimório)

Manter o painel à direita, com título e objetivo resumido sempre visíveis. Abaixo, quatro abas iguais em todas as fases:

1. **Tarefa**: aberta inicialmente; três a cinco passos curtos indicando o que fazer, onde editar, o que preservar e como concluir. Distinguir requisitos obrigatórios, ajustes opcionais e assuntos de fases futuras.
2. **Entenda**: conceito em linguagem simples, motivo de uso e relação com a ação no jogo. Apresentar termos novos antes de usá-los nas tarefas.
3. **Exemplo**: trecho curto, indentado e explicado, de outro contexto. O exemplo ensina uma estrutura; não usa o conjunto de nomes e valores que resolve a fase. Identificar se é um trecho dentro de uma classe ou um programa completo.
4. **Dicas**: botão “Preciso de uma dica”, indicador de ajuda revelada e acesso às dicas anteriores. A ajuda orienta sem preencher o editor ou copiar a solução.

Usar rolagem dentro da área de conteúdo quando necessário, preservando cabeçalho e navegação. Evitar reduzir a fonte para encaixar um parágrafo. Meta inicial: corpo de 18 px em Full HD; conferir a escala e leitura real em resoluções menores antes de fechar a medida final. Código com quebras de linha e indentação; verificar suporte de uma fonte monoespaçada já disponível.

Alternativas consideradas: um painel único com rolagem mantém todo o contexto junto, mas continua longo; um tutorial passo a passo em todas as fases aumenta a navegação e dificulta consultar um tópico anterior. As abas permitem consultar cada tipo de apoio sem repetir o tutorial exclusivo da fase 5.

## Regra para quatro dicas graduais

| Nível | Função | Limite |
| --- | --- | --- |
| 1 | Retomar o conceito ou fazer uma pergunta que direcione o raciocínio. | Não fornecer uma declaração pronta. |
| 2 | Orientar a localização e a estrutura: classe, corpo, antes/depois ou chamada externa. | Não montar todos os elementos exigidos. |
| 3 | Explicar um erro comum e a relação entre os elementos. | Não enumerar todas as linhas da solução. |
| 4 | Oferecer uma checklist de conferência dos requisitos. | Continuar sem entregar a implementação final. |

Pedir ajuda manualmente não deve registrar falha nem aplicar penalidade de combate. Separar a quantidade de dicas reveladas da quantidade de tentativas inválidas; o método atual `RegisterFailedAttempt` não deve ser reutilizado para o botão de ajuda. Preservar dicas reveladas no save com compatibilidade para saves antigos. Na falha, manter o diagnóstico específico do código e tornar a ajuda acessível; definir a associação entre erro e orientação sem substituir o diagnóstico por uma dica genérica.

## Protótipo de conteúdo — fase 4

**Objetivo:** permitir alterar um atributo privado por meio de um método da própria classe.

**Tarefa:**

1. Dentro de Mago, depois do construtor, crie os cinco métodos pedidos: setVida, setDano, setAlcance, setIniciativa e setVelocidadeAtaque.
2. Cada método deve ser público, receber um número inteiro e atualizar o atributo correspondente do objeto. Ele não precisa devolver um valor.
3. Preserve os atributos, o construtor e a criação do Mago das fases anteriores.
4. Valide o código. Quando quiser usar esses métodos para redistribuir pontos, coloque as chamadas em Ajustes, respeitando os limites dos atributos e o total de 25 pontos.

**Entenda:** um setter oferece uma forma de alterar um dado privado. O valor recebido é o parâmetro; o dado que pertence ao objeto é o atributo. A declaração do método ensina o objeto a fazer a alteração. A chamada pede que uma instância faça essa alteração.

**Exemplo de outro contexto:** trecho dentro de uma classe Livro que já possui o atributo privado paginas:

```java
public void setPaginas(int quantidade) {
    this.paginas = quantidade;
}
```

`quantidade` recebe o novo número; `this.paginas` identifica o atributo desse livro. `void` informa que o método não devolve um resultado. O jogador adapta esse raciocínio aos atributos e nomes pedidos para Mago.

**Dicas:**

1. Se o atributo é privado, quem pode alterar diretamente seu valor? Pense na responsabilidade da própria classe.
2. Esses métodos pertencem a Mago: escreva-os dentro das chaves da classe, mas fora do corpo do construtor.
3. Diferencie o valor recebido do atributo que será atualizado. Confira se cada método altera o atributo correspondente ao seu nome.
4. Confira os cinco nomes pedidos, o acesso público, um parâmetro inteiro em cada método e a ausência de retorno. As chamadas para usar os métodos ficam em Ajustes.

## Alinhamento com as regras atuais

Nomes e dados exigidos pelo motor precisam estar explícitos; esconder um requisito não torna o exercício melhor. Isso inclui os nomes das classes/métodos, dados dos inimigos, orçamento e informação de que o motor fornece `alvo`. As relações entre elementos podem ser explicadas em tabela, sem fornecer os ramos completos do código.

A fase 4 verifica os setters; nas fases seguintes, a validação atual permite omiti-los quando Ajustes está vazio. Há também migração de saves antigos que pode marcar essa fase como concluída. O texto pode recomendar preservar o aprendizado, mas não deve alegar uma exigência que o validador não aplica. Alterar essa regra ou a migração exige um escopo separado.

Erros e sucesso devem usar o mesmo vocabulário das tarefas. Na falha: explicar o que aconteceu, indicar onde revisar e sugerir a próxima ação; manter bloco/linha quando conhecidos. No sucesso: relacionar o conceito demonstrado ao próximo objetivo sem revelar a atividade seguinte inteira.

## Sequência de implementação proposta

### Decisão aprovada após o diagnóstico

O usuário aprovou substituir a apresentação detalhada à direita por um **Grimório do Mago**. O painel do jogo mantém título e tarefa curta e oferece um botão com livro em pixel art. `GrimoireScene` é uma quarta cena, carregada de forma aditiva sobre a partida. Animação curta abre o livro; cada fase corresponde a uma página lógica exibida em duas folhas: conceito à esquerda, exemplo/tarefa/dicas à direita. As nove páginas podem ser folheadas; páginas futuras exibem símbolos mágicos e uma mensagem de bloqueio, sem expor a explicação real. Concluir a fase anterior libera o conteúdo seguinte; páginas anteriores permanecem legíveis. O desbloqueio deriva do progresso salvo, não da fase selecionada. A partida permanece carregada, com código intacto e combate pausado; fechar restaura o estado de pausa anterior. O nome definitivo aprovado é “Grimório do Mago”.

Plano concreto: testar desbloqueio e ajuda persistida sem falhas; implementar modelo e persistência; escrever os conteúdos e exemplos das nove fases; construir e salvar a cena e o botão no Editor; integrar navegação aditiva e pausa; verificar retorno, páginas bloqueadas, dicas e legibilidade no Unity. O fluxo exclusivo de copiar Inimigo será substituído pela consulta do grimório.

1. Fechar o padrão de painel e de ajuda a partir deste diagnóstico.
2. Montar uma matriz editorial com objetivo, requisitos reais, local, conceito, exemplo, quatro dicas e feedback das nove fases. Conferir cada item contra os validadores.
3. Implementar o painel compartilhado e integrar a fase 5, preservando a edição livre dos blocos.
4. Separar revelação de ajuda de falhas e adaptar persistência com compatibilidade.
5. Revisar mensagens de falha, sucesso e revisão de fases concluídas.
6. Verificar as nove atividades com código desenvolvido a partir das instruções, sem copiar uma solução; conferir também dicas, navegação, progressão e saves.

## Validação da futura implementação

- Testes de progressão: revelar ajuda manual sem tentativa/falha/dano, limite de quatro dicas, histórico, restauração e troca de fase.
- Testes de integração: todas as fases usam o mesmo painel; fase 5 mantém tarefa/dicas acessíveis; abas, rolagem e botão de ajuda funcionam.
- Exercícios reais contra os validadores: cada tarefa descreve o necessário para avançar; os exemplos do outro contexto não concluem a atividade por simples colagem. Revisão pedagógica manual complementa os testes.
- Conferência visual em 1920 × 1080, 1366 × 768 e formato 4:3, com as instruções mais longas e as quatro dicas reveladas. Não ocultar texto nem usar fonte minúscula para fazê-lo caber.
- Percurso completo com progressão salva, retorno pelo mapa e revisão de fases concluídas. O diagnóstico atual não substitui essa validação final.

## Refinamento visual aprovado

O usuário pediu artes originais inspiradas nas referências, com o livro fechado de frente e o livro aberto próximo da tela. Foram gerados dois sprites exclusivos, com fundo transparente: `Grimoire-Closed-PixelArt.png` e `Grimoire-Open-PixelArt.png`. As referências não foram incorporadas ao projeto. Os sprites usam filtragem Point, sem mipmaps, e as páginas não contêm texto rasterizado: o conteúdo continua editável, acessível por rolagem e renderizado em TMP.

O botão apresenta o livro fechado flutuando, com círculos roxos e azuis e partículas orbitando em tempo não escalado. Ao abrir, uma animação curta expande o livro na cena própria. A arte aberta ocupa a maior parte da tela; as duas áreas de leitura ficam sobre o pergaminho, sem encobrir os ornamentos. O conteúdo das nove fases segue o mesmo padrão e usa exemplos de outro contexto, sem fornecer a solução completa do exercício.

Implementação na branch `codex/TCC-48-hints`, baseada em `dev` (`a04f3a3`). A consulta pausa a batalha, preserva o código e permite retornar por Fechar/Esc. As páginas futuras usam escrita mágica; desbloqueio e dicas consultadas são persistidos junto ao progresso, com compatibilidade para saves anteriores.

## Resultado da validação

### Polimento solicitado após a revisão do jogador

Escopo autorizado: preservar a proporção do livro aberto e ampliar o espaço vertical em 16:9; alinhar a silhueta pela base do sprite espectral; dar margem interna aos seletores e títulos de código; desenhar um alfabeto de runas fictícias nas páginas seladas; encaminhar Próxima fase ao mapa e remover o botão de mapa da vitória; separar Pausar e Mapa na arena. O item 7 permanece em avaliação: recomenda-se um livro maior clicável, sem o retângulo marrom, com legenda discreta e área de clique generosa; não será implementado antes da escolha do usuário.

Plano: adicionar verificações dos problemas; corrigir geometria e navegação; aplicar alterações pontuais às cenas existentes; conferir 16:9/4:3, runas, silhueta, combate e vitória; executar regressão das partes afetadas.

- Treze testes específicos do grimório aprovados: três de progresso/persistência, dois das artes importadas e oito de integração. Os quatro testes adicionais verificam proporção e runas desenhadas, silhueta acima do solo, margens dos controles e separação entre pausa/mapa, além da transição da vitória ao mapa sem botão redundante. Foram observadas as falhas antes das correções e a aprovação depois.
- Regressão: LearningProgress 32/32; LearningFlowPresenter 17/17; LevelMapScene 9/9; GameplaySceneAsset 20/22; GameplayScene 60/61. Total com os testes do grimório: 151/154. Os testes de progressão agora percorrem o mapa entre fases, conforme o fluxo solicitado.
- As três falhas restantes já estavam presentes: expectativa do sprite antigo `Mago-pixel.png`; preparação da cena não idempotente (164 objetos esperados, 169 na execução atual); enquadramento do cenário sob pai escalado (0,464241 contra 0,5 na execução atual, dependente do formato da Game View). Elas não foram mascaradas nem tiveram seus testes removidos.
- Conferidas as nove páginas no início/fim da rolagem em 1920 × 1080, quatro dicas da última fase, página futura selada, formato 4:3 em 1440 × 1080 e 1366 × 768. A revisão com código inválido encontrou sobreposição do botão com o diagnóstico; isso foi corrigido e ganhou teste de regressão.
- Save pessoal, cena e configurações da Game View foram restaurados após as conferências. Os testes existentes exercitam construção do Mago, setters, Inimigo/getter, magia, herança, sobrescrita e batalha final com recarga do progresso; a revisão pedagógica do jogador continua sendo parte da avaliação humana.
- A revisão do polimento conferiu o livro e as runas em 16:9, o livro em 4:3, a silhueta, o espaçamento do rodapé, pausa/mapa e vitória/mapa. O item 7 recebeu apenas uma prévia temporária em Play Mode, com livro ampliado sem fundo/moldura marrom; o botão salvo permanece como estava, aguardando avaliação.

### Item 7 aprovado: livro maior sem moldura

Após avaliar a prévia, o jogador autorizou retirar o fundo e a moldura marrom e ampliar ainda mais apenas a arte do grimório. A legenda mantém o limite de fonte de 28 e recebe a cor de tinta do pergaminho. O livro e seus círculos mágicos ocupam uma área maior abaixo da tarefa; o botão conserva uma área transparente de clique e a própria imagem recebe cliques inclusive onde ultrapassa o retângulo do botão. A abertura, a flutuação e os efeitos de interação permanecem. A apresentação é aplicada tanto à cena salva quanto na inicialização, para que o estilo global dos botões não restaure a moldura.

Plano de conferência: comparar visualmente com a prévia em 16:9 e 4:3, verificar a distância da tarefa e do diagnóstico, clicar para abrir/fechar a cena do grimório e executar os testes de integração existentes.

Resultado: arte aproximadamente um terço maior que na prévia em 16:9, legenda com o mesmo tamanho, fundo e borda removidos; conferidos os dois formatos e abertura/retorno pelo botão. Os oito testes de integração do grimório passaram novamente. Save e configurações da revisão visual foram restaurados.

Refinamento de espaçamento aprovado: deslocar livro, efeitos e legenda 20 unidades de UI para baixo, mantendo suas dimensões e a fonte, para dar mais distância ao texto da tarefa. A área de diagnóstico permanece reservada.

Revisão seguinte: livro, efeitos e legenda descem mais 10 unidades (deslocamento total de 30). A silhueta agora encosta os pés visíveis no solo: o cálculo desconta os 148 pixels transparentes abaixo dos pés na arte de 1254 pixels e remove o movimento de flutuação vertical. O teste de contato dos pés falhou antes da correção (3,119 contra solo 2,9) e verifica o alinhamento em vários frames.

Conferência visual desta revisão em 16:9 e 4:3 e abertura/retorno pelo botão aprovados; oito testes de integração passaram, incluindo o contato dos pés corrigido. Save pessoal e configurações da revisão restaurados.
