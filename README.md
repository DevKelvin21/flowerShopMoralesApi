# FlowerShop Morales API

A .NET 9 Web API for managing flower shop transactions, inventory, and expenses with Firebase Authentication.

## Features

- **Transaction Management**: Create and manage sales transactions
- **Translation Service**: OpenAI-powered text translation
- **Firebase Authentication**: JWT-based authentication using Firebase
- **API Versioning**: Support for multiple API versions
- **Health Monitoring**: Health check endpoint for monitoring
- **Database Support**: SQLite (development) and PostgreSQL (production)

## API Endpoints

### Health Check
- **GET** `/api/v1/health` - Check API health status (no authentication required)

**Response Example:**
```json
{
  "status": "Healthy",
  "timestamp": "2024-01-15T10:30:00Z",
  "database": {
    "isConnected": true,
    "provider": "Postgres"
  },
  "version": "1.0.0"
}
```

### Protected Endpoints
All other endpoints require Firebase JWT authentication.

- **POST** `/api/v1/transactions/sale` - Create a new sale transaction
- **POST** `/api/v1/translation` - Translate text using OpenAI

## Authentication

This API uses **Firebase Authentication** for JWT token validation. The API does not provide login/signup endpoints - users authenticate through Firebase directly.

### How Authentication Works

1. **User Authentication**: Users sign up/log in through your frontend application using Firebase SDK
2. **Token Generation**: Firebase issues a JWT token to the authenticated user
3. **API Requests**: Users include the token in the `Authorization` header:
   ```
   Authorization: Bearer <firebase-jwt-token>
   ```
4. **Token Validation**: The API validates the token against Firebase's public keys

### Frontend Integration

To authenticate users in your frontend application:

```javascript
// Example using Firebase Web SDK
import { initializeApp } from 'firebase/app';
import { getAuth, signInWithEmailAndPassword } from 'firebase/auth';

const auth = getAuth();

// Sign in user
const signIn = async (email, password) => {
  const userCredential = await signInWithEmailAndPassword(auth, email, password);
  const token = await userCredential.user.getIdToken();
  
  // Use token in API requests
  const response = await fetch('/api/v1/transactions/sale', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(transactionData)
  });
};
```

### Firebase Project Configuration

The API is configured for the Firebase project `flowershop-morales`. Ensure your frontend uses the same project for authentication.

## Development Setup

### Prerequisites
- .NET 9 SDK
- SQLite (for local development)
- PostgreSQL (for production)

### Local Development

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd flowerShopMoralesApi
   ```

2. **Update connection string**
   Edit `appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Data Source=./data/app.db"
     }
   }
   ```

3. **Run database migrations**
   ```bash
   dotnet ef database update
   ```

4. **Start the application**
   ```bash
   dotnet run
   ```

The API will be available at `https://localhost:7000` (or the configured port).

### Environment Variables

For production deployment:

- `CONNECTION_STRING`: PostgreSQL connection string
- `DB_PROVIDER`: Database provider ("Postgres" for production)
- `PORT`: Port number (default: 8080)

## API Versioning

The API supports versioning through URL segments:
- `/api/v1/health` - Version 1.0 endpoints
- Future versions can be added as `/api/v2/...`

## Monitoring

Use the health endpoint for monitoring:
- **Healthy**: All systems operational
- **Degraded**: API operational but database issues detected

The health endpoint returns HTTP 200 for both healthy and degraded states, making it suitable for load balancer health checks.

## Docker Deployment

The application includes Docker support for containerized deployment:

```bash
docker build -t flowershop-api .
docker run -p 8080:8080 flowershop-api
```
