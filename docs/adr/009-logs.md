# Logs

## Contexto e Definição do Problema

Adoção de uma estrutura centralizada para logs com opção de expansão futura para outros serviços e storages.

## Opções Consideradas

- Configuração de serilog e uso de ILogger nos serviços utilizando console e file por enquanto.

## Consequências
- Logs de difícil acesso atualmente, porém o necessário e organizado para evolução de novos storages no futuro.
