# Action 3: Middleware Implementation Notes

## Summary

The User Management API now includes three custom middleware components for request/response logging, global exception handling, and simple configured bearer-token authentication. The middleware is registered in the exact order required by the coursework.

## Request and Response Logging Middleware

`RequestResponseLoggingMiddleware` records basic request and response information without logging sensitive data. It logs the HTTP method, request path, response status code, and elapsed processing time.

The middleware uses `Stopwatch` to measure elapsed time and calls `await _next(context)` so the request can continue to the next middleware or endpoint. If downstream code throws an exception, the middleware logs the method, path, and elapsed time, then rethrows the exception so the global exception-handling middleware can produce the final error response.

The middleware does not log request bodies, response bodies, `Authorization` headers, or bearer tokens.

## Global Exception-Handling Middleware

`ExceptionHandlingMiddleware` catches unhandled exceptions thrown by later middleware or endpoints. It logs the full exception internally using `ILogger<ExceptionHandlingMiddleware>`, including the request method, path, and trace identifier.

The client receives a safe, consistent JSON response:

```json
{
  "error": "Internal server error.",
  "statusCode": 500,
  "traceId": "the current request trace identifier"
}
```

The middleware does not return the exception message or stack trace to the client. This avoids exposing internal implementation details while still giving the client a trace ID that can be matched with server logs.

As part of the final middleware review, redundant controller-level write-failure catch blocks were removed from `UsersController`. Unexpected database or endpoint failures are now allowed to propagate to `ExceptionHandlingMiddleware`, which keeps exception handling centralized and avoids duplicating generic `500` response logic inside controller actions.

## Token-Authentication Middleware

`TokenAuthenticationMiddleware` protects routes beginning with `/api`. It reads a bearer token from the `Authorization` header and compares it with the configured value at `ApiAuthentication:Token`.

The expected token is not hardcoded in the middleware source code. For development coursework testing, `appsettings.Development.json` contains a clearly fake placeholder token.

The middleware returns `401 Unauthorized` with a consistent JSON response when the header is missing, the header does not use the Bearer scheme, the token is empty, or the token does not match the configured value:

```json
{
  "error": "Unauthorized.",
  "statusCode": 401
}
```

If `ApiAuthentication:Token` is missing or empty, the middleware logs a server configuration error and returns a safe `500` response. It does not log the supplied token, expected token, or full `Authorization` header.

## Configured Pipeline Order

The coursework-required pipeline order is:

```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<TokenAuthenticationMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.MapGet("/api/test/error", static IResult () =>
        throw new InvalidOperationException("Test-only exception for middleware validation."));
}

app.MapControllers();
```

Swagger/OpenAPI remains available in development because it is mapped before the API token middleware and the token middleware only protects paths beginning with `/api`.

The `/api/test/error` endpoint is only mapped in development. It exists only to test the global exception-handling middleware.

## Why Middleware Order Matters

ASP.NET Core middleware runs in registration order for incoming requests. Each middleware can inspect the `HttpContext`, call the next middleware with `await _next(context)`, or short-circuit the pipeline by writing a response and returning.

For successful authenticated API requests, the incoming order is:

```text
Exception handling -> Authentication -> Logging -> Endpoint
```

The outgoing response then travels back in reverse:

```text
Endpoint -> Logging -> Authentication -> Exception handling
```

This order allows the exception-handling middleware to catch unhandled exceptions from authentication, logging, and endpoints. It also allows the logging middleware to record the final status code and elapsed time for requests that pass authentication.

## Limitation Caused by Authentication Before Logging

`TokenAuthenticationMiddleware` can short-circuit the request pipeline. If a request is missing an `Authorization` header, uses the wrong authentication scheme, has an empty bearer token, or supplies an invalid token, the authentication middleware writes a `401 Unauthorized` response and does not call the next middleware.

Because `RequestResponseLoggingMiddleware` is registered after authentication, rejected authentication requests may not reach the logging middleware. This conflicts with a broad requirement to log all incoming requests because missing-token and invalid-token requests can be rejected before request/response logging begins.

