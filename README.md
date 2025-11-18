# 📦 File Monitoring - Backend API

Sistema de monitoramento e processamento de arquivos financeiros enviados por adquirentes (UfCard e FagammonCard).

---

## 🏗️ Arquitetura

O projeto segue os princípios de **Clean Architecture** com separação em camadas:

```
FileMonitoring/
├── FileMonitoring.API/          # Camada de apresentação (Controllers, Middlewares)
├── FileMonitoring.Application/  # Camada de aplicação (Services, DTOs, Validações)
├── FileMonitoring.Domain/       # Camada de domínio (Entities, Enums, Interfaces)
├── FileMonitoring.Infrastructure/ # Camada de infraestrutura (Repositórios, BD, Storage)
└── FileMonitoring.Tests/        # Testes unitários
```

### Padrões Utilizados

- **Repository Pattern** - Abstração de acesso a dados
- **Unit of Work** - Gerenciamento de transações
- **Dependency Injection** - Injeção de dependências nativa do .NET
- **DTO Pattern** - Transferência de dados entre camadas
- **Service Layer** - Lógica de negócio centralizada

---

## 🛠️ Tecnologias

### Stack Principal
- **.NET 6** - Framework
- **ASP.NET Core Web API** - API REST
- **Entity Framework Core 6.0.36** - ORM
- **PostgreSQL 15** - Banco de dados
- **AutoMapper 12.0.1** - Mapeamento de objetos
- **FluentValidation 11.9.0** - Validações
- **Swagger/OpenAPI** - Documentação da API
- **Docker** - Containerização

### Pacotes NuGet
```xml
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="6.0.29" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="6.0.36" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.9.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
```

---

## 📊 Modelagem do Banco de Dados

### Entidades

#### Arquivo
Representa um arquivo recebido de adquirente.

| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | int | Identificador único |
| NomeArquivo | varchar(255) | Nome original do arquivo |
| DataRecebimento | timestamp | Data/hora de recepção |
| DataExpiracao | timestamp (nullable) | Data de expiração (TTL) |
| Status | int | 1=Recepcionado, 2=NaoRecepcionado |
| CaminhoBackup | varchar(500) | Path do backup físico |
| HashMD5 | varchar(32) | Hash para detecção de duplicatas |
| TamanhoBytes | bigint | Tamanho do arquivo |
| TipoAdquirente | int | 1=UfCard, 2=FagammonCard |
| MensagemErro | varchar(1000) (nullable) | Mensagem de erro se houver |

#### TransacaoArquivo
Representa transações parseadas do arquivo.

| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | int | Identificador único |
| ArquivoId | int | FK para Arquivo |
| TipoRegistro | int | 0=Tipo0, 1=Tipo1 |
| Estabelecimento | varchar(10) | Código do estabelecimento |
| DataProcessamento | date | Data de processamento |
| PeriodoInicial | date | Período inicial |
| PeriodoFinal | date | Período final |
| Sequencia | varchar(7) | Número sequencial |
| Empresa | varchar(20) | Nome da adquirente |

### Relacionamentos
- **Arquivo** 1:N **TransacaoArquivo** (CASCADE DELETE)

### Índices
- `HashMD5` - UNIQUE (detecção de duplicatas)
- `ArquivoId` - Performance em consultas
- `Estabelecimento` - Filtros por estabelecimento

---

## 📝 Layouts de Arquivo Suportados

### Tipo 0 - UfCard (50 caracteres)

| Posição | Tamanho | Tipo | Campo | Exemplo |
|---------|---------|------|-------|---------|
| 001-001 | 1 | NUM | Tipo de Registro | 0 |
| 002-011 | 10 | NUM | Estabelecimento | 0987564321 |
| 012-019 | 8 | NUM | Data Processamento (AAAAMMDD) | 20190626 |
| 020-027 | 8 | NUM | Período Inicial | 20190625 |
| 028-035 | 8 | NUM | Período Final | 20190625 |
| 036-042 | 7 | NUM | Sequência | 0000001 |
| 043-050 | 8 | ALFA | Empresa | UfCard |

**Exemplo:**
```
009875643212019062620190625201906250000001UfCard
```

