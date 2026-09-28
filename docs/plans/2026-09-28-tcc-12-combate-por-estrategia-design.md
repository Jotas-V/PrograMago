# TCC-12: combate por estratégia — desenho aprovado

## Problema

A interface atual separa a declaração de métodos da classe Mago, mostra um bloco genérico sem finalidade explícita e oferece uma timeline arrastável embora o combate atual aceite uma única ação. O código também congela as classes inteiras depois da terceira etapa, impedindo que o jogador acrescente os setters dentro da classe. O conteúdo anuncia oito etapas, mas só as quatro primeiras têm critérios implementados. A seleção elemental é automática no motor, então um `if/else` escrito pelo jogador não teria efeito.

## Resultado esperado

O aluno monta as classes progressivamente, mantém o código aprovado e acrescenta membros nos pontos corretos. Um único Mago preserva atributos, vida e posição ao mudar de forma elemental. A Estratégia do jogador escolhe a magia em um programa `if/else` que o motor executa em cada turno. O CombatEngine continua controlando as regras e o estado do combate.

## Organização dos editores

- **Mago:** documento cumulativo com a classe, atributos privados, construtor, setters, método de magia, especializações e instância. Métodos pertencem à classe. Não há workspace de Métodos nem botão “Aprovar método”.
- **Inimigo:** documento cumulativo com a classe, atributos, construtor, métodos de consulta e instâncias.
- **Ajustes:** chamadas aos setters. Aplicadas uma vez antes de cada tentativa, sempre sobre os valores originais do construtor.
- **Estratégia:** um único bloco de código de batalha, sem reordenar ações. O motor fornece a referência `alvo` para o inimigo vivo escolhido naquele turno. O código seleciona uma forma/magia e chama `mago.lancarMagia(alvo)`.

O código já aprovado fica visível e somente para leitura. A etapa atual abre uma área editável que é montada no ponto correto: métodos antes da chave que fecha a classe; instâncias depois da classe. O programa montado aparece como um documento contínuo, e erros apontam a área e a linha em edição.

## Mago único e formas elementais

Existe apenas um personagem na arena. Piromante, Hidromante, Eletromante e neutro são formas do mesmo Mago, não combatentes adicionais. Selecionar uma forma muda o sprite e o elemento da próxima magia, mantendo os mesmos atributos, vida, posição e identidade. A forma é escolhida novamente em cada turno pela Estratégia.

O mapeamento aprovado é:

| Elemento do alvo | Forma/magia eficaz |
| --- | --- |
| gelo | fogo / Piromante |
| fogo | água / Hidromante |
| água | eletricidade / Eletromante |
| neutro | magia neutra |

O sprite de fogo usa tons vermelhos e alaranjados com efeito de fogo; água usa tons azuis e efeito aquático; eletricidade usa amarelo e raio; neutro mantém a aparência base. O projétil acompanha a forma escolhida. O motor deixa de selecionar a magia automaticamente quando recebe uma Estratégia válida. Uma escolha errada continua podendo causar um ataque ineficaz.

## Progressão em nove etapas

1. Declarar `public class Mago`.
2. Adicionar os cinco atributos privados.
3. Implementar o construtor e criar a instância do Mago.
4. Declarar setters dentro de Mago e usar Ajustes para redistribuir os 25 pontos. Validar por código, sem combate, e atualizar a prévia da arena.
5. Declarar Inimigo e instanciar o Boneco de Treinamento. Começa a primeira batalha.
6. Implementar o método de magia que recebe o alvo.
7. Criar as formas especializadas por herança.
8. Sobrescrever o método de magia e apresentar inimigos elementais; as formas já existem antes de surgirem esses inimigos.
9. Aplicar polimorfismo e escrever a Estratégia condicional. Não adicionar uma décima etapa.

## Exemplos progressivos

Os exemplos a seguir definem o contrato pedagógico pretendido. As etapas 5–9 ainda não passam nos validadores atuais e só serão exemplos copiáveis para teste depois da implementação dos respectivos critérios.

### Etapa 1 — classe

```java
public class Mago {}
```

### Etapa 2 — encapsulamento

```java
public class Mago {
    private int vida;
    private int dano;
    private int alcance;
    private int iniciativa;
    private int velocidadeAtaque;
}
```

### Etapa 3 — construtor e instância

```java
public class Mago {
    private int vida;
    private int dano;
    private int alcance;
    private int iniciativa;
    private int velocidadeAtaque;

    public Mago(int vida, int dano, int alcance, int iniciativa, int velocidadeAtaque) {
        this.vida = vida;
        this.dano = dano;
        this.alcance = alcance;
        this.iniciativa = iniciativa;
        this.velocidadeAtaque = velocidadeAtaque;
    }
}

Mago mago = new Mago(5, 5, 5, 5, 5);
```