In a production system, logging is often placed before authentication for more complete auditing:

```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseMiddleware<TokenAuthenticationMiddleware>();
app.MapControllers();
```

That alternative order would allow the logging middleware to see every incoming API request, including requests that authentication later rejects with `401 Unauthorized`. The current project does not use that alternative because the coursework requires authentication before logging.

## How the Middleware Was Tested

Middleware test requests were added to `UserManagementAPI.http` and a Postman/Bruno collection was added at `Docs/Activity3-Middleware-Postman.postman_collection.json`. These requests cover valid-token access, missing authorization, invalid bearer tokens, incorrectly formatted authorization headers, normal controller responses, invalid model data, and the development-only exception endpoint.

All manual Activity 3 middleware tests passed with the expected results.

| Test | Token condition | Expected status | Expected log or response | Actual result | Pass/Fail |
| --- | --- | --- | --- | --- | --- |
| GET `/api/users` with valid token | `Authorization: Bearer fake-development-token-for-coursework-only` | 200 | Response from users endpoint; request/response log includes method, path, status code, and elapsed time | Returned 200 and logged request/response details | Pass |
| GET `/api/users` with no Authorization header | Missing | 401 | JSON response with `error` set to `Unauthorized.` and `statusCode` set to `401`; may not be logged by request/response logging middleware because authentication short-circuits first | Returned 401 with expected unauthorized JSON | Pass |
| GET `/api/users` with invalid bearer token | Invalid bearer token | 401 | JSON response with `error` set to `Unauthorized.` and `statusCode` set to `401`; supplied token is not logged | Returned 401 with expected unauthorized JSON | Pass |
| GET `/api/users` with incorrectly formatted Authorization header | Header does not use Bearer scheme | 401 | JSON response with `error` set to `Unauthorized.` and `statusCode` set to `401` | Returned 401 with expected unauthorized JSON | Pass |
| GET `/api/users/9999` with valid token | Valid bearer token | 404 | Controller returns `404 Not Found`; request/response log records final status code | Returned 404 and logged final status code | Pass |
| GET `/api/test/error` with valid token in Development | Valid bearer token | 500 | Global exception middleware returns JSON with `error`, `statusCode`, and `traceId`; test-only exception message is not exposed | Returned 500 with expected safe JSON and no test exception details | Pass |
| POST `/api/users` with invalid user data and valid token | Valid bearer token | 400 | ASP.NET Core validation returns `400 Bad Request`; request/response log records final status code | Returned 400 validation response and logged final status code | Pass |

## How Copilot Assisted

Copilot assisted by reviewing the existing ASP.NET Core request pipeline, identifying where custom middleware should be placed, and helping create focused middleware classes with clear responsibilities.

Copilot also helped review the implementation for common middleware mistakes, including failing to await the next middleware, calling the next middleware more than once, exposing exception details, logging tokens, accidentally blocking Swagger, and making the development exception endpoint available outside development.

Copilot also helped identify that the controller-level generic database exception handling duplicated the new global exception middleware. Those redundant catch blocks were removed so unexpected exceptions are not swallowed or handled inconsistently.

During review, Copilot identified that the coursework-required order creates an auditing limitation because authentication can reject requests before they reach the logging middleware. This limitation was documented rather than changing the order, because the activity instructions require the current order.

## Production Security Limitation

The configured bearer token approach is a simple coursework demonstration. It is not suitable for a production authentication system.

Limitations include:

- A single shared static token is used for all API clients.
- There is no user identity, claims, roles, or permissions model.
- There is no token expiry, refresh, revocation, or rotation mechanism.
- The token is not cryptographically signed like a JWT.
- The middleware does not integrate with ASP.NET Core authorization policies.
- If the shared token is leaked, any holder of the token can access the protected API routes.

A production system should use a proper authentication and authorization approach, such as ASP.NET Core authentication with JWT bearer tokens, OAuth/OIDC, managed API keys, or an identity provider appropriate for the application.
