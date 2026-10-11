using UnityEditor;
using PrograMago.UnityIntegration;

namespace PrograMago.Editor
{
    // Authoring source for the serialized learning path, shared by gameplay and the book.
    public static class TCC48GrimoireContent
    {
        private sealed class Page
        {
            public string concept, explanation, purpose, example, effect, task;
            public string[] hints;
            public Page(string concept, string explanation, string purpose, string example, string effect, string task, params string[] hints)
            { this.concept = concept; this.explanation = explanation; this.purpose = purpose; this.example = example; this.effect = effect; this.task = task; this.hints = hints; }
        }
        private static readonly Page[] Pages = {
            new Page("Classe, objeto e primeira declaração.",
                "Imagine uma ficha em branco que define quais informações um personagem pode ter. Ela funciona como um modelo para criar personagens.\n\nUma <b>classe</b> é esse modelo: define as características e os comportamentos que objetos daquele tipo poderão ter.\n\nUm <b>objeto</b> é uma criação concreta feita a partir do modelo. Cada objeto pode ter seus próprios valores. A classe Mago poderá definir que magos têm vida e lançam magia. Um mago específico criado a partir dela será um objeto, com sua própria vida.\n\nHoje vamos declarar apenas o modelo, com o corpo vazio. Nas próximas fases, adicionaremos informações, criaremos um objeto e ensinaremos esse mago a agir.",
                "Classes organizam o código. Um mesmo modelo pode dar origem a vários objetos: dois magos podem compartilhar os mesmos tipos de características, mas ter quantidades diferentes de vida.",
                "Uma classe em outro contexto:\n\n<noparse>public class Livro {\n}</noparse>\n\n<b>public</b>: permite que outras partes do programa acessem a classe.\n<b>class</b>: declara uma classe.\n<b>Livro</b>: nome do modelo.\n<b>Chaves</b>: delimitam o corpo onde escreveremos suas características e comportamentos.\n\nIsso declara o modelo Livro; ainda não cria um livro específico. Adapte a estrutura ao nome pedido na tarefa.",
                "Validar a declaração apresenta o Mago na arena. Você ainda não precisa escrever atributos nem criar uma instância.",
                "1. No bloco Mago, declare uma classe pública chamada <b>Mago</b>, com M maiúsculo.\n2. Abra e feche seu corpo com chaves; deixe-o vazio.\n3. Clique em <b>Validar código</b>.",
                "Você está criando um modelo. Qual palavra do exemplo indica a declaração de uma classe?",
                "Antes do nome, indique acesso público e declaração de classe.",
                "Confira o nome Mago e as chaves que delimitam o corpo.",
                "Revise: acesso público, palavra de declaração, nome correto e corpo vazio."),
            new Page("Atributos, tipos de dados e encapsulamento.",
                "Um objeto precisa guardar informações. Essas informações são seus <b>atributos</b>: um livro tem uma quantidade de páginas; um mago tem vida e dano.\n\nO <b>tipo</b> diz qual espécie de valor o atributo aceita. Usaremos int para números inteiros. O nome identifica a informação.\n\n<b>private</b> restringe o acesso direto ao atributo à própria classe. Essa proteção é parte do encapsulamento. Métodos da classe poderão oferecer formas controladas de consultar e alterar esses dados; vamos aprender isso depois.",
                "Guardar o estado dentro da classe aproxima o código do personagem representado. Proteger esse estado ajuda a concentrar sua manipulação nos comportamentos do próprio objeto.",
                "Trecho dentro de uma classe Livro:\n\n<noparse>private int paginas;</noparse>\n\nprivate é o acesso; int é o tipo; paginas é o nome. O ponto e vírgula encerra a declaração.\n\nO atributo fica dentro das chaves da classe. Esse trecho não cria um livro nem escolhe seu número de páginas.",
                "Os cinco atributos descreverão as capacidades do Mago. A distribuição de pontos será escolhida na próxima fase.",
                "1. Dentro da classe Mago, declare cinco atributos com acesso privado e tipo int.\n2. Use os nomes <b>vida, dano, alcance, iniciativa e velocidadeAtaque</b>.\n3. Declare cada atributo uma única vez e preserve a classe.\n4. Clique em <b>Validar código</b>.",
                "Quais informações o Mago precisa guardar? Cada uma precisa de uma declaração própria.",
                "Escreva os atributos dentro das chaves de Mago, sem abrir outra classe.",
                "Confira acesso, tipo, nome e ponto e vírgula em cada declaração.",
                "Conte cinco atributos. Confira os nomes, maiúsculas e ausência de repetições."),
            new Page("Construtor, parâmetros, this e criação de um objeto.",
                "O <b>construtor</b> prepara o estado inicial de um objeto. Ele tem o mesmo nome da classe e não declara um tipo de retorno.\n\nOs <b>parâmetros</b> recebem os valores usados nessa preparação. Dentro do construtor, atribuímos esses valores aos atributos. Quando atributo e parâmetro têm o mesmo nome, <b>this</b> identifica o atributo daquele objeto.\n\nDeclarar o construtor define como criar objetos. Usar <b>new</b> faz a criação de uma instância concreta. Os argumentos fornecidos devem seguir a ordem dos parâmetros do construtor.",
                "Uma classe pode criar objetos com estados diferentes. O construtor permite escolher esses valores no momento em que cada objeto nasce.",
                "Trecho dentro de Livro, que já tem o atributo paginas:\n\n<noparse>public Livro(int paginas) {\n    this.paginas = paginas;\n}</noparse>\n\nO lado esquerdo é o atributo; o direito é o valor recebido.\n\nCriação de um objeto no exercício:\n<noparse>Livro diario = new Livro(80);</noparse>\n\ndiario identifica o objeto; 80 é o argumento enviado ao construtor. No editor do jogo, as instâncias ficam fora das classes.",
                "Cada atributo do Mago deve ficar entre 1 e 15. A soma dos cinco não pode ultrapassar 25 pontos. Escolha sua distribuição; nenhum valor isolado substitui a conferência do total.",
                "1. No bloco Mago, depois dos atributos e dentro da classe, crie seu construtor público.\n2. Receba os cinco atributos como parâmetros int e atribua cada um ao campo correspondente usando this.\n3. Fora da classe, crie uma instância de Mago com cinco argumentos.\n4. Respeite os limites e a soma de 25; clique em <b>Validar código</b>.",
                "Criar o modelo e criar um objeto são ações diferentes. Você precisa das duas nesta fase.",
                "O construtor pertence à classe. A instância fica depois da chave que fecha a classe no editor do jogo.",
                "Confira se todos os parâmetros chegam aos atributos; usar this distingue o campo do valor recebido.",
                "Confira nome do construtor, cinco parâmetros, cinco atribuições, ordem dos argumentos e orçamento."),
            new Page("Setters: declarar um método e depois chamá-lo.",
                "Um <b>setter</b> é um método que altera um atributo do objeto. Ele recebe o novo valor em um parâmetro e atualiza o campo correspondente.\n\n<b>void</b> informa que esse método não devolve um resultado. Alterar e consultar são responsabilidades diferentes: uma consulta será ensinada com o getter de Inimigo.\n\nDeclarar o método dentro da classe ensina o objeto a fazer a alteração. Chamar o método por uma instância pede que ela faça essa alteração. O atributo pode continuar privado.",
                "Setters permitem alterar o estado por meio de um comportamento público da própria classe, sem liberar acesso direto aos campos privados.",
                "Trecho dentro de Livro, que já possui paginas:\n\n<noparse>public void setPaginas(int quantidade) {\n    this.paginas = quantidade;\n}</noparse>\n\nquantidade recebe o número; this.paginas é o atributo.\n\nSe diario é um objeto Livro, a chamada abaixo pede uma alteração:\n<noparse>diario.setPaginas(120);</noparse>\n\nDeclaração e chamada ficam em lugares diferentes.",
                "Ajustes é opcional: use-o quando quiser redistribuir os pontos antes da batalha. Os valores continuam sujeitos aos limites e à soma de até 25. Preserve seu trabalho anterior para poder reutilizá-lo.",
                "1. Dentro de Mago, fora do construtor, crie <b>setVida, setDano, setAlcance, setIniciativa e setVelocidadeAtaque</b>.\n2. Cada método deve ser público, não devolver valor e receber um int para atualizar seu atributo usando this.\n3. Preserve o construtor e a instância; clique em <b>Validar código</b>.\n4. Na próxima fase, abra <b>Ajustes</b> para usar os setters. Escreva chamadas pelo nome da instância criada, uma por linha, com ponto e vírgula; não copie a classe Mago para Ajustes.\n5. Cada valor deve ficar de 1 a 15 e a soma dos cinco até 25 pontos. Ao aumentar um atributo, reduza outro se necessário.\n6. As chamadas em Ajustes serão aplicadas antes da batalha. Se apagar esse bloco, voltam os valores do construtor.",
                "Se o campo é privado, pense em qual classe pode alterar diretamente seu valor.",
                "Os métodos ficam dentro de Mago, mas fora do corpo do construtor. As chamadas ficam em Ajustes.",
                "Diferencie o valor recebido do atributo atualizado. Cada setter deve alterar o campo correspondente ao seu nome.",
                "Confira os cinco nomes, acesso público, um parâmetro inteiro por método, atribuição e ausência de retorno."),
            new Page("Outra classe, dados de texto e getter.",
                "Agora construiremos um segundo tipo de objeto: o <b>Inimigo</b>. Ele tem seu próprio estado e seu próprio construtor, separados de Mago.\n\n<b>String</b> representa texto; int representa um número inteiro. Uma classe pode reunir atributos de tipos diferentes.\n\nUm <b>getter</b> consulta um atributo. O tipo antes do nome do método informa o tipo do resultado. <b>return</b> devolve esse resultado a quem fez a chamada. Um getter consulta; um setter altera. Como o campo é privado, outros objetos podem usar essa consulta pública.",
                "Separar Mago e Inimigo dá uma responsabilidade a cada modelo. O getter permite consultar o elemento necessário para decisões futuras sem acessar o campo privado diretamente.",
                "Trechos dentro de Livro:\n\n<noparse>private String titulo;\n\npublic String getTitulo() {\n    return titulo;\n}</noparse>\n\nO método devolve um texto já guardado no atributo. Ele não recebe um novo título nem altera o livro.\n\nAplique ao elemento do Inimigo e reutilize o raciocínio de construtor aprendido na fase 3.",
                "O Boneco começa na casa 16 e não anda nem ataca. O Mago começa na 1 e avança uma casa por turno até alcançar o alvo. Atacar se repete automaticamente. Ajustes é opcional; Estratégia ainda não é necessária.",
                "1. Em um bloco vazio, declare a classe pública <b>Inimigo</b>. Preserve Mago.\n2. Declare atributos privados: nome (String), vida (int) e elemento (String).\n3. Dentro da classe, crie um construtor público que receba e guarde esses três valores.\n4. Adicione <b>getElemento</b>: público, sem parâmetros, retorna String e devolve elemento.\n5. Fora da classe, crie o inimigo chamado <b>Boneco de Treinamento</b>, com vida <b>10</b> e elemento <b>neutro</b>.\n6. Clique em <b>Batalhar</b> e derrote o Boneco.",
                "Inimigo é outro modelo. Seus dados e comportamentos pertencem a ele, não à classe Mago.",
                "Atributos, construtor e getter ficam dentro de Inimigo. A criação da instância fica fora da classe.",
                "Uma consulta precisa devolver um resultado. Confira o tipo do getter e o atributo que ele retorna.",
                "Confira os três campos, as atribuições do construtor, o getter e os dados exatos do Boneco. Preserve o Mago e seu orçamento."),
            new Page("Método com parâmetro e chamada por um objeto.",
                "Um <b>método</b> descreve um comportamento. Os parâmetros informam os dados ou objetos necessários para realizá-lo.\n\nA declaração pertence à classe. A chamada usa um objeto concreto, seguido de ponto, nome do método e argumentos entre parênteses.\n\nO parâmetro é um nome local ao método; o argumento é o objeto que você envia na chamada. Eles não precisam ter o mesmo nome, mas precisam ter tipos compatíveis.",
                "Métodos permitem que objetos colaborem: um personagem pode realizar uma ação que envolve outro objeto, sem misturar suas classes.",
                "Trecho dentro de uma classe Bibliotecario, em um exercício simplificado:\n\n<noparse>public void conferir(Livro item) {\n}</noparse>\n\nO parâmetro item representa o livro recebido. Se atendente é um Bibliotecario e diario é um Livro, a chamada seria:\n\n<noparse>atendente.conferir(diario);</noparse>\n\nO método vazio é apenas um exemplo de assinatura e chamada.",
                "No jogo, a declaração válida da magia habilita a ação. A simulação executa o ataque ao encontrar a chamada com a instância do inimigo. Não é preciso programar o dano dentro do método nesta fase.",
                "1. Dentro de Mago, declare um método público <b>lancarMagia</b>, sem retorno, com um parâmetro do tipo Inimigo. Seu corpo deve ficar vazio nesta atividade.\n2. Fora das classes, faça a instância do Mago chamar o método usando a instância do Boneco como argumento.\n3. Preserve o código anterior e clique em <b>Batalhar</b>.",
                "Ensinar uma ação ao objeto e pedir que ele execute a ação são passos diferentes.",
                "Declare o método dentro de Mago; escreva a chamada fora das classes usando os objetos já criados.",
                "Na chamada, use o nome da instância do inimigo. O nome do parâmetro existe somente dentro do método.",
                "Confira acesso, ausência de retorno, nome da magia, tipo do parâmetro, corpo vazio e chamada com o objeto correto."),
            new Page("Herança: classe base e especialização.",
                "<b>Herança</b> descreve uma relação entre uma classe mais geral e uma especialização. A classe especializada é a <b>subclasse</b>; a geral é a <b>classe base</b>.\n\nA palavra <b>extends</b> declara essa relação. Uma especialização continua sendo um tipo da classe base e pode aproveitar seus comportamentos, além de acrescentar seus próprios.\n\nCampos privados continuam privados: herdar não libera acesso direto a eles.",
                "A herança concentra o que os tipos compartilham e permite especializar o que muda. Ela representa uma relação de tipo, não uma cópia de todo o código da base.",
                "Em um exemplo independente, com uma classe Livro que não declara construtor próprio:\n\n<noparse>public class LivroIlustrado extends Livro {\n}</noparse>\n\nLivroIlustrado é uma especialização de Livro. O nome depois de extends é a classe base.\n\nNesta atividade, o jogo usa uma forma simplificada de herança para introduzir a relação. Construtores explícitos das subclasses serão pedidos na fase final.",
                "A forma Piromante usa fogo nesta atividade. A escolha de formas por elemento ainda não é necessária. Preserve o Boneco e a chamada da magia.",
                "1. No bloco Mago, fora da classe já existente, declare uma classe pública chamada <b>Piromante</b>.\n2. Indique que ela herda de Mago. Deixe seu corpo vazio nesta fase.\n3. Preserve Mago, o Boneco e a chamada da magia.\n4. Clique em <b>Batalhar</b>.",
                "Piromante é um tipo de Mago. Qual relação entre classes expressa essa especialização?",
                "A nova classe fica fora das chaves de Mago, no mesmo bloco de código.",
                "Na relação de herança, primeiro vem o nome da especialização; depois da palavra de relação vem a base.",
                "Confira nome, acesso público, relação com Mago, corpo vazio e preservação dos objetos anteriores."),
            new Page("Sobrescrita e reutilização com super.",
                "<b>Sobrescrever</b> é oferecer, na subclasse, uma versão de um método herdado. A assinatura precisa corresponder ao método da base: nome e tipos dos parâmetros.\n\n<b>@Override</b> deixa explícita a intenção de sobrescrever. <b>super</b> permite acessar o comportamento da classe base.\n\nA versão especializada pode reutilizar o que a base já faz. Nesta atividade, a nova magia deve apenas chamar a versão herdada com o mesmo alvo recebido.",
                "Uma especialização pode responder à mesma ação sem duplicar a implementação comum. A forma do objeto determina qual versão do comportamento será usada.",
                "Trecho dentro de LivroIlustrado, supondo que Livro já tenha um método público mostrar sem parâmetros:\n\n<noparse>@Override\npublic void mostrar() {\n    super.mostrar();\n}</noparse>\n\nA assinatura corresponde à base. A chamada com super reutiliza o método herdado. No exercício do Mago, lembre-se de encaminhar o parâmetro recebido.",
                "O Golem tem 12 de vida e elemento gelo. Nesta fase, a forma Piromante permanece fixa. Trocar a instância do inimigo também exige atualizar a chamada para o novo objeto.",
                "1. Dentro de Piromante, sobrescreva <b>lancarMagia</b> com a assinatura definida em Mago e marque com @Override.\n2. No corpo, chame apenas a magia herdada via super, passando o parâmetro recebido.\n3. No bloco Inimigo, substitua o Boneco por <b>Golem de Gelo</b>, vida <b>12</b>, elemento <b>gelo</b>.\n4. Atualize a chamada da magia para usar essa instância e clique em <b>Batalhar</b>.",
                "Você vai especializar um método existente, não criar outra ação com nome diferente.",
                "O novo método pertence a Piromante, dentro de suas chaves. A troca do objeto inimigo fica fora de Inimigo.",
                "Confira a assinatura herdada. A chamada via super deve encaminhar o mesmo parâmetro recebido.",
                "Confira anotação, assinatura, chamada herdada, dados do Golem e referência ao novo inimigo na chamada externa."),
            new Page("Polimorfismo e decisão por elemento.",
                "<b>Polimorfismo</b> permite usar uma referência de um tipo geral para objetos de tipos especializados. Uma referência Mago pode apontar para Piromante, Hidromante ou Eletromante.\n\nA chamada da magia é a mesma; o objeto escolhido determina o comportamento concreto. <b>if/else</b> permite escolher esse objeto de acordo com o elemento do alvo.\n\nO motor fornece <b>alvo</b> para cada inimigo. Consulte seu elemento pelo getter. Para comparar textos, use equals. Você não precisa criar listas nem laços: a simulação percorre os alvos.",
                "O código que chama a magia trabalha com o tipo comum. A decisão escolhe a especialização adequada; a chamada final aproveita o comportamento desse objeto.",
                "Supondo que LivroIlustrado herde de Livro, um trecho independente seria:\n\n<noparse>Livro escolhido = diario;\nif (querIlustracoes) {\n    escolhido = ilustrado;\n}\nescolhido.mostrar();</noparse>\n\ndiario e ilustrado são objetos já criados; querIlustracoes é uma condição booleana deste exemplo. A referência é do tipo Livro e a chamada usa o objeto escolhido.\n\nPara comparar texto, por exemplo:\n<noparse>titulo.equals(\"Diário\")</noparse>",
                "Correspondências: gelo → Piromante; fogo → Hidromante; água → Eletromante; neutro → Mago.\n\nO Elemental ataca de longe. Preserve vida na distribuição de até 25 pontos. Uma tentativa com código inválido custa 20% da vida máxima; corrigir não cura. Após derrota, tentar novamente restaura a vida. Consultar o grimório e pedir dicas não causa essa penalidade.",
                "1. Preserve Mago e Inimigo. No bloco Mago, prepare <b>Piromante, Hidromante e Eletromante</b>, todas herdando de Mago.\n2. Cada subclasse precisa de um construtor com os cinco parâmetros, encaminhados à base por super, e da magia sobrescrita com @Override e chamada herdada.\n3. No bloco Inimigo, crie uma instância de cada alvo: <b>Boneco de Treinamento</b> (10, neutro), <b>Golem de Gelo</b> (12, gelo), <b>Elemental de Fogo</b> (12, fogo) e <b>Slime Aquático</b> (12, água). Os números indicam vida.\n4. Em Estratégia, inicialize uma referência do tipo Mago com sua instância original. Escolha as formas por if/else conforme o elemento de alvo. Repita os cinco valores do seu Mago nas formas.\n5. Para o alvo neutro, mantenha o Mago original. Termine chamando a magia pela referência escolhida, depois da decisão.\n6. Clique em <b>Batalhar</b> e vença todos os alvos.",
                "A referência tem um tipo comum; o objeto apontado pode ser uma das especializações.",
                "Prepare as subclasses no bloco Mago. A escolha por elemento e a chamada final ficam em Estratégia; alvo é fornecido pelo motor.",
                "Consulte o getter, compare o texto e use as correspondências explicadas. Confira os construtores e o encaminhamento à base.",
                "Revise as três formas, a mesma distribuição de pontos, o caso neutro e a chamada pela referência comum depois da decisão.")
        };