### Etapa 4 — setters dentro da classe e chamadas em Ajustes

Acrescentar dentro de `Mago`, antes da chave final:

```java
public void setAlcance(int valor) {
    this.alcance = valor;
}

public void setDano(int valor) {
    this.dano = valor;
}
```

No bloco Ajustes:

```java
mago.setAlcance(7);
mago.setDano(3);
```

Com os valores iniciais `5, 5, 5, 5, 5`, a nova build fica `vida=5, dano=3, alcance=7, iniciativa=5, velocidadeAtaque=5`, ainda totalizando 25. Atributos sem chamada mantêm seus valores de origem. Se a soma final exceder 25 ou algum valor sair de 1–15, rejeitar a build inteira sem aplicação parcial.

### Etapa 5 — Boneco de Treinamento

```java
public class Inimigo {
    private String nome;
    private int vida;
    private String elemento;

    public Inimigo(String nome, int vida, String elemento) {
        this.nome = nome;
        this.vida = vida;
        this.elemento = elemento;
    }

    public String getElemento() {
        return elemento;
    }
}

Inimigo boneco = new Inimigo("Boneco de Treinamento", 10, "neutro");
```

### Etapa 6 — método de magia

Dentro da classe Mago:

```java
public void lancarMagia(Inimigo alvo) {}
```

A Estratégia passará o alvo selecionado pelo motor para esse método. O efeito da magia será resolvido pelo CombatEngine, sem permitir que código arbitrário altere vida ou posição.

### Etapa 7 — formas especializadas

```java
public class Piromante extends Mago {}
public class Hidromante extends Mago {}
public class Eletromante extends Mago {}
```

As formas são recursos de comportamento/visual do único personagem; sua instanciação não cria Mago adicional na arena.

### Etapa 8 — sobrescrita e inimigos elementais

Dentro de cada forma, sobrescrever a mesma operação de magia e chamar a versão base:

```java
@Override
public void lancarMagia(Inimigo alvo) {
    super.lancarMagia(alvo);
}
```

Exemplos de instâncias elementais para validar a Estratégia:

```java
Inimigo golem = new Inimigo("Golem de Gelo", 12, "gelo");
Inimigo elemental = new Inimigo("Elemental de Fogo", 12, "fogo");
Inimigo slime = new Inimigo("Slime Aquático", 12, "água");
```

### Etapa 9 — Estratégia condicional

Forma conceitual do único bloco executado a cada turno:

```java
if (alvo.getElemento().equals("gelo")) {
    mago.selecionarForma("piromante");
} else if (alvo.getElemento().equals("fogo")) {
    mago.selecionarForma("hidromante");
} else if (alvo.getElemento().equals("água")) {
    mago.selecionarForma("eletromante");
} else {
    mago.selecionarForma("neutro");
}
mago.lancarMagia(alvo);
```

O vocabulário final do compilador deve permanecer pequeno e seguro, aceitar somente as estruturas e chamadas ensinadas, e gerar diagnóstico claro para condição ou chamada inválida.

## Regras de combate preservadas

- Arena de 16 casas; Mago começa na casa 1 e o Boneco na casa 16.
- Personagens móveis avançam uma casa por turno quando estão fora do próprio alcance; atacam quando entram no alcance. O Boneco não se move nem ataca.
- Vida, dano, alcance, iniciativa e velocidade de ataque são lidos da build validada; cada atributo final permanece entre 1 e 15 e o orçamento é 25.
- Estratégia escolhe forma/magia. CombatEngine continua controlando alvo atual, turnos, movimentação, alcance, dano, eficácia elemental, vitória, derrota e reinício.
- A tentativa reinicia posições e vida e reaplica a build atual sobre os valores do construtor, sem reaplicar setters por turno.

## Verificação

- Testes EditMode para composição do código progressivo, proteção dos trechos concluídos, inserção de métodos/instâncias nos pontos corretos, orçamento dos setters, compilação condicional e seleção de forma por elemento.
- Testes PlayMode para uma única entidade Mago, mudança de sprite/projétil sem perder vida/atributos/posição, seleção de forma por turno, movimento automático, Boneco parado, vitória, derrota e nova tentativa.
- Abrir a MainScene sem Play para conferir a prévia e validar visualmente as quatro formas durante o combate no Unity.
- Executar EditMode e PlayMode no Unity Editor.
