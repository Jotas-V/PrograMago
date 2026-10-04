# TCC-41 — navegação dos blocos de código

A decisão já aprovada pelo usuário é manter o código livremente editável.
Os bloqueios antigos por critério pedagógico impediam selecionar Inimigo e
Estratégia, fazendo os cliques parecerem quebrados.

Todos os três blocos devem poder ser selecionados e editados durante a preparação.
Trocar de bloco deve preservar texto e seleção no save. Durante combate ou telas
de vitória, a interação continua desativada. Liberar a edição não libera a etapa 9
nem muda o critério de validação da etapa atual.

Correção mínima: retirar a restrição por etapa da disponibilidade dos blocos,
mantendo os controles de interação e combate. Evitar refazer a UI ou adiantar
polimorfismo. Testes PlayMode verificam alternância entre os três blocos,
persistência após recarregar e disponibilidade na etapa de setters.

Plano: reproduzir falha nos testes de navegação; corrigir disponibilidade;
executar regressão PlayMode; atualizar exemplos completos das etapas 1–8;
recompilar o projeto e restaurar o Play do usuário.