# PrograMago — códigos completos por fase

As fases 1–9 estão disponíveis. A fase 9 reúne polimorfismo e estratégia condicional com os quatro inimigos.

Em cada fase, substitua o conteúdo inteiro do bloco indicado pelo exemplo correspondente. Os exemplos já incluem o código aprendido nas fases anteriores. Clique em **Validar código** nas fases 1–4 e em **Batalhar** nas fases 5–9; depois avance pela tela de vitória.

Os blocos Mago, Inimigo e Estratégia ficam livremente editáveis durante a preparação. Nas fases 1–4 deixe **Inimigo** vazio. Deixe **Estratégia** vazio em todas as fases 1–8: sua lógica só será ensinada na fase 9, e conteúdo antecipado pode impedir a validação da fase atual. Os botões ficam desativados durante o combate e a tela de vitória.

Nas fases 1–8, a build usada abaixo é `(5, 5, 5, 5, 5)`. A fase 9 usa `(1, 6, 15, 2, 1)` para alcançar os quatro inimigos. Ambas totalizam 25 pontos. **Ajustes** pode ficar vazio; se você já colocou chamadas de setter nesse bloco, elas também precisam manter cada atributo entre 1 e 15 e o total em até 25.

## Fase 1 — Declarar a classe

Bloco **Mago**:

```java
public class Mago {
}
```

## Fase 2 — Declarar atributos privados

Bloco **Mago**:

```java
public class Mago {
    private int vida;
    private int dano;
    private int alcance;
    private int iniciativa;
    private int velocidadeAtaque;
}
```

## Fase 3 — Construtor e instância

Bloco **Mago**:

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

## Fase 4 — Criar setters

Bloco **Mago**:

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

    public void setVida(int valor) { this.vida = valor; }
    public void setDano(int valor) { this.dano = valor; }
    public void setAlcance(int valor) { this.alcance = valor; }
    public void setIniciativa(int valor) { this.iniciativa = valor; }
    public void setVelocidadeAtaque(int valor) { this.velocidadeAtaque = valor; }
}

Mago mago = new Mago(5, 5, 5, 5, 5);
```

## Fase 5 — Criar o Boneco e batalhar

Bloco **Mago**:

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

    public void setVida(int valor) { this.vida = valor; }
    public void setDano(int valor) { this.dano = valor; }
    public void setAlcance(int valor) { this.alcance = valor; }
    public void setIniciativa(int valor) { this.iniciativa = valor; }
    public void setVelocidadeAtaque(int valor) { this.velocidadeAtaque = valor; }
}

Mago mago = new Mago(5, 5, 5, 5, 5);
```

Bloco **Inimigo**:

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

## Fase 6 — Declarar e chamar magia

Bloco **Mago**:

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

    public void setVida(int valor) { this.vida = valor; }
    public void setDano(int valor) { this.dano = valor; }
    public void setAlcance(int valor) { this.alcance = valor; }
    public void setIniciativa(int valor) { this.iniciativa = valor; }
    public void setVelocidadeAtaque(int valor) { this.velocidadeAtaque = valor; }

    public void lancarMagia(Inimigo alvo) {
    }
}

Mago mago = new Mago(5, 5, 5, 5, 5);
mago.lancarMagia(boneco);
```

Bloco **Inimigo**:

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

## Fase 7 — Herança: Piromante

Bloco **Mago**:

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

    public void setVida(int valor) { this.vida = valor; }
    public void setDano(int valor) { this.dano = valor; }
    public void setAlcance(int valor) { this.alcance = valor; }
    public void setIniciativa(int valor) { this.iniciativa = valor; }
    public void setVelocidadeAtaque(int valor) { this.velocidadeAtaque = valor; }

    public void lancarMagia(Inimigo alvo) {
    }
}

Mago mago = new Mago(5, 5, 5, 5, 5);
mago.lancarMagia(boneco);

public class Piromante extends Mago {
}
```

Bloco **Inimigo**:

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

## Fase 8 — Sobrescrita contra Golem de Gelo

