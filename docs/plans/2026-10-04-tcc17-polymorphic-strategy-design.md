# TCC-17 — fase 9 e chamada polimórfica

Recorte aprovado pelo usuário em 04/10/2026: quatro inimigos, referência Mago, condições alvo.getElemento().equals(...) em qualquer ordem, else neutro e uma única chamada lancarMagia(alvo). Integrar explicação, dicas, resultado e salvamento; não acrescentar operadores ou condições aninhadas.

A opção escolhida ensina a referência comum explicitamente. Manter apenas selecionarForma(string), como no compilador antigo, ocultaria o objeto concreto; executar Java genérico ampliaria o escopo. O analisador continuará reconhecendo apenas o contrato educativo e verificará toda a entrada.

Mago e Inimigo mantêm o código cumulativo. As três subclasses declaram a sobrescrita e um construtor que encaminha os cinco atributos a super. A Estratégia declara Mago ativo = mago; atribui new Piromante(...), new Hidromante(...) ou new Eletromante(...) conforme o elemento e usa ativo = mago no else. As especializações usam os mesmos valores da instância base; não redefinem os atributos do personagem durante a batalha. A chamada ativo.lancarMagia(alvo) fica após o condicional. O motor mantém vida, posição e estados dos inimigos, aplicando o comportamento concreto em cada turno.

Plano: testes de linguagem e diagnósticos; compilação da estratégia validada; desbloqueio e migração do save com oito etapas; integração da submissão, Ajustes e arena; atualização da lição e exemplos; testes de vitória e regressão. Falhas preexistentes serão discriminadas. Sem alteração do restante da arte local.

## Validação final

EditMode: 381/383 passaram. PlayMode: 51/52 passaram. Os 13 casos novos de linguagem/compilação passaram; a cena validou estratégia inválida com dica e tentativa registrada, submissão dos três blocos, vitória contra os quatro inimigos, reload da vitória e conclusão da jornada. O teste da fase 8 agora confirma avanço para 9 após restaurar o save. O compilador validou os três blocos completos do documento de exemplos. Compilação sem erros.

Persistem somente as falhas preexistentes MagoPrefab_UsesProjectPixelArtWithPointFiltering (caminho antigo), Scene_PreparingPresentationAgainReusesAuthoredObjects (159/163 objetos) e Arena_BackdropFillsFrameEvenUnderScaledParent (escala do fundo). Relatórios locais: tmp/tcc17-editmode.json e tmp/tcc17-playmode.json. A serialização do LearningPath pela Unity foi mantida. A arte local anterior à TCC-17 permanece fora desta entrega.

## Correção do editor — 04/10/2026

O usuário reportou que Batalhar deixava o bloco em branco e pediu destaque persistente dos seletores. A inspeção da sessão confirmou que o texto estava salvo, mas seu RectTransform mantinha deslocamento vertical de 1002 pixels e estava oculto pela máscara. A exibição de documentos agora reinicia a rolagem e a seleção do cursor ao trocar de bloco, restaurar o save ou validar. O clique em Batalhar preserva o documento aberto e seu conteúdo. Mago, Inimigo, Estratégia, Ajustes e Atacar usam a paleta persistente do documento selecionado, independente do foco do botão.

Testes reproduziram o deslocamento e as cores iguais antes da correção; depois passaram os nove casos focados do editor e o percurso da fase 9 com código válido/inválido, vitória e reload. Regressão: EditMode 381/383 e PlayMode 53/54, somente as três falhas preexistentes. Relatórios locais: tmp/tcc17-editor-fix-editmode.json e tmp/tcc17-editor-fix-playmode.json.

Na cópia do jogador havia uma cerca Markdown no final do bloco Inimigo. Foi preservado backup local, removido apenas o delimitador acidental e validado o código salvo completo da fase 9 com sucesso. A sessão foi reaberta no Play Mode no bloco original.
A conferência visual também identificou sobreposição da tarefa longa da fase 9 com a área de dicas; os textos foram compactados, mantendo os códigos completos no documento de exemplos.
