# 🚀 Quick Start - File Monitoring

Guia rápido para rodar o projeto completo com Docker.

---

## ⚡ Início Rápido (Recomendado)

### Pré-requisitos
- Docker Desktop instalado
- Git

### Passos

```bash
# 1. Clonar o repositório
git clone <repository-url>
cd FileMonitoring

# 2. Subir TUDO com Docker Compose
docker-compose up -d

# 3. Aguardar ~30 segundos (build + migrations)

# 4. Acessar a aplicação
```

**API:** http://localhost:5000  
**Swagger:** http://localhost:5000/swagger  
**PostgreSQL:** localhost:5432

---

## 📦 O que o Docker Compose faz?

1. ✅ Sobe o PostgreSQL
2. ✅ Builda a API .NET
3. ✅ Aplica as migrations automaticamente
4. ✅ Expõe as portas
5. ✅ Cria volume para backups de arquivos

---

## 🧪 Testando

### Via Swagger (Recomendado)
1. Acesse: http://localhost:5000/swagger
2. Teste o endpoint `POST /api/arquivos/upload`
3. Faça upload de um arquivo de exemplo

### Arquivo de Exemplo (UfCard - Tipo 0)

Crie um arquivo `teste_ufcard.txt` com o conteúdo:
```
09875643212019062620190625201906250000001UfCard
```

### Arquivo de Exemplo (FagammonCard - Tipo 1)

Crie um arquivo `teste_fagammon.txt` com o conteúdo:
```
12019052632165487FagammonCard0002451
```

---

## 📊 Verificando o Banco de Dados

```bash
# Acessar PostgreSQL
docker exec -it filemonitoring_db psql -U postgres -d FileMonitoringDB

# Dentro do psql
\dt                              # Listar tabelas
SELECT * FROM "Arquivos";        # Ver arquivos
SELECT * FROM "TransacoesArquivo"; # Ver transações
\q                               # Sair
```

---

## 🛑 Comandos Úteis

### Ver logs
```bash
# Logs da API
docker logs filemonitoring_api -f

# Logs do PostgreSQL
docker logs filemonitoring_db -f
```

### Parar tudo
```bash
docker-compose down
```

### Reconstruir e reiniciar
```bash
docker-compose down
docker-compose up -d --build
```

### Resetar TUDO (apaga dados)
```bash
docker-compose down -v
docker-compose up -d --build
```

---

## 🐛 Troubleshooting

### Porta 5000 já em uso
Edite o `docker-compose.yml` e mude a porta:
```yaml
ports:
  - "5500:80"  # Mude 5000 para 5500
```

### API não conecta no banco
Aguarde ~10 segundos após o `docker-compose up`. O PostgreSQL precisa terminar de inicializar.

### Rebuild não pega mudanças no código
```bash
docker-compose down
docker-compose build --no-cache
docker-compose up -d
```

---

## 📁 Estrutura de Arquivos Necessária

```
FileMonitoring/
├── docker-compose.yml       ← Arquivo principal
├── Dockerfile              ← Build da API
├── .dockerignore           ← Arquivos a ignorar
├── FileMonitoring.sln
├── FileMonitoring.API/
├── FileMonitoring.Application/
├── FileMonitoring.Domain/
├── FileMonitoring.Infrastructure/
└── Backups/                ← Será criado automaticamente
    └── .gitkeep           ← Para commitar pasta vazia
```

---

## 🎯 Endpoints Principais

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/api/arquivos/upload` | Upload de arquivo |
| GET | `/api/arquivos` | Listar todos |
| GET | `/api/arquivos/{id}` | Detalhes |
| GET | `/api/dashboard/estatisticas` | Estatísticas |
| GET | `/api/dashboard/grafico` | Dados para gráfico |
| DELETE | `/api/arquivos/expired` | Limpar expirados |

---

## ✅ Checklist de Funcionamento

Após `docker-compose up -d`, verifique:

- [ ] Containers rodando: `docker ps` (deve mostrar 2 containers)
- [ ] API respondendo: http://localhost:5000/swagger
- [ ] PostgreSQL acessível: `docker exec -it filemonitoring_db psql -U postgres`
- [ ] Migrations aplicadas: Tabelas `Arquivos` e `TransacoesArquivo` existem
- [ ] Upload funciona: Testa no Swagger

---

## 🚀 Próximos Passos

Após rodar o backend com sucesso:

1. Teste todos os endpoints no Swagger
2. Verifique os dados no PostgreSQL
3. Teste upload de arquivos válidos e inválidos
4. Confira os backups na pasta `Backups/`

---

## 📞 Suporte

Se encontrar problemas:

1. Veja os logs: `docker logs filemonitoring_api`
2. Verifique se as portas estão livres
3. Tente resetar: `docker-compose down -v && docker-compose up -d --build`