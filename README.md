# Exam project in the subjects Database For Developers, System Integration and Development of large systems.
This is our final exam project where we for the Database For Developers subject make use of polyglot databases (Document, relational and key-value store) and compare use of different ORM Frameworks (EF Core and Dapper). 
For our System Integration subject we will make use of two API Gateway solutions (Kong and Ocelot) and compare them.

## Authors

- [@Mathias](https://github.com/MathiasKrarup)
- [@Andreas](https://github.com/AndreasBerthelsen)
- [@Jens](https://github.com/JensIssa)

## The different microservices
* Product Microservice 
The product microservice is responsible for crud regarding the products. For this microservice we have made use of MongoDB.
* Category
The category microservice is responsible for crud regarding the categories. For this microservice we have made use of MongoDB.
* Auth
The Auth microservice is responsible for managing our user login and the different token. This service also hashes the password, so it is securely stored. For this microservice we have made use of PostgreSQL.
* Order
The Order microservice is responsible for crud regarding the order. For this microservice we have made use of PostgreSQL. 
* User
The Order microservice is responsible for crud regarding the order. For this microservice we have made use of PostgreSQL.

The services are accessible through the different gateways (Ocelot and Kong) by the addresses: localhost:8005 (KongAPI gateway) and localhost:8082 (Ocelot).

## Setting up the vault
The way we store our secrets (connectionstrings) is through the hashicorp vault. 
To set up the vault do following:

1. Run the following command in cmd and go to the root of the project:
```
docker compose up --build
```
2. Go into the vault interface through the docker engine.
![Vault interface in docker](./Images/VaultInDockerEngine.png)

3. Login with the provided token that the vault interface provides at the startup. Save the two tokens provided at the startup in a secure place, so you can find them again whenever you wish to use the vault.
![LoginToVaultWithToken](./Images/loginwithtoken.png)

4. Make a new secret engine called "connectionstring" with KV
![AddNewSecretEngine](./Images/enablesecretengine.png) 

5. Create 4 different secrets with the following names, with the provided connectionStrings inside the connectionstring secret engine: CONNECTIONSTRING_MONGODB, CONNECTIONSTRING_ORDER_POSTGRESS, CONNECTIONSTRING_AUTH_POSTGRESS and CONNECTIONSTRING_USER_POSTGRESS
![CreateSecret](./Images/createSecret.png)

6. Add a new authentication method and use the Username & Password authentication method.
![AddAuthMethod](./Images/enableAuthMethod.png)

7. Create a new user with the login credentials you wish.
![AddAuthMethod](./Images/createUser.png)

8. Login with the newly made user.
![LoginWithUser](./Images/signInWithUser.png)

9. Logout of the newly made user, and login through the token again.

10. Add a policy so the user can access the newly created secret engine:
![AddPolicy](./Images/connectionStringPolicy.png)

11. Add the policy to the user
![AddPolicyToUser](./Images/addPolicyToUser.png)

Now one should be able to get through the vault, if everything is set up correctly.

## Setting up the different appsettings.json

In our application, we also make use of various of configs inside the appsettings.json file.

To be able to run the program, one would need to add these different appsettings.json inside each of the different microservices.

The way that these appsettings.json look for each of the services is like this:

1. AuthenticationService

```
{
  "Jwt": {
    "Key": "YOUROWNJWTKEY",
    "Issuerkey": "YOURISSUERKEY"
  },
  "Vault": {
    "Address": "http://vault:8200/",
    "UserPass": {
      "Username": "The username you provided to the vault",
      "Password": "The Password you provide to the vault"
    }
  }
}

```

2. CategoryService

```
{
  "ProductService": {
    "Url": "http://productservice:8080/"
  },
  "Redis": {
    "Configuration": "redis:6379"
  },
  "Vault": {
    "Address": "http://vault:8200/",
    "UserPass": {
      "Username": "jens",
      "Password": "123456"
    }
  }
}
```

3. OrderService


```
{
  "ProductService": {
    "Url": "http://productservice:8080/"
  },
  "Redis": {
    "Configuration": "redis:6379"
  },
  "Vault": {
    "Address": "http://vault:8200/",
    "UserPass": {
      "Username": "The username you provided to the vault",
      "Password": "The Password you provide to the vault"
    }
  }
}
```

4. ProductService
```
{
  "CategoryService": {
    "Url": "http://categoryservice:8080/"
  },
  "Redis": {
    "Configuration": "redis:6379"
  },
  "Vault": {
    "Address": "http://vault:8200/",
    "UserPass": {
      "Username": "The username you provided to the vault",
      "Password": "The Password you provide to the vault"
    }
  }
}
```

5. Userservice

```
{
  "OrderService": {
    "Url": "http://orderservice:8080/"
  },
  "AuthService": {
    "Url": "http://authservice:8080/"
  },
  "Redis": {
    "Configuration": "redis:6379"
  },
  "Vault": {
    "Address": "http://vault:8200/",
    "UserPass": {
      "Username": "The username you provided to the vault",
      "Password": "The Password you provide to the vault"
    }
  }
}

```

6. KongGateway

```
{
  "KongSettings": {
    "JwtSecret": "YOUROWNJWTKEY",
    "key":  "YOURISSUERKEY"
  }
}
```

7. OcelotGateway

```
{
  "Jwt": {
    "Key": "YOUROWNJWTKEY"
  }
```

## Start up of the application

After the vault and the appsettings has been setup, one should be able to run the following docker command, start making requests to the application.

```
docker compose up --build
```

One should note, that before making requests, one have to unseal the vault with the unseal token that was provided at the start. This is shown in the picture below.

![UnsealVault](./Images/UnsealVault.png)

## Different nuget packages used in the application (The most vital ones)


HashiCorp.Vault v. 0.3.0

Polly v. 8.4.0

OpenTelemetry v. 1.8.1

AutoMapper v. 13.0.1

Dapper v. 2.1.35

Npgsql v. 8.0-3

MongoDB.EntityFrameworkCore v. 8.0.0

Microsoft.AspNetCore.Authentication.JwtBearer v. 8.0.5

Ocelot v. 23.2.2

Kong Admin API (Not a nuget package, communication with the Kong Admin API is done through a HTTPClient)


