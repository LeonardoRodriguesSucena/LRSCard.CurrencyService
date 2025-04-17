## 💱 Currency Service API

Author <a href="https://www.linkedin.com/in/leonardorodriguessucena/" target="_blank">Leonardo Rodrigues Sucena</a>. 

This is a robust and extensible .NET 8 Web API that provides real-time, historical and conversion currency exchange rates. 
The service uses a pluggable provider architecture (factory pattern) and supports caching, JWT authentication, rate limiting, structured logging and testing.

The solution follows the Clean Architecture pattern, with a focus on simplicity and maintainability avoiding complex folder structures and code duplication.

For ease of setup and testing, the necessary configuration files are included in the repository.


## 🚀 Operations
<h3>1) Retrieve Latest Exchange Rates:</h3>
Return the current exchange rate for the desired baseCurrency. 
This information must be the most updated, so there is no cache involved in this operation.

**Endpoint**: GET: `http://localhost:8080/api/v1.0/exchange-rates/lastest` </br>
**Query**: `baseCurrency=USD, provider=Frankfurter` </br>
**Response**: 
```json
{
  "amount": 1,
  "baseCurrency": "USD",
  "date": "2025-04-17T09:31:57.0245585+04:00",
  "targetCurrencies": {
    "AUD": 1.569,
    "BGN": 1.7224,
    "BRL": 5.8715,
    "CAD": 1.3921
  }
}
```

<h3>2) Currency Conversion:</h3>
Convert amounts between different currencies using the lastest exchange rates.
This operation also bypasses caching for real-time accuracy.
<b>Remarks:</b>
The ExchangeProvider(Frankfurter) can do the conversion natively, so I am using it. No extra calculations needed for now.</br>
This could change if additional providers are added in the future.
For now it is not necessary, so I prefered to keep it simple.</br></br>

**Endpoint**: POST: `http://localhost:8080/api/v1.0/exchange-rates/convert` </br>
**Payload**: 
```json
{
  "amount": 1,
  "baseCurrency": "USD",
  "destinationCurrencies": [
    "EUR",
    "CAD"
  ],
  "provider": "Frankfurter"
}
```

Response: 
```json
{
  "amount": 1,
  "baseCurrency": "USD",
  "date": "2025-04-17T09:31:57.0245585+04:00",
  "targetCurrencies": {
    "EUR": 1.569,
    "CAD": 1.7224,
  }
}
```

<h3>3) Historical Exchange Rates with Pagination:</h3>
Returns exchange rates for a date range. <br>
This operation uses caching for past dates to improve performance.
For today's exchange rates, the cache is not used.</br></br>

**Endpoint**: GET: `http://localhost:8080/api/v1.0/exchange-rates/history` <br/>
**Query**: `baseCurrency=USD&initialDate=2024-01-01&endDate=2024-01-30&page=1&pageSize=10&provider=Frankfurter`

Response:
```json
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
        "CAD": 1.3921
      }
    }
  ],
  "totalPages": 2,
  "hasPrevious": false,
  "hasNext": true
}
```
---

## 🚀 Features

- ✅ **Factory-based currency provider selection**</br>  
  - the desired currency provider is injected depending on the request information
- ✅ **Redis caching**<br>
  - enabling multiple service instances to use the same cache source
- ✅ **JWT-based Authentication & RBAC**
  - all operations are protected by JWT authentication and role-based access control (RBAC). 
  - To get an valid access token, you need to use the `/auth/login` endpoint. See more in <b>Setup Instructions</b>.
- ✅**API Rate Limiting**
  - The API is protected by rateLimiting and there are 2 policies totally configurable in config file:<br>
    - `default`: for authenticated user (it is less restrictive)<br>
    - `anonynoys`: for unauthenticated user, for example to use /auth/login endpoint to get the acessToken to use the other operations.<br>
    ⚠️<i>To keep the API simple to use, I am not asking for an <b>API key</b> to enable access to <b>/auth</b> operations, but it should exist in an real-world scenario.</i>
- ✅ **Circuit Breaker & Retry Policies with Exponential Backoff**
  - Circuit breaker implemented to avoid overloading the external service (`Frankfurter`) in case of failure.
  - `ExchangeProviderResiliency` parameters are configurable in the `appsettings.json` file for all ExchangeProviders.
- ✅ **Environment-specific configurations**
  - Application is configured to run in multiple environments (`Development`, `Staging`, `Production`) using a cfg for each environment.
- ✅ **Horizontal Scaling**
  -  API is configured to be horizontally scalable, so you can run multiple instances of the API behind a load balancer.
- ✅ **API versioning**
- ✅ **Structured logging using Serilog + Seq and Distributed Tracing**
  - For logging, I am using Serilog with Seq. So It is possible to track the request over all layers using the **CorrelationId**.<br>
  - User information like UserID(Login), SUB, IP, etc... is also injected in the logs. </br>
  Seq server: `http://localhost:5341`
- ✅ **Unit & Integration Tests implemented with `XUnit` and `Moq`**
- ✅ **Swagger documentation with JWT auth**

---

## 🛠️ Setup Instructions
To clone the repository, run the following command in your terminal:
```bash
git clone http://github.com/LeonardoRodriguesSucena/LRSCard.CurrencyService.git
cd LRSCard.CurrencyService
```

The application is Dockerized, so you need <b>Docker</b> installed and running.

Once you cloned the repository, open the project folder and execute in cmd or poweshell:
```bash
docker compose up --build
```
You can use **Swagger** to test:<br>
<a href="http://localhost:8080/swagger" target="_blank">
         http://localhost:8080/swagger</a><br>    

To check the logs, you can use the **Seq Dashboard**. <br>
<a href="http://localhost:5341" target="_blank">
         http://localhost:5341</a><br>

The API will be acessible in:<br>

<a href="http://localhost:8080/api/v1.0/auth/login" target="_blank"> http://localhost:8080/api/v1.0/auth/login</a>  --example with login operation 
<br>
<a href="http://localhost:8080/api/v1.0/exchange-rates/latest" target="_blank">http://localhost:8080/api/v1.0/exchange-rates/latest </a> --example with latest operation 

## 🔐 Authentication

To access the API operations, you need to get the `AccessToken` using the `v1/auth/login` endpoint. <br>
You can use any login and password, it will return the AccessToken with admin role.<br><br>
**Endpoint**: POST:  `http://localhost:8080/api/v1.0/auth/login` <br>
**Payload:**
```json
{
  "login": "leonardo.sucena",
  "password": "pwd123"
}
```

### 🔑 Using the Access Token ###
Now with the `AccessToken` you can use all protected endpoints.</br>
Just include it in the request Header as a Bearer token.<br>

**Header**
```javascript
Authorization: Bearer ADD_YOUR_TOKEN_HERE
```




