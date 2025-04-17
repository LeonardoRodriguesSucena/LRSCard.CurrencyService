# 💱 Currency Service API

This is a robust and extensible .NET 8 Web API that provides real-time and historical currency exchange rates. 
The service uses a pluggable provider architecture (factory pattern) and supports caching, 
JWT authentication, rate limiting, structured logging and testing.

# 💱 Operations
1. Retrieve Latest Exchange Rates:
Return the currenct exchange rate for the desired baseCurrency. 
This information must be the most updated, so there is no cache involved in this operation.

Endpoint: get: https://localhost/api/v1.0/exchange-rates/lastest
Payload: baseCurrency=USD, provider=Frankfurter
Response: 
{
  "amount": 1,
  "baseCurrency": "USD",
  "date": "2025-04-17T09:31:57.0245585+04:00",
  "targetCurrencies": {
    "AUD": 1.569,
    "BGN": 1.7224,
    "BRL": 5.8715,
    "CAD": 1.3921,
    ...
  }
}

2. Currency Conversion:
Convert amounts between different currencies using the lastest exchange rates.
This information must be the most updated, so there is no cache involved in this operation.

Remarks:
The ExchangeProvider(Frankfurter) can do the convertion out of the box. So I am using it. No extra calculations needed for now.
Maybe in the future adding others ExchangeProviders, we need to implement this extra logic.
For now it is not necessary, so I prefered to keep it simple.

Endpoint: post: https://localhost/api/v1.0/exchange-rates/convert
Payload: 
{
  "amount": 1,
  "baseCurrency": "USD",
  "destinationCurrencies": [
    "EUR",
    "CAD"
  ],
  "provider": "Frankfurter"
}
Response: 
{
  "amount": 1,
  "baseCurrency": "USD",
  "date": "2025-04-17T09:31:57.0245585+04:00",
  "targetCurrencies": {
    "EUR": 1.569,
    "CAD": 1.7224,
  }
}

3. Historical Exchange Rates with Pagination:
This operation return the exchange rate for a range of days. 
This operation use cache strategy caching the past days currencies rates, once they don't change anymore.
For today's exchange rate, the cache is not used.

Endpoint: get: https://localhost/api/v1.0/exchange-rates/history
Payload: baseCurrency=USD, initialDate=2024-01-01, endDate=2024-01-30, page=1, pageSize=10,  provider=Frankfurter

Response: 
{
  "page": 1,
  "pageSize": 10,
  "totalCount": 20,
  "items": [
    {
      "amount": 1,
      "baseCurrency": "USD",
      "date": "2025-04-17T05:35:12.448Z",
      "targetCurrencies": {
        "BGN": 1.7224,
        "BRL": 5.8715,
        "CAD": 1.3921,
      }
    }
  ],
  "totalPages": 2,
  "hasPrevious": false,
  "hasNext": true
}

---

## 🚀 Features

- ✅ Currency conversion & historical rates
- ✅ Factory-based provider selection
- ✅ Redis caching
- ✅ JWT-based Authentication & RBAC
- ✅ API Rate Limiting
- ✅ Structured logging using Serilog + Seq
- ✅ Distributed Tracing
- ✅ Basic Tests implemented (unit & integration) with XUnit and Moq
- ✅ Swagger documentation with JWT auth

---

### Clone the Repository
git clone https://github.com/LeonardoRodriguesSucena/LRSCard.CurrencyService.git

## 🛠️ Setup Instructions
The application is Dockerized, so you need docker installed to start it locally.

Once you cloned the repository, open the project folder and execute:
docker compose up --build

The API will be acessible in:
http://localhost:5077

You can use Swagger to test:
https://localhost:7146/swagger/index.html
