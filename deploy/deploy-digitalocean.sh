#!/bin/bash

# Configuration
APP_NAME="neoron"
DROPLET_IP="your-droplet-ip"
DEPLOY_USER="neoronservice"
DEPLOY_PATH="/var/www/neoron"
DOTNET_ENV="Production"

# Build application
echo "Building application..."
dotnet publish ../Source/Neoron.API/Neoron.API.csproj -c Release -o ./publish

# Create deployment package
echo "Creating deployment package..."
cd publish
tar czf ../deploy.tar.gz .
cd ..

# Copy files to server
echo "Copying files to server..."
scp deploy.tar.gz $DEPLOY_USER@$DROPLET_IP:/tmp/
rm deploy.tar.gz

# Deploy on server
ssh $DEPLOY_USER@$DROPLET_IP << 'ENDSSH'
    # Stop service
    sudo systemctl stop neoron

    # Deploy new version
    cd /tmp
    sudo rm -rf $DEPLOY_PATH/*
    sudo tar xzf deploy.tar.gz -C $DEPLOY_PATH
    sudo chown -R $DEPLOY_USER:$DEPLOY_USER $DEPLOY_PATH
    rm deploy.tar.gz

    # Start service
    sudo systemctl start neoron

    # Check status
    sudo systemctl status neoron
ENDSSH

echo "Deployment completed!"
