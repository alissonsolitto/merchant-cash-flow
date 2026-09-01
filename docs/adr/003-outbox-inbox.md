# Outbox e Inbox

## Contexto e Definição do Problema

Foi adotado o padrão de outbox para o serviço de lançamento e de inbox para o serviço consolidado para possuirmos resiliência e confiabilidade nos dados.

## Opções Consideradas

- Tabelas de inbox e outbox inicialmente sem espurgo de dados.
- No serviço de consolidado usamos `INSERT ON CONFLICT` para garantir que em momentos de falhas ou reprocessamentos não ocorra duplicidade no consolidado.- 
- Criação do índice `ix_outbox_pending` usado para o producer que faz leituras na tabela outbox.
- Uso de `FOR UPDATE SKIP LOCKED` para garantir bloqueio entre processos.

## Consequências / Futuro

- Tabelas de inbox e outbox podem crescer muito. No futuro implementar TTL com pg_cron para fazer a limpeza.
- No futuro poderia ser utilizado tecnologias de CDC ou bibliotecas que abstraem o padrão outbox.
