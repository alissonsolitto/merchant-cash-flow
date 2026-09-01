# Gateway Yarp

## Contexto e Definição do Problema

Criação de um projeto para gateway/proxy reverso para centralizar autenticação/autorização de serviços internos e configuração de rate limit.

## Opções Consideradas

- Utilizar a biblioteca Yarp para abstrair a complexidade de um proxy reverso e utilizar como porta de entrada para serviços internos.
- Rate limit por janela para o serviço de auth e rate limit compartilhado com concorrência nos serviços internos.

## Consequências
- Para implementação em produção adotar um proxy PaaS e refatorar/refazer a lógica de autenticação.
