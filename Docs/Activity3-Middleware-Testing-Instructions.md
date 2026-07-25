# Activity 3: Middleware Testing Instructions

## Purpose

Use `Activity3-Middleware-Postman.postman_collection.json` to test the custom middleware added for Activity 3:

- Global exception handling
- Configured bearer-token authentication
- Request and response logging
- Development-only test exception endpoint
- Swagger/OpenAPI access outside protected `/api` routes

The collection is a Postman v2.1 collection and can also be imported into Bruno.

## Prerequisites

Start the API in Development mode:

```bash
dotnet run --project UserManagementAPI/UserManagementAPI.csproj
```

The default local URL is:

```text
http://localhost:5012
```

The collection uses this fake coursework token from `appsettings.Development.json`:

```text
fake-development-token-for-coursework-only
```

This is not a real secret and should not be used for production.

## Importing the Collection

In Postman:

1. Open Postman.
2. Select Import.
3. Choose `Docs/Activity3-Middleware-Postman.postman_collection.json`.
4. Confirm the collection variables:
   - `baseUrl`: `http://localhost:5012`
   - `apiToken`: `fake-development-token-for-coursework-only`
   - `invalidToken`: `fake-invalid-token`
5. Run the collection.

In Bruno:

1. Open Bruno.
2. Import a collection.
3. Choose the Postman collection file.
4. Confirm the same variables after import.
5. Run the requests in order.

## Test Coverage

| Test | Token condition | Expected status | Expected response or behavior |
| --- | --- | --- | --- |
| Swagger/OpenAPI request | No token | 200 | OpenAPI JSON is returned, showing non-API development resources are not blocked |
| GET `/api/users` | Valid bearer token | 200 | Request reaches the API and returns a JSON array |
| GET `/api/users` | Missing Authorization header | 401 | JSON response: `error` is `Unauthorized.` and `statusCode` is `401` |
| GET `/api/users` | Invalid bearer token | 401 | JSON response: `error` is `Unauthorized.` and `statusCode` is `401` |
| GET `/api/users` | Incorrectly formatted Authorization header | 401 | JSON response: `error` is `Unauthorized.` and `statusCode` is `401` |
| GET `/api/users` | Empty bearer token | 401 | JSON response: `error` is `Unauthorized.` and `statusCode` is `401` |
| GET `/api/users/9999` | Valid bearer token | 404 | Request reaches the controller and returns `404 Not Found` |
| POST `/api/users` with invalid data | Valid bearer token | 400 | Request reaches model validation and returns `400 Bad Request` |
| GET `/api/test/error` | Valid bearer token | 500 | Global exception middleware returns safe JSON with `error`, `statusCode`, and `traceId` |

## What to Check in the API Logs

For valid-token requests that pass authentication, `RequestResponseLoggingMiddleware` should log:

- HTTP method
- Request path
- Response status code
- Elapsed processing time

For the development error endpoint, the logging middleware should log that the request threw after an elapsed time, then rethrow the exception. The global exception middleware should log the full exception internally and return a safe client response.

Because the coursework-required pipeline order is:

```text
Exception handling -> Authentication -> Logging -> Endpoints
```

requests rejected by authentication may not reach `RequestResponseLoggingMiddleware`. Missing-token and invalid-token requests can return `401 Unauthorized` without appearing in the request/response logging middleware output. This is expected for the current coursework order.

## Expected Error Response Shapes

Authentication failures should return:

```json
{
  "error": "Unauthorized.",
  "statusCode": 401
}
```

Unhandled exceptions should return:

```json
{
  "error": "Internal server error.",
  "statusCode": 500,
  "traceId": "the current request trace identifier"
}
```

The `500` response must not include the thrown exception message, stack trace, or exception type.

## Notes

The `/api/test/error` route is only available when the app is running in Development. If the app is not running in Development, that request should not be used as proof of the exception middleware behavior.

This middleware uses a simple configured bearer token for coursework demonstration only. It is not a production JWT or OAuth implementation.
