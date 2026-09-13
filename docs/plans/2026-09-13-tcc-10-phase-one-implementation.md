# Plano de implementação — TCC-10

1. Executar a suíte atual e registrar a linha de base.
2. Escrever testes falhos do domínio para tentativas por batalha, conclusão da fase e restauração do progresso.
3. Implementar o estado mínimo da fase 1 e manter compatibilidade com as batalhas futuras.
4. Escrever testes falhos do registro JSON para escrita, leitura, dados inválidos e retomada; implementar o armazenamento Unity e sua composição.
5. Escrever testes falhos de apresentação e Play Mode para revisão da fase, totais de tentativas, retomada do código e do Mago; implementar os ajustes de interface.
6. Gerar um sprite inicial de Mago em pixel art, importar no prefab e verificar a cena.
7. Executar Edit Mode e Play Mode, revisar as alterações e registrar o resultado na TCC-10.

## Resultado

- Edit Mode: 211 testes aprovados, nenhum falho.
- Play Mode: 21 testes aprovados, nenhum falho.
- Testes vermelhos confirmaram o avanço indevido ao capítulo 2, o sprite padrão, a ausência do registro e a arena restaurada incorretamente após reinício.
- A suíte foi executada em uma cópia isolada porque o Unity Editor permanecia aberto no diretório principal.
