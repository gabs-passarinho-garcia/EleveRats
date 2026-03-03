---
trigger: always_on
glob: 
description: Diretrizes globais para o desenvolvimento do ecossistema EleveRats 2026.
---

# 🐀 Project EleveRats: O Código de Conduta da Nave-Mãe

> "Disciplino meu corpo como um atleta, treinando-o para fazer o que deve, de modo que, depois de ter pregado a outros, eu mesmo não seja desqualificado."
> — **1 Coríntios 9:27**

## 📜 Preâmbulo: A Nossa Missão

Este documento é o farol da nossa infraestrutura. O EleveRats não é apenas um sistema de check-in; é o motor de validação para um desafio de constância espiritual, física e familiar. Nós construímos essa Nave-Mãe para ser resiliente, blindada e eficiente. Cada linha de código em C#, cada fluxo no n8n e cada recurso no OpenTofu deve refletir a excelência exigida por Aquele que nos chamou. *Soli Deo Gloria*.

## 🛡️ Princípios Guia (The Bigode Way)

**Muralha de Fogo (Zero Trust):** A Nave-Mãe opera nas sombras. Nenhuma porta de entrada (inbound) é exposta para a internet. O tráfego flui exclusivamente pelo Cloudflare Tunnels. Segurança não é feature, é fundação.

**O Trator e o Cérebro:** Dividimos as responsabilidades brutalmente. O `n8n` é o nosso operário braçal (Gateway, Webhooks, ETL do Strava e Backups). A Minimal API em `.NET 10` é o cérebro inviolável onde residem as regras de negócio puras e a integração com MinIO e PostgreSQL.

**Economia da Ação (Realismo Poético Brutal):** Código afiado. Nada de alocações de memória desnecessárias. C# moderno exige o uso inteligente de `Records`, processamento assíncrono real e injeção de dependência nativa.

## 📜 As Leis de Ferro (Unbreakable Rules)

**C# Type Safety é Lei:** O uso de `dynamic` ou retornos tipados frouxamente (como cast direto de `object` sem validação) é a versão C# da heresia do `any`. Use generics, interfaces estritas e DTOs baseados em `record` para transporte de dados.

**O Protocolo de Comando (Safety First):** O assistente de IA opera como um Mestre Conselheiro. É ESTRITAMENTE PROIBIDO executar comandos destrutivos automaticamente. Ações como `tofu apply`, `docker compose down -v`, deleção de volumes (`/mnt/dados`), ou `git push` requerem a confirmação explícita do usuário (Gabs).

**Idioma Oficial:** Discussões, brainstorms e documentação de arquitetura no chat são em **Português Brasileiro**. Código (variáveis, classes, métodos), comentários no código, JSDoc/XML Docs e mensagens de commit (Conventional Commits) devem ser **ESTRITAMENTE EM INGLÊS**.

**Async/Await até o Talo:** Qualquer operação de I/O (ida ao PostgreSQL, upload pro MinIO, chamada ao Redis) DEVE ser assíncrona. Bloquear a thread principal (usar `.Result` ou `.Wait()`) é passível de excomunhão técnica.

## 🏛️ A Cidadela da Arquitetura (Clean Architecture & DDD)

Nossa aplicação respeita as fronteiras da Clean Architecture. A regra de ouro é inquebrável: **a dependência aponta sempre para o centro**. As camadas externas conhecem as internas, mas o núcleo não faz ideia do que existe lá fora. Não acoplamos regras de negócio a infraestrutura de banco de dados ou a frameworks HTTP.

O fluxo é sagrado: `[Presentation] -> [Application/Service] -> [Infrastructure/Repository] -> [Domain]`

### 1. The Domain Layer (O Santo dos Santos)

**Responsabilidade:** O coração do EleveRats. Contém as regras de negócio puras, o modelo de domínio do desafio, entidades (Corpo, Espírito, Casa) e Value Objects.

* **Regras de Ferro:**
  * ZERO dependências externas (nada de Entity Framework, MinIO SDK ou bibliotecas HTTP).
  * Entidades são vivas e ricas. A lógica de alteração de estado fica dentro da entidade (ex: `CheckIn.Approve()`), nunca em um serviço anêmico.
  * O domínio dita os contratos (Interfaces) que a infraestrutura terá que implementar.

