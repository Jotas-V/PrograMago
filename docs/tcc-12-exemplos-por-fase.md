# PrograMago — exemplos das nove fases

O caminho pedagógico tem nove etapas. No TCC-12 ficam jogáveis as fases 1 a 5, incluindo a primeira batalha com o Boneco. As fases 6 a 9 documentam a continuação de métodos, subclasses e estratégia elemental, mas continuam bloqueadas até a progressão correspondente.

Os exemplos abaixo são cumulativos quando indicado. As três primeiras fases preenchem o bloco **Mago**; a fase 4 acrescenta os setters à mesma classe. Na fase 5, o jogador mantém esse bloco e escreve a classe/instância do inimigo no bloco **Inimigo**. **Ajustes** é um espaço separado para chamadas de setter antes da batalha. **Estratégia** só será liberado depois que classes elementais e inimigos elementais estiverem disponíveis.

## Fase 1 — declarar a classe

No bloco Mago:

```java
public class Mago {
}
```

## Fase 2 — declarar atributos privados

Acrescente dentro das chaves de Mago:

```java
public class Mago {
    private int vida;
    private int dano;
    private int alcance;
    private int iniciativa;
    private int velocidadeAtaque;
}
```

## Fase 3 — construtor e instância

Complete o bloco Mago. Use valores cuja soma não passe de 25; a ordem de argumentos deve seguir a ordem dos parâmetros do construtor.

```java
public class Mago {
    private int vida;
    private int dano;
    private int alcance;
    private int iniciativa;
    private int velocidadeAtaque;

    public Mago(int vida, int dano, int alcance,
                int iniciativa, int velocidadeAtaque) {
        this.vida = vida;
        this.dano = dano;
        this.alcance = alcance;
        this.iniciativa = iniciativa;
        this.velocidadeAtaque = velocidadeAtaque;
    }
}

Mago mago = new Mago(5, 5, 5, 5, 5);
```

## Fase 4 — criar setters na classe

Acrescente os cinco métodos depois do construtor, ainda dentro das chaves da classe Mago:

```java
    public void setVida(int valor) {
        this.vida = valor;
    }

    public void setDano(int valor) {
        this.dano = valor;
    }

    public void setAlcance(int valor) {
        this.alcance = valor;
    }

    public void setIniciativa(int valor) {
        this.iniciativa = valor;
    }

    public void setVelocidadeAtaque(int valor) {
        this.velocidadeAtaque = valor;
    }
```

Depois da validação desta fase, o bloco Mago fica protegido. Em **Ajustes**, uma build válida para o Boneco é:

```java
mago.setVida(5);
mago.setDano(5);
mago.setAlcance(5);
mago.setIniciativa(5);
mago.setVelocidadeAtaque(5);
```

As chamadas podem alterar só os atributos que precisam mudar. O resultado final deve manter cada valor entre 1 e 15 e o total em até 25. Se um setter ultrapassar o orçamento, redistribua também os demais antes de iniciar a batalha.

## Fase 5 — criar o Boneco e batalhar

No bloco Inimigo:

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

O Mago começa na casa 1 e o Boneco na casa 16. Fora do alcance, o Mago avança uma casa por turno. O Boneco neutro não se move nem ataca. O comando **Atacar** é executado automaticamente até vitória ou derrota.

## Fase 6 — declarar uma magia

Etapa preparada para a continuação; a validação e a disponibilidade em jogo ainda dependem da progressão posterior.

```java
public void lancarMagia(Inimigo alvo) {
    // O motor resolve alcance, movimento, dano e turno.
}
```

## Fase 7 — criar subclasses elementais

Uma subclasse representa uma forma do único Mago, sem criar um segundo ator:

```java
public class Piromante extends Mago {
}
```

## Fase 8 — sobrescrever comportamento

As formas alteram aparência e magia do mesmo Mago. A referência à classe base preserva os atributos e a posição.

```java
@Override
public void lancarMagia(Inimigo alvo) {
    super.lancarMagia(alvo);
}
```

## Fase 9 — escolher a forma por elemento

No bloco **Estratégia**, o código do jogador seleciona a forma. O motor continua responsável pelos turnos, movimento, alcance e dano.

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