Bloco **Mago**:

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

    public void setVida(int valor) { this.vida = valor; }
    public void setDano(int valor) { this.dano = valor; }
    public void setAlcance(int valor) { this.alcance = valor; }
    public void setIniciativa(int valor) { this.iniciativa = valor; }
    public void setVelocidadeAtaque(int valor) { this.velocidadeAtaque = valor; }

    public void lancarMagia(Inimigo alvo) {
    }
}

Mago mago = new Mago(5, 5, 5, 5, 5);
mago.lancarMagia(golem);

public class Piromante extends Mago {
    @Override
    public void lancarMagia(Inimigo alvo) {
        super.lancarMagia(alvo);
    }
}
```

Bloco **Inimigo**:

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

Inimigo golem = new Inimigo("Golem de Gelo", 12, "gelo");
```

## Ajustes opcionais — fases 5–8

Não é necessário preencher este bloco com a build dos exemplos. Para restaurá-la, use:

```java
mago.setVida(5);
mago.setDano(5);
mago.setAlcance(5);
mago.setIniciativa(5);
mago.setVelocidadeAtaque(5);
```

## Fase 9 — polimorfismo e if/else (TCC-17)

Depois de vencer a fase 8, avance para a batalha final. Edite os três blocos abaixo. Deixe **Ajustes** vazio para usar esta build de 25 pontos. Os construtores das subclasses encaminham os parâmetros a `super(...)`; eles são diferentes da chamada `super.lancarMagia(alvo)` na sobrescrita.

Bloco **Mago** (os setters anteriores podem ser mantidos dentro de Mago):

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

    public void lancarMagia(Inimigo alvo) {}
}

Mago mago = new Mago(1, 6, 15, 2, 1);

public class Piromante extends Mago {
    public Piromante(int vida, int dano, int alcance, int iniciativa, int velocidadeAtaque) {
        super(vida, dano, alcance, iniciativa, velocidadeAtaque);
    }
    @Override
    public void lancarMagia(Inimigo alvo) { super.lancarMagia(alvo); }
}

public class Hidromante extends Mago {
    public Hidromante(int vida, int dano, int alcance, int iniciativa, int velocidadeAtaque) {
        super(vida, dano, alcance, iniciativa, velocidadeAtaque);
    }
    @Override
    public void lancarMagia(Inimigo alvo) { super.lancarMagia(alvo); }
}

public class Eletromante extends Mago {
    public Eletromante(int vida, int dano, int alcance, int iniciativa, int velocidadeAtaque) {
        super(vida, dano, alcance, iniciativa, velocidadeAtaque);
    }
    @Override
    public void lancarMagia(Inimigo alvo) { super.lancarMagia(alvo); }
}
```

Bloco **Inimigo**:

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

    public String getElemento() { return elemento; }
}

Inimigo boneco = new Inimigo("Boneco de Treinamento", 10, "neutro");
Inimigo golem = new Inimigo("Golem de Gelo", 12, "gelo");
Inimigo elemental = new Inimigo("Elemental de Fogo", 12, "fogo");
Inimigo slime = new Inimigo("Slime Aquático", 12, "água");
```

Bloco **Estratégia**:

```java
Mago ativo = mago;
if (alvo.getElemento().equals("gelo")) {
    ativo = new Piromante(1, 6, 15, 2, 1);
} else if (alvo.getElemento().equals("fogo")) {
    ativo = new Hidromante(1, 6, 15, 2, 1);
} else if (alvo.getElemento().equals("água")) {
    ativo = new Eletromante(1, 6, 15, 2, 1);
} else {
    ativo = mago;
}
ativo.lancarMagia(alvo);
```

A referência `ativo` tem tipo Mago, mas recebe objetos concretos diferentes. A mesma chamada usa a magia da especialização escolhida. O motor fornece `alvo` a cada turno e preserva a vida, posição e build durante as trocas; não é preciso escrever uma lista de inimigos nem um laço.

Pode renomear `mago` e `ativo`, alterar espaços/quebras de linha e reordenar as três condições. Cada elemento deve aparecer uma vez. Use sempre os mesmos cinco valores da instância base nas especializações. Uma combinação elemental incorreta é aceita estruturalmente, mas seu ataque pode ser ineficaz. Não são aceitos operadores adicionais, if aninhado, comandos extras ou uma segunda chamada de magia. Os exemplos não usam comentários, que não fazem parte do tokenizador atual.
