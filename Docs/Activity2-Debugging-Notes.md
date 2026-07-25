# Activity 2: Debugging Notes

## Summary
Copilot helped identify several reliability issues in the User Management API and suggested targeted fixes for validation, error handling, and query performance. The implementation was adjusted to return clearer responses for bad input and missing resources, while keeping the controller logic simple and readable.

## Debugging Review Table

| Problem identified | Why it was a problem | Copilot suggestion | Change implemented | Test used | Result |
| --- | --- | --- | --- | --- | --- |
| Invalid user input | Empty or whitespace-only values could be accepted and stored incorrectly. | Add validation attributes and rely on automatic model validation. | Added DTO validation with required fields, string length limits, email validation, and checks for whitespace-only values. | Manual HTTP request to POST/PUT endpoints | Pending: manual test still needs to be run |
| Invalid email addresses | Bad email values could be stored and later cause inconsistent data. | Validate request data before saving. | Added EmailAddress validation and trimmed input before persistence. | Manual HTTP request to POST/PUT endpoints | Pending: manual test still needs to be run |
| Duplicate email addresses | Multiple users could share the same email, which would create ambiguous identity data. | Check for existing emails before save and return a conflict response. | Added asynchronous EF Core checks and return 409 Conflict when an email already exists. | Manual HTTP request to POST/PUT endpoints | Pending: manual test still needs to be run |
| Non-existent IDs | Requests for missing users could fail without a clear API response. | Return 404 Not Found for missing resources. | GET, PUT, and DELETE now return 404 Not Found when the requested user does not exist. | Manual HTTP request to GET/PUT/DELETE endpoints | Pending: manual test still needs to be run |
| Unhandled database errors | Write operations could fail without a controlled API response. | Catch write failures and return a safe server error response. | Wrapped SaveChangesAsync calls with targeted error handling and logging, returning a 500 Problem response without exposing internals. | Manual HTTP request to create/update/delete flows | Pending: manual test still needs to be run |
| Read-only query performance | Read-only requests were doing unnecessary tracking work. | Use AsNoTracking and filtered EF Core queries. | Updated GET and lookup operations to use AsNoTracking and asynchronous queries. | Build and code review | Build verified; manual runtime test still pending |
