# Plano de implementação — TCC-5 responsiva

1. Criar testes EditMode que descrevam a cena desejada:
   - `WizardSpawnPoint` vazio e preservado em `ArenaWorld`;
   - ausência dos objetos de silhueta criados anteriormente;
   - referência serializada para um prefab `Mago` de um único quadrado;
   - `CanvasScaler` responsivo e painéis presos por âncoras relativas.
2. Executar os testes e confirmar as falhas causadas pela estrutura atual.
3. Criar testes PlayMode para a prévia instanciada sobre o spawn point e para a
   permanência do editor em modo multilinha.
4. Executar os testes e confirmar as falhas de comportamento.
5. Implementar a instanciação preguiçosa e reutilizável do prefab no
   `GameplayBootstrapper`.
6. Criar o prefab provisório `Mago`, limpar os placeholders antigos e configurar
   a cena responsiva diretamente no Unity.
7. Corrigir o tratamento de Enter com a menor alteração necessária comprovada
   pelo teste e pelo playtest manual.
8. Executar todas as suítes EditMode e PlayMode.
9. Jogar a cena, validar código válido/inválido, Enter e redimensionamento do
   Game View.
10. Commitar a correção e atualizar a TCC-5 no Linear.