### Tipo 1 - FagammonCard (36 caracteres)

| Posição | Tamanho | Tipo | Campo | Exemplo |
|---------|---------|------|-------|---------|
| 001-001 | 1 | NUM | Tipo de Registro | 1 |
| 002-009 | 8 | NUM | Data Processamento (AAAAMMDD) | 20190526 |
| 010-017 | 8 | NUM | Estabelecimento | 32165487 |
| 018-029 | 12 | ALFA | Empresa | FagammonCard |
| 030-036 | 7 | NUM | Sequência | 0002451 |

**Exemplo:**
```
12019052632165487FagammonCard0002451
```

---

## 🔌 API Endpoints

### Base URL
```
https://localhost:5000/api
```

---

### 📂 Arquivos

#### `POST /arquivos/upload`
Upload e processamento de arquivo.

**Request:**
```
Content-Type: multipart/form-data

file: [arquivo.txt]
```

**Response 200 - Sucesso:**
```json
{
  "sucesso": true,
  "mensagem": "Arquivo processado com sucesso",
  "arquivo": {
    "id": 1,
    "nomeArquivo": "arquivo.txt",
    "dataRecebimento": "2025-01-16T10:30:00Z",
    "dataExpiracao": "2025-02-15T10:30:00Z",
    "status": "Recepcionado",
    "tipoAdquirente": "UfCard",
    "tamanhoBytes": 50,
    "mensagemErro": null,
    "quantidadeTransacoes": 1
  }
}
```

**Response 400 - Erro:**
```json
{
  "sucesso": false,
  "mensagem": "Erro ao processar arquivo: Layout do arquivo é inválido"
}
```

---

#### `GET /arquivos`
Lista todos os arquivos processados.

**Response 200:**
```json
[
  {
    "id": 1,
    "nomeArquivo": "arquivo1.txt",
    "dataRecebimento": "2025-01-16T10:30:00Z",
    "dataExpiracao": "2025-02-15T10:30:00Z",
    "status": "Recepcionado",
    "tipoAdquirente": "UfCard",
    "tamanhoBytes": 50,
    "mensagemErro": null,
    "quantidadeTransacoes": 1
  },
  {
    "id": 2,
    "nomeArquivo": "arquivo2.txt",
    "dataRecebimento": "2025-01-16T11:00:00Z",
    "dataExpiracao": "2025-01-23T11:00:00Z",
    "status": "NaoRecepcionado",
    "tipoAdquirente": "UfCard",
    "tamanhoBytes": 45,
    "mensagemErro": "Layout do arquivo é inválido",
    "quantidadeTransacoes": 0
  }
]
```

---

#### `GET /arquivos/{id}`
Obtém detalhes de um arquivo específico com transações.

**Response 200:**
```json
{
  "id": 1,
  "nomeArquivo": "arquivo1.txt",
  "dataRecebimento": "2025-01-16T10:30:00Z",
  "dataExpiracao": "2025-02-15T10:30:00Z",
  "status": "Recepcionado",
  "tipoAdquirente": "UfCard",
  "tamanhoBytes": 50,
  "caminhoBackup": "/Backups/20250116_103000_arquivo1.txt",
  "hashMD5": "5d41402abc4b2a76b9719d911017c592",
  "mensagemErro": null,
  "transacoes": [
    {
      "id": 1,
      "tipoRegistro": "Tipo0",
      "estabelecimento": "0987564321",
      "dataProcessamento": "2019-06-26",
      "periodoInicial": "2019-06-25",
      "periodoFinal": "2019-06-25",
      "sequencia": "0000001",
      "empresa": "UfCard"
    }
  ]
}
```

**Response 404:**
```json
{
  "mensagem": "Arquivo não encontrado."
}
```

---

#### `DELETE /arquivos/{id}`
Deleta um arquivo e suas transações.

**Response 204:** No Content

**Response 404:**
```json
{
  "mensagem": "Arquivo não encontrado"
}
```

---

#### `DELETE /arquivos/expired`
Deleta todos os arquivos expirados (TTL).

**Response 200:**
```json
{
  "mensagem": "3 arquivo(s) expirado(s) deletado(s) com sucesso.",
  "quantidade": 3
}
```

---

