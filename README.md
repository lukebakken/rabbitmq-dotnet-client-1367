# RabbitMQ.Explore

## Usage

* Set everything up
    ```
    .\setup.ps1
    ```
* Start RabbitMQ in one Powershell session
    ```
    .\run-rabbitmq-server.ps1
    ```
* Run client application in another Powershell session
    ```
    dotnet build
    cd RabbitMQ.Explore
    dotnet run
    ```
* Revert changes
    ```
    cd -
    git restore RabbitMQ.Explore/appsettings.json
    ```

Note: you can run the project from Visual Studio as well, just be sure that `appsettings.json` contains the relative path to the `pfx` certificate.

## Info

### Convert `pem` to `crt`

```
openssl x509 -in .\certs\ca_certificate.pem -out .\certs\ca_certificate.crt
```

### Create `pfx` file

```
openssl pkcs12 -inkey .\certs\client_localhost_key.pem -in .\certs\client_localhost_certificate.pem -certfile .\certs\ca_certificate.pem -export -out .\certs\client_localhost_certificate.pfx
```

*NOTE*: password used `test1234`

### Run OpenSSL client

```
openssl s_client -tls1_2 -connect localhost:5671 -CAfile .\certs\ca_certificate.pem -cert .\certs\client_localhost_certificate.pem -key .\certs\client_localhost_key.pem
```