### 2. The Application/Service Layer (O Salão de Estratégia)

**Responsabilidade:** Orquestra os casos de uso (Use Cases). É a ponte entre a porta de entrada da API e as regras do Domínio.

* **Regras de Ferro:**
  * Coordena a dança: busca a entidade no repositório, invoca a regra de negócio na entidade e manda o repositório salvar.
  * Serviços/Handlers devem ter uma única responsabilidade (SRP).
  * NUNCA retorna entidades puras do domínio para os endpoints. Retorna sempre um DTO/Record de resposta.
  * Foco na classe `Iterator` e padrões funcionais/assíncronos quando lidando com lotes de dados ou processamento de evidências.

### 3. The Infrastructure/Repository Layer (Os Guardiões dos Portões)

**Responsabilidade:** A única camada que sabe o que é um banco de dados PostgreSQL, um bucket no MinIO ou o cache no Redis.

* **Regras de Ferro:**
  * Implementa os contratos (Interfaces) definidos na camada de Domínio.
  * Lida com o mapeamento entre o modelo de banco de dados e as Entidades de Domínio.
  * Repositórios não devem ser "faz-tudo" (CRUDL gigante). Especialize as operações se necessário (separação de leitura/escrita, estilo CQRS leve).

### 4. The Presentation Layer (As Muralhas e Portões)

**Responsabilidade:** Receber e responder requisições do mundo exterior. É aqui que vivem as Minimal APIs no `Program.cs` e os módulos de roteamento.

* **Regras de Ferro:**
  * Mapeamento direto de Endpoints: Valida a requisição HTTP e os Records de entrada (DTOs).
  * Despacha o comando para a camada de Application/Service imediatamente.
  * Lida exclusivamente com Status Codes HTTP (200, 400, 404, 500) e cabeçalhos.
  * A injeção de dependências no contêiner do .NET Core ocorre aqui (Composition Root), amarrando as Interfaces de Domínio às implementações de Infraestrutura.

## ⚔️ O Arsenal da Nave-Mãe (Tech Stack)

* **API / Lógica de Negócio:** C# / .NET 10 (Minimal APIs)
* **Orquestração de Dados & Webhooks:** n8n
* **Banco de Dados:** PostgreSQL 18
* **Armazenamento de Objetos (Mídias):** MinIO
* **Fila e Cache:** Redis 8
* **Segurança de Borda:** Cloudflare Tunnels (`cloudflared`)
* **Observabilidade:** Prometheus + Grafana + cAdvisor
* **Infraestrutura como Código:** OpenTofu (Terraform) na Oracle Cloud (Arquitetura ARM - aarch64)
* **Deploy:** Docker Compose

## 📜 O Código do Artesão: Práticas Diárias

**1. Minimal APIs, Máxima Limpeza:**
Apesar de usarmos Minimal APIs, o `Program.cs` não é uma lixeira. Mapeie os endpoints via *Extension Methods* (ex: `app.MapCheckInEndpoints()`) e delegue a execução real para classes de serviço (Services/Handlers) injetadas via DI.

**2. A Magia do LINQ (Hybrid Vigor):**
Em vez de loops `for` ou `foreach` pesados e imperativos, favoreça as High-Order Functions do C# (LINQ): `.Select()`, `.Where()`, `.Aggregate()`. Mantenha as transformações de dados funcionais e puras.

**3. Injeção de Dependência Sempre:**
Utilize o contêiner nativo do .NET Core (`builder.Services.AddScoped`, `AddSingleton`, etc.). Nenhuma classe deve instanciar suas próprias dependências pesadas de I/O.

**4. Gestão de Segredos Implacável:**
Senhas do DB, tokens do Meta, encriptação do n8n e chaves do MinIO nascem no Oracle Vault via OpenTofu, são injetadas na máquina pelo `init.sh` no `.env`, e passadas aos containers via `docker-compose.yml`. NUNCA commite secrets no repositório.

**5. O Paradigma do Dominio:**
Mesmo sem um framework gigante por trás, respeite os limites de domínio delineados na Cidadela da Arquitetura. Vamos blindar esse núcleo!

Vamos codar com maestria, porque o que a gente constrói aqui não é só software, é ferramenta pro Reino.

*Jesus Maromba nos abençoe.*
