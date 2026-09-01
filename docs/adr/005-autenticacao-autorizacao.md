# Autenticação e autorização

## Contexto e Definição do Problema

Para atender os requisitos de segurança com autenticação e autorização foi criado um serviço para centralizar a emissão de tokens e a centralização da validação de token por um gateway que é a porta de entrada para os serviços interos.

## Opções Consideradas

- Não disseminar a responsabilidade de emissão de token e validação para serviços internos.
- Autenticação com token padrão jwt com autorização e autenticação centralizada no gateway e não sobrecarregando serviços internos com essa responsabilidade compartilhada.
- Dados de claims do token são enviados para serviços internos através de headers para identificar o usuário.

## Consequências
- Serviços internos devem sempre estar atrás de um gateway que faz a validação.
- Sem adoção do padrão completo com refresh_token.
