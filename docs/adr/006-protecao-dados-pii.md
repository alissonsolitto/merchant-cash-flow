# Proteção de dados PII

## Contexto e Definição do Problema

Para atender os requisitos de criptografia adotar a criação de um tipo completo `ProtectedValue` que criptografa dados sensíveis e armazena o hash para filtros.

## Opções Consideradas

- Utilizar a biblioteca DataProtection da Microsoft para criptografia de dados sensíveis (documento e conta) uso do SHA-256 base64.

## Consequências
- Cuidado com a rotação de chaves do Data Protection
- Futuramente armazenar chaves e rotação em key vault.