        private static readonly string[] Reviews = {
            "Você declarou uma classe: o modelo existe, mas ainda não criou um objeto. Na próxima fase, vamos definir os atributos desse modelo.",
            "Os atributos guardam o estado. O acesso privado mantém esses dados dentro da classe; a próxima etapa prepara os valores de um objeto.",
            "O construtor recebe valores e prepara os atributos. A criação da instância usa esse construtor; confira a diferença entre modelo e objeto.",
            "Você definiu métodos que alteram atributos privados. Declarar os setters é diferente de chamá-los por uma instância em Ajustes.",
            "Mago e Inimigo têm responsabilidades distintas. O getter consulta e devolve um dado; o setter recebe um valor para alterá-lo.",
            "O método descreve o comportamento; a chamada pede sua execução. O argumento enviado representa o inimigo recebido pelo parâmetro.",
            "Piromante é uma especialização de Mago. A herança representa essa relação sem copiar todo o modelo base.",
            "A sobrescrita define uma versão na subclasse. A chamada com super reutiliza o comportamento da base com o mesmo alvo.",
            "A referência comum aponta para objetos especializados. Sua decisão escolhe o objeto, e a mesma chamada de magia usa o comportamento dessa forma." };

        public static void Apply()
        {
            var asset = AssetDatabase.LoadAssetAtPath<LearningPathAsset>("Assets/PrograMago/Content/LearningPath.asset");
            var serialized = new SerializedObject(asset);
            var battles = serialized.FindProperty("battles");
            if (battles.arraySize != Pages.Length) throw new System.InvalidOperationException("O grimório espera as nove fases atuais.");
            for (int i = 0; i < Pages.Length; i++) {
                var entry = battles.GetArrayElementAtIndex(i); var page = Pages[i];
                Set(entry, "concept", page.concept); Set(entry, "whatItIs", page.explanation); Set(entry, "purpose", page.purpose);
                Set(entry, "usageExample", page.example); Set(entry, "gameEffect", page.effect); Set(entry, "task", page.task);
                var hints = entry.FindPropertyRelative("hints"); hints.arraySize = page.hints.Length;
                for (int j = 0; j < page.hints.Length; j++) hints.GetArrayElementAtIndex(j).stringValue = page.hints[j];
                Set(entry, "victoryAchievement", "Você aplicou o conceito: " + page.concept);
                Set(entry, "victoryReview", Reviews[i] + "\n\nEsta página permanece disponível no Grimório do Mago.");
            }
            serialized.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(asset);
        }
        private static void Set(SerializedProperty target, string name, string value) => target.FindPropertyRelative(name).stringValue = value;
    }
}
