#!groovy

def ProjectName = 'scs-store-FridgeStore'
def AgentLabel = "${ProjectName}-agent"

podTemplate(
  label: AgentLabel,
  containers: [
    containerTemplate(name: 'build-git', image: 'alpine/git:v2.24.3', command: 'cat', ttyEnabled: true),
    containerTemplate(name: 'build-dotnet', image: 'mcr.microsoft.com/dotnet/core/sdk:3.1', command: 'cat', ttyEnabled: true),
    containerTemplate(name: 'build-docker', image: 'docker:19.03.5', command: 'cat', ttyEnabled: true),
    containerTemplate(name: 'build-helm', image: 'alpine/helm:3.1.2', command: 'cat', ttyEnabled: true),
    containerTemplate(name: 'build-gatling', image: 'grassvalleyprerelease-on.azurecr.io/scs-gatling-gradle:0.0.440', command: 'cat', ttyEnabled: true),
    containerTemplate(name: 'build-octopus-cli', image: 'octopusdeploy/octo:7.4.1', command: 'cat', ttyEnabled: true)
  ],
  volumes: [
    secretVolume(secretName: 'environment', mountPath: '/bin/config/'),
    configMapVolume(configMapName: 'values-yamls', mountPath: '/config/values-yamls/'),
    hostPathVolume(hostPath: "/var/run/docker.sock", mountPath: "/var/run/docker.sock"),
    secretVolume(secretName: 'jenkins-uptime2-kubeconfig', mountPath: '/root/.kube/'),
    secretVolume(secretName: 'scs-ci-uptime2-cred', mountPath: '/home/jenkins/.aws/'),
  ],
  envVars: [
      secretEnvVar(key: "SCS_GATLING_GVPLATFORM_ADDRESS", secretName: "environment", secretKey: "GATLING_GVPLATFORM_ADDRESS"),
      secretEnvVar(key: "SCS_GATLING_CLIENT_KEY", secretName: "environment", secretKey: "GATLING_CLIENT_ID"),
      secretEnvVar(key: "SCS_GATLING_SECRET_KEY", secretName: "environment", secretKey: "GATLING_CLIENT_SECRET"),
      secretEnvVar(key: 'DOCKER_RELEASE_REGISTRY', secretName: 'environment', secretKey: 'DOCKER_RELEASE_REGISTRY'),
      secretEnvVar(key: 'DOCKER_RELEASE_USERNAME', secretName: 'environment', secretKey: 'DOCKER_RELEASE_USERNAME'),
      secretEnvVar(key: 'DOCKER_RELEASE_PASSWORD', secretName: 'environment', secretKey: 'DOCKER_RELEASE_PASSWORD'),
      secretEnvVar(key: 'DOCKER_PRERELEASE_REGISTRY', secretName: 'environment', secretKey: 'DOCKER_PRERELEASE_REGISTRY'),
      secretEnvVar(key: 'DOCKER_PRERELEASE_USERNAME', secretName: 'environment', secretKey: 'DOCKER_PRERELEASE_USERNAME'),
      secretEnvVar(key: 'DOCKER_PRERELEASE_PASSWORD', secretName: 'environment', secretKey: 'DOCKER_PRERELEASE_PASSWORD'),
      secretEnvVar(key: 'OCTOPUS_SERVER_URL', secretName: 'environment', secretKey: 'OCTOPUS_SERVER_URL'),
      secretEnvVar(key: 'OCTOPUS_API_KEY', secretName: 'environment', secretKey: 'OCTOPUS_API_KEY')
  ])
{
  node(AgentLabel)
  {
    sh 'git config --global http.sslVerify false'
    checkout scm

    def currentBranch = env.BRANCH_NAME
    def hostName = env.SCS_GATLING_GVPLATFORM_ADDRESS

    //Default Docker information
    def imageTag
    def shortSemanticVersion
    def imageName = ProjectName

    def releaseDockerRegistry = env.DOCKER_RELEASE_REGISTRY
    def releaseDockerUsername = env.DOCKER_RELEASE_USERNAME
    def releaseDockerPassword = env.DOCKER_RELEASE_PASSWORD

    def prereleaseDockerRegistry = env.DOCKER_PRERELEASE_REGISTRY
    def prereleaseDockerUsername = env.DOCKER_PRERELEASE_USERNAME
    def prereleaseDockerPassword = env.DOCKER_PRERELEASE_PASSWORD

    // Default Helm information
    def k8sName
    def helmChartVersion
    def routePrefix
    def octopusServerUrl = env.OCTOPUS_SERVER_URL
    def octopusApiKey = env.OCTOPUS_API_KEY

    stage("Configure")
    {
      container("build-git")
      {
        sh 'git config --global http.sslVerify false'

        def versioner = buildVersioner ProjectName, currentBranch
        imageTag = versioner.imageTag.toString()
        helmChartVersion = imageTag
        routePrefix = versioner.routePrefix
        k8sName = versioner.serviceObjectName
        shortSemanticVersion = versioner.imageTag.getShortVersion()

        echo "imageTag: ${imageTag}"
        echo "helmChartVersion: ${helmChartVersion}"
        echo "routePrefix: ${routePrefix}"
        echo "k8sName: ${k8sName}"
        echo "shortSemanticVersion: ${shortSemanticVersion}"
      }
    }

    script {
      currentBuild.setDisplayName "#$BUILD_NUMBER-$imageTag"
      currentBuild.setDescription "$prereleaseDockerRegistry/$imageName:$imageTag"
    }

    stage("Build")
    {
      container("build-dotnet")
      {
        sh "dotnet build -c Release /p:Version=${imageTag}"
      }
    }

    stage("Test")
    {
      container("build-dotnet")
      {
        try
        {
          dir("Test/Unit")
          {
            sh "dotnet test --no-build --test-adapter-path:. --logger:\"nunit;LogFilePath=unit-test.xml\" /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:Exclude=\"[xunit.*]*\""
          }
          /*dir("Test/Integration")
          {
            sh "dotnet test --no-build --test-adapter-path:. --logger:\"nunit;LogFilePath=integration-test.xml\" /p:Exclude=\"[xunit.*]*\""
          }*/
        }
        finally
        {
          step([
            $class: 'NUnitPublisher',
            testResultsPattern: '**/*-test*.xml',
            debug: false,
            keepJUnitReports: true,
            skipJUnitArchiver:false,
            failIfNoResults: true])

          cobertura coberturaReportFile: 'Test/Unit/coverage.cobertura.xml'
        }
      }
    }

    stage("Publish")
    {
      container("build-dotnet")
      {
        sh "dotnet publish --no-build -c Release -o ${env.WORKSPACE}/publish/out"

        sh 'cp ./Dockerfile ./publish/Dockerfile'
      }
    }

    stage("Dockerise")
    {
      container("build-docker")
      {
        if (prereleaseDockerUsername)
        {
          sh "echo ${prereleaseDockerPassword} | docker login -u ${prereleaseDockerUsername} --password-stdin ${prereleaseDockerRegistry}"
        }
        dir('publish')
        {
          sh "docker build -t ${prereleaseDockerRegistry}/${imageName}:${imageTag} ."
          sh "docker push ${prereleaseDockerRegistry}/${imageName}:${imageTag}"
          if (scsGit.isMasterBranch())
          {
            sh "docker tag ${prereleaseDockerRegistry}/${imageName}:${imageTag} ${prereleaseDockerRegistry}/${imageName}:latest"
            sh "docker push ${prereleaseDockerRegistry}/${imageName}:latest"
          }
        }
        if (prereleaseDockerUsername)
        {
          sh "docker logout ${prereleaseDockerRegistry}"
        }

        if (scsGit.isMasterBranch() && releaseDockerUsername)
        {
          sh "echo ${releaseDockerPassword} | docker login -u ${releaseDockerUsername} --password-stdin ${releaseDockerRegistry}"

          parallel 'Push version tag to Release Registry': {
            sh "docker tag ${prereleaseDockerRegistry}/${imageName}:${imageTag} ${releaseDockerRegistry}/${imageName}:${imageTag}"
            sh "docker push ${releaseDockerRegistry}/${imageName}:${imageTag}"
          }, 'Push latest tag to Release Registry': {
            sh "docker tag ${prereleaseDockerRegistry}/${imageName}:${imageTag} ${releaseDockerRegistry}/${imageName}:latest"
            sh "docker push ${releaseDockerRegistry}/${imageName}:latest"
          }

          sh "docker logout ${releaseDockerRegistry}"
        }
      }
    }

    stage('Helm Package') {
      container('build-helm') {
        // Remove this 'mv' line if we ever rename the helm chart folder properly.
        sh "mv Helm $ProjectName"
        sh "helm package ./$ProjectName --app-version $imageTag --version $helmChartVersion"
      }
    }

    stage('Helm Push') {
      container('build-octopus-cli') {
        def helmPackage = "$ProjectName-${helmChartVersion}.tgz"
        def renamedHelmPackage = scsOctopus.renameHelmPackage(helmPackage)
        sh "octo push --package $renamedHelmPackage --replace-existing --server $octopusServerUrl --apiKey $octopusApiKey"
      }
    }

    stage('Create Release') {
      container('build-octopus-cli') {
        def targetChannel = scsOctopus.getTargetChannel()
        def targetEnvironment = scsOctopus.getTargetEnvironment()
        // Add or remove any pipeline/branch-specific variables (like "routePrefix", for the Kubernetes Ingress Route Prefix) required for your service after the "k8sName" variable on this line.
        // Variable format is "label:value", where label must match the label of a Prompted Variable in Octopus (see "Octopus Settings")
        //sh "octo create-release --project $ProjectName --server $octopusServerUrl --apiKey $octopusApiKey --version $imageTag --packageVersion $helmChartVersion --channel '$targetChannel' --deployTo '$targetEnvironment' --variable 'k8sName:$k8sName' --variable 'routePrefix:$routePrefix'"
        sh "octo create-release --ignoreExisting --project $ProjectName --server $octopusServerUrl --apiKey $octopusApiKey --version $imageTag --packageVersion $helmChartVersion --channel '$targetChannel'"
        sh "octo deploy-release --waitForDeployment --project $ProjectName --server $octopusServerUrl --apiKey $octopusApiKey --version $imageTag --channel '$targetChannel' --deployTo '$targetEnvironment' --variable 'k8sName:$k8sName' --variable 'routePrefix:$routePrefix'"
      }
    }

    stage('Tag Commit') {
      container('build-git') {
        if (scsGit.isMasterBranch()) {
            latestTag = scsGit.getTailTagOnHeadCommit()
            if (latestTag) {
              echo 'This commit has already been tagged. Will not assign a new tag.'
            } else {
              scsGit.tagHead shortSemanticVersion, env.BUILD_URL
            }
        }
      }
    }
  }
}
