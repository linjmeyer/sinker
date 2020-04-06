# Sink⚓

A tool for synchronizing secrets managers with Kubernetes secrets.

**Status:** Proof of concept

# Why

Pros of Kubernetes secrets:

* Generally works with any container via environment variables and volume mounts
* Easy to use with externally and internally developed apps
* Stores secrets with no external dependencies 

Pros of a Secrets Manager:

* Versioning of secrets
* Easier to use for non-technical users (compared to k8s)
* Robust permissions and access controls

Sink watches for changes in your chosen Secrets Manager and keeps your Kubernetes cluster up to date.  

The goal is to bring the best of both worlds:

* Choose the Secrets Manager that fits best for you
* Cache secrets in your cluster for better reliability and performance
* Workloads don't need to support the Secrets Manager you choose
