pipeline {
    agent any
    options {
        disableConcurrentBuilds();
    }
    stages {
        stage('Checkout') {
            steps {
                shHide( 'git remote set-url origin https://${GHTOKEN}@github.com/CompulsiveCoder/ArduinoPlugAndPlay.git' )
                sh "git config --add remote.origin.fetch +refs/heads/master:refs/remotes/origin/master"
                sh "git fetch --no-tags"
                sh 'git checkout $BRANCH_NAME'
                sh 'git pull origin $BRANCH_NAME'
            }
        }
        stage('Prepare') {
            when { expression { !shouldSkipBuild() } }
            steps {
                sh 'echo "Prepare script skipped" #sh prepare.sh'
            }
        }
        stage('Init') {
            when { expression { !shouldSkipBuild() } }
            steps {
                sh 'sh init.sh'
            }
        }
        stage('Inject Version') {
            when { expression { !shouldSkipBuild() } }
            steps {
                sh 'sh inject-version.sh'
            }
        }
        stage('Build') {
            when { expression { !shouldSkipBuild() } }
            steps {
                sh 'sh build-all.sh'
            }
        }
        stage('Test') {
            when { expression { !shouldSkipBuild() } }
            steps {
                sh 'sh test-all.sh'
            }
        }
        stage('Create Release Zip') {
            when { expression { !shouldSkipBuild() } }
            steps {
                sh 'sh create-release-zip.sh'
            }
        }
        stage('Publish GitHub Release') {
            when { expression { !shouldSkipBuild() } }
            steps {
                sh 'sh publish-github-release.sh'
            }
        }
        stage('Clean') {
            when { expression { !shouldSkipBuild() } }
            steps {
                sh 'sh clean.sh'
            }
        }
        stage('Nuget') {
            when { expression { !shouldSkipBuild() } }
            steps {
                shHide( ' #sh nuget-set-api-key.sh ${NUGETTOKEN}' )
                sh ' #sh nuget-pack-and-push.sh'
            }
        } 
        stage('Graduate') {
            when { expression { !shouldSkipBuild() } }
            steps {
                sh 'sh graduate.sh'
            }
        }
        stage('Increment Version') {
            when { expression { !shouldSkipBuild() } }
            steps {
                sh 'sh increment-version.sh'
            }
        } 
        stage('Push Version') {
            when { expression { !shouldSkipBuild() } }
            steps {
                sh 'sh push-version.sh'
            }
        } 
        stage('CleanWS') {
            steps {
                cleanWs()
            }
        }
    }
    post {
        success() {
          emailext (
              subject: "SUCCESSFUL: Job '${env.JOB_NAME} [${env.BUILD_NUMBER}]'",
              body: """<p>SUCCESSFUL: Job '${env.JOB_NAME} [${env.BUILD_NUMBER}]':</p>
                <p>Check console output at "<a href="${env.BUILD_URL}">${env.JOB_NAME} [${env.BUILD_NUMBER}]</a>"</p>""",
              recipientProviders: [[$class: 'DevelopersRecipientProvider']]
            )
        }
        failure() {
          emailext (
              subject: "FAILED: Job '${env.JOB_NAME} [${env.BUILD_NUMBER}]'",
              body: """<p>FAILED: Job '${env.JOB_NAME} [${env.BUILD_NUMBER}]':</p>
                <p>Check console output at "<a href="${env.BUILD_URL}">${env.JOB_NAME} [${env.BUILD_NUMBER}]</a>"</p>""",
              recipientProviders: [[$class: 'DevelopersRecipientProvider']]
            )
        }
    }
}
Boolean shouldSkipBuild() {
    return sh( script: 'sh check-ci-skip.sh', returnStatus: true )
}
def shHide(cmd) {
    sh('#!/bin/sh -e\n' + cmd)
}


 
 
 
 
 
 
