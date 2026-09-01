# Idempotência

## Contexto e Definição do Problema

No serviço de lançamento foi adotado a criação de uma chave de idempotência via header para consistência de dados e no serviço de consolidado uma tabela de inbox utilizando a chave única de cada lançamento.

## Opções Consideradas

- Criação de índice para manter consistência no domínio `ux_ledger_document_idempotency_key`
- Em concorrências no registro do ledger o processo que concluiu a transação primeiro ganha, senão lança exceção para manter a consistência dos dados.

## Consequências
- No serviço de ledger existe idempotência no lançamento de registros e por isso "pagamos" uma consulta de verificação
