# Organização e arquitetura do projeto

## Contexto e Definição do Problema

O projeto está organizado utilizando adaptações de design da literatura para podermos diminuir o impacto de complexidade e manutenabilidade (sem sobrecarga de camadas) em um projeto pequeno, porém considerando organização e segregação de responsabilidade para crescimento futuro.

## Opções Consideradas

- Cada projeto possui duas camadas principais: Apresentação (API) e Application (Contém domain e repository propositalmente).
- Uso de um padrão de use case `interface IUseCase<in TInput, TOutput>` adaptado e fortemente tipado sem uso de handlers ou bibliotecas (MediatR)

## Consequências

- Não mantém a arquitetura com todas as camadas desde o ínicio.
- Mantém dependências fortemente acopladas na mesma camada.