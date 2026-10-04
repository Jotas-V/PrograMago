# TCC-17 — fase 9 e chamada polimórfica

Recorte aprovado pelo usuário em 04/10/2026: quatro inimigos, referência Mago, condições alvo.getElemento().equals(...) em qualquer ordem, else neutro e uma única chamada lancarMagia(alvo). Integrar explicação, dicas, resultado e salvamento; não acrescentar operadores ou condições aninhadas.

A opção escolhida ensina a referência comum explicitamente. Manter apenas selecionarForma(string), como no compilador antigo, ocultaria o objeto concreto; executar Java genérico ampliaria o escopo. O analisador continuará reconhecendo apenas o contrato educativo e verificará toda a entrada.

Mago e Inimigo mantêm o código cumulativo. As três subclasses declaram a sobrescrita e um construtor que encaminha os cinco atributos a super. A Estratégia declara Mago ativo = mago; atribui new Piromante(...), new Hidromante(...) ou new Eletromante(...) conforme o elemento e usa ativo = mago no else. As especializações usam os mesmos valores da instância base; não redefinem os atributos do personagem durante a batalha. A chamada ativo.lancarMagia(alvo) fica após o condicional. O motor mantém vida, posição e estados dos inimigos, aplicando o comportamento concreto em cada turno.

Plano: testes de linguagem e diagnósticos; compilação da estratégia validada; desbloqueio e migração do save com oito etapas; integração da submissão, Ajustes e arena; atualização da lição e exemplos; testes de vitória e regressão. Falhas preexistentes serão discriminadas. Sem alteração do restante da arte local.
