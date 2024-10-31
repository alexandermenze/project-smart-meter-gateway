# Project: Smart Meter Gateway

This basic implementation of a Smart Meter Gateway was developed throughout the course DLMCSEEDSO02_D at [IU](https://www.iu.de/) by Alexander Menze.

## Repository Structure

- The **infrastructure** directory contains the the configuration resources and helm values for running PostgreSQL in a Kubernetes cluster. During development a local cluster was setup using [kind](https://kind.sigs.k8s.io/).
- **resources** contains the certificates used during testing. They were uploaded to GitHub to allow for easy initial setup of the project. Instructions for the issuance of production certificates can be found in this directory.
- **src** contains the .NET solution with multiple projects for the services contained in the gateway. *OperatorReceiveWebhook*, *PasswordHasher* and *SignatureGenerator* are tools written for testing and ensuring that the implemented security measures work properly.
