# Deployment (Linux + Kubernetes)

The service is deployed with [Helm](https://helm.sh/).

```bash
$ helm install --name scs-store-FridgeStore --namespace scs --set <see configurable/required values below> .
```

### Example:
```bash
$ helm install --name scs-store-FridgeStore --namespace scs --set 'dockerRegistry=grassvalleyprerelease-on.azurecr.io,image.name=scs-store-FridgeStore,dockerImageTag=latest,deployment.namespace=,global.imagePullSecret=scs-docker-registry,secrets.databases=databases,config.environment=environment' .
```

The following table lists the configurable parameters of the scs-store-FridgeStore chart and their default values.

|              Parameter               |                             Description                             |                       Default                       |
| ------------------------------------ | ------------------------------------------------------------------- | --------------------------------------------------- |
| `dockerRegistry`                     | Container registry                                                  | `grassvalleyprerelease-on.azurecr.io`                                 |
| `dockerImageTag`                     | Container image tag                                                 | `latest`                                            |
| `image.name`                         | Container image name                                                | `scs-store-FridgeStore`                                   |
| `image.pullPolicy`                   | Container pull policy                                               | `Always`                                            |
| `deployment.namespace`               | Deployed container namespace                                        |                                                  |
| `deployment.replicas`                | Deployed node replicas (deployment)                                 | `3`                                                 |
| `deployment.port`                    | Container port exposed.                                             | `5000`                                              |
| `secrets.api`          	           | The name of existing secret with a GV Platform "clientId" and "clientSecret" key                                   |  		                           |
| `secrets.gvpClientId`          	   | Service's client Id keys.                                   |  		                           |
| `secrets.gvpClientSecret`            | Service's client secret keys.                                   |  		                           |
| `secrets.databases`          		   | The name of existing collection of datastore related urls                                        					 |                             			   |
| `secrets.scsMongoUrl`                | A SCS Mongo DB url                                        					 |                             			   |
| `config.environment`                  | Environment is the name of existing config with key for the GVP Address                                        |                                                  |
| `config.gvpAddress`                  | GV Platform URL                                        |                                                  |
| `global.imagePullSecret`		   | imagePullSecret is the authorization token for the docker repository                                  |                             					   |
| `ingress.annotations.kubernetes.io/ingress.class`           | Ingress controller to use                                                                                                                                                                                                       | `traefik`                                           |
| `ingress.enabled`                   			              | Whether to use Ingress                                                                                                                                                                                                          | `true`                                              |
| `ingress.path`          			                          | Base url for the service                                                                                                                                                                                                        | `api/v1/store/FridgeStore`                                |
| `ingress.routePrefix`                     			      | Route prefix prepended to the service route                                                                                                                                                                                     | `""`                                                |
| `healthcheck.readiness.initialDelaySeconds` 	              | Seconds to wait before first readiness probe is done                                                                                                                                                                            | `10`                            				  	  |
| `healthcheck.readiness.periodSeconds` 		              | Seconds to wait between each readiness probe                                                                                                                                                                                    | `10`                            					  |
| `healthcheck.readiness.timeoutSeconds` 		              | Seconds to wait before timeout of each readiness probe                                                                                                                                                                                    | `80`                            					  |
| `healthcheck.readiness.port` 					              | Port to reach readiness endpoint                              	                                                                                                                                                                | `5000`                            				  |
| `healthcheck.liveness.initialDelaySeconds` 	              | Seconds to wait before first liveness probe is done                                                                                                                                                                             | `15`                            					  |
| `healthcheck.liveness.periodSeconds` 			              | Seconds to wait between each liveness probe                                                                                                                                                                                     | `20`                            					  |
| `healthcheck.liveness.timeoutSeconds` 			              | Seconds to wait before timeout of each liveness probe                                                                                                                                                                                     | `80`                            					  |
| `healthcheck.liveness.port` 					              | Port to reach liveness endpoint                              	                                                                                                                                                                | `5000`                            			      |

A tutorial for helm can be found here -> [Kubernetes Deployments with Helm][1]


# Additional secrets

The following additional secrets are required which have been standard since the first SCS deployment:
- databases
- environment
- The secret defined in secrets.api

---
[1]: https://developer.epages.com/blog/tech-stories/kubernetes-deployments-with-helm/