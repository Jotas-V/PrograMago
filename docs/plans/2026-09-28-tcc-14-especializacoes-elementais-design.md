# TCC-14 — Especializações elementais

## Contexto

MagoState é o retrato imutável dos atributos aprovados pelo jogador. CombatWizard carrega esses atributos para o combate e restringe quais magias estão disponíveis. CombatStrategy valida os nomes das formas, enquanto CombatEngine executa turnos, movimento, alcance e dano. GameplayBootstrapper já colore o sprite base conforme a forma selecionada, e os prefabs FireProjectile, WaterProjectile e ElectricProjectile já fornecem efeitos distintos.

## Desenho aprovado

Piromante, Hidromante e Eletromante serão modelos de combate tipados derivados de CombatWizard. O modelo base exporá os atributos compartilhados e a forma/magia neutra; cada classe derivada preservará uma cópia imutável dos mesmos atributos e identificará sua magia elemental.

A estratégia continuará sendo a fonte da escolha do jogador. Ao resolver a forma para o inimigo atual, CombatEngine criará o modelo especializado correspondente e usará sua magia no evento de combate. O motor continuará responsável por verificar disponibilidade da magia, alcance, movimento, turnos, efetividade e dano. Uma escolha de magia ainda não disponível continuará produzindo MissingSpell.

A apresentação continuará usando os projéteis por elemento e a coloração da forma que já existem. O sprite base será mantido até que a TCC-40 forneça as artes próprias das três especializações; esta implementação não produzirá assets de personagem.

## Fluxo de dados

1. O programa validado cria MagoState com os cinco atributos.
2. CombatWizard.FromMago carrega esses atributos e o conjunto de magias liberadas para a batalha.
3. CombatStrategy resolve o elemento do inimigo para um nome de forma.
4. CombatEngine cria a subclasse tipada correspondente a partir do CombatWizard base.
5. A magia da especialização passa pelas mesmas regras do motor e gera um CombatEvent.
6. GameplayBootstrapper escolhe o prefab pelo elemento do evento e aplica a cor da forma.

## Critérios de comportamento

- Cada especialização pode ser instanciada a partir de um Mago de combate.
- Vida, dano, alcance, iniciativa, velocidade de ataque e magias liberadas são preservados.
- Piromante, Hidromante e Eletromante identificam fogo, água e eletricidade, respectivamente.
- Estratégias existentes continuam respeitando a escolha escrita pelo jogador.
- Movimento, alcance, ritmo de ataque, dano elemental e o caso MissingSpell mantêm o comportamento atual.
- Os prefabs elementais existentes continuam sendo usados para apresentar os ataques.

## Verificação

Adicionar testes de domínio que comecem falhando para cópia dos atributos, identidade da magia e seleção da especialização pela estratégia. Executar a suíte EditMode e os testes PlayMode relevantes depois da implementação.

## Fora de escopo

- Aceitar código Java do jogador com extends, override ou super; esse trabalho pertence à TCC-15.
- Produzir ou integrar artes inéditas de personagens; a TCC-40 produzirá essas variações.
- Alterar equilíbrio, regras de dano, turnos ou progressão pedagógica.