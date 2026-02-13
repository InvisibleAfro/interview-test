# Coding Interview Test

## Prerequisites

- [Docker](https://docs.docker.com/get-docker/) installed and running
- [.NET SDK](https://dotnet.microsoft.com/download) (.NET 8 or newer)
- [Azure Functions Core Tools v4](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local)

## Task

Build an Azure Function that reads the employee CSV file from the SFTP server and imports each employee into the HRM system by calling the API's create-employment endpoint.

## Tech Stack

| Technology       | Purpose                |
| ---------------- | ---------------------- |
| .NET 8+          | Runtime                |
| Azure Functions  | Hosting / trigger      |
| C#               | Language               |

## What We Value

- **Working software** — the import runs end-to-end against the provided services
- **Clean, readable code** — structure, naming, separation of concerns
- **Error handling** — the real world is not always happy-path
- **Tests** — demonstrate that your code works and cover meaningful scenarios
- **Commit history** — small, well-described commits that show your thought process

## Rules

- Do **not** modify anything in the `setup/` folder. The SFTP server, CSV data, and API mock are provided as-is.
- Treat this as production code — commit as you go, just as you would in a real project.
- If an employee record is missing required fields, log a warning and skip that record — do not let it halt the entire import.

## Getting Started

1. **Fork** this repository to your own GitHub account.
2. Clone your fork and work from there.
3. When you are finished, make sure all your changes are pushed to your fork and share the link with us.

Start the environment:

```bash
docker compose -f setup/docker-compose.yml up -d
```

This brings up two services:

| Service  | Protocol | Address                  |
| -------- | -------- | ------------------------ |
| SFTP     | SFTP     | `localhost:2222`         |
| HTTP API | HTTP     | `http://localhost:8080`  |

## Services

### SFTP Server

An SFTP server is running with a pre-loaded CSV file containing employee data.

**Connection details:**

| Field    | Value       |
| -------- | ----------- |
| Host     | `localhost` |
| Port     | `2222`      |
| Username | `candidate` |
| Password | `test123`   |

**File location:** `/upload/employees.csv`

You can verify the connection with:

```bash
sftp -P 2222 candidate@localhost
# Enter password: test123
# Then run: ls upload/
```

The CSV is semicolon-delimited (`;`) and contains 30 employee records with the following columns:

| Column                   | Type   | Example                       | Description                              |
| ------------------------ | ------ | ----------------------------- | ---------------------------------------- |
| `EmployeeID`             | int    | `101`                         | Unique employee identifier               |
| `FirstName`              | string | `Nora`                        | First name                               |
| `LastName`               | string | `Klevstad`                    | Last name                                |
| `OrgUnitId`              | string | `161311`                      | Organisational unit identifier           |
| `EmailWork`              | string | `nora.klevstad@acmecorp.no`   | Work email address                       |
| `EmailPrivate`           | string | `nora.k@example.com`          | Private email address                    |
| `MobilePhone`            | string | `912 34 567`                  | Mobile phone number                      |
| `StreetAddress`          | string | `Storgata 12`                 | Street address                           |
| `ZipCode`                | string | `0184`                        | Postal code                              |
| `City`                   | string | `OSLO`                        | City                                     |
| `Country`                | string | `NO`                          | Country code (ISO 3166-1 alpha-2)        |
| `StartDate`              | string | `01.03.2010`                  | Employment start date (`dd.MM.yyyy`)     |
| `EndDate`                | string |                               | Employment end date (blank if active)    |
| `FTEFactor`              | float  | `1`                           | Full-time equivalent (0.5, 0.6, 0.8, 1)  |
| `Location`               | string | `Oslo`                        | Office location                          |
| `JobID`                  | string | `5164102`                     | Job role identifier                      |
| `IdNumber`               | string | `01019012345`                 | National ID number                       |
| `IdType`                 | string | `SSN`                         | ID type                                  |
| `IdCountry`              | string | `NO`                          | ID issuing country                       |
| `ActiveDirectoryLogin`   | string | `nora.klevstad@acmecorp.no`   | AD login                                 |
| `ManagerID`              | int    |                               | EmployeeID of the manager (blank if top) |
| `Gender`                 | string | `Female`                      | Gender                                   |
| `DateOfBirth`            | string | `01.01.1990`                  | Date of birth (`dd.MM.yyyy`)             |
| `EmploymentID`           | string | `E40200001`                   | Employment record identifier             |

**Notable data characteristics:**

- **Locations:** Oslo, Bergen, Asker & Bærum, Hamar
- **FTE values:** 0.5, 0.6, 0.8, 1 (not all employees are full-time)
- **Manager hierarchy:** some employees reference a manager via `ManagerID`
- **One terminated employee:** EmployeeID 130 has an `EndDate` set

### HRM API (WireMock)

A mock HRM API is running at `http://localhost:8080`. It mimics the 4Human HRM API and requires authentication.

#### 1. Authenticate

Obtain a bearer token before calling any other endpoint.

```bash
curl -X POST http://localhost:8080/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials&client_id=interview&client_secret=secret"
```

**Response** (HTTP 200):

```json
{
  "access_token": "mock-jwt-token-for-interview",
  "token_type": "bearer",
  "expires_in": 7200
}
```

#### 2. Create Employment

Submit employee data to the API. All requests require the `Authorization` header.

```
POST http://localhost:8080/personnel/employments
Content-Type: application/json
Authorization: Bearer <access_token>
```

**Example request:**

```bash
curl -X POST http://localhost:8080/personnel/employments \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer mock-jwt-token-for-interview" \
  -d '{
    "user": {
      "firstName": "Nora",
      "lastName": "Klevstad",
      "email": "nora.klevstad@acmecorp.no",
      "dateOfBirth": "1990-01-01T00:00:00+0000",
      "gender": "Female",
      "phoneNumberWork": {
        "phoneNumber": "91234567",
        "dialCode": "+47",
        "countryCode": "NOR"
      },
      "emailWork": "nora.klevstad@acmecorp.no",
      "emailPrivate": "nora.k@example.com",
      "addressInfo": [
        {
          "type": "primary",
          "postAddress": {
            "address": "Storgata 12",
            "zipCode": "0184",
            "city": "OSLO",
            "country": "NOR"
          }
        }
      ]
    },
    "personalIdentification": {
      "idType": "SSN",
      "idCountry": "NOR",
      "idNumber": "01019012345"
    },
    "employment": {
      "number": "E40200001",
      "startDate": "2010-03-01T00:00:00+0000",
      "jobId": 5164102,
      "orgUnitId": 161311,
      "fteFactor": 1.0,
      "location": "Oslo",
      "managerId": null
    },
    "companyEmployee": {
      "employeeId": "101"
    }
  }'
```

**Response** (HTTP 201):

```json
{
  "id": "a3f1b2c4-d5e6-7890-abcd-ef1234567890"
}
```

**Error responses:**

| Status | Cause                                                |
| ------ | ---------------------------------------------------- |
| `400`  | Bad Request — missing or invalid fields in the body  |
| `401`  | Unauthorized — missing or invalid `Authorization`    |
| `503`  | Service Unavailable — intermittent transient failure |

### Field Mapping

The table below describes how each CSV column maps to the API request body. Some CSV fields require transformation (date format conversion, country code expansion, etc.).

| CSV Column               | API Field                                     | Transformation                                        |
| ------------------------ | --------------------------------------------- | ----------------------------------------------------- |
| `FirstName`              | `user.firstName`                              |                                                       |
| `LastName`               | `user.lastName`                               |                                                       |
| `DateOfBirth`            | `user.dateOfBirth`                            | `dd.MM.yyyy` → ISO 8601 (`yyyy-MM-ddT00:00:00+0000`)  |
| `Gender`                 | `user.gender`                                 |                                                       |
| `EmailWork`              | `user.email`                                  | Same value as `emailWork`                             |
| `EmailWork`              | `user.emailWork`                              |                                                       |
| `EmailPrivate`           | `user.emailPrivate`                           |                                                       |
| `MobilePhone`            | `user.phoneNumberWork.phoneNumber`            | Remove spaces (e.g. `912 34 567` → `91234567`)        |
| `Country`                | `user.phoneNumberWork.dialCode`               | Map country to dial code (e.g. `NO` → `+47`)          |
| `Country`                | `user.phoneNumberWork.countryCode`            | ISO alpha-2 → ISO alpha-3 (e.g. `NO` → `NOR`)         |
| `ActiveDirectoryLogin`   | `user.activeDirectoryLogin`                   |                                                       |
| `StreetAddress`          | `user.addressInfo[0].postAddress.address`     |                                                       |
| `ZipCode`                | `user.addressInfo[0].postAddress.zipCode`     |                                                       |
| `City`                   | `user.addressInfo[0].postAddress.city`        |                                                       |
| `Country`                | `user.addressInfo[0].postAddress.country`     | ISO alpha-2 → ISO alpha-3 (e.g. `NO` → `NOR`)         |
| `IdNumber`               | `personalIdentification.idNumber`             |                                                       |
| `IdType`                 | `personalIdentification.idType`               |                                                       |
| `IdCountry`              | `personalIdentification.idCountry`            | ISO alpha-2 → ISO alpha-3 (e.g. `NO` → `NOR`)         |
| `EmploymentID`           | `employment.number`                           |                                                       |
| `StartDate`              | `employment.startDate`                        | `dd.MM.yyyy` → ISO 8601 (`yyyy-MM-ddT00:00:00+0000`)  |
| `EndDate`                | `employment.endDate`                          | `dd.MM.yyyy` → ISO 8601 (omit if blank)               |
| `FTEFactor`              | `employment.fteFactor`                        | Parse as decimal                                      |
| `JobID`                  | `employment.jobId`                            | Parse as integer                                      |
| `OrgUnitId`              | `employment.orgUnitId`                        | Parse as integer                                      |
| `Location`               | `employment.location`                         |                                                       |
| `ManagerID`              | `employment.managerId`                        | Parse as integer (omit if blank)                      |
| `EmployeeID`             | `companyEmployee.employeeId`                  | String                                                |

#### Inspecting Received Requests

You can review all requests the API has received at any time:

```bash
curl -s http://localhost:8080/__admin/requests | jq .
```

To see just the status codes:

```bash
curl -s http://localhost:8080/__admin/requests | jq '.requests[] | .response.status'
```

To reset the request log:

```bash
curl -X DELETE http://localhost:8080/__admin/requests
```

## Teardown

When you are done:

```bash
docker compose -f setup/docker-compose.yml down
```
