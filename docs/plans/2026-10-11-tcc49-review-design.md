# TCC-49 - Revisao aprovada em 11/10/2026

Branch codex/TCC-49-review-fixes baseada na entrega de audio 74898ea.
Issue TCC-49 (TCC / PrograMago), In Progress antes de alterar codigo.

Desenho aprovado: editor primeiro; combate/audio depois; ensino por ultimo.
Editor: Ctrl+Z por documento, selecao e cursor consistentes, foco recuperavel,
scrollbars internas finas no codigo e nas duas paginas do grimorio.
Combate: passos apenas ao mover, hover fora do menu com metade do ganho,
inimigos olhando o mago e com caminhada real usando os clipes existentes.
Balancear alcance/frequencia com simulacoes e preservar o boneco tutorial.
Dano recebido tem flash vermelho e som. Erro de codigo retira vida apenas no
final, conforme regra existente; nao introduzir penalidades nas fases iniciais.
Ensino: chamadas dos setters em Ajustes, limite de pontos e instancia usada;
conferir todos os exemplos do grimorio em outro contexto, sem respostas da tarefa.

Plano manual (writing-plans nao instalado; fluxo manual ja autorizado):
1. Registrar testes de regressao que falham para editor, scroll, audio e animacao.
2. Adicionar historico limitado por documento, sem gravar historico no save;
   agrupar digitacao consecutiva e separar colagem/apagamento/selecao.
3. Corrigir sincronizacao do texto, selecao e geometria do caret e foco.
4. Adicionar barras finas sem alterar a arte/layout geral.
5. Conectar passos, ganho de hover, orientacao, caminhada e dano vermelho.
6. Simular builds de mago e ajustar o minimo necessario para contra-ataques.
7. Melhorar orientacao de Ajustes e auditar os nove exemplos.
8. Rodar regressao, conferir em Play Mode, preservar save/preferencias, gerar build.
9. Registrar resultados/limitacoes e commits no Linear.
