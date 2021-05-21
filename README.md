GV.SCS.Store.FridgeStore
=======================

Swagger generated template project.

# FridgeStore Store

The FridgeStore Store is used to perform crud operations on FridgeStore storage.

---

## Prerequisites

The following are required to build:

- [.Net Core][dotnet core]
- [Docker][docker]
- [Helm][helm]

The following are required to deploy (You will need secrets in your cluster for these):

- 
- **XXX** is for ...

---

## Build

To build against GVPlatform you will need to create a **ClientId** and **API Key** inside the client manager it can be reached at `[Your Cluster IP]/management/clients/`. Create a new client with `Grant Type: ClientCredentials` and `Allowed Scopes: platform, platform.readonly`.

### Build Docker Image

To create a Docker image use `docker build`.

```sh
docker build --rm -f "Dockerfile" -t 10.251.54.244:5000/scs-store-FridgeStore:latest .
```

To then push the image to our docker registry.

```sh
docker push 10.251.54.244:5000/scs-store-FridgeStore:latest
```

---

## Tests

Unit tests use the XUnit framework. The tests can be ran inside Visual Studio using the Test Explorer or by using the dotnet core cli.

```sh
dotnet test
```

---

## Deployment

Currently we are deploying with [Helm](https://helm.sh/). After pushing your image to the docker registry run the Helm install command.
You may refer to Helm [README.md](Helm/README.md) for deployment detail.

---

## Swagger Doc

[Swagger Doc](http://10.251.54.245/api/v1/store/FridgeStore/swagger/index.html)

---

[1]: https://developer.epages.com/blog/tech-stories/kubernetes-deployments-with-helm/
[dotnet core]: https://dotnet.microsoft.com/download
[docker]: https://docs.docker.com/install/
[helm]: https://helm.sh/docs/using_helm/#installing-helm
