pipeline {
    agent any
    options {
      skipDefaultCheckout true
    }
    stages {
        stage('CleanWSStart') {
            steps {
                deleteDir()
            }
        }
        stage('Checkout') {
            steps {
                shHide( 'git clone -b $BRANCH_NAME https://${GHTOKEN}@github.com/CompulsiveCoder/ArduinoPlugAndPlay.git .' )
                sh 'git config --global user.email "compulsivecoder@gmail.com"'
                sh 'git config --global user.name "CompulsiveCoderCI"'
                sh 'sh view-version.sh'
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
        stage('Increment Version') {
            when { expression { !shouldSkipBuild() } }
            steps {
                sh 'sh increment-version.sh'
            }
        }
        stage('Inject Version') {
            when { expression { !shouldSkipBuild() } }
            steps {
                sh 'sh inject-version.sh'
            }
        }
        stage('Update Version in Script') {
            when { expression { !shouldSkipBuild() } }
            steps {
                sh 'sh update-version-in-script.sh'
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
        stage('Tag and Push') {
            when { expression { !shouldSkipBuild() } }
            steps {
                sh 'sh tag-and-push.sh'
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
                sh 'git checkout scripts-installation/init.sh'
                sh 'git checkout buildnumber.txt'
                sh 'sh graduate.sh'
            }
        }
        stage('Push Version') {
            when { expression { !shouldSkipBuild() } }
            steps {
                sh 'sh increment-version.sh'
                sh 'sh update-version-in-script.sh'
                sh 'sh push-updated-version-in-script.sh'
                sh 'sh test-category.sh OLS'
                sh 'sh push-version.sh'
            }
        } 
        stage('CleanWSEnd') {
            steps {
                deleteDir()
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
          deleteDir()
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
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
 
