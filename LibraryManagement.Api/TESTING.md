## API Testing

### Authentication & Authorization

| Test                                            | Expected Result                               | Initial Result | Fix | Retest |
| ----------------------------------------------- | --------------------------------------------- | -------------- | --- | ------ |
| Register user                                   | `200 OK`                                      | Passed         | —   | Passed |
| Register user with existing username            | `400 Bad Request`                             | Passed         | —   | Passed |
| Login with correct username/password            | `200 OK` + access token and refresh token     | Passed         | —   | Passed |
| Login with incorrect username/password          | `400 Bad Request`                             | Passed         | —   | Passed |
| Refresh token with correct user ID/token        | `200 OK` + new access token and refresh token | Passed         | —   | Passed |
| Refresh token with incorrect user ID/token      | `401 Unauthorized`                            | Passed         | —   | Passed |
| Authentication with access and refreshed tokens | Authorization successful                      | Passed         | —   | Passed |
| `[Authorize]` with Admin and User roles         | `200 OK`                                      | Passed         | —   | Passed |
| `[User]` with Admin role                        | `403 Forbidden`                               | Passed         | —   | Passed |
| `[Admin]` with Admin role                       | `200 OK`                                      | Passed         | —   | Passed |
| Access public GET endpoints                     | `200 OK`                                      | Passed         | —   | Passed |
| Public access to protected endpoints            | `401 Unauthorized`                            | Passed         | —   | Passed |

### CRUD

| Test                 | Expected Result                   | Initial Result                                                                  | Fix                                                                         | Retest |
| -------------------- | --------------------------------- | ------------------------------------------------------------------------------- | --------------------------------------------------------------------------- | ------ |
| Book and Author CRUD | `200 OK` with related entity data | Operations succeeded, but related books/authors were missing when reading by ID | Changed `FindById` functions to load related entity data into memory        | Passed |
| Missing resource     | `404 Not Found`                   | `500 Internal Server Error` with customized response                            | Created a global exception handler to return the appropriate HTTP responses | Passed |
| Conflict operation   | `409 Conflict`                    | Passed                                                                          | —                                                                           | Passed |

### Book-Author Relationships

| Test                        | Expected Result               | Initial Result                  | Fix                                             | Retest |
| --------------------------- | ----------------------------- | ------------------------------- | ----------------------------------------------- | ------ |
| Book-Author relationship    | Related entity data displayed | Related entity data was missing | Changed response to include related entity data | Passed |
| Invalid Book ID / Author ID | `404 Not Found`               | `500 Internal Server Error`     | Fixed exception handling                        | Passed |

### Search & Pagination

| Test                          | Expected Result                              | Initial Result | Fix | Retest |
| ----------------------------- | -------------------------------------------- | -------------- | --- | ------ |
| Valid search and pagination   | Successful result                            | Passed         | —   | Passed |
| No search results             | `200 OK` with empty collection `[]`          | Passed         | —   | Passed |
| Invalid pagination parameters | `400 Bad Request` or parameter normalization | Passed         | —   | Passed |

### Borrowing

| Test                    | Expected Result          | Initial Result                                                                    | Fix                                                                  | Retest |
| ----------------------- | ------------------------ | --------------------------------------------------------------------------------- | -------------------------------------------------------------------- | ------ |
| Successful borrow       | Successful loan creation | Passed                                                                            | —                                                                    | Passed |
| Borrow unavailable book | Request rejected         | Failed — loan was created                                                         | Added availability check                                             | Passed |
| Already borrowed        | `409 Conflict`           | Passed when no copies were available, but a user could borrow the same book twice | Added a method to check whether the user already has a borrowed copy | Passed |
| Missing book            | `404 Not Found`          | Passed                                                                            | —                                                                    | Passed |
| Invalid User ID         | `404 Not Found`          | `500 Internal Server Error`                                                       | Called the Find User by ID method in the endpoints                   | Passed |

### Returning

| Test                                                   | Expected Result | Initial Result | Fix | Retest |
| ------------------------------------------------------ | --------------- | -------------- | --- | ------ |
| Successful return                                      | `200 OK`        | Passed         | —   | Passed |
| Book not currently borrowed or belongs to another user | `409 Conflict`  | Passed         | —   | Passed |
| Missing book                                           | `404 Not Found` | Passed         | —   | Passed |