### 📊 Dashboard

#### `GET /dashboard/estatisticas`
Estatísticas gerais do sistema (com cache de 5 minutos).

**Response 200:**
```json
{
  "totalArquivos": 150,
  "arquivosRecepcionados": 142,
  "arquivosNaoRecepcionados": 8,
  "percentualSucesso": 94.67,
  "porAdquirente": {}
}
```



---

#### `GET /dashboard/resumo`
Resumo executivo com status visual.

**Response 200:**
```json
{
  "totalArquivos": 150,
  "recepcionados": 142,
  "naoRecepcionados": 8,
  "percentualSucesso": 94.67,
  "status": "Saudável"
}
```

**Status possíveis:**
- `Saudável` - ≥ 80% de sucesso
- `Atenção` - 50-79% de sucesso
- `Crítico` - < 50% de sucesso

---

#### `POST /dashboard/limpar-cache`
Limpa o cache de estatísticas.

**Response 200:**
```json
{
  "mensagem": "Cache limpo com sucesso."
}
```

---

## ⚙️ Configuração

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=FileMonitoringDB;Username=postgres;Password=postgres"
  },
  "FileStorage": {
    "BackupPath": "Backups"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

---

## 🚀 Como Executar

### Pré-requisitos
- .NET 6 SDK
- Docker Desktop
- PostgreSQL 15 (via Docker)

### Passo 1: Clonar o Repositório
```bash
git clone <repository-url>
cd FileMonitoring
```

### Passo 2: Subir a aplicação (API + BANCO)
```bash
docker-compose up -d --build
```

### Passo 3: Acessar Swagger
```
https://localhost:5000/swagger
```

---

## 🐳 Docker Compose


### Comandos Úteis
```bash
# Subir todos os serviços
docker-compose up -d

# Ver logs
docker logs filemonitoring_db

# Parar serviços
docker-compose down

# Remover volumes (apaga dados)
docker-compose down -v

# Acessar PostgreSQL
docker exec -it filemonitoring_db psql -U postgres -d FileMonitoringDB
```

---

## 🔒 Validações

### Upload de Arquivo
- ✅ Arquivo não pode ser nulo
- ✅ Arquivo não pode estar vazio
- ✅ Tamanho máximo: 10MB
- ✅ Nome do arquivo não pode conter caracteres inválidos

### Layout
- ✅ Tipo 0: exatamente 50 caracteres
- ✅ Tipo 1: exatamente 36 caracteres
- ✅ Tipo de registro deve ser 0 ou 1
- ✅ Datas devem estar no formato AAAAMMDD
- ✅ Campos numéricos devem ser válidos

### Duplicatas
- ✅ Arquivo com mesmo MD5 não pode ser processado novamente

---

## ⏱️ TTL (Time To Live)

O sistema implementa expiração automática de arquivos:

| Status | TTL | Justificativa |
|--------|-----|---------------|
| **Recepcionado** | 30 dias | Dados parseados já estão no banco |
| **NaoRecepcionado** | 7 dias | Tempo para investigação de erros |

### Limpeza
- **Manual:** `DELETE /api/arquivos/expired`
- **Automática:** Implementar job agendado (Hangfire/Cron) em produção

---

## 🛡️ Tratamento de Erros

### Middleware Global
Captura todas as exceções não tratadas e retorna resposta padronizada:

```json
{
  "statusCode": 500,
  "message": "Ocorreu um erro interno no servidor.",
  "detailed": "NullReferenceException: Object reference not set..."
}
```

### Logs
Todas as exceções são registradas via `ILogger`.

---

## 📦 Backup de Arquivos

### Estratégia
- Arquivos são salvos fisicamente na pasta `Backups/`
- Nomenclatura: `YYYYMMDD_HHMMSS_nomeoriginal.txt`
- Caracteres especiais são sanitizados

### Path
```
FileMonitoring/
└── Backups/
    ├── 20250116_103000_arquivo1.txt
    ├── 20250116_110530_arquivo2.txt
    └── ...
```

---

## 🧪 Testando a API

### Swagger UI
Acesse `https://localhost:5000/swagger` para testar interativamente.

### cURL - Upload
```bash
curl -X POST "https://localhost:5000/api/arquivos/upload" \
  -H "Content-Type: multipart/form-data" \
  -F "file=@arquivo.txt"
```

### cURL - Listar
```bash
curl -X GET "https://localhost:5000/api/arquivos"
```

### cURL - Estatísticas
```bash
curl -X GET "https://localhost:5000/api/dashboard/estatisticas"
```

---

## 📈 Performance

### Cache
- **Estatísticas:** Cache in-memory de 5 minutos
- **Invalidação:** Endpoint `/dashboard/limpar-cache`

### Índices de Banco
- `HashMD5` (UNIQUE) - O(1) para detecção de duplicatas
- `ArquivoId` - Melhora JOINs
- `Estabelecimento` - Filtros rápidos

---

## 🔐 Segurança

### Implementado
- ✅ Validação de entrada (FluentValidation)
- ✅ Sanitização de nomes de arquivo
- ✅ Detecção de duplicatas (MD5)
- ✅ Middleware de exceção global
- ✅ Logs de erros

### Melhorias Futuras
- Autenticação JWT
- Rate limiting
- HTTPS obrigatório
- Rota para retornar dados da transação

---


## 📚 Estrutura de Pastas Completa

```
FileMonitoring/
│
├── FileMonitoring.API/
│   ├── Controllers/
│   │   ├── ArquivosController.cs
│   │   └── DashboardController.cs
│   ├── Middlewares/
│   │   └── GlobalExceptionMiddleware.cs
│   ├── Program.cs
│   ├── appsettings.json
│   └── appsettings.Development.json
│
├── FileMonitoring.Application/
│   ├── DTOs/
│   │   ├── ArquivoDto.cs
│   │   ├── ArquivoDetalhadoDto.cs
│   │   ├── TransacaoArquivoDto.cs
│   │   ├── EstatisticasDto.cs
│   │   └── UploadResultDto.cs
│   ├── Interfaces/
│   │   ├── IParsingService.cs
│   │   └── IArquivoService.cs
│   ├── Services/
│   │   ├── ParsingService.cs
│   │   └── ArquivoService.cs
│   ├── Mappings/
│   │   └── MappingProfile.cs
│   └── Validators/
│       └── ArquivoUploadValidator.cs
│
├── FileMonitoring.Domain/
│   ├── Entities/
│   │   ├── Arquivo.cs
│   │   └── TransacaoArquivo.cs
│   ├── Enums/
│   │   ├── StatusArquivo.cs
│   │   ├── TipoAdquirente.cs
│   │   └── TipoRegistro.cs
│   └── Interfaces/
│       ├── IBaseRepository.cs
│       ├── IArquivoRepository.cs
│       ├── ITransacaoArquivoRepository.cs
│       └── IUnitOfWork.cs
│
├── FileMonitoring.Infrastructure/
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   ├── UnitOfWork.cs
│   │   ├── Configurations/
│   │   │   ├── ArquivoConfiguration.cs
│   │   │   └── TransacaoArquivoConfiguration.cs
│   │   ├── Repositories/
│   │   │   ├── BaseRepository.cs
│   │   │   ├── ArquivoRepository.cs
│   │   │   └── TransacaoArquivoRepository.cs
│   │   └── Migrations/
│   │       └── (geradas automaticamente)
│   └── FileStorage/
│       ├── IFileStorageService.cs
│       └── LocalFileStorageService.cs
│
├── FileMonitoring.Tests/
│   └── (testes unitários)
│
├── Backups/
│   └── (arquivos físicos salvos aqui)
│
├── docker-compose.yml
├── .gitignore
└── README.md
```

---

## 🤝 Contribuindo

1. Fork o projeto
2. Crie uma branch (`git checkout -b feature/nova-feature`)
3. Commit suas mudanças (`git commit -m 'feat: adicionar nova feature'`)
4. Push para a branch (`git push origin feature/nova-feature`)
5. Abra um Pull Request

---

## 📄 Licença

Este projeto foi desenvolvido como case técnico.

---

## 👨‍💻 Autor

Desenvolvido como parte do processo seletivo para Desenvolvedor Full Stack Sênior.

**Stack:** .NET 6, PostgreSQL, Angular, Docker

---
