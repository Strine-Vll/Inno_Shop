# Inno_Shop
Inno_Shop consists of 2 microservices, API gateway and tests.
## User Management:
- RESTful API for CRUD operations with users;
- Provides registration and login actions using JWT tokens;
- Provides mechanisms for account confirmation and reset password  via email;
- Tests with XUnit;
## Product Management:
- RESTful API for CRUD operations with products;
- Provides search and filtering mechanisms based on various parameters;
- Create, update and delete operations restrictions (only authorized users can create product, only owner of the product can updaate and delete it);
- Tests with XUnit;

This project was implemented using Docker, Entity Framework, MSSql Server.
