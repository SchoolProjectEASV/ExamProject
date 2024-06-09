job "microservices" {
  datacenters = ["dc1"]

  group "gatewayservice" {
    network {
      port "gatewayservice-port" {
        to = 8080
      }
    }

    task "build" {
      driver = "raw_exec"
      lifecycle {
        hook = "prestart"
        sidecar = false
      }
      artifact {
        source = "git::https://github.com/SchoolProjectEASV/ExamProject.git"
        destination = "local/repo"
      }
      config {
        args = ["-c", "cd local/repo/ && exec docker build --file packages/OcelotGateway/Dockerfile --tag gatewayservice:local ."]
        command = "/bin/sh"
      }
    }

    task "docker-run" {
      driver = "docker"
      config {
        image = "gatewayservice:local"
        ports = ["gatewayservice-port"]
      }
      resources {
        cpu = 512
        memory = 256
      }
    }
  }

  group "productdb" {
    network {
      port "productdb-port" {
        to = 27017
      }
    }
    task "productdb" {
      driver = "docker"
      config {
        image = "mongo:latest"
        ports = ["productdb-port"]
        volumes = ["local/productdb:/data/db"]
      }
      resources {
        cpu = 512
        memory = 512
      }
    }
  }

  group "categorydb" {
    network {
      port "categorydb-port" {
        to = 27017
      }
    }
    task "categorydb" {
      driver = "docker"
      config {
        image = "mongo:latest"
        ports = ["categorydb-port"]
        volumes = ["local/categorydb:/data/db"]
      }
      resources {
        cpu = 512
        memory = 512
      }
    }
  }

  group "productservice" {
    network {
      port "productservice-http" {
        to = 8080
      }
      port "productservice-https" {
        to = 443
      }
    }
    task "build" {
      driver = "raw_exec"
      lifecycle {
        hook = "prestart"
        sidecar = false
      }
      artifact {
        source = "git::https://github.com/SchoolProjectEASV/ExamProject.git"
        destination = "local/repo"
      }
      config {
        args = ["-c", "cd local/repo/ && exec docker build --file projects/Product/ProductService/Dockerfile --tag productservice:local ."]
        command = "/bin/sh"
      }
    }
    task "productservice" {
      driver = "docker"
      config {
        image = "productservice:local"
        ports = ["productservice-http", "productservice-https"]
      }
      resources {
        cpu = 512
        memory = 256
      }
    }
  }

  group "orderservice" {
    network {
      port "orderservice-port" {
        to = 8080
      }
    }
    task "build" {
      driver = "raw_exec"
      lifecycle {
        hook = "prestart"
        sidecar = false
      }
      artifact {
        source = "git::https://github.com/SchoolProjectEASV/ExamProject.git"
        destination = "local/repo"
      }
      config {
        args = ["-c", "cd local/repo/ && exec docker build --file projects/Order/OrderService/Dockerfile  --tag orderservice:local ."]
        command = "/bin/sh"
      }
    }
    task "orderservice" {
      driver = "docker"
      config {
        image = "orderservice:local"
        ports = ["orderservice-port"]
      }
      resources {
        cpu = 512
        memory = 256
      }
    }
  }

  group "categoryservice" {
    network {
      port "categoryservice-http" {
        to = 8080
      }
      port "categoryservice-https" {
        to = 443
      }
    }
    task "build" {
      driver = "raw_exec"
      lifecycle {
        hook = "prestart"
        sidecar = false
      }
      artifact {
        source = "git::https://github.com/SchoolProjectEASV/ExamProject.git"
        destination = "local/repo"
      }
      config {
        args = ["-c", "cd local/repo/ && exec docker build --file projects/Category/CategoryService/Dockerfile  --tag categoryservice:local ."]
        command = "/bin/sh"
      }
    }
    task "categoryservice" {
      driver = "docker"
      config {
        image = "categoryservice:local"
        ports = ["categoryservice-http", "categoryservice-https"]
      }
      resources {
        cpu = 512
        memory = 256
      }
    }
  }

  group "zipkin" {
    network {
      port "zipkin-port" {
        to = 9411
      }
    }
    task "zipkin" {
      driver = "docker"
      config {
        image = "openzipkin/zipkin:latest"
        ports = ["zipkin-port"]
      }
      resources {
        cpu = 512
        memory = 512
      }
    }
  }

  group "seq" {
    count = 1
    service {
      name = "seq"
      port = "seq-http"
    }
    network {
      port "seq-http" { to = 80 }
      port "seq-other" { to = 5341 }
    }
    task "docker-run" {
      driver = "docker"
      config {
        image = "datalust/seq:latest"
        ports = ["seq-http", "seq-other"]
        volumes = ["local/seq:/data"]
      }
      env {
        ACCEPT_EULA = "Y"
      }
      resources {
        cpu = 512
        memory = 256
      }
    }
  }

  group "orderdb" {
    count = 1
    service {
      name = "orderdb"
      port = "orderdb-port"
    }
    network {
      port "orderdb-port" { to = 5432 }
    }

    task "docker-run" {
      driver = "docker"
      config {
        image = "postgres"
        ports = ["orderdb-port"]
        volumes = ["local/orderdb:/var/lib/postgresql/data"]
      }
      env {
        POSTGRES_USER = "postgres"
        POSTGRES_PASSWORD = "SuperSecret7!"
        POSTGRES_DB = "Order_db"
      }
      resources {
        cpu = 512
        memory = 256
      }
    }
  }

  group "userdb" {
    count = 1
    service {
      name = "userdb"
      port = "userdb-port"
    }
    network {
      port "userdb-port" { to = 5432 }
    }

    task "docker-run" {
      driver = "docker"
      config {
        image = "postgres"
        ports = ["userdb-port"]
        volumes = ["local/userdb:/var/lib/postgresql/data"]
      }
      env {
        POSTGRES_USER = "postgres"
        POSTGRES_PASSWORD = "example"
        POSTGRES_DB = "userdb"
      }
      resources {
        cpu = 512
        memory = 256
      }
    }
  }

  group "userservice" {
    network {
      port "userservice-http" {
        to = 8080
      }
      port "userservice-https" {
        to = 443
      }
    }
    task "build" {
      driver = "raw_exec"
      lifecycle {
        hook = "prestart"
        sidecar = false
      }
      artifact {
        source = "git::https://github.com/SchoolProjectEASV/ExamProject.git"
        destination = "local/repo"
      }
      config {
        args = ["-c", "cd local/repo/ && exec docker build --file projects/User/UserService/Dockerfile  --tag userservice:local ."]
        command = "/bin/sh"
      }
    }
    task "userservice" {
      driver = "docker"
      config {
        image = "userservice:local"
        ports = ["userservice-http", "userservice-https"]
      }
      resources {
        cpu = 512
        memory = 256
      }
    }
  }

  group "redis" {
    network {
      port "redis-port" {
        to = 6379
      }
    }
    task "redis" {
      driver = "docker"
      config {
        image = "redislabs/redisearch:latest"
        ports = ["redis-port"]
      }
      resources {
        cpu = 512
        memory = 256
      }
    }
  }

  group "authdb" {
    count = 1
    service {
      name = "authdb"
      port = "authdb-port"
    }
    network {
      port "authdb-port" { to = 5432 }
    }

    task "docker-run" {
      driver = "docker"
      config {
        image = "postgres"
        ports = ["authdb-port"]
        volumes = ["local/authdb:/var/lib/postgresql/data"]
      }
      env {
        POSTGRES_USER = "postgres"
        POSTGRES_PASSWORD = "example"
        POSTGRES_DB = "Auth_db"
      }
      resources {
        cpu = 256
        memory = 256
      }
    }
  }

  group "authservice" {
    network {
      port "authservice-http" {
        to = 8080
      }
      port "authservice-https" {
        to = 443
      }
    }
    task "build" {
      driver = "raw_exec"
      lifecycle {
        hook = "prestart"
        sidecar = false
      }
      artifact {
        source = "git::https://github.com/SchoolProjectEASV/ExamProject.git"
        destination = "local/repo"
      }
      config {
        args = ["-c", "cd local/repo/ && exec docker build --file projects/Auth/AuthService/Dockerfile  --tag authservice:local ."]
        command = "/bin/sh"
      }
    }
    task "authservice" {
      driver = "docker"
      config {
        image = "authservice:local"
        ports = ["authservice-http", "authservice-https"]
      }
      resources {
        cpu = 512
        memory = 256
      }
    }
  }
}