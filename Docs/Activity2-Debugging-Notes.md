# Activity 2: Debugging Notes

## Summary
Copilot helped identify several reliability issues in the User Management API and suggested targeted fixes for validation, error handling, and query performance. The implementation was adjusted to return clearer responses for bad input and missing resources, while keeping the controller logic simple and readable.

## Debugging Review Table

| Problem identified | Why it was a problem | Copilot suggestion | Change implemented | Test used | Result |
| --- | --- | --- | --- | --- | --- |
| Invalid user input | Empty or whitespace-only values could be accepted and stored incorrectly. | Add validation attributes and rely on automatic model validation. | Added DTO validation with required fields, string length limits, email validation, and checks for whitespace-only values. | Bruno/Postman endpoint tests for POST/PUT validation failures | Passed: requests returned 400 Bad Request |
| Invalid email addresses | Bad email values could be stored and later cause inconsistent data. | Validate request data before saving. | Added EmailAddress validation and trimmed input before persistence. | Bruno/Postman endpoint tests for POST/PUT invalid email values | Passed: requests returned 400 Bad Request |
| Duplicate email addresses | Multiple users could share the same email, which would create ambiguous identity data. | Check for existing emails before save and return a conflict response. | Added asynchronous EF Core checks and return 409 Conflict when an email already exists. | Bruno/Postman endpoint tests for duplicate POST and duplicate-email PUT | Passed: requests returned 409 Conflict |
| Non-existent IDs | Requests for missing users could fail without a clear API response. | Return 404 Not Found for missing resources. | GET, PUT, and DELETE now return 404 Not Found when the requested user does not exist. | Bruno/Postman endpoint tests for missing GET, PUT, and DELETE IDs | Passed: requests returned 404 Not Found |
| Unhandled database errors | Write operations could fail without a controlled API response. | Catch write failures and return a safe server error response. | Wrapped SaveChangesAsync calls with targeted error handling and logging, returning a 500 Problem response without exposing internals. | Code review only; the Bruno/Postman endpoint sequence did not force a database write failure | Not runtime-tested: would require a forced DbUpdateException or failing database provider |
| Read-only query performance | Read-only requests were doing unnecessary tracking work. | Use AsNoTracking and filtered EF Core queries. | Updated GET and lookup operations to use AsNoTracking and asynchronous queries. | Build and code review; Bruno/Postman can confirm endpoint success but not query performance | Build/code review verified; performance behavior not measured by endpoint tests |
