# Sistema de Dashboard Climatológico

Sistema web em .NET 8 (C#) com backend (Web API), frontend (HTML5/CSS/JS) e banco em memória (EF Core InMemory) para monitorar dados climatológicos das capitais brasileiras. Exibe dashboard com gráficos de temperatura e umidade, estatísticas do dia e filtros por data. Coleta automática a cada 15 minutos com persistência de cada leitura.

## Requisitos atendidos

- Backend em .NET (C#), frontend HTML5/CSS/JS e base de dados para persistência (EF Core InMemory).
- Dashboard na home com dados históricos da cidade selecionada (capitais brasileiras).
- Responsivo (adapta-se a smartphones e tablets).
- Uso de API aberta: OpenWeatherMap (https://openweathermap.org/current).
- Atualização a cada 15 min e persistência de cada consulta.
- Filtros de data inicial e final.
- Dois gráficos (Temperatura e Umidade) e lista de estatísticas do dia (mínima, máxima, média etc.).
- Diagrama de arquitetura física e lógica incluído na UI (index.html).
- Instruções de instalação e execução (este README) e base para a Wiki.

## Arquitetura

- Lógica: UI (wwwroot) → API (/api/weather) → Serviço de clima (OpenWeatherMap) → Persistência (EF InMemory).
- Física: Aplicação .NET única servindo API e arquivos estáticos; chamadas HTTP à OpenWeatherMap.
- Diagrama: disponível em WeatherDashboard.Api/wwwroot/index.html (seção “Diagrama de Arquitetura”).

## Tecnologias

- .NET 8, ASP.NET Core, EF Core InMemory
- OpenWeatherMap API
- HTML5, CSS, JavaScript sem frameworks
- BackgroundService para atualização periódica

## Estrutura principal

- API
  - Controllers: WeatherDashboard.Api/Controllers/WeatherController.cs
  - Services: WeatherService, WeatherBackgroundService, WeatherSeedService
  - Models: WeatherData, BrazilianCapital
  - Configuração: WeatherDashboard.Api/appsettings.json
  - Host: WeatherDashboard.Api/Program.cs
- Frontend
  - Página: WeatherDashboard.Api/wwwroot/index.html
  - Estilos: WeatherDashboard.Api/wwwroot/styles.css
  - Script: WeatherDashboard.Api/wwwroot/script.js

## Configuração

Defina sua chave da OpenWeatherMap (não publique chaves reais) em appsettings.json:

```json
{
  "OpenWeatherMap": {
    "ApiKey": "<SUA_CHAVE_AQUI>"
  },
  "WeatherSettings": {
    "DefaultCity": "São Paulo",
    "UpdateIntervalMinutes": 15
  }
}
```

Observações:
- A UI não expõe a chave; as chamadas à OpenWeatherMap são feitas somente no backend.
- UpdateIntervalMinutes controla o intervalo do serviço em background (padrão 15 min).

## Como executar (Windows)

```powershell
cd WeatherDashboard.Api
dotnet restore
dotnet run
```

Acesse:
- UI: http://localhost:5000
- Swagger: http://localhost:5000/swagger

Se abrir a UI em outra origem (ex.: Live Server), no index.html defina:
```html
<script>window.API_BASE_URL = "http://localhost:5000";</script>
```
e habilite CORS em Program.cs para essa origem se necessário.

## Funcionalidades do dashboard

- Seleção de capital (27 capitais brasileiras).
- Filtros de data inicial e final.
- Gráficos:
  - Temperatura (valor, mínima e máxima)
  - Umidade (%)
- Estatísticas do dia atual (mínima, máxima, média, e outros campos conforme dados disponíveis).
- Clima atual.
- Responsivo para mobile e tablet.

Nota: Se ainda não houver histórico para a capital no período, o sistema exibe um ponto único com o “clima atual”. O histórico cresce enquanto a aplicação estiver rodando (InMemory é volátil).

## API (principais endpoints)

- GET /api/weather/capitals
- GET /api/weather/current/{city}/{state}
- GET /api/weather/historical/{city}/{state}?startDate=YYYY-MM-DDTHH:mm:ss&endDate=YYYY-MM-DDTHH:mm:ss
- GET /api/weather/statistics/{city}/{state}?date=YYYY-MM-DD
- GET /api/weather/chart-data/{city}/{state}?startDate=...&endDate=...
  - Retorna estrutura pronta para os gráficos (labels, temperatureData, humidityData)
  - Faz fallback para 1 ponto com “current” quando não há histórico

Consulte o Swagger para a lista completa e modelos.

## Coleta e persistência

- Cada consulta bem-sucedida é salva como WeatherData (EF Core InMemory).
- Seed inicial garante dados mínimos na primeira execução.
- WeatherBackgroundService coleta periodicamente (15 min por padrão) para todas as capitais, respeitando pequenos atrasos entre chamadas para evitar rate limit.

## Deploy

Publicação:
```powershell
cd WeatherDashboard.Api
dotnet publish -c Release -o .\publish
```

Execução:
- Kestrel: .\publish\WeatherDashboard.Api.exe
- IIS: publique a pasta e instale o ASP.NET Core Hosting Bundle.

Configurações de produção:
- Configure a chave via appsettings.Production.json ou variáveis de ambiente.
- Ajuste WeatherSettings.UpdateIntervalMinutes conforme a necessidade.

## Limitações e melhorias

- InMemory: dados são perdidos a cada reinício. Para persistência real, troque para SQL Server/PostgreSQL.
- Rate limit da OpenWeatherMap: implementar caching/backoff se necessário.
- Monitoramento e logs centralizados são recomendáveis em produção.
