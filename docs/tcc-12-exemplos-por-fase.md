# PrograMago — códigos completos por fase

As fases 1–8 estão disponíveis. A fase 9 ainda aguarda a integração de estratégia e polimorfismo.

Em cada fase, substitua o conteúdo inteiro do bloco indicado pelo exemplo correspondente. Os exemplos já incluem o código aprendido nas fases anteriores. Clique em **Validar código** nas fases 1–4 e em **Batalhar** nas fases 5–8; depois avance pela tela de vitória.

Os blocos Mago, Inimigo e Estratégia ficam livremente editáveis durante a preparação. Nas fases 1–4 deixe **Inimigo** vazio. Deixe **Estratégia** vazio em todas as fases 1–8: sua lógica só será ensinada na fase 9, e conteúdo antecipado pode impedir a validação da fase atual. Os botões ficam desativados durante o combate e a tela de vitória.

A build usada abaixo é `(5, 5, 5, 5, 5)`, totalizando 25 pontos. **Ajustes** pode ficar vazio; se você já colocou chamadas de setter nesse bloco, elas também precisam manter cada atributo entre 1 e 15 e o total em até 25.

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

## Fase 9 — ainda bloqueada

Esta etapa deverá ensinar seleção de forma com `if/else` pelo elemento do alvo. Ainda não existe uma solução jogável para ela na integração atual. O botão **Estratégia** abre o documento, mas não libera a fase 9. Deixe esse bloco vazio para jogar as fases anteriores.
