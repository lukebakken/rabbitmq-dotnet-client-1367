# RabbitMQ .NET client with X509 certs

## Usage

* Import CA certificate into the current user's Trusted Root store
    ```
    .\import-ca-certificate.ps1
    ```
* Start RabbitMQ in one Powershell session. Note: Erlang 26 must be installed!
    ```
    .\run-rabbitmq.ps1
    ```
* Run client application in another Powershell session
    ```
    cd dotnet
    dotnet build
    dotnet run
    ```

## Info

### Convert `pem` to `crt`

```
openssl x509 -in ./certs/ca_certificate.pem -out ./certs/ca_certificate.crt
```

### Create `pfx` file

```
openssl pkcs12 -inkey ./certs/client_localhost_key.pem -in ./certs/client_localhost_certificate.pem -certfile ./certs/ca_certificate.pem -export -out ./certs/client_localhost_certificate.pfx
```

*NOTE*: password used `test1234`

### Run OpenSSL client against RabbitMQ

```
openssl s_client -connect localhost:5671 -CAfile ./certs/ca_certificate.pem -cert ./certs/client_localhost_certificate.pem -key ./certs/client_localhost_key.pem
```
