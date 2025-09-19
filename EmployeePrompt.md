# Employee 

I need a new entity called Employee. Employee contains following properties:
- Id of type Guid
- FirstName of type string
- LastName of type string
- Role of type string
- EMail of type string
- StartDate of type DateOnly
- EndDate of type DateOnly?
- LastStartDate of type DateOnly?
- ManagerId of type Guid?
- Manager of type string
- LoginName of type string
- EmployeeNumber of type string
- OrganizationNumber of type int
- Organization of type string
- IsDeleted of type bool

1. Implement a buld-insert, bulk-update, and bulk-delete in command.api controller with the corresponding commands and handlers. also implement query.api side.

2. UI is not directly necessery