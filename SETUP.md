# Configuração do Sistema de Dashboard Climatológico

## Pré-requisitos
- .NET 8.0 SDK instalado
- Visual Studio 2022 ou VS Code
- Conexão com internet (para dados meteorológicos reais)
- Uma chave da OpenWeatherMap (gratuita) para a API

## Configuração Passo a Passo

### 1. Sobre a API Meteorológica (OpenWeatherMap)
O sistema utiliza a OpenWeatherMap (dados reais). A integração é feita exclusivamente no backend:
- Endpoint usado: `https://api.openweathermap.org/data/2.5/weather`
- Parâmetros principais:
  - `lat` e `lon`: coordenadas da capital
  - `appid`: sua chave da OpenWeatherMap
  - `units=metric`: resultados em °C
  - `lang=pt_br`: textos em português
- Exemplo (não use direto no front):  
  `https://api.openweathermap.org/data/2.5/weather?lat=-23.5505&lon=-46.6333&appid=SEU_APPID&units=metric&lang=pt_br`

Configuração da chave no backend (não exponha no front):
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
Arquivo: WeatherDashboard.Api/appsettings.json

### 2. Executar o Sistema
```bash
cd WeatherDashboard.Api
dotnet restore
dotnet run
```

### 3. Acessar a Aplicação
- Dashboard: http://localhost:5000
- API Swagger: http://localhost:5000/swagger

Se abrir o HTML em outra origem (ex.: Live Server), defina no index.html:
```html
<script>window.API_BASE_URL = "http://localhost:5000";</script>
```
e habilite CORS no Program.cs para essa origem.

### 4. Funcionalidades Disponíveis
- Dados meteorológicos reais (temperatura, umidade, pressão, vento)
- Dashboard com:
  - 2 gráficos: Temperatura e Umidade
  - Estatísticas do dia (mínima, máxima, média, etc.)
  - Filtros de data inicial e final
- Capitais brasileiras (27) para seleção
- Responsivo (desktop, tablet e mobile)
- Coleta automática a cada 15 minutos (serviço em background)
- Persistência de cada consulta (EF Core InMemory)

## Testando o Sistema

### Teste 1: Dashboard Principal
1) Acesse http://localhost:5000  
2) Selecione uma capital  
3) Verifique clima atual, estatísticas e gráficos

### Teste 2: Dados Históricos
1) Ajuste data inicial e final  
2) Clique em “Atualizar”  
3) Observe a atualização dos gráficos

### Teste 3: Múltiplas Capitais
1) Selecione diferentes capitais  
2) Compare os resultados

### Teste 4: Responsividade
1) Redimensione a janela  
2) Teste em dispositivo móvel

### Teste 5: API Endpoints (Swagger)
Acesse http://localhost:5000/swagger e teste:
- GET /api/weather/capitals
- GET /api/weather/current/{city}/{state}
- GET /api/weather/historical/{city}/{state}?startDate=...&endDate=...
- GET /api/weather/statistics/{city}/{state}?date=YYYY-MM-DD
- GET /api/weather/chart-data/{city}/{state}?startDate=...&endDate=...

## Logs e Monitoramento
O backend registra:
- Requisições à OpenWeatherMap
- Coleta automática (BackgroundService)
- Persistência no EF InMemory
- Situações sem histórico (fallback para “current” como 1 ponto)

## Troubleshooting

- Porta/Origem errada no front
  - Sintoma: “Cannot GET /api/weather/...”
  - Causa: Front aberto em outra origem e API_BASE apontando para o servidor errado
  - Solução: Use a UI servida pela API (http://localhost:5000) ou defina window.API_BASE_URL

- Chave da OpenWeatherMap inválida/ausente
  - Sintoma: Erros 401/403 ou falha nas consultas
  - Solução: Configure corretamente OpenWeatherMap:ApiKey no appsettings.json ou variável de ambiente

- Gráficos vazios
  - Causa: Sem histórico no período selecionado
  - Solução: Aguarde a coleta automática (a cada 15 min) ou selecione “Hoje”. O sistema faz fallback para 1 ponto (current) se necessário

- CORS (apenas se servir front e API em origens diferentes)
  - Solução: Configure app.UseCors() e origens permitidas no Program.cs

- Performance/Rate limit
  - Causa: Muitas requisições à OpenWeather
  - Solução: O BackgroundService já usa delays entre capitais. Ajuste o intervalo em WeatherSettings.UpdateIntervalMinutes

## Arquivos Importantes
- Backend/API:
  - Controller: WeatherDashboard.Api/Controllers/WeatherController.cs
  - Serviço OpenWeather: [`WeatherDashboard.Api.Services.WeatherService`](WeatherDashboard.Api/Services/WeatherService.cs)
  - Background: [`WeatherDashboard.Api.Services.WeatherBackgroundService`](WeatherDashboard.Api/Services/WeatherBackgroundService.cs)
  - Seed: WeatherDashboard.Api/Services/WeatherSeedService.cs
  - Modelos: [`WeatherDashboard.Api.Models.WeatherData`](WeatherDashboard.Api/Models/WeatherData.cs), [`WeatherDashboard.Api.Models.BrazilianCapital`](WeatherDashboard.Api/Models/BrazilianCapital.cs)
  - Host/Config: WeatherDashboard.Api/Program.cs, WeatherDashboard.Api/appsettings.json
- Frontend:
  - Página: WeatherDashboard.Api/wwwroot/index.html
  - Script: WeatherDashboard.Api/wwwroot/script.js
  - Estilos: WeatherDashboard.Api/wwwroot/styles.css

## Observações
- Banco InMemory: dados são perdidos ao reiniciar a aplicação (adequado para testes). Para produção, troque por um banco real (SQL Server/PostgreSQL) e aplique migrations.
- A chave da OpenWeather deve ficar apenas no backend (nunca no front).