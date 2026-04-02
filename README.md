<p align="center">
  <img width="100" height="100" alt="Logo ObsidianSync" src="https://github.com/user-attachments/assets/41bb61a6-bf5b-4032-8471-c00e97a3f8ee" />
</p>

<h1 align="center">ObsidianSync</h1>

**ObsidianSync** é um projeto de sincronização para usuários do Obsidian que desejam manter seus cofres sincronizados entre dispositivos.  
Ele serve como base para criar soluções próprias de sync — seja auto-hospedado ou integrado a servidores de sincronização existentes.

> Obs.: Este repositório ainda não possui descrição detalhada no GitHub. Este README foi criado para explicar melhor o propósito e facilitar contribuições.

---

## Sobre

Este projeto contém código para implementar ou experimentar estratégias de **sincronização de notas do Obsidian** entre diferentes ambientes (desktop, servidor, mobile).  
Sincronizar notas no Obsidian envolve atualizar arquivos Markdown de forma consistente e segura. Embora o Obsidian Sync oficial exista, soluções customizadas oferecem mais controle e podem ser gratuitas ou privadas.

---

## Funcionalidades

- Sincronização entre dispositivos para cofres Obsidian  
- Controle de versões de arquivos  
- Estrutura para servidor + cliente  
- Possível integração com plugins do Obsidian  
- APIs e utilitários para estender o sync  

> Dependendo da implementação, o projeto pode não ter todas as funcionalidades prontas.

---

## Como começar

### Requisitos

- .NET (versão compatível com o projeto)  
- Visual Studio ou VS Code  
- Obsidian instalado para testar integração de plugins

### Passos

```bash
# Clone o repositório
git clone https://github.com/snortexware/ObsidianSync.git
Abra a solução ObsidianSync.sln no Visual Studio
Compile o projeto
Teste os módulos de sincronização e componentes de integração

Recomenda-se criar um plano de arquitetura antes de estender funcionalidades existentes, garantindo sync seguro e resiliente.

Contribuição

Contribuições são bem-vindas! Você pode ajudar com:

Melhor documentação
Correção de bugs
Exemplos de integração com Obsidian
Testes automáticos
Adição de suporte a servidores auto-hospedados

Como contribuir:

Faça um fork do repositório
Crie uma branch para sua feature
Envie um Pull Request
