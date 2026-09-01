# Arquitetura orientada e eventos

## Contexto e Definição do Problema

Foi definida a arquitetura orientada a eventos para atendener o requisito arquitetural de indepêndencia entre os serviços e requisitos não funcionais.

## Opções Consideradas

- Aplicações para lançamento e consulta de saldo consolidado diários separadas.
- Consistência forte no serviço de lançamento com outbox e idempotência.
- Consistência eventual no serviço de consolidado.
- Background service para producer e consumer.

## Consequências

- Garante requisitos de escala e resiliência.
- Producer e consumer dependem da disponibilidade do serviço de lançamento e consolidado. No futuro pode ser extraído para um novo projeto se necessário.